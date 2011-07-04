#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Game.Data;
using Game.Setup;
using MySql.Data.MySqlClient;

#endregion

namespace Game.Database.Managers
{
    class MySqlDbTransaction : DbTransaction
    {
        internal MySqlDbTransaction(MySqlDbManager manager, MySqlTransaction transaction)
            : base(manager, transaction)
        {
        }

        public override void Rollback()
        {
            base.Rollback();

            if (ReferenceCount > 0 || Transaction == null)
                return;

            ((MySqlTransaction)Transaction).Rollback();
        }

        public void RerunTransaction()
        {
            Global.DbLogger.Warn("Rerunning transactions due to deadlock");

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
                        Global.DbLogger.Warn("Rerunning transaction AGAIN due to deadlock");
                        Thread.Sleep(0);
                        continue;
                    }

                    Global.DbLogger.Error("Failed while trying to rerun transactions", e);
                    MySqlDbManager.LogCommand(currentCommand, false);
                }                
            } while (true);
        }

        protected override void Commit()
        {
            base.Commit();

            if (ReferenceCount > 0)
                return;

            if (Transaction == null) //no queries ran
                return;

            try
            {
                ((MySqlTransaction)Transaction).Commit();

                if (Config.database_verbose)
                    Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction committed");
            }
            catch(Exception e)
            {
                Global.Logger.Error(e.Message + " " + e.StackTrace);
                try
                {
                    ((MySqlTransaction)Transaction).Rollback();
                    manager.Close(((MySqlTransaction)Transaction).Connection);
                }
                catch
                {
                }

                (manager as MySqlDbManager).HandleGeneralException(e, null);
                throw;
            }

            manager.Close(((MySqlTransaction)Transaction).Connection);
        }
    }
}