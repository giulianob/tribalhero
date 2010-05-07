#region

using System;

#endregion

namespace Game.Util {
    public class LargeIdGenerator {
        private long last;
        private long max;

        private object objLock = new object();

        public LargeIdGenerator(long max) {
            this.max = max;
        }

        public int GetNext() {
            lock (objLock) {
                last++;
                if (last > max)
                    throw new ArgumentOutOfRangeException();

                return (int) last;
            }
        }

        public void Set(long id) {
            lock (objLock) {
                if (last < id)
                    last = id;
            }
        }

        public void Release(long id) {
            lock (objLock) {
                if (last == id)
                    last--;
            }
        }
    }
}