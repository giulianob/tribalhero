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

#endregion

namespace Game.Logic.Actions
{
    public class ObjectRemovePassiveAction : ScheduledPassiveAction
    {
        private readonly List<uint> cancelActions;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly Procedure procedure;

        private readonly CallbackProcedure callbackProcedure;

        private readonly uint cityId;

        private readonly uint objectId;

        private readonly bool wasKilled;

        public ObjectRemovePassiveAction(uint cityId,
                                         uint objectId,
                                         bool wasKilled,
                                         List<uint> cancelActions,
                                         IGameObjectLocator gameObjectLocator,
                                         ILocker locker,
                                         Procedure procedure,
                                         CallbackProcedure callbackProcedure)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            this.wasKilled = wasKilled;
            this.cancelActions = cancelActions;
            this.gameObjectLocator = gameObjectLocator;
            this.locker = locker;
            this.procedure = procedure;
            this.callbackProcedure = callbackProcedure;
        }

        public ObjectRemovePassiveAction(uint id,
                                         DateTime beginTime,
                                         DateTime nextTime,
                                         DateTime endTime,
                                         bool isVisible,
                                         string nlsDescription,
                                         Dictionary<string, string> properties,
                                         IGameObjectLocator gameObjectLocator,
                                         ILocker locker,
                                         Procedure procedure,
                                         CallbackProcedure callbackProcedure)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.gameObjectLocator = gameObjectLocator;
            this.locker = locker;
            this.procedure = procedure;
            this.callbackProcedure = callbackProcedure;
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
            IGameObject obj;

            using (locker.Lock(cityId, out city))
            {
                if (city == null)
                {
                    throw new Exception("City is missing");
                }

                if (!city.TryGetObject(objectId, out obj))
                {
                    throw new Exception("Obj is missing");
                }
            }

            // Cancel all active actions
            int loopCount = 0;
            while (true)
            {
                GameAction action;

                using (locker.Lock(cityId, out city))
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    IGameObject obj1 = obj;
                    action = city.Worker.ActiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj1);

                    if (action == null)
                    {
                        break;
                    }

                    loopCount++;
                    if (loopCount == 1000)
                    {
                        throw new Exception(string.Format("Unable to cancel all active actions. Stuck cancelling {0}", action.Type));
                    }
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all passive actions
            loopCount = 0;
            while (true)
            {
                GameAction action;

                using (locker.Lock(cityId, out city))
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    IGameObject obj1 = obj;
                    action = city.Worker.PassiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj1);

                    if (action == null)
                    {
                        break;
                    }

                    loopCount++;
                    if (loopCount == 1000)
                    {
                        throw new Exception(string.Format("Unable to cancel all passive actions. Stuck cancelling {0}", action.Type)); 
                    }
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all references
            foreach (var actionId in cancelActions)
            {
                GameAction action;
                using (locker.Lock(cityId, out city))
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    uint actionId1 = actionId;
                    action = city.Worker.ActiveActions.Values.FirstOrDefault(x => x.ActionId == actionId1);
                    if (action == null)
                    {
                        continue;
                    }
                }

                action.WorkerRemoved(wasKilled);
            }

            using (locker.Lock(cityId, out city))
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
                    city.DoRemove((TroopObject)obj);
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

                    callbackProcedure.OnStructureDowngrade(structure);

                    foreach (var tech in techs)
                    {
                        callbackProcedure.OnTechnologyDelete(structure, tech.TechBase);
                    }
                }

                StateChange(ActionState.Completed);
            }
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