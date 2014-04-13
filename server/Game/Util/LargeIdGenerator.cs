#region

using System;

#endregion

namespace Game.Util
{
    public class LargeIdGenerator
    {
        private readonly uint max;

        private readonly object objLock = new object();

        private uint last;

        public LargeIdGenerator(uint max, uint startValue = 0)
        {
            if (startValue == uint.MaxValue || startValue >= max)
            {
                throw new Exception("Invalid start value");
            }

            this.max = max;
            Set(startValue);
        }

        public uint GetNext()
        {
            lock (objLock)
            {
                last++;
                if (last > max || last == uint.MaxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return last;
            }
        }

        public void Set(uint id)
        {
            lock (objLock)
            {
                if (last < id)
                {
                    last = id;
                }
            }
        }

        public void Release(uint id)
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