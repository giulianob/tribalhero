#region

using System;

#endregion

namespace Game.Util {
    public class LargeIdGenerator {
        private long last = 0;
        private long max;

        private object objLock = new object();

        public LargeIdGenerator(long max) {
            this.max = max;
        }

        public int getNext() {
            lock (objLock) {
                last++;
                if (last > max)
                    throw new ArgumentOutOfRangeException();

                return (int) last;
            }
        }

        public void set(int id) {
            lock (objLock) {
                if (last < id)
                    last = id;
            }
        }

        public void release(int id) {
            lock (objLock) {
                if (last == id)
                    last--;
            }
        }
    }
}