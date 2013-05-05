#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class ForestCampHarvestPassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;

        private readonly uint forestId;

        public ForestCampHarvestPassiveAction(uint cityId, uint forestId)
        {
            IsCancellable = true;
            this.forestId = forestId;
            this.cityId = cityId;
        }

        public ForestCampHarvestPassiveAction(uint id,
                                              DateTime beginTime,
                                              DateTime nextTime,
                                              DateTime endTime,
                                              bool isVisible,
                                              string nlsDescription,
                                              Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            IsCancellable = true;
            forestId = uint.Parse(properties["forest_id"]);
            cityId = uint.Parse(properties["city_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ForestCampHarvestPassive;
            }
        }

        public override Error Execute()
        {
            Forest forest;

            if (!World.Current.Forests.TryGetValue(forestId, out forest))
            {
                return Error.ObjectNotFound;
            }

            // add to queue for completion
            endTime = forest.DepleteTime.AddSeconds(30);
            BeginTime = DateTime.UtcNow;

            return Error.Ok;
        }

        public void Reschedule()
        {
            Forest forest;
            if (!World.Current.Forests.TryGetValue(forestId, out forest))
            {
                throw new Exception("Forest is missing");
            }

            if (IsScheduled)
            {
                Scheduler.Current.Remove(this);
            }

            endTime = forest.DepleteTime.AddSeconds(30);
            StateChange(ActionState.Rescheduled);
        }

        public override void Callback(object custom)
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (
                    Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler,
                                             new object[] {forestId},
                                             city))
            {
                if (!IsValid())
                {
                    return;
                }

                endTime = DateTime.UtcNow.AddSeconds(30);
                StateChange(ActionState.Rescheduled);
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override void UserCancelled()
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (
                    Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler,
                                             new object[] {forestId},
                                             city))
            {
                if (!IsValid())
                {
                    return;
                }

                var structure = (IStructure)WorkerObject;

                Forest forest;
                if (World.Current.Forests.TryGetValue(forestId, out forest))
                {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Reset the rate
                city.BeginUpdate();
                city.Resource.Wood.Rate -= (int)structure["Rate"];
                city.EndUpdate();

                // Remove ourselves
                structure.BeginUpdate();
                World.Current.Regions.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (
                    Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler,
                                             new object[] {forestId},
                                             city))
            {
                if (!IsValid())
                {
                    return;
                }

                var structure = (IStructure)WorkerObject;

                Forest forest;
                if (World.Current.Forests.TryGetValue(forestId, out forest))
                {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Reset the rate
                city.BeginUpdate();
                city.Resource.Wood.Rate -= (int)structure["Rate"];
                city.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {new XmlKvPair("forest_id", forestId), new XmlKvPair("city_id", cityId)});
            }
        }

        #endregion
    }
}