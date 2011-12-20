using Game.Setup;
using Ninject;
using Persistance;

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
            Ioc.Kernel.Get<IDbManager>().GetThreadTransaction();
        }

        public void UnlockAll()
        {
            var transaction = Ioc.Kernel.Get<IDbManager>().GetThreadTransaction(true);
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
