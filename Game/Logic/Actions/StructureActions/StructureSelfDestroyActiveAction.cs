#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    public class StructureSelfDestroyActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly uint objectId;
        private TimeSpan ts;

        public StructureSelfDestroyActiveAction(uint cityId, uint objectId)
        {
            this.cityId = cityId;
            this.objectId = objectId;
        }

        public StructureSelfDestroyActiveAction(uint id,
                                            DateTime beginTime,
                                            DateTime nextTime,
                                            DateTime endTime,
                                            int workerType,
                                            byte workerIndex,
                                            ushort actionCount,
                                            Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)        
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
        }

        public override ConcurrencyType Concurrency
        {
            get
            {
                return ConcurrencyType.Concurrent;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureSelfDestroyActive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId), new XmlKvPair("object_id", objectId),});
            }
        }

        
        public override void Callback(object custom)
        {
            City city;
            Structure structure;

            using (new MultiObjectLock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;

                if (structure == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                if (structure.State.Type == ObjectState.Battle)
                {
                    endTime = DateTime.UtcNow.AddMinutes(5);
                    StateChange(ActionState.Rescheduled);
                    return;
                }

                city.BeginUpdate();
                structure.BeginUpdate();

                Global.World.Remove(structure);
                city.ScheduleRemove(structure, false);

                structure.EndUpdate();
                city.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Execute()
        {
            City city;
            Structure structure;

            endTime = SystemClock.Now.AddSeconds(CalculateTime(ts.TotalSeconds));
            BeginTime = SystemClock.Now;

            if (!Global.World.TryGetObjects(cityId, objectId, out city, out structure))
                return Error.ObjectNotFound;

            return Error.Ok;
        }

        public override Error Validate(string[] parms)
        {
            City city;

            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            ts = TimeSpan.FromSeconds(int.Parse(parms[0]));

            return Error.Ok;
        }

        public override void UserCancelled()
        {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;

                StateChange(ActionState.Failed);
            }
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;                

                StateChange(ActionState.Failed);
            }
        }
    }
}