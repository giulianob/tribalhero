#region

using System;
using System.Threading;
using Game.Data;
using Game.Setup;
using MySql.Data.MySqlClient;

#endregion

namespace Game.Database.Managers
{
    class MySqlDbTransaction : DbTransaction
    {
        internal MySqlDbTransaction(MySqlDbManager manager, MySqlTransaction transaction) : base(manager, transaction)
        {
        }

        public override void Rollback()
        {
            base.Rollback();

            if (ReferenceCount > 0 || Transaction == null)
                return;

            ((MySqlTransaction)Transaction).Rollback();
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

                MySqlDbManager.HandleGeneralException(e, null);
                throw;
            }

            manager.Close(((MySqlTransaction)Transaction).Connection);
        }
    }
}