using Game.Data;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Ninject;

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

        private static bool AreaClear(uint ox, uint oy, uint positionX, uint positionY, object custom)
        {

            return true;
        }

        public static void GenerateResource(Vector2D cityLocation, ushort[] map)
        {
            uint x;
            uint y;

            foreach (var position in TileLocator.Current.ForeachTile(cityLocation.X, cityLocation.Y, Ioc.Kernel.Get<Formula>().GetInitialCityRadius()))
            {
                while (map[position.Y * Program.REGION_WIDTH + position.X] == FARM_TILE || map[position.Y * Program.REGION_WIDTH + position.X] == WOODLAND_TILE)
                {
                    map[position.Y * Program.REGION_WIDTH + position.X] = (ushort)Program.Random.Next(1, numberOfTiles);
                }
            }
            
            for (int i = 0; i < numberOfFarm; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(cityLocation.X,
                                                    cityLocation.Y,
                                                    (byte)(Ioc.Kernel.Get<Formula>().GetInitialCityRadius() - 1),
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
                                                    (byte)(Ioc.Kernel.Get<Formula>().GetInitialCityRadius() - 1),
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