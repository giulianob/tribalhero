#region

using System;
using System.Data.Common;
using System.Threading;
using MySql.Data.MySqlClient;
using Ninject.Extensions.Logging;

#endregion

namespace Persistance.Managers
{
    class MySqlDbTransaction : DbTransaction
    {
        internal MySqlDbTransaction(MySqlDbManager manager, ILogger logger, bool verbose, MySqlTransaction transaction)
                : base(manager, logger, verbose, transaction)
        {
        }

        public override void Rollback()
        {
            base.Rollback();

            if (ReferenceCount > 0 || Transaction == null)
            {
                return;
            }

            ((MySqlTransaction)Transaction).Rollback();
        }

        public void RerunTransaction()
        {
            logger.Warn("Rerunning transactions due to deadlock");

            do
            {
                DbCommand currentCommand = null;

                try
                {
                    foreach (var command in Commands)
                    {
                        currentCommand = command;
                        command.ExecuteNonQuery();
                    }

                    break;
                }
                catch(MySqlException e)
                {
                    if (e.ErrorCode == 1213)
                    {
                        logger.Warn("Rerunning transaction AGAIN due to deadlock");
                        Thread.Sleep(0);
                        continue;
                    }

                    logger.Error(e, "Failed while trying to rerun transactions");
                    Manager.LogCommand(currentCommand, false);
                }
            }
            while (true);
        }

        protected override void Commit()
        {
            base.Commit();

            if (ReferenceCount > 0)
            {
                return;
            }

            if (Transaction == null) //no queries ran
            {
                return;
            }

            try
            {
                ((MySqlTransaction)Transaction).Commit();

                if (verbose)
                {
                    logger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction committed");
                }
            }
            catch(Exception e)
            {
                logger.Error(e, "Error committing transaction");
                try
                {
                    ((MySqlTransaction)Transaction).Rollback();
                    Manager.Close(((MySqlTransaction)Transaction).Connection);
                }
                catch
                {
                }

                ((MySqlDbManager)Manager).HandleGeneralException(e);
                throw;
            }

            Manager.Close(((MySqlTransaction)Transaction).Connection);
        }
    }
}