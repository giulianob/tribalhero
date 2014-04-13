using System.Collections.Generic;
using Game.Map;

namespace MapGenerator
{
    class SpiralSorter
    {
        private const int REGION_WIDTH = MapGenerator.TMX_REGION_WIDTH * 2;

        private const int REGION_HEIGHT = MapGenerator.TMX_REGION_HEIGHT * 2;

        private const int REGION_COL = MapGenerator.WIDTH / REGION_WIDTH;

        private const int REGION_ROW = MapGenerator.HEIGHT / REGION_HEIGHT;

        public IEnumerable<Position> Sort(ushort[,] map)
        {
            var spiral = new SpiralOutOrder();

            foreach (var spiralForeach in spiral.Foreach(REGION_COL, REGION_ROW, REGION_COL / 2, REGION_ROW / 2))
            {
                //Make the first region empty
                if (spiralForeach.Index == 0)
                {
                    continue;
                }

                uint offsetX = spiralForeach.Position.X * REGION_WIDTH;
                uint offsetY = spiralForeach.Position.Y * REGION_HEIGHT;

                for (uint y = 0; y < REGION_HEIGHT; ++y)
                {
                    for (uint x = 0; x < REGION_WIDTH; ++x)
                    {
                        if (map[offsetX + x, offsetY + y] == MapGenerator.CITY_TILE)
                        {
                            yield return new Position(offsetX + x, offsetY + y);
                        }
                    }
                }
            }
        }
    }
}