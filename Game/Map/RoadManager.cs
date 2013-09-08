#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Setup;
using Game.Util;
using Ninject;

#endregion

namespace Game.Map
{
    public class RoadManager : IRoadManager
    {
        [Flags]
        private enum RoadPositions
        {
            None = 0,

            TopLeft = 1,

            BottomLeft = 2,

            TopRight = 4,

            BottomRight = 8
        }

        private readonly IRegionManager regionManager;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IChannel channel;

        private readonly IRegionLocator regionLocator;

        private readonly ITileLocator tileLocator;

        public RoadManager(IRegionManager regionManager, IObjectTypeFactory objectTypeFactory, IChannel channel, IRegionLocator regionLocator, ITileLocator tileLocator)
        {
            this.regionManager = regionManager;
            this.objectTypeFactory = objectTypeFactory;
            this.channel = channel;
            this.regionLocator = regionLocator;
            this.tileLocator = tileLocator;

            regionManager.ObjectAdded += RegionManagerOnObjectAdded;
        }

        private void RegionManagerOnObjectAdded(object sender, ObjectEvent e)
        {
            if (objectTypeFactory.IsObjectType("NoRoadRequired", e.GameObject.Type))
            {
                return;
            }

            foreach (var position in tileLocator.ForeachMultitile(e.GameObject))
            {
                CreateRoad(position.X, position.Y);
            }
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

                channel.Post("/WORLD/" + list.Key, packet);
            }
        }

        public void CreateRoad(uint x, uint y)
        {
            var tilePosition = new Position(x, y);
            var tiles = new List<Position>(5)
            {
                tilePosition,
                tilePosition.TopLeft(),
                tilePosition.TopRight(),
                tilePosition.BottomLeft(),
                tilePosition.BottomRight()
            };

            var updates = new Dictionary<ushort, List<TileUpdate>>();

            for (int i = 0; i < tiles.Count; i++)
            {
                ushort regionId = regionLocator.GetRegionIndex(tiles[i].X, tiles[i].Y);
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
            var tilePosition = new Position(x, y);
            var tiles = new List<Position>(5)
            {
                tilePosition,
                tilePosition.TopLeft(),
                tilePosition.TopRight(),
                tilePosition.BottomLeft(),
                tilePosition.BottomRight()
            };

            var updates = new Dictionary<ushort, List<TileUpdate>>();

            for (int i = 0; i < tiles.Count; i++)
            {
                ushort regionId = regionLocator.GetRegionIndex(tiles[i].X, tiles[i].Y);

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

            var tilePosition = new Position(x, y);
            var structureAtRoadPosition = regionManager.GetObjectsInTile(x, y).OfType<IStructure>().FirstOrDefault();

            RoadPositions neighbors =
                    (ShouldConnectRoad(structureAtRoadPosition, tilePosition.TopLeft()) ? RoadPositions.TopLeft : RoadPositions.None) |
                    (ShouldConnectRoad(structureAtRoadPosition, tilePosition.BottomLeft()) ? RoadPositions.BottomLeft : RoadPositions.None) |
                    (ShouldConnectRoad(structureAtRoadPosition, tilePosition.TopRight()) ? RoadPositions.TopRight : RoadPositions.None) |
                    (ShouldConnectRoad(structureAtRoadPosition, tilePosition.BottomRight()) ? RoadPositions.BottomRight : RoadPositions.None);           

            // Select appropriate tile based on the neighbors around this tile
            uint roadType = 0;
            if (neighbors == RoadPositions.None)
            {
                roadType = 15;
            }
            else if (neighbors == RoadPositions.TopLeft)
            {
                roadType = 11;
            }
            else if (neighbors == RoadPositions.BottomLeft)
            {
                roadType = 14;
            }
            else if (neighbors == RoadPositions.TopRight)
            {
                roadType = 13;
            }
            else if (neighbors == RoadPositions.BottomRight)
            {
                roadType = 12;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.BottomLeft))
            {
                roadType = 7;
            }
            else if (neighbors == (RoadPositions.TopRight | RoadPositions.BottomRight))
            {
                roadType = 8;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.TopRight))            
            {
                roadType = 9;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.BottomRight))            
            {
                roadType = 0;
            }
            else if (neighbors == (RoadPositions.BottomLeft | RoadPositions.TopRight))            
            {
                roadType = 1;
            }
            else if (neighbors == (RoadPositions.BottomLeft | RoadPositions.BottomRight))            
            {
                roadType = 10;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.BottomLeft | RoadPositions.TopRight))            
            {
                roadType = 2;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.BottomLeft | RoadPositions.BottomRight))            
            {
                roadType = 5;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.TopRight | RoadPositions.BottomRight))            
            {
                roadType = 3;
            }
            else if (neighbors == (RoadPositions.BottomLeft | RoadPositions.TopRight | RoadPositions.BottomRight))            
            {
                roadType = 4;
            }
            else if (neighbors == (RoadPositions.TopLeft | RoadPositions.TopRight | RoadPositions.BottomLeft | RoadPositions.BottomRight))            
            {
                roadType = 6;
            }

            // Grab the list of actual tiles based on the road type we need.
            uint[] types;

            if (structureAtRoadPosition != null)
            {
                types = objectTypeFactory.GetTypes("RoadSetStructures");
            }
            else
            {
                types = objectTypeFactory.GetTypes("RoadSet1");
            }

            // Set the new road tile
            regionManager.SetTileType(x, y, (ushort)types[roadType], false);

            return (ushort)types[roadType];
        }

        public bool IsRoad(uint x, uint y)
        {
            return IsRoad(regionManager.GetTileType(x, y));
        }

        private bool ShouldConnectRoad(IStructure sourceStructure, Position position)
        {
            if (!IsRoad(position.X, position.Y))
            {
                return false;
            }

            if (sourceStructure == null)
            {
                return true;
            }

            var structureAtNeighborRoad = regionManager.GetObjectsInTile(position.X, position.Y)
                                                       .OfType<IStructure>()
                                                       .FirstOrDefault();

            if (structureAtNeighborRoad == null)
            {
                return true;
            }

            return sourceStructure == structureAtNeighborRoad;
        }

        private bool IsRoad(ushort tileId)
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