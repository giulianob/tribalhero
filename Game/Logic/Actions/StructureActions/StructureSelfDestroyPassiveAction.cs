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
    public class StructureSelfDestroyPassiveAction : ScheduledPassiveAction, IScriptable
    {
        private uint cityId;

        private uint objectId;

        private TimeSpan ts;

        private readonly IWorld world;

        private readonly ILocker locker;

        public StructureSelfDestroyPassiveAction(IWorld world, ILocker locker)
        {
            this.world = world;
            this.locker = locker;
        }

        public StructureSelfDestroyPassiveAction(uint cityId, uint objectId, IWorld world, ILocker locker) 
            : this(world, locker)
        {
            this.cityId = cityId;
            this.objectId = objectId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureSelfDestroyPassive;
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

        #region IScriptable Members

        public void ScriptInit(IGameObject obj, string[] parms)
        {
            ICity city;
            IStructure structure;

            if (!(obj is IStructure))
            {
                throw new Exception();
            }
            cityId = obj.City.Id;
            objectId = obj.ObjectId;
            ts = TimeSpan.FromSeconds(int.Parse(parms[0]));
            NlsDescription = parms[1];

            if (!world.TryGetObjects(cityId, objectId, out city, out structure))
            {
                return;
            }

            city.Worker.DoPassive(structure, this, true);
        }

        #endregion

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

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            endTime = SystemClock.Now.AddSeconds(ts.TotalSeconds);
            BeginTime = SystemClock.Now;

            if (!world.TryGetObjects(cityId, objectId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            return Error.Ok;
        }

        public override void UserCancelled()
        {
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