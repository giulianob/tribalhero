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

        private int startX;

        private int startY;

        private int width;

        private int Index(int x, int y)
        {
            return y * width + x;
        }

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

            startX = x;
            startY = y;

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

        public void Foreach(int width, int height, int x, int y, DoWork work, object custom)
        {
            BuildOrder(width, height, x, y);
            for (int i = 0; i < resultIndex; ++i)
            {
                work(i, result[i] % width, result[i] / width, custom);
            }
        }
    }
}