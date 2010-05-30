using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Map {
    public class RoadManager {
        public void CreateRoad(uint x, uint y) {
            CalculateRoad(x, y, true);

            if (y % 2 == 0) {
                CalculateRoad(x, y - 1, false);
                CalculateRoad(x, y + 1, false);
                CalculateRoad(x - 1, y - 1, false);
                CalculateRoad(x - 1, y + 1, false);
            }
            else {
                CalculateRoad(x + 1, y - 1, false);
                CalculateRoad(x + 1, y + 1, false);
                CalculateRoad(x, y - 1, false);
                CalculateRoad(x, y + 1, false);
            }            
        }

        public void DestroyRoad(uint x, uint y)
        {
            Global.World.RevertTileType(x, y);

            if (y % 2 == 0)
            {
                CalculateRoad(x, y - 1, false);
                CalculateRoad(x, y + 1, false);
                CalculateRoad(x - 1, y - 1, false);
                CalculateRoad(x - 1, y + 1, false);
            }
            else
            {
                CalculateRoad(x + 1, y - 1, false);
                CalculateRoad(x + 1, y + 1, false);
                CalculateRoad(x, y - 1, false);
                CalculateRoad(x, y + 1, false);
            }
        }

        private void CalculateRoad(uint x, uint y, bool createHere) {
            if (x <= 1 || y <= 1 || x >= Config.map_width || y >= Config.map_height)
                return;

            if (!createHere && !IsRoad(Global.World.GetTileType(x, y)))
                return;

            // Create array of neighbor roads
            byte[] neighbors;

            if (y % 2 == 0) {
                neighbors = new[]
                                {
                                    IsRoad(x - 1, y - 1) ? (byte) 1 : (byte) 0, 
                                    IsRoad(x - 1, y + 1) ? (byte) 1 : (byte) 0, 
                                    IsRoad(x, y - 1) ? (byte) 1 : (byte) 0, 
                                    IsRoad(x, y + 1) ? (byte) 1 : (byte) 0,
                                };
            }
            else {
                neighbors = new[]
                                {
                                    IsRoad(x, y - 1) ? (byte) 1 : (byte) 0, 
                                    IsRoad(x, y + 1) ? (byte) 1 : (byte) 0, 
                                    IsRoad(x + 1, y - 1) ? (byte) 1 : (byte) 0, 
                                    IsRoad(x + 1, y + 1) ? (byte) 1 : (byte) 0,
                                };
            }

            // Select appropriate tile based on the neighbors around this tile
            uint roadType = 0;
            if (neighbors.SequenceEqual(new byte[] { 0, 0, 0, 0 })) roadType = 0;
            else if (neighbors.SequenceEqual(new byte[] { 1, 0, 0, 0 })) roadType = 0;
            else if (neighbors.SequenceEqual(new byte[] { 0, 1, 0, 0 })) roadType = 1;
            else if (neighbors.SequenceEqual(new byte[] { 0, 0, 1, 0 })) roadType = 1;
            else if (neighbors.SequenceEqual(new byte[] { 0, 0, 0, 1 })) roadType = 0;
            else if (neighbors.SequenceEqual(new byte[] { 1, 1, 0, 0 })) roadType = 7;
            else if (neighbors.SequenceEqual(new byte[] { 0, 0, 1, 1 })) roadType = 8;
            else if (neighbors.SequenceEqual(new byte[] { 1, 0, 1, 0 })) roadType = 9;
            else if (neighbors.SequenceEqual(new byte[] { 1, 0, 0, 1 })) roadType = 0;
            else if (neighbors.SequenceEqual(new byte[] { 0, 1, 1, 0 })) roadType = 1;
            else if (neighbors.SequenceEqual(new byte[] { 0, 1, 0, 1 })) roadType = 10;
            else if (neighbors.SequenceEqual(new byte[] { 1, 1, 1, 0 })) roadType = 2;
            else if (neighbors.SequenceEqual(new byte[] { 1, 1, 0, 1 })) roadType = 5;
            else if (neighbors.SequenceEqual(new byte[] { 1, 0, 1, 1 })) roadType = 3;
            else if (neighbors.SequenceEqual(new byte[] { 0, 1, 1, 1 })) roadType = 4;
            else if (neighbors.SequenceEqual(new byte[] { 1, 1, 1, 1 })) roadType = 6;

            // Grab the list of actual tiles based on the road type we need.
            ushort[] types = ObjectTypeFactory.GetTypes("RoadSet1");

            // Set the new road tile
            Global.World.SetTileType(x, y, types[roadType]);
        }

        public static bool IsRoad(uint x, uint y) {
            return IsRoad(Global.World.GetTileType(x, y));
        }

        public static bool IsRoad(ushort tileId) {
            return (tileId >= Config.road_start_tile_id && tileId <= Config.road_end_tile_id);
        }
    }
}