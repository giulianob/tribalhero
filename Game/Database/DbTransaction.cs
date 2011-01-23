#region

using System;

#endregion

namespace Game.Database
{
    public abstract class DbTransaction : IDisposable
    {
        #region DbTransactionState enum

        public enum DbTransactionState
        {
            InProgress,
            Committed,
            Rolledback,
            Disposed
        }

        #endregion

        internal object Transaction;

        protected IDbManager manager;

        private int referenceCount;
        private DbTransactionState state = DbTransactionState.InProgress;

        internal DbTransaction(IDbManager manager, object transaction)
        {
            this.manager = manager;
            Transaction = transaction;
        }

        public int ReferenceCount
        {
            get
            {
                return referenceCount;
            }
            set
            {
                referenceCount = value;
                if (value > 1)
                    throw new Exception("Only 1 transaction reference is allowed");
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            referenceCount--;

            if (referenceCount != 0)
                return;

            switch(state)
            {
                case DbTransactionState.Rolledback:
                    Rollback();
                    break;
                case DbTransactionState.InProgress:
                    Commit();
                    break;
            }

            manager.ClearThreadTransaction();
        }

        #endregion

        protected virtual void Commit()
        {
            state = DbTransactionState.Committed;

            if (referenceCount == 0)
                state = DbTransactionState.Disposed;
        }

        public virtual void Rollback()
        {
            state = DbTransactionState.Rolledback;

            if (referenceCount == 0)
                state = DbTransactionState.Disposed;
        }
    }
}