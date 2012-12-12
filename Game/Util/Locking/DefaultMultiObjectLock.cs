#region

using System;
using System.Linq;
using System.Threading;
        // ReSharper disable RedundantUsingDirective

// ReSharper restore RedundantUsingDirective

#endregion

namespace Game.Util.Locking
{
    public class DefaultMultiObjectLock : IMultiObjectLock
    {
        public delegate IMultiObjectLock Factory();

        [ThreadStatic]
        private static DefaultMultiObjectLock currentLock;

        private object[] lockedObjects = new object[] {};

        public void Lock(ILockable[] list)
        {
            if (currentLock != null)
            {
                throw new LockException("Attempting to nest MultiObjectLock");
            }

            currentLock = this;

            lockedObjects = new object[list.Length];

            Array.Sort(list, CompareObject);
            for (int i = 0; i < list.Length; ++i)
            {
                Monitor.Enter(list[i].Lock);
                lockedObjects[i] = list[i].Lock;
            }
        }

        public void Dispose()
        {
            UnlockAll();
        }

        public void UnlockAll()
        {
            for (int i = lockedObjects.Length - 1; i >= 0; --i)
            {
                Monitor.Exit(lockedObjects[i]);
            }

            lockedObjects = new object[] {};

            currentLock = null;
        }

        public static bool IsLocked(ILockable obj)
        {
            return currentLock != null && currentLock.lockedObjects.Any(lck => lck == obj.Lock);
        }

        private static int CompareObject(ILockable x, ILockable y)
        {
            return x.Hash.CompareTo(y.Hash);
        }

        public static void ThrowExceptionIfNotLocked(ILockable obj)
        {
#if DEBUG
            if (!IsLocked(obj))
            {
                throw new LockException("Object not locked");
            }

#elif CHECK_LOCKS
            if (!IsLocked(obj)) 
                Global.Logger.Error(string.Format("Object not locked id[{0}] {1}", obj.Hash, Environment.StackTrace));
#endif
        }
    }
}