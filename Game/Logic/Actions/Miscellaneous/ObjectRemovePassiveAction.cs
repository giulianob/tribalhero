#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class ObjectRemovePassiveAction : ScheduledPassiveAction
    {
        private readonly List<uint> cancelActions;

        private readonly uint cityId;

        private readonly uint objectId;

        private readonly bool wasKilled;

        public ObjectRemovePassiveAction(uint cityId, uint objectId, bool wasKilled, List<uint> cancelActions)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            this.wasKilled = wasKilled;
            this.cancelActions = cancelActions;
        }

        public ObjectRemovePassiveAction(uint id,
                                         DateTime beginTime,
                                         DateTime nextTime,
                                         DateTime endTime,
                                         bool isVisible,
                                         string nlsDescription,
                                         Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
            wasKilled = bool.Parse(properties["was_killed"]);

            cancelActions = new List<uint>();
            foreach (var actionId in properties["cancel_references"].Split(','))
            {
                if (actionId == string.Empty)
                {
                    continue;
                }

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
                                new XmlKvPair("city_id", cityId), new XmlKvPair("object_id", objectId),
                                new XmlKvPair("was_killed", wasKilled),
                                new XmlKvPair("cancel_references",
                                              string.Join(",",
                                                          cancelActions.ConvertAll(
                                                                                   t =>
                                                                                   t.ToString(
                                                                                              CultureInfo
                                                                                                      .InvariantCulture))
                                                                       .ToArray())),
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

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            IGameObject obj;

            using (Concurrency.Current.Lock(cityId, out city))
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

                using (Concurrency.Current.Lock(cityId, out city))
                {
                    if (city == null)
                    {
                        throw new Exception("City is missing");
                    }

                    IGameObject obj1 = obj;
                    action = city.Worker.ActiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj1);

                    loopCount++;
                    if (loopCount == 1000)
                    {
                        throw new Exception(string.Format("Unable to cancel all active actions. Stuck cancelling {0}",
                                                          action.Type));
                    }

                    if (action == null)
                    {
                        break;
                    }
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all passive actions
            loopCount = 0;
            while (true)
            {
                GameAction action;

                using (Concurrency.Current.Lock(cityId, out city))
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
                        throw new Exception(string.Format("Unable to cancel all passive actions. Stuck cancelling {0}",
                                                          action.Type));
                    }

                    if (action == null)
                    {
                        break;
                    }
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all references
            foreach (var actionId in cancelActions)
            {
                GameAction action;
                using (Concurrency.Current.Lock(cityId, out city))
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

            using (Concurrency.Current.Lock(cityId, out city))
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
                    city.BeginUpdate();
                    if (!wasKilled)
                    {
                        // Give laborers back to the city if obj was not killed off
                        ushort laborers = ((IStructure)obj).Stats.Labor;
                        city.Resource.Labor.Add(laborers);
                    }

                    city.DoRemove(((IStructure)obj));
                    Procedure.Current.OnStructureUpgradeDowngrade((IStructure)obj);
                    city.EndUpdate();
                }
                StateChange(ActionState.Completed);
            }
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new Exception("City was destroyed?");
        }
    }
}