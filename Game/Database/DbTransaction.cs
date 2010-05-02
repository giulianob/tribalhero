#region

using System;

#endregion

namespace Game.Database {
    public abstract class DbTransaction : IDisposable {
        public enum DbTransactionState {
            IN_PROGRESS,
            COMMITTED,
            ROLLEDBACK,  
            DISPOSED
        }

        internal object transaction;
        protected IDbManager manager;

        private DbTransactionState state = DbTransactionState.IN_PROGRESS;

        private int referenceCount;

        public int ReferenceCount {
            get { return referenceCount; }
            set {
                referenceCount = value;
                if (value > 1)
                    throw new Exception("Only 1 transaction reference is allowed");
            }
        }

        internal DbTransaction(IDbManager manager, object transaction) {
            this.manager = manager;
            this.transaction = transaction;
        }

        protected virtual void Commit() {
            state = DbTransactionState.COMMITTED;

            if (referenceCount == 0)
                state = DbTransactionState.DISPOSED;
        }

        public virtual void Rollback() {
            state = DbTransactionState.ROLLEDBACK;

            if (referenceCount == 0)
                state = DbTransactionState.DISPOSED;
        }

        #region IDisposable Members

        public void Dispose() {
            referenceCount--;

            if (referenceCount != 0)
                return;

            switch (state) {
                case DbTransactionState.ROLLEDBACK:
                    Rollback();
                    break;
                case DbTransactionState.IN_PROGRESS:
                    Commit();
                    break;
            }            

            manager.ClearThreadTransaction();
        }

        #endregion
    }
}