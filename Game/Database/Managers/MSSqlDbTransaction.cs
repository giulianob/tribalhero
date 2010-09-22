using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Game.Database {
    class MSSqlDbTransaction : DbTransaction {

        internal MSSqlDbTransaction(MSSqlDbManager manager, SqlTransaction transaction) :
        base(manager, transaction) {
            
        }

        public override void Rollback() {
            SqlTransaction transaction = this.transaction as SqlTransaction;

            transaction.Rollback();
        }

        public override bool Commit() {
            SqlTransaction transaction = this.transaction as SqlTransaction;

            if (transaction.Connection.State == System.Data.ConnectionState.Closed)
                return false;

            try {
                transaction.Commit();

                base.Commit();
            }
            catch (Exception e) {
                Global.Logger.Error(e.Message + " " + e.StackTrace);
                try {
                    transaction.Rollback();
                }
                catch (Exception ex) {
                    Global.Logger.Error(ex.Message + " " + ex.StackTrace);
                    transaction.Connection.Close();
                    return false;
                }
                transaction.Connection.Close();
                return false;
            }
            return true;
        }
    }
}
