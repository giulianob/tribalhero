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
    public class ResourceWithdrawActiveAction : ScheduledActiveAction
    {
        private readonly ILocker locker;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IWorld world;

        private readonly IActionFactory actionFactory;

        private uint cityId;

        private uint objectId;

        private Resource resource;

        public ResourceWithdrawActiveAction(ILocker locker,
                                          IObjectTypeFactory objectTypeFactory,
                                          IWorld world,
                                          IActionFactory actionFactory)
        {
            this.locker = locker;
            this.objectTypeFactory = objectTypeFactory;
            this.world = world;
            this.actionFactory = actionFactory;
        }

        public ResourceWithdrawActiveAction(uint cityId,
                                          uint objectId,
                                          Resource resource,
                                          ILocker locker,
                                          IObjectTypeFactory objectTypeFactory,
                                          IWorld world,
                                          IActionFactory actionFactory)
            :this(locker, objectTypeFactory, world, actionFactory)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            this.resource = resource;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
            resource = new Resource(int.Parse(properties["crop"]),
                                    int.Parse(properties["gold"]),
                                    int.Parse(properties["iron"]),
                                    int.Parse(properties["wood"]));
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
                return ActionType.ResourceWithdrawActive;
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
                            new XmlKvPair("crop", resource.Crop),
                            new XmlKvPair("gold", resource.Gold),
                            new XmlKvPair("iron", resource.Iron),
                            new XmlKvPair("wood", resource.Wood),
                        });
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

                StateChange(ActionState.Completed);
            });
        }

        private int GetActualValue(int current, int limit, int stored, int requested)
        {
            int available = stored < requested ? stored : requested;
            return Math.Min(available, limit - current);
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
                int actual = GetActualValue(city.Resource.Crop.Value, city.Resource.Crop.Limit, (int)value, resource.Crop);
                city.Resource.Crop.Add(actual);
                structure["Crop"] = (int)value - actual;
            }

            if (structure.Properties.TryGet("Gold", out value))
            {
                int actual = GetActualValue(city.Resource.Gold.Value, city.Resource.Gold.Limit, (int)value, resource.Gold);
                city.Resource.Gold.Add(actual);
                structure["Gold"] = (int)value - actual;
            }

            if (structure.Properties.TryGet("Iron", out value))
            {
                int actual = GetActualValue(city.Resource.Iron.Value, city.Resource.Iron.Limit, (int)value, resource.Iron);
                city.Resource.Iron.Add(actual);
                structure["Iron"] = (int)value - actual;
            }

            if (structure.Properties.TryGet("Wood", out value))
            {
                int actual = GetActualValue(city.Resource.Wood.Value, city.Resource.Wood.Limit, (int)value, resource.Wood);
                city.Resource.Wood.Add(actual);
                structure["Wood"] = (int)value - actual;
            }

            structure.EndUpdate();

            city.Resource.EndUpdate();
            city.EndUpdate();

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