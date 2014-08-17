using System.Collections.Generic;
using Game.Map;

namespace MapGenerator
{
    enum Direction
    {
        North,

        East,

        South,

        West
    }

    class SpiralOutOrder
    {
        public delegate bool DoWork(int index, int x, int y, object custom);

        private int count;

        private int height;

        private int[] result;

        private int resultIndex;

        private int width;
        
        private void Walk(Direction direction, int steps, ref int x, ref int y)
        {
            while (steps-- > 0)
            {
                switch(direction)
                {
                    case Direction.East:
                        ++x;
                        break;
                    case Direction.South:
                        ++y;
                        break;
                    case Direction.West:
                        --x;
                        break;
                    case Direction.North:
                        --y;
                        break;
                }
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    result[resultIndex++] = y * width + x;
                }
            }
        }

        private void BuildOrder(int width, int height, int x, int y)
        {
            this.width = width;
            this.height = height;

            count = width * height;
            resultIndex = 0;
            result = new int[count];

            result[resultIndex++] = y * width + x;

            Direction direction = Direction.East;
            int steps = 1;
            int currentSteps = 1;

            while (resultIndex < count)
            {
                Walk(direction, steps, ref x, ref y);
                switch(direction)
                {
                    case Direction.East:
                        direction = Direction.South;
                        steps = currentSteps;
                        break;
                    case Direction.South:
                        direction = Direction.West;
                        steps = ++currentSteps;
                        break;
                    case Direction.West:
                        direction = Direction.North;
                        steps = currentSteps;
                        break;
                    case Direction.North:
                        direction = Direction.East;
                        steps = ++currentSteps;
                        break;
                }
            }
        }

        public IEnumerable<SpiralForeach> Foreach(int width, int height, int x, int y)
        {
            BuildOrder(width, height, x, y);
            for (int i = 0; i < resultIndex; ++i)
            {
                yield return new SpiralForeach
                {
                    Index = i,
                    Position = new Position((uint)(result[i] % width), (uint)(result[i] / width))
                };
            }
        }

        public class SpiralForeach
        {
            public int Index { get; set; }

            public Position Position { get; set; }
        }
    }
}