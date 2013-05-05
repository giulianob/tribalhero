#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Forest;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Logic.Actions
{
    public class ForestCampHarvestPassiveAction : ScheduledPassiveAction
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly uint cityId;

        private readonly uint forestId;

        private readonly IScheduler scheduler;

        private readonly IWorld world;

        private readonly ILocker locker;

        public ForestCampHarvestPassiveAction(uint cityId,
                                              uint forestId,
                                              IScheduler scheduler,
                                              IWorld world,
                                              ILocker locker)
        {
            IsCancellable = true;
            this.forestId = forestId;
            this.scheduler = scheduler;
            this.world = world;
            this.locker = locker;
            this.cityId = cityId;
        }

        public ForestCampHarvestPassiveAction(uint id,
                                              DateTime beginTime,
                                              DateTime nextTime,
                                              DateTime endTime,
                                              bool isVisible,
                                              string nlsDescription,
                                              Dictionary<string, string> properties,
                                              IScheduler scheduler,
                                              IWorld world,
                                              ILocker locker)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.scheduler = scheduler;
            this.world = world;
            this.locker = locker;
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
            IForest forest;

            if (!world.Forests.TryGetValue(forestId, out forest))
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
            IForest forest;
            if (!world.Forests.TryGetValue(forestId, out forest))
            {
                throw new Exception("Forest is missing");
            }

            if (IsScheduled)
            {
                scheduler.Remove(this);
            }

            endTime = forest.DepleteTime.AddSeconds(30);
            StateChange(ActionState.Rescheduled);
        }

        public override void Callback(object custom)
        {
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (locker.Lock(world.Forests.CallbackLockHandler, new object[] {forestId}, city))
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
            if (!world.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (locker.Lock(world.Forests.CallbackLockHandler, new object[] {forestId}, city))
            {
                if (!IsValid())
                {
                    return;
                }

                var structure = (IStructure)WorkerObject;

                IForest forest;
                if (world.Forests.TryGetValue(forestId, out forest))
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
                world.Regions.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (locker.Lock(world.Forests.CallbackLockHandler, new object[] {forestId}, city))
            {
                if (!IsValid())
                {
                    return;
                }

                var structure = (IStructure)WorkerObject;

                IForest forest;
                if (world.Forests.TryGetValue(forestId, out forest))
                {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Reset the rate
                city.BeginUpdate();
                var newRate = city.Resource.Wood.Rate - (int)structure["Rate"];
                if (newRate < 0)
                {
                    logger.Warn("Forest rate going below 0 for forest id[{0}]", forestId);
                }
                city.Resource.Wood.Rate = Math.Max(0, newRate);
                city.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[]
                {
                        new XmlKvPair("forest_id", forestId),
                        new XmlKvPair("city_id", cityId)
                });
            }
        }

        #endregion
    }
}