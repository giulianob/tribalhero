#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using Common;

#endregion

namespace Persistance
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

        protected ILogger logger;

        private int referenceCount;

        private DbTransactionState state = DbTransactionState.InProgress;

        protected bool verbose;

        protected DbTransaction() {}

        internal DbTransaction(IDbManager manager, ILogger logger, bool verbose, object transaction)
        {
            Manager = manager;
            Transaction = transaction;
            this.logger = logger;
            this.verbose = verbose;
            Commands = new List<DbCommand>();
        }

        protected IDbManager Manager { get; set; }

        protected internal object Transaction { get; set; }

        public List<DbCommand> Commands { get; set; }

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
                {
                    throw new Exception("Only 1 transaction reference is allowed");
                }
            }
        }

        protected virtual void Commit()
        {
            state = DbTransactionState.Committed;

            if (referenceCount == 0)
            {
                state = DbTransactionState.Disposed;
            }
        }

        public virtual void Rollback()
        {
            state = DbTransactionState.Rolledback;

            if (referenceCount == 0)
            {
                state = DbTransactionState.Disposed;
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            referenceCount--;

            if (referenceCount != 0)
            {
                return;
            }

            switch(state)
            {
                case DbTransactionState.Rolledback:
                    Rollback();
                    break;
                case DbTransactionState.InProgress:
                    Commit();
                    break;
            }

            Manager.ClearThreadTransaction();
        }

        #endregion
    }
}