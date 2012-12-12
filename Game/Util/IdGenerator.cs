#region

using System.Collections;

#endregion

namespace Game.Util
{
    public class IdGenerator
    {
        private readonly BitArray bitarray;

        private readonly bool fetchLowest;

        private readonly int offset;

        private int start = 1;

        public IdGenerator(int max, bool fetchLowest = false, int startValue = 0)
        {
            this.fetchLowest = fetchLowest;
            bitarray = new BitArray(max, false);
            offset = startValue;
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
                        return idx + offset;
                    }
                }
            }

            return -1;
        }

        public void Set(int id)
        {
            lock (bitarray)
            {
                bitarray.Set(id - offset, true);
            }
        }

        public void Release(int id)
        {
            lock (bitarray)
            {
                bitarray.Set(id - offset, false);
            }
        }
    }
}