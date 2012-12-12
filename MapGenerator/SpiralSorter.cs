using System.Collections.Generic;

namespace MapGenerator
{
    class SpiralSorter
    {
        private const int REGION_WIDTH = Program.REGION_WIDTH * 2;

        private const int REGION_HEIGHT = Program.REGION_HEIGHT * 2;

        private const int REGION_COL = Program.WIDTH / REGION_WIDTH;

        private const int REGION_ROW = Program.HEIGHT / REGION_HEIGHT;

        public IEnumerable<Vector2D> Sort(ushort[,] map)
        {
            var list = new List<Vector2D>();
            var spiral = new SpiralOutOrder();

            spiral.Foreach(REGION_COL, REGION_ROW, REGION_COL / 2, REGION_ROW / 2, Work, new {Map = map, List = list});
            return list;
        }

        private bool Work(int index, int col, int row, object custom)
        {
            //Make the first region 
            if (index == 0)
            {
                return true;
            }
            dynamic dyn = custom;
            uint offsetX = (uint)(col * REGION_WIDTH);
            uint offsetY = (uint)(row * REGION_HEIGHT);

            for (uint y = 0; y < REGION_HEIGHT; ++y)
            {
                for (uint x = 0; x < REGION_WIDTH; ++x)
                {
                    if (dyn.Map[offsetX + x, offsetY + y] == Program.CITY_TILE)
                    {
                        dyn.List.Add(new Vector2D(offsetX + x, offsetY + y));
                    }
                }
            }
            return true;
        }
    }
}