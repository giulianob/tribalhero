using System;
using System.Runtime.InteropServices;
using Game.Database;
using Ninject.Extensions.Logging;

namespace Game.Util.Locking
{
    class TransactionalMultiObjectLock : IMultiObjectLock
    {
        private static readonly ILogger Logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly IMultiObjectLock lck;

        public TransactionalMultiObjectLock(IMultiObjectLock lck)
        {
            this.lck = lck;
        }

        public IMultiObjectLock Lock(params ILockable[] list)
        {
            lck.Lock(list);
            DbPersistance.Current.GetThreadTransaction();

            return this;
        }

        public void UnlockAll()
        {
            if (Marshal.GetExceptionPointers() != IntPtr.Zero || Marshal.GetExceptionCode() != 0)
            {
                Logger.Warn("Not unlocking because of exception.");
                return;
            }

            var transaction = DbPersistance.Current.GetThreadTransaction(true);
            if (transaction != null)
            {                
                transaction.Dispose();
            }

            lck.UnlockAll();
        }

        public void SortLocks(ILockable[] list)
        {
            lck.SortLocks(list);
        }

        public void Dispose()
        {
            UnlockAll();
        }
    }
}