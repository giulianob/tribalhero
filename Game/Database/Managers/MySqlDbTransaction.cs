using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Game.Setup;
using System.Threading;

namespace Game.Database {
    class MySqlDbTransaction : DbTransaction, IDisposable {

        internal MySqlDbTransaction(MySqlDbManager manager, MySqlTransaction transaction) :
        base(manager, transaction) {
            
        }

        public override void Rollback() {
            base.Rollback();

            if (ReferenceCount > 0 || this.transaction == null)
                return;

            MySqlTransaction transaction = this.transaction as MySqlTransaction;

            transaction.Rollback();
        }

        protected override bool Commit() {
            base.Commit();

            if (ReferenceCount > 0)
                return true;

            MySqlTransaction transaction = this.transaction as MySqlTransaction;

            if (transaction == null) //no queries ran
                return true;

            try {
                transaction.Commit();

                if (Config.database_verbose)
                    Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction committed");
            }
            catch (Exception e) {
                Global.Logger.Error(e.Message + " " + e.StackTrace);
                try {
                    transaction.Rollback();
                }
                catch (Exception ex) {
                    Global.Logger.Error(ex.Message + " " + ex.StackTrace);
                    manager.Close(transaction.Connection);
                    return false;
                }
                manager.Close(transaction.Connection);                         

                return false;
            }

            manager.Close(transaction.Connection);

            return true;
        }
    }
}
