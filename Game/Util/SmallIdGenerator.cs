#region

using System.Collections;

#endregion

namespace Game.Util
{
    public class SmallIdGenerator
    {
        private readonly BitArray bitarray;

        private readonly bool fetchLowest;

        private int start = 1;

        public SmallIdGenerator(int max)
                : this(max, false)
        {
        }

        public SmallIdGenerator(int max, bool fetchLowest)
        {
            this.fetchLowest = fetchLowest;
            bitarray = new BitArray(max, false);
        }

        public int GetNext()
        {
            lock (bitarray)
            {
                int size = bitarray.Length;
                for (int i = 0; i < size; ++i)
                {
                    int idx;
                    if (fetchLowest)
                    {
                        idx = i;
                    }
                    else
                    {
                        idx = (i + start) % bitarray.Length;
                    }

                    if (idx == 0)
                    {
                        continue;
                    }

                    if (bitarray.Get(idx) == false)
                    {
                        bitarray.Set(idx, true);
                        start = idx;
                        return idx;
                    }
                }
            }

            return -1;
        }

        public void Set(int id)
        {
            lock (bitarray)
            {
                bitarray.Set(id, true);
            }
        }

        public void Release(int id)
        {
            lock (bitarray)
            {
                bitarray.Set(id, false);
            }
        }
    }
}