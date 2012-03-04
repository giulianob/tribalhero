using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

namespace Game.Logic.Actions
{
    class TileChangeActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly ushort type;
        private readonly uint x;
        private readonly uint y;

        public TileChangeActiveAction(uint cityId, ushort type, uint x, uint y)
        {
            this.cityId = cityId;
            this.type = type;
            this.x = x;
            this.y = y;
        }

        public TileChangeActiveAction(uint id,
                                    DateTime beginTime,
                                    DateTime nextTime,
                                    DateTime endTime,
                                    int workerType,
                                    byte workerIndex,
                                    ushort actionCount,
                                    IDictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            type = ushort.Parse(properties["type"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.TileChangeActive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                                                {
                                                        new XmlKvPair("type", type), new XmlKvPair("x", x), new XmlKvPair("y", y), new XmlKvPair("city_id", cityId)
                                                });
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            if (!World.Current.IsValidXandY(x, y))
            {
                return Error.ActionInvalid;
            }

            // Make sure there is no structure at this point that has no road requirement

            ICity city;
            if(!World.Current.TryGetObjects(cityId,out city)) return Error.ObjectNotFound;

            // Make sure user is building road within city walls
            if (SimpleGameObject.TileDistance(city.X, city.Y, x, y) < city.Radius)
            {
                return Error.NotWithinWalls;
            }
           
            BeginTime = DateTime.UtcNow;
            EndTime = DateTime.UtcNow;
            return Error.Ok;
        }

        public override void UserCancelled()
        {
            throw new NotImplementedException();
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new NotImplementedException();
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Normal;
            }
        }

        public override void Callback(object custom)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                World.Current.LockRegion(x, y);

                if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("TileBuildable", World.Current.GetTileType(x, y)))
                {
                    World.Current.UnlockRegion(x, y);
                    return;
                }

                World.Current.RoadManager.CreateRoad(x, y);

                World.Current.UnlockRegion(x, y);
            }
        }
    }
}
