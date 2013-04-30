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
    public class ResourceGatherActiveAction : ScheduledActiveAction
    {
        private readonly ILocker locker;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly IWorld world;

        private readonly IActionFactory actionFactory;

        private readonly uint cityId;

        private readonly uint objectId;

        public ResourceGatherActiveAction(uint cityId, uint objectId, ILocker locker, ObjectTypeFactory objectTypeFactory, IWorld world, IActionFactory actionFactory)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            this.locker = locker;
            this.objectTypeFactory = objectTypeFactory;
            this.world = world;
            this.actionFactory = actionFactory;
        }

        public ResourceGatherActiveAction(uint id,
                                          DateTime beginTime,
                                          DateTime nextTime,
                                          DateTime endTime,
                                          int workerType,
                                          byte workerIndex,
                                          ushort actionCount,
                                          Dictionary<string, string> properties,
                                          ILocker locker,
                                          ObjectTypeFactory objectTypeFactory,
                                          IWorld world, 
                                          IActionFactory actionFactory)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.locker = locker;
            this.objectTypeFactory = objectTypeFactory;
            this.world = world;
            this.actionFactory = actionFactory;
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Normal;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ResourceGatherActive;
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

            using (locker.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Completed);
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;
            object value;

            if (!world.TryGetObjects(cityId, objectId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            city.BeginUpdate();
            city.Resource.BeginUpdate();

            structure.BeginUpdate();

            if (structure.Properties.TryGet("Crop", out value))
            {
                city.Resource.Crop.Add((int)structure["Crop"]);
                structure["Crop"] = 0;
            }

            if (structure.Properties.TryGet("Gold", out value))
            {
                city.Resource.Gold.Add((int)structure["Gold"]);
                structure["Gold"] = 0;
            }

            if (structure.Properties.TryGet("Iron", out value))
            {
                city.Resource.Iron.Add((int)structure["Iron"]);
                structure["Iron"] = 0;
            }

            if (structure.Properties.TryGet("Wood", out value))
            {
                city.Resource.Wood.Add((int)structure["Wood"]);
                structure["Wood"] = 0;
            }

            if (structure.Properties.TryGet("Labor", out value))
            {
                city.Resource.Labor.Add((int)structure["Labor"]);
                structure["Labor"] = 0;
            }

            structure.EndUpdate();

            city.Resource.EndUpdate();
            city.EndUpdate();

            var changeAction = actionFactory.CreateStructureChangePassiveAction(cityId, objectId, 0, (ushort)objectTypeFactory.GetTypes("EmptyField")[0], 1);
            city.Worker.DoPassive(structure, changeAction, true);

            StateChange(ActionState.Completed);
            return Error.Ok;
        }

        public override Error Validate(string[] parms)
        {
            ICity city;

            if (!world.TryGetObjects(cityId, out city))
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
            using (locker.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            }
        }
    }
}