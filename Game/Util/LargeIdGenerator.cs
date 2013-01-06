#region

using System;

#endregion

namespace Game.Util
{
    public class LargeIdGenerator
    {
        private readonly long max;

        private readonly object objLock = new object();

        private long last;

        public LargeIdGenerator(long max, long startValue = 0)
        {
            this.max = max;
            Set(startValue);
        }

        public int GetNext()
        {
            lock (objLock)
            {
                last++;
                if (last > max)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return (int)last;
            }
        }

        public void Set(long id)
        {
            lock (objLock)
            {
                if (last < id)
                {
                    last = id;
                }
            }
        }

        public void Release(long id)
        {
            lock (objLock)
            {
                if (last == id)
                {
                    last--;
                }
            }
        }

        public void Reset()
        {
            last = 0;
        }
    }
}