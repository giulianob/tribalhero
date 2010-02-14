#region

using System;
using System.Threading;
using Game.Data;
using Game.Setup;
using MySql.Data.MySqlClient;

#endregion

namespace Game.Database.Managers {
    class MySqlDbTransaction : DbTransaction {
        internal MySqlDbTransaction(MySqlDbManager manager, MySqlTransaction transaction) : base(manager, transaction) {}

        public override void Rollback() {
            base.Rollback();

            if (ReferenceCount > 0 || transaction == null)
                return;            

            ((MySqlTransaction)transaction).Rollback();
        }

        protected override bool Commit() {
            base.Commit();

            if (ReferenceCount > 0)
                return true;

            if (transaction == null) //no queries ran
                return true;

            try {
                ((MySqlTransaction)transaction).Commit();

                if (Config.database_verbose)
                    Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction committed");
            }
            catch (Exception e) {
                Global.Logger.Error(e.Message + " " + e.StackTrace);
                try {
                    ((MySqlTransaction)transaction).Rollback();
                }
                catch (Exception ex) {
                    Global.Logger.Error(ex.Message + " " + ex.StackTrace);
                    manager.Close(((MySqlTransaction)transaction).Connection);
                    return false;
                }
                manager.Close(((MySqlTransaction)transaction).Connection);

                return false;
            }

            manager.Close(((MySqlTransaction)transaction).Connection);

            return true;
        }
    }
}