#region

using System.Collections.Generic;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Map
{
    public class RoadManager
    {
        private readonly IRegionManager regionManager;

        public RoadManager(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        private void SendUpdate(Dictionary<ushort, List<TileUpdate>> updates)
        {
            foreach (var list in updates)
            {
                var packet = new Packet(Command.RegionSetTile);
                packet.AddUInt16((ushort)list.Value.Count);
                foreach (var update in list.Value)
                {
                    packet.AddUInt32(update.X);
                    packet.AddUInt32(update.Y);
                    packet.AddUInt16(update.TileType);
                }

                Global.Channel.Post("/WORLD/" + list.Key, packet);
            }
        }

        public void CreateRoad(uint x, uint y)
        {
            var tiles = new List<Position>(5) {new Position(x, y)};

            if (y % 2 == 0)
            {
                tiles.Add(new Position(x, y - 1));
                tiles.Add(new Position(x, y + 1));
                tiles.Add(new Position(x - 1, y - 1));
                tiles.Add(new Position(x - 1, y + 1));
            }
            else
            {
                tiles.Add(new Position(x + 1, y - 1));
                tiles.Add(new Position(x + 1, y + 1));
                tiles.Add(new Position(x, y - 1));
                tiles.Add(new Position(x, y + 1));
            }

            var updates = new Dictionary<ushort, List<TileUpdate>>();

            for (int i = 0; i < tiles.Count; i++)
            {
                ushort regionId = Region.GetRegionIndex(tiles[i].X, tiles[i].Y);
                var update = new TileUpdate(tiles[i].X, tiles[i].Y, CalculateRoad(tiles[i].X, tiles[i].Y, i == 0));
                if (update.TileType == ushort.MaxValue)
                {
                    continue; // Not a road here
                }
                List<TileUpdate> list;
                if (!updates.TryGetValue(regionId, out list))
                {
                    list = new List<TileUpdate> {update};
                    updates.Add(regionId, list);
                }
                else
                {
                    updates[regionId].Add(update);
                }
            }

            SendUpdate(updates);
        }

        public void DestroyRoad(uint x, uint y)
        {
            var tiles = new List<Position>(5) {new Position(x, y)};

            if (y % 2 == 0)
            {
                tiles.Add(new Position(x, y - 1));
                tiles.Add(new Position(x, y + 1));
                tiles.Add(new Position(x - 1, y - 1));
                tiles.Add(new Position(x - 1, y + 1));
            }
            else
            {
                tiles.Add(new Position(x + 1, y - 1));
                tiles.Add(new Position(x + 1, y + 1));
                tiles.Add(new Position(x, y - 1));
                tiles.Add(new Position(x, y + 1));
            }

            var updates = new Dictionary<ushort, List<TileUpdate>>();

            for (int i = 0; i < tiles.Count; i++)
            {
                ushort regionId = Region.GetRegionIndex(tiles[i].X, tiles[i].Y);

                TileUpdate update;
                if (i == 0)
                {
                    update = new TileUpdate(tiles[i].X,
                                            tiles[i].Y,
                                            regionManager.RevertTileType(tiles[i].X, tiles[i].Y, false));
                }
                else
                {
                    update = new TileUpdate(tiles[i].X, tiles[i].Y, CalculateRoad(tiles[i].X, tiles[i].Y, false));
                }
                if (update.TileType == ushort.MaxValue)
                {
                    continue; // Not a road here
                }
                List<TileUpdate> list;
                if (!updates.TryGetValue(regionId, out list))
                {
                    list = new List<TileUpdate> {update};
                    updates.Add(regionId, list);
                }
                else
                {
                    updates[regionId].Add(update);
                }
            }

            SendUpdate(updates);
        }

        private ushort CalculateRoad(uint x, uint y, bool createHere)
        {
            if (x <= 1 || y <= 1 || x >= Config.map_width || y >= Config.map_height)
            {
                return ushort.MaxValue;
            }

            if (!createHere && !IsRoad(regionManager.GetTileType(x, y)))
            {
                return ushort.MaxValue;
            }

            // Create array of neighbor roads
            byte[] neighbors;

            if (y % 2 == 0)
            {
                neighbors = new[]
                {
                        IsRoad(x - 1, y - 1) ? (byte)1 : (byte)0, IsRoad(x - 1, y + 1) ? (byte)1 : (byte)0,
                        IsRoad(x, y - 1) ? (byte)1 : (byte)0, IsRoad(x, y + 1) ? (byte)1 : (byte)0,
                };
            }
            else
            {
                neighbors = new[]
                {
                        IsRoad(x, y - 1) ? (byte)1 : (byte)0, IsRoad(x, y + 1) ? (byte)1 : (byte)0,
                        IsRoad(x + 1, y - 1) ? (byte)1 : (byte)0, IsRoad(x + 1, y + 1) ? (byte)1 : (byte)0,
                };
            }

            // Select appropriate tile based on the neighbors around this tile
            uint roadType = 0;
            if (neighbors.SequenceEqual(new byte[] {0, 0, 0, 0}))
            {
                roadType = 15;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 0, 0, 0}))
            {
                roadType = 11;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 1, 0, 0}))
            {
                roadType = 14;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 0, 1, 0}))
            {
                roadType = 13;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 0, 0, 1}))
            {
                roadType = 12;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 1, 0, 0}))
            {
                roadType = 7;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 0, 1, 1}))
            {
                roadType = 8;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 0, 1, 0}))
            {
                roadType = 9;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 0, 0, 1}))
            {
                roadType = 0;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 1, 1, 0}))
            {
                roadType = 1;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 1, 0, 1}))
            {
                roadType = 10;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 1, 1, 0}))
            {
                roadType = 2;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 1, 0, 1}))
            {
                roadType = 5;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 0, 1, 1}))
            {
                roadType = 3;
            }
            else if (neighbors.SequenceEqual(new byte[] {0, 1, 1, 1}))
            {
                roadType = 4;
            }
            else if (neighbors.SequenceEqual(new byte[] {1, 1, 1, 1}))
            {
                roadType = 6;
            }

            // Grab the list of actual tiles based on the road type we need.
            ushort[] types;

            if (World.Current[x, y].Exists(s => s is IStructure))
            {
                types = Ioc.Kernel.Get<ObjectTypeFactory>().GetTypes("RoadSetStructures");
            }
            else
            {
                //TODO: Load random road set
                types = Ioc.Kernel.Get<ObjectTypeFactory>().GetTypes("RoadSet1");
            }

            // Set the new road tile
            regionManager.SetTileType(x, y, types[roadType], false);

            return types[roadType];
        }

        public bool IsRoad(uint x, uint y)
        {
            return IsRoad(regionManager.GetTileType(x, y));
        }

        public static bool IsRoad(ushort tileId)
        {
            return (tileId >= Config.road_start_tile_id && tileId <= Config.road_end_tile_id);
        }

        #region Nested type: TileUpdate

        /// <summary>
        ///     Simple wrapper to keep track of tiles that were updated so we can send it in one shot to the client.
        /// </summary>
        private class TileUpdate
        {
            public TileUpdate(uint x, uint y, ushort tileType)
            {
                X = x;
                Y = y;
                TileType = tileType;
            }

            public uint X { get; set; }

            public uint Y { get; set; }

            public ushort TileType { get; set; }
        }

        #endregion
    }
}