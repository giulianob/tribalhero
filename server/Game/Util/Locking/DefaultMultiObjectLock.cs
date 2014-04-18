#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common;
using Game.Setup;
using Persistance;

#endregion

namespace Game.Util.Locking
{
    public class DefaultMultiObjectLock : IMultiObjectLock
    {
        private static readonly ILogger Logger = LoggerFactory.Current.GetLogger<DefaultMultiObjectLock>();

        private readonly Action<object> lockEnter;

        private readonly Action<object> lockExit;

        private readonly IDbManager dbManager;

        public delegate IMultiObjectLock Factory();

        private bool hasHadException;

        [ThreadStatic]
        private static DefaultMultiObjectLock currentLock;

        private List<object> lockedObjects = new List<object>(0);

        public DefaultMultiObjectLock(IDbManager dbManager)
                : this(Monitor.Enter, Monitor.Exit, dbManager)
        {
        }

        public DefaultMultiObjectLock(Action<object> lockEnter, Action<object> lockExit, IDbManager dbManager)
        {
            this.lockEnter = lockEnter;
            this.lockExit = lockExit;
            this.dbManager = dbManager;
        }

        public IMultiObjectLock Lock(ILockable[] list)
        {            
            if (currentLock != null)
            {
                throw new LockException("Attempting to nest MultiObjectLock");
            }

            currentLock = this;
            
            lockedObjects = new List<object>(list.Length);

            SortLocks(list);
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < list.Length; ++i)
            {
                var lockObj = list[i].Lock;
                if (lockObj == null)
                {
                    Logger.Warn("Received a null Lock obj when trying to lock object type {0} with hash {1}", list[i].GetType().FullName, list[i].Hash);
                    continue;
                }

                lockEnter(lockObj);
                lockedObjects.Add(lockObj);
            }

            // Initialize transaction
            dbManager.GetThreadTransaction();

            return this;
        }

        [DebuggerStepThrough]
        public T Do<T>(Func<T> protectedBlock)
        {
            T blockReturnValue;
            try
            {
                blockReturnValue = protectedBlock();
            }
            catch(Exception)
            {
                hasHadException = true;
                throw;
            }
            finally
            {
                UnlockAll();                
            }
            
            return blockReturnValue;
        }

        [DebuggerStepThrough]
        public void Do(Action protectedBlock)
        {
            try
            {
                protectedBlock();
            }
            catch(Exception)
            {
                hasHadException = true;
                throw;
            }
            finally
            {
                UnlockAll();                
            }
        }

        public void SortLocks(ILockable[] list)
        {
            Array.Sort(list, CompareObject);
        }

        public void UnlockAll()
        {
            var transaction = dbManager.GetThreadTransaction(true);

            if (hasHadException)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
                return;
            }

            if (transaction != null)
            {                
                transaction.Dispose();
            }

            for (int i = lockedObjects.Count - 1; i >= 0; --i)
            {
                lockExit(lockedObjects[i]);
            }

            lockedObjects = new List<object>(0);
            
            ClearCurrentLock();
        }

        public static bool IsLocked(ILockable obj)
        {
            return currentLock != null && currentLock.lockedObjects.Any(lck => lck == obj.Lock);
        }

        public static int CompareObject(ILockable x, ILockable y)
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

        public static void ClearCurrentLock()
        {
            currentLock = null;
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
                Logger.Error(string.Format("Object not locked id[{0}] {1}", obj.Hash, Environment.StackTrace));
            }
#endif
        }
    }
}