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

        private static readonly MapMath mapMath = new MapMath();
        
        public static void GenerateResource(Vector2D cityLocation, ushort[] map)
        {
            Position position;

            foreach (var eachPosition in TileLocator.Current.ForeachTile(cityLocation.X, cityLocation.Y, Ioc.Kernel.Get<Formula>().GetInitialCityRadius()))
            {
                while (map[eachPosition.Y * Program.REGION_WIDTH + eachPosition.X] == FARM_TILE || map[eachPosition.Y * Program.REGION_WIDTH + eachPosition.X] == WOODLAND_TILE)
                {
                    map[eachPosition.Y * Program.REGION_WIDTH + eachPosition.X] = (ushort)Program.Random.Next(1, numberOfTiles);
                }
            }
            
            for (int i = 0; i < numberOfFarm; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(new Position(cityLocation.X, cityLocation.Y),
                                                    (byte)(Ioc.Kernel.Get<Formula>().GetInitialCityRadius() - 1),
                                                    false,
                                                    out position);
                }
                while (map[position.Y * Program.REGION_WIDTH + position.X] == FARM_TILE ||
                       map[position.Y * Program.REGION_WIDTH + position.X] == WOODLAND_TILE ||
                       TileLocator.Current.TileDistance(position, 1, new Position(cityLocation.X, cityLocation.Y), 1) <= 1 ||
                       !mapMath.IsPerpendicular(position.X, position.Y, cityLocation.X, cityLocation.Y));

                map[position.Y * Program.REGION_WIDTH + position.X] = FARM_TILE;
            }

            for (int i = 0; i < numberOfWoodland; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(new Position(cityLocation.X, cityLocation.Y),
                                                    (byte)(Ioc.Kernel.Get<Formula>().GetInitialCityRadius() - 1),
                                                    false,
                                                    out position);
                }
                while (TileLocator.Current.TileDistance(position, 1, new Position(cityLocation.X, cityLocation.Y), 1) <= 1 ||
                       map[position.Y * Program.REGION_WIDTH + position.X] == FARM_TILE ||
                       map[position.Y * Program.REGION_WIDTH + position.X] == WOODLAND_TILE ||
                       !mapMath.IsPerpendicular(position.X, position.Y, cityLocation.X, cityLocation.Y));
                map[position.Y * Program.REGION_WIDTH + position.X] = WOODLAND_TILE;
            }
        }
    }
}