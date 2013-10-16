using System;
using Game.Logic.Formulas;
using Game.Map;

namespace MapGenerator
{
    public class CityResourceTileGenerator
    {
        private readonly TileLocator tileLocator;

        private readonly Formula formula;

        private const ushort RESOURCE_TILE = 17;

        private const int NUMBER_OF_TILES = 10;

        private const int NUMBER_OF_RESOURCE_TILES = 4;

        private readonly MapMath mapMath = new MapMath();

        public CityResourceTileGenerator(TileLocator tileLocator, Formula formula)
        {
            this.tileLocator = tileLocator;
            this.formula = formula;
        }

        public void GenerateResource(Position cityLocation, ushort[,] map)
        {
            var random = new Random();

            foreach (var eachPosition in tileLocator.ForeachTile(cityLocation.X, cityLocation.Y, formula.GetInitialCityRadius()))
            {
                while (map[eachPosition.X, eachPosition.Y] == RESOURCE_TILE)
                {
                    map[eachPosition.X, eachPosition.Y] = (ushort)random.Next(1, NUMBER_OF_TILES);
                }
            }

            for (int i = 0; i < NUMBER_OF_RESOURCE_TILES; ++i)
            {
                Position position;
                do
                {
                    tileLocator.RandomPoint(new Position(cityLocation.X, cityLocation.Y),
                                            (byte)(formula.GetInitialCityRadius() - 1),
                                            false,
                                            out position);
                }
                while (map[position.X, position.Y] == RESOURCE_TILE ||
                       tileLocator.TileDistance(position, 1, new Position(cityLocation.X, cityLocation.Y), 1) <= 2 ||
                       !mapMath.IsPerpendicular(position.X, position.Y, cityLocation.X, cityLocation.Y));

                map[position.X, position.Y] = RESOURCE_TILE;
            }
        }
    }
}