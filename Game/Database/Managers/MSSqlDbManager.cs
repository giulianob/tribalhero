using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace Game.Database {
    public class MSSqlDbManager: IDbManager {
        Dictionary<Type, String> saveCommands = new Dictionary<Type, String>();
        Dictionary<Type, String> createCommands = new Dictionary<Type, String>();
        Dictionary<Type, String> deleteCommands = new Dictionary<Type, String>();

        string connectionString;

        public MSSqlDbManager() {
            connectionString = "Data Source=OM704\\SQLEXPRESS;Initial Catalog=Game;Integrated Security=True;Pooling=False";
        }

        SqlConnection GetConnection() {
            SqlConnection connection = new SqlConnection(connectionString);

            try {
                connection.Open();
            }

            catch (SqlException ex) {
                Global.Logger.Error("An exception of type " + ex.GetType() + ":"+ ex.Message);
                return null;
            }
            catch (Exception ex) {
                Global.Logger.Error("An exception of type " + ex.GetType() + " was encountered while attempting to open the connection.");
                return null;
            }
            return connection;
        }

        public bool CreateTransaction(out DbTransaction transaction) {
            transaction = null;

            SqlConnection connection = GetConnection();
            
            if (connection == null)
                return false;

            transaction = new MSSqlDbTransaction(this, connection.BeginTransaction());
            
            return true;
        }

        public bool Save(params IPersistable[] objects) {
            return Save(null, objects);
        }

        public bool Save(DbTransaction transaction, params IPersistable[] objects) {
            SqlTransaction msSqlTransaction = null;

            if (transaction != null) {
                msSqlTransaction = (transaction.transaction as SqlTransaction);
                if (msSqlTransaction == null)
                    throw new Exception("Invalid transaction specified");
            }

            if (objects.Length == 0)
                return true;

            SqlConnection connection;
            if (msSqlTransaction != null)
                connection = msSqlTransaction.Connection;
            else
                connection = GetConnection();

            try {
                foreach (IPersistable obj in objects) {

                    SqlCommand command = null;

                    if (obj is IPersistableObject) {
                        IPersistableObject persistableObj = obj as IPersistableObject;
                        if (persistableObj.DbPersisted)
                            command = UpdateObject(connection, msSqlTransaction, persistableObj);
                        else
                            command = CreateObject(connection, msSqlTransaction, persistableObj);

                        command.ExecuteNonQuery();

                        if (msSqlTransaction == null)
                            persistableObj.DbPersisted = true;
                        else
                            transaction.addToPersistance(persistableObj, true);
                    }
                    else if (obj is IPersistableList) {
                        IPersistableList persistableObj = obj as IPersistableList;
                        if (!Delete(transaction, false, persistableObj))
                            return false;
                        command = CreateList(connection, msSqlTransaction, persistableObj);
                        command.ExecuteNonQuery();
                    }
                    else
                        throw new Exception("Unknown IPersistable type");

                    Type objType = obj.GetType();
                    foreach (DbDependency dependency in obj.DbDependencies) {
                        if (dependency.AutoSave) {
                            PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                            IPersistable persistable = propInfo.GetValue(obj, null) as IPersistable;
                            if (persistable != null) {
                                if (!Save(transaction, persistable))
                                    return false;
                            }
                            else
                                return false;
                        }
                    }
                }
            }
            catch (Exception e) {
                Global.Logger.Error(e.Message + " " + e.StackTrace);
                connection.Close();
                return false;
            }         

            if (msSqlTransaction == null)
                connection.Close();

            return true;
        }

        private SqlCommand UpdateObject(SqlConnection connection, SqlTransaction transaction, IPersistableObject obj) {
            string commandText;

            if (!saveCommands.TryGetValue(obj.GetType(), out commandText)) {
                bool startComma = false;
                string values = "";
                foreach (DbColumn column in obj.DbColumns) {
                    if (startComma)
                        values += ", ";
                    else
                        startComma = true;

                    values += string.Format("{0}=@{0}", column.Column);
                }

                startComma = false;
                string where = "";
                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma)
                        where += " AND ";
                    else
                        startComma = true;

                    where += string.Format("{0}=@{0}", column.Column);
                }

                commandText = string.Format("UPDATE `{0}` SET {1} WHERE {2} LIMIT 1", obj.DbTable, values, where);
                
                saveCommands[obj.GetType()] = commandText;
            }

            SqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in obj.DbColumns)
                addParameter(command, column);

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column);
            
            return command;
        }

        private SqlCommand CreateList(SqlConnection connection, SqlTransaction transaction, IPersistableList obj) {         
            string commandText;

            if (!createCommands.TryGetValue(obj.GetType(), out commandText)) {
                string columns = "";

                bool startComma = false;

                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma) {
                        columns += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                }

                foreach (DbColumn column in obj.DbColumns) {
                    if (startComma) {
                        columns += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);                    
                }

                commandText = string.Format("INSERT INTO `{0}` ({1}) VALUES ", obj.DbTable, columns);

                createCommands[obj.GetType()] = commandText;
            }

            StringBuilder builder = new StringBuilder(commandText);

            bool startColumnsComma = false;
            foreach (DbColumn[] columns in obj) {
                if (startColumnsComma)
                    builder.Append(", (");
                else {
                    builder.Append("(");
                    startColumnsComma = true;
                }
                
                bool startComma = false;

                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma)
                        builder.Append(", ");
                    else
                        startComma = true;

                    builder.Append("'" + column.Value + "'");                    
                }

                foreach (DbColumn column in columns) {
                    if (startComma)
                        builder.Append(", ");
                    else
                        startComma = true;

                    builder.Append("'" + column.Value + "'");
                }
                builder.Append(")");
            }

            SqlCommand command = connection.CreateCommand();
            
            if (transaction != null)
                command.Transaction = transaction;

            command.CommandText = builder.ToString();
            
            return command;
        }

        private SqlCommand CreateObject(SqlConnection connection, SqlTransaction transaction, IPersistableObject obj) {
            string commandText;

            if (!createCommands.TryGetValue(obj.GetType(), out commandText)) {
                string columns = "";
                string values = "";

                bool startComma = false;

                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma) {
                        columns += ",";
                        values += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                    values += "@" + column.Column;
                }

                foreach (DbColumn column in obj.DbColumns) {
                    if (startComma) {
                        columns += ",";
                        values += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                    values += "@" + column.Column;
                }
                commandText = string.Format("INSERT INTO `{0}` ({1}) VALUES ({2})", obj.DbTable, columns, values);

                createCommands[obj.GetType()] = commandText;
            }

            SqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in obj.DbColumns)
                addParameter(command, column);

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column);

            return command;
        }

        public bool Delete(params IPersistable[] objects) {
            return Delete(null, true, objects);
        }

        public bool Delete(DbTransaction transaction, bool cascade, params IPersistable[] objects) {
            SqlTransaction msSqlTransaction = null;
            if (transaction != null) {
                msSqlTransaction = (transaction.transaction as SqlTransaction);
                if (msSqlTransaction == null)
                    throw new Exception("Invalid transaction specified");
            }

            if (objects.Length == 0)
                return true;

            SqlConnection connection;
            if (msSqlTransaction != null)
                connection = msSqlTransaction.Connection;
            else
                connection = GetConnection();

            try {                
                foreach (IPersistable obj in objects) {
                    if (obj is IPersistableObject) {
                        IPersistableObject persistableObj = obj as IPersistableObject;

                        if (!persistableObj.DbPersisted)
                            continue;

                        SqlCommand command = DeleteObject(connection, msSqlTransaction, persistableObj);

                        command.ExecuteNonQuery();

                        if (msSqlTransaction == null)
                            persistableObj.DbPersisted = false;
                        else
                            transaction.addToPersistance(persistableObj, false);
                    }
                    else if (obj is IPersistableList) {
                        IPersistableList persistableObj = obj as IPersistableList;

                        SqlCommand command = DeleteList(connection, msSqlTransaction, persistableObj);

                        command.ExecuteNonQuery();
                    }
                    else
                        throw new Exception("Unknown IPersistable type");

                    if (cascade) {
                        Type objType = obj.GetType();
                        foreach (DbDependency dependency in obj.DbDependencies) {
                            if (dependency.AutoDelete) {
                                PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                                IPersistable persistable = propInfo.GetValue(obj, null) as IPersistable;
                                if (persistable != null) {
                                    if (!Delete(transaction, true, persistable))
                                        return false;
                                }
                                else
                                    return false;
                            }
                        }
                    }
                }

            }
            catch (Exception e) {
                Global.Logger.Error(e.Message + " " + e.StackTrace);
                connection.Close();
                return false;
            }

            if (msSqlTransaction == null)
                connection.Close();

            return true;
        }

        private SqlCommand DeleteList(SqlConnection connection, SqlTransaction transaction, IPersistableList obj) {
            string commandText;

            if (!deleteCommands.TryGetValue(obj.GetType(), out commandText)) {

                bool startComma = false;
                string where = "";
                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma)
                        where += " AND ";
                    else
                        startComma = true;

                    where += string.Format("{0}=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}` WHERE {1}", obj.DbTable, where);

                deleteCommands[obj.GetType()] = commandText;
            }

            SqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column, DataRowVersion.Original);

            return command;
        }

        SqlCommand DeleteObject(SqlConnection connection, SqlTransaction transaction, IPersistableObject obj) {

            string commandText;

            if (!deleteCommands.TryGetValue(obj.GetType(), out commandText)) {

                bool startComma = false;
                string where = "";
                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma)
                        where += " AND ";
                    else
                        startComma = true;

                    where += string.Format("{0}=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}` WHERE {1} LIMIT 1", obj.DbTable, where);

                deleteCommands[obj.GetType()] = commandText;
            }

            SqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column, DataRowVersion.Original);

            return command;
        }

        void addParameter(SqlCommand command, DbColumn column) {
            addParameter(command, column, DataRowVersion.Default);
        }

        void addParameter(SqlCommand command, DbColumn column, DataRowVersion sourceVersion) {
            SqlParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@" + column.Column;
            parameter.DbType = column.Type;
            if (column.Size > 0)
                parameter.Size = column.Size;
            parameter.SourceVersion = sourceVersion;
            parameter.Value = column.Value;

            command.Parameters.Add(parameter);
        }

        #region IDbManager Members

        public DbDataReader Select(string table) {
            throw new Exception("The method or operation is not implemented.");
        }

        public void EmptyDatabase() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        DbDataReader IDbManager.SelectList(IPersistableList obj) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
