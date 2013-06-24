#region

using System;
using System.Linq;
using System.Threading;
using Game.Data;
using Game.Setup;
using Ninject.Extensions.Logging;

// ReSharper disable RedundantUsingDirective

// ReSharper restore RedundantUsingDirective

#endregion

namespace Game.Util.Locking
{
    public class DefaultMultiObjectLock : IMultiObjectLock
    {
        private static ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly Action<object> lockEnter;

        private readonly Action<object> lockExit;

        public delegate IMultiObjectLock Factory();

        [ThreadStatic]
        private static DefaultMultiObjectLock currentLock;

        private object[] lockedObjects = new object[] {};

        public DefaultMultiObjectLock()
                : this(Monitor.Enter, Monitor.Exit)
        {
        }

        public DefaultMultiObjectLock(Action<object> lockEnter, Action<object> lockExit)
        {
            this.lockEnter = lockEnter;
            this.lockExit = lockExit;
        }

        public IMultiObjectLock Lock(ILockable[] list)
        {            
            if (currentLock != null)
            {
                throw new LockException("Attempting to nest MultiObjectLock");
            }

            currentLock = this;
            
            lockedObjects = new object[list.Length];

            SortLocks(list);
            for (int i = 0; i < list.Length; ++i)
            {
                var lockObj = list[i].Lock;
                if (lockObj == null)
                {
                    logger.Warn("Received a null Lock obj when trying to lock object type {0} with hash {1}", list[i].GetType().FullName, list[i].Hash);
                    continue;
                }

                lockEnter(lockObj);
                lockedObjects[i] = lockObj;
            }

            return this;
        }

        public void SortLocks(ILockable[] list)
        {
            Array.Sort(list, CompareObject);
        }

        public void Dispose()
        {
            UnlockAll();
            GC.SuppressFinalize(this);
        }

        public void UnlockAll()
        {            
            for (int i = lockedObjects.Length - 1; i >= 0; --i)
            {
                lockExit(lockedObjects[i]);
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
            var hashDiff = x.Hash.CompareTo(y.Hash);
            if (hashDiff != 0)
            {
                return hashDiff;
            }

            var xType = x.GetType();
            var yType = y.GetType();

            return String.Compare(xType.AssemblyQualifiedName, yType.AssemblyQualifiedName, StringComparison.InvariantCulture);
        }

        public static void ThrowExceptionIfNotLocked(ILockable obj)
        {
            if (!Config.locks_check)
            {
                return;
            }
#if DEBUG
            if (!IsLocked(obj))
            {
                throw new LockException("Object not locked");
            }
#else
            if (!IsLocked(obj)) {
                logger.Error(string.Format("Object not locked id[{0}] {1}", obj.Hash, Environment.StackTrace));
            }
#endif
        }
    }
}