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
    public class StructureSelfDestroyActiveAction : ScheduledActiveAction
    {
        private uint cityId;

        private uint objectId;

        private TimeSpan ts;

        private readonly ILocker locker;

        private readonly IWorld world;

        public StructureSelfDestroyActiveAction(ILocker locker, IWorld world)
        {
            this.locker = locker;
            this.world = world;
        }

        public StructureSelfDestroyActiveAction(uint cityId, uint objectId, ILocker locker, IWorld world)
            : this(locker, world)
        {
            this.cityId = cityId;
            this.objectId = objectId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.StandAlone;
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
                return
                        XmlSerializer.Serialize(new[]
                        {new XmlKvPair("city_id", cityId), new XmlKvPair("object_id", objectId),});
            }
        }

        public override void Callback(object custom)
        {
            ICity city;
            IStructure structure;

            locker.Lock(cityId, objectId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

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

                world.Regions.Remove(structure);
                city.ScheduleRemove(structure, false);

                structure.EndUpdate();
                city.EndUpdate();

                StateChange(ActionState.Completed);
            });
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            endTime = SystemClock.Now.AddSeconds(CalculateTime(ts.TotalSeconds));
            BeginTime = SystemClock.Now;

            if (!world.TryGetObjects(cityId, objectId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            return Error.Ok;
        }

        public override Error Validate(string[] parms)
        {
            ICity city;

            if (!world.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            ts = TimeSpan.FromSeconds(int.Parse(parms[0]));

            return Error.Ok;
        }

        public override void UserCancelled()
        {
            ICity city;
            IStructure structure;
            locker.Lock(cityId, objectId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            });
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            IStructure structure;
            locker.Lock(cityId, objectId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            });
        }
    }
}