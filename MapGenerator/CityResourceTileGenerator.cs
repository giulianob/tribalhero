﻿using Game.Data;
using Game.Logic.Formulas;
using Game.Map;

namespace MapGenerator
{
    public class CityResourceTileGenerator
    {
        private const ushort FARM_TILE = 17;

        private const ushort WOODLAND_TILE = 17;

        private static int numberOfTiles = 10;

        private static int numberOfFarm = 2;

        private static int numberOfWoodland = 2;

        private static MapMath mapMath = new MapMath();

        private static bool AreaClear(uint ox, uint oy, uint x, uint y, object custom)
        {
            var map = (ushort[])custom;
            while (map[y * Program.REGION_WIDTH + x] == FARM_TILE || map[y * Program.REGION_WIDTH + x] == WOODLAND_TILE)
            {
                map[y * Program.REGION_WIDTH + x] = (ushort)Program.Random.Next(1, numberOfTiles);
            }
            return true;
        }

        public static void GenerateResource(Vector2D cityLocation, ushort[] map)
        {
            uint x;
            uint y;

            TileLocator.Current.ForeachObject(cityLocation.X,
                                              cityLocation.Y,
                                              Formula.Current.GetInitialCityRadius(),
                                              true,
                                              AreaClear,
                                              map);
            for (int i = 0; i < numberOfFarm; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(cityLocation.X,
                                                    cityLocation.Y,
                                                    (byte)(Formula.Current.GetInitialCityRadius() - 1),
                                                    false,
                                                    out x,
                                                    out y);
                }
                while (map[y * Program.REGION_WIDTH + x] == FARM_TILE ||
                       map[y * Program.REGION_WIDTH + x] == WOODLAND_TILE ||
                       TileLocator.Current.TileDistance(x, y, cityLocation.X, cityLocation.Y) <= 1 ||
                       !mapMath.IsPerpendicular(x, y, cityLocation.X, cityLocation.Y));

                map[y * Program.REGION_WIDTH + x] = FARM_TILE;
            }

            for (int i = 0; i < numberOfWoodland; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(cityLocation.X,
                                                    cityLocation.Y,
                                                    (byte)(Formula.Current.GetInitialCityRadius() - 1),
                                                    false,
                                                    out x,
                                                    out y);
                }
                while (TileLocator.Current.TileDistance(x, y, cityLocation.X, cityLocation.Y) <= 1 ||
                       map[y * Program.REGION_WIDTH + x] == FARM_TILE ||
                       map[y * Program.REGION_WIDTH + x] == WOODLAND_TILE ||
                       !mapMath.IsPerpendicular(x, y, cityLocation.X, cityLocation.Y));
                map[y * Program.REGION_WIDTH + x] = WOODLAND_TILE;
            }
        }
    }
}