#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class ObjectRemovePassiveAction : ScheduledPassiveAction
    {
        private List<uint> cancelActions;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly IDbManager dbmanager;

        private readonly ILocker locker;

        private readonly Procedure procedure;

        private readonly CallbackProcedure callbackProcedure;

        private uint cityId;

        private uint objectId;

        private bool wasKilled;

        public ObjectRemovePassiveAction(IGameObjectLocator gameObjectLocator,
                                         ILocker locker,
                                         Procedure procedure,
                                         CallbackProcedure callbackProcedure,
                                         IDbManager dbmanager)
        {
            this.gameObjectLocator = gameObjectLocator;
            this.locker = locker;
            this.procedure = procedure;
            this.callbackProcedure = callbackProcedure;
            this.dbmanager = dbmanager;
        }

        public ObjectRemovePassiveAction(uint cityId,
                                         uint objectId,
                                         bool wasKilled,
                                         List<uint> cancelActions,
                                         IGameObjectLocator gameObjectLocator,
                                         ILocker locker,
                                         Procedure procedure,
                                         CallbackProcedure callbackProcedure, 
                                         IDbManager dbmanager)
            : this(gameObjectLocator, locker, procedure, callbackProcedure, dbmanager)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            this.wasKilled = wasKilled;
            this.cancelActions = cancelActions;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
            wasKilled = bool.Parse(properties["was_killed"]);

            cancelActions = new List<uint>();
            foreach (var actionId in properties["cancel_references"].Split(',').Where(actionId => actionId != string.Empty))
            {
                cancelActions.Add(uint.Parse(actionId));
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ObjectRemovePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId),
                                new XmlKvPair("object_id", objectId),
                                new XmlKvPair("was_killed", wasKilled),
                                new XmlKvPair("cancel_references", string.Join(",", cancelActions.ConvertAll(t => t.ToString(CultureInfo.InvariantCulture)).ToArray())),
                        });
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            BeginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow;

            ICity city;
            IGameObject obj;
            if (!gameObjectLocator.TryGetObjects(cityId, out city) || !city.TryGetObject(objectId, out obj))
            {
                return Error.ObjectNotFound;
            }

            obj.IsBlocked = ActionId;

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            IGameObject obj = null;

            locker.Lock(cityId, out city).Do(() =>
            {
                if (city == null)
                {
                    throw new Exception("City is missing");
                }

                if (!city.TryGetObject(objectId, out obj))
                {
                    throw new Exception("Obj is missing");
                }
            });

            // Cancel all active actions
            int loopCount = 0;
            while (true)
            {
                GameAction action = null;

                locker.Lock(cityId, out city).Do(() =>
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    action = city.Worker.ActiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj);

                    loopCount++;
                    if (loopCount == 1000)
                    {
                        throw new Exception(string.Format("Unable to cancel all active actions. Stuck cancelling {0}", action.Type));
                    }
                });

                if (action == null)
                {
                    break;
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all passive actions
            loopCount = 0;
            while (true)
            {
                GameAction action = null;

                locker.Lock(cityId, out city).Do(() =>
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    IGameObject obj1 = obj;
                    action = city.Worker.PassiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj1);

                    loopCount++;
                    if (loopCount == 1000)
                    {
                        throw new Exception(string.Format("Unable to cancel all passive actions. Stuck cancelling {0}", action.Type));
                    }
                });
                
                if (action == null)
                {
                    break;
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all references
            foreach (var actionId in cancelActions)
            {
                GameAction action = null;
                locker.Lock(cityId, out city).Do(() =>
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    uint actionId1 = actionId;
                    action = city.Worker.ActiveActions.Values.FirstOrDefault(x => x.ActionId == actionId1);

                });
                
                if (action == null)
                {
                    continue;
                }

                action.WorkerRemoved(wasKilled);
            }

            locker.Lock(cityId, out city).Do(() =>
            {
                if (city == null)
                {
                    throw new Exception("City is missing");
                }

                if (!city.TryGetObject(objectId, out obj))
                {
                    throw new Exception("Obj is missing");
                }

                if (city.Worker.GetActions(obj).Count() != 0)
                {
                    throw new Exception("Not all actions were cancelled for this obj");
                }

                // Finish cleaning object
                if (obj is ITroopObject)
                {
                    var troopObject = (TroopObject)obj;
                    city.DoRemove(troopObject);

                    dbmanager.Delete(troopObject);
                }
                else if (obj is IStructure)
                {
                    var structure = obj as IStructure;

                    // Save technogies for "on technology delete" event.
                    var techs = new List<Technology>(structure.Technologies.Where(x => x.OwnerLocation == EffectLocation.Object && x.OwnerId == obj.ObjectId));

                    city.BeginUpdate();

                    if (!wasKilled)
                    {
                        // Give laborers back to the city if obj was not killed off
                        ushort laborers = structure.Stats.Labor;
                        city.Resource.Labor.Add(laborers);
                    }

                    city.DoRemove(structure);
                    procedure.OnStructureUpgradeDowngrade(structure);
                    city.EndUpdate();

                    callbackProcedure.OnStructureRemove(structure);

                    foreach (var tech in techs)
                    {
                        callbackProcedure.OnTechnologyDelete(structure, tech.TechBase);
                    }                    

                    dbmanager.Delete(structure);
                }                

                StateChange(ActionState.Completed);
            });
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new Exception("Cannot remove worker while deleting objects");
        }
    }
}