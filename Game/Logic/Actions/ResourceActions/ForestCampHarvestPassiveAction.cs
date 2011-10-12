#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    class ForestCampHarvestPassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;
        private readonly uint forestId;

        public ForestCampHarvestPassiveAction(uint cityId, uint forestId)
        {
            IsCancellable = true;
            this.forestId = forestId;
            this.cityId = cityId;
        }

        public ForestCampHarvestPassiveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, string nlsDescription, Dictionary<string, string> properties)
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

            if (!Global.World.Forests.TryGetValue(forestId, out forest))
                return Error.ObjectNotFound;

            // add to queue for completion
            endTime = forest.DepleteTime.AddSeconds(30);
            BeginTime = DateTime.UtcNow;

            return Error.Ok;
        }

        public void Reschedule()
        {
            MultiObjectLock.ThrowExceptionIfNotLocked(WorkerObject.City);

            Forest forest;
            if (!Global.World.Forests.TryGetValue(forestId, out forest))
                throw new Exception("Forest is missing");

            if (IsScheduled)
                Global.Scheduler.Remove(this);

            endTime = forest.DepleteTime.AddSeconds(30);
            StateChange(ActionState.Rescheduled);
        }

        public override void Callback(object custom)
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City is missing");

            using (Ioc.Kernel.Get<CallbackLock>().Lock(Global.World.Forests.CallbackLockHandler, new object[] {forestId}, city, Global.World.Forests))
            {
                if (!IsValid())
                    return;

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
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City is missing");

            using (Ioc.Kernel.Get<CallbackLock>().Lock(Global.World.Forests.CallbackLockHandler, new object[] {forestId}, city, Global.World.Forests))
            {
                if (!IsValid())
                    return;

                var structure = (Structure)WorkerObject;

                Forest forest;
                if (Global.World.Forests.TryGetValue(forestId, out forest))
                {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Give back the labors to the city
                city.BeginUpdate();
                city.Resource.Wood.Rate -= (int)structure["Rate"];
                city.Resource.Labor.Add(structure.Stats.Labor);
                city.EndUpdate();

                // Remove ourselves
                structure.BeginUpdate();
                Global.World.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City is missing");

            using (Ioc.Kernel.Get<CallbackLock>().Lock(Global.World.Forests.CallbackLockHandler, new object[] {forestId}, city, Global.World.Forests))
            {
                if (!IsValid())
                    return;

                var structure = (Structure)WorkerObject;

                Forest forest;
                if (Global.World.Forests.TryGetValue(forestId, out forest))
                {
                    // Recalculate the forest
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Give back the labors to the city
                city.BeginUpdate();
                city.Resource.Wood.Rate -= (int)structure["Rate"];
                city.Resource.Labor.Add(structure.Stats.Labor);
                city.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("forest_id", forestId), new XmlKvPair("city_id", cityId)});
            }
        }

        #endregion
    }
}