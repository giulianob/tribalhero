using Game.Database;

namespace Game.Util.Locking
{
    class TransactionalMultiObjectLock : IMultiObjectLock
    {
        private readonly IMultiObjectLock lck;

        public TransactionalMultiObjectLock(IMultiObjectLock lck)
        {
            this.lck = lck;
        }

        public void Lock(params ILockable[] list)
        {
            lck.Lock(list);
            DbPersistance.Current.GetThreadTransaction();
        }

        public void UnlockAll()
        {
            var transaction = DbPersistance.Current.GetThreadTransaction(true);
            if (transaction != null)
            {
                transaction.Dispose();
            }

            lck.UnlockAll();
        }

        public void Dispose()
        {
            UnlockAll();
        }
    }
}