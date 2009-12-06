using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MySql.Data.MySqlClient;
using System.Data;
using Game.Setup;
using System.Data.Common;
using System.Threading;
using System.IO;
using log4net.Repository.Hierarchy;
using System.Data.SqlClient;

namespace Game.Database {
    public class MySqlDbManager: IDbManager {
        
        Dictionary<Type, String> saveCommands = new Dictionary<Type, String>();
        Dictionary<Type, String> createCommands = new Dictionary<Type, String>();
        Dictionary<Type, String> createListCommands = new Dictionary<Type, String>();
        Dictionary<Type, String> deleteCommands = new Dictionary<Type, String>();
        Dictionary<Type, String> deleteListCommands = new Dictionary<Type, String>();

        string connectionString;

        bool paused = false;

        [ThreadStatic]
        static MySqlDbTransaction persistantTransaction;

        public MySqlDbManager(string hostname, string username, string password, string database) {
            connectionString = string.Format("Database={0};Host={1};User Id={2};Password={3};", database, hostname, username, password);
        }

        MySqlConnection GetConnection() {
            lock (this) {
                MySqlConnection connection = new MySqlConnection(connectionString);
                
                try {
                    connection.Open();

                }
                catch (Exception ex) {
                    Global.Logger.Error("An exception of type " + ex.GetType() + " was encountered while attempting to open the connection.", ex);
                    Environment.Exit(-1);
                }
                return connection;
            }                       
        }

        public void Close(DbConnection connection) {
            lock (this) {                
                connection.Close();
            }
        }

        public void ClearThreadTransaction() {
            persistantTransaction = null;
        }

        public DbTransaction GetThreadTransaction() {
            if (persistantTransaction != null) {
                persistantTransaction.ReferenceCount++;

                return (DbTransaction)persistantTransaction;
            }

            persistantTransaction = new MySqlDbTransaction(this, null);

            persistantTransaction.ReferenceCount = 1;

            return (DbTransaction)persistantTransaction;
        }

        void InitPersistantTransaction() {
            if (persistantTransaction == null || persistantTransaction.transaction != null) return;

            MySqlConnection connection = GetConnection();

            if (connection == null)
                throw new Exception("Did not get a connection"); //Should blow up ?          

            if (Config.database_verbose)
                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction started");

            persistantTransaction.transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);

            MySqlCommand command = new MySqlCommand("SET AUTOCOMMIT = 0", (persistantTransaction.transaction as MySqlTransaction).Connection, (persistantTransaction.transaction as MySqlTransaction));
            ExecuteNonQuery(command);
        }

        MySqlDbTransaction CreateTransaction() {
            MySqlConnection connection = GetConnection();

            if (connection == null)
                return null; //Should blow up ? 

            if (Config.database_verbose)
                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction started");
            
            MySqlDbTransaction transaction = new MySqlDbTransaction(this, connection.BeginTransaction(IsolationLevel.RepeatableRead));
            
            MySqlCommand command = new MySqlCommand("SET AUTOCOMMIT = 0", (transaction.transaction as MySqlTransaction).Connection, (transaction.transaction as MySqlTransaction));

            ExecuteNonQuery(command);

            return transaction;
        }

        public bool Save(params IPersistable[] objects) {
            if (paused) return true;

            InitPersistantTransaction();

            MySqlTransaction mySqlTransaction = (persistantTransaction.transaction as MySqlTransaction);

            if (mySqlTransaction == null)
                throw new Exception("Invalid transaction specified");

            if (objects.Length == 0)
                return true;

            try {
                foreach (IPersistable obj in objects) {

                    MySqlCommand command = null;

                    if (obj is IPersistableObject) {
                        IPersistableObject persistableObj = obj as IPersistableObject;
                        if (persistableObj.DbPersisted)
                            command = UpdateObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);
                        else
                            command = CreateObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                        if (command != null)
                            ExecuteNonQuery(command);
                                                 
                        persistableObj.DbPersisted = true;
                    }
                    
                    if (obj is IPersistableList) {
                        IPersistableList persistableObj = obj as IPersistableList;

                        command = DeleteList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                        if (command != null)
                            ExecuteNonQuery(command);

                        command = CreateList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                        if (command != null)
                            ExecuteNonQuery(command);
                    }
                    
                    Type objType = obj.GetType();
                    foreach (DbDependency dependency in obj.DbDependencies) {
                        if (dependency.AutoSave) {
                            PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                            IPersistable persistable = propInfo.GetValue(obj, null) as IPersistable;
                            if (persistable != null) {
                                if (!Save(persistable))
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
                return false;
            }         

            return true;
        }

        private MySqlCommand UpdateObject(MySqlConnection connection, MySqlTransaction transaction, IPersistableObject obj) {
            string commandText;

            DbColumn[] columns = obj.DbColumns;
            if (columns.Length == 0)
                return null;

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

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in columns)
                addParameter(command, column);

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column);
            
            return command;
        }

        private MySqlCommand CreateList(MySqlConnection connection, MySqlTransaction transaction, IPersistableList obj) {         
            string commandText;

            if (!createListCommands.TryGetValue(obj.GetType(), out commandText)) {
                string columns = "";

                bool startComma = false;

                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma) {
                        columns += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                }

                foreach (DbColumn column in obj.DbListColumns) {
                    if (startComma) {
                        columns += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);                    
                }

                commandText = string.Format("INSERT INTO `{0}_list` ({1}) VALUES ", obj.DbTable, columns);

                createListCommands[obj.GetType()] = commandText;
            }

            StringBuilder builder = new StringBuilder(commandText);

            bool startColumnsComma = false;
            bool empty = true;
            
            foreach (DbColumn[] columns in obj) {
                empty = false;

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

            if (empty)
                return null;

            MySqlCommand command = connection.CreateCommand();
            
            if (transaction != null)
                command.Transaction = transaction;

            command.CommandText = builder.ToString();
            
            return command;
        }

        private MySqlCommand CreateObject(MySqlConnection connection, MySqlTransaction transaction, IPersistableObject obj) {
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

            MySqlCommand command = connection.CreateCommand();
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
            if (paused) return true;

            InitPersistantTransaction();

            MySqlTransaction mySqlTransaction = (persistantTransaction.transaction as MySqlTransaction);
            if (mySqlTransaction == null)
                throw new Exception("Invalid transaction specified");

            if (objects.Length == 0)
                return true;

            MySqlConnection connection = mySqlTransaction.Connection;

            try {                
                foreach (IPersistable obj in objects) {
                    if (obj is IPersistableObject && !(obj as IPersistableObject).DbPersisted)
                        continue;

                    if (obj is IPersistableObject) {
                        IPersistableObject persistableObj = obj as IPersistableObject;

                        MySqlCommand command = DeleteObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                        ExecuteNonQuery(command);

                        persistableObj.DbPersisted = false;
                    }
                    
                    if (obj is IPersistableList) {
                        IPersistableList persistableObj = obj as IPersistableList;

                        MySqlCommand command = DeleteList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                        ExecuteNonQuery(command);
                    }
                    
                    Type objType = obj.GetType();
                    foreach (DbDependency dependency in obj.DbDependencies) {
                        if (dependency.AutoDelete) {
                            PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                            IPersistable persistable = propInfo.GetValue(obj, null) as IPersistable;
                            if (persistable != null) {
                                if (!Delete(persistable))
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
                return false;
            }

            return true;
        }

        private MySqlCommand DeleteList(MySqlConnection connection, MySqlTransaction transaction, IPersistableList obj) {
            string commandText;

            if (!deleteListCommands.TryGetValue(obj.GetType(), out commandText)) {

                bool startComma = false;
                string where = "";
                foreach (DbColumn column in obj.DbPrimaryKey) {
                    if (startComma)
                        where += " AND ";
                    else
                        startComma = true;

                    where += string.Format("{0}=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}_list` WHERE {1}", obj.DbTable, where);

                deleteListCommands[obj.GetType()] = commandText;
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column, DataRowVersion.Original);

            return command;
        }

        MySqlCommand DeleteObject(MySqlConnection connection, MySqlTransaction transaction, IPersistableObject obj) {

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

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column, DataRowVersion.Original);

            return command;
        }

        void addParameter(MySqlCommand command, DbColumn column) {
            addParameter(command, column, DataRowVersion.Default);
        }

        void addParameter(MySqlCommand command, DbColumn column, DataRowVersion sourceVersion) {
            MySqlParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@" + column.Column;
            parameter.DbType = column.Type;
            if (column.Size > 0)
                parameter.Size = column.Size;
            parameter.SourceVersion = sourceVersion;
            parameter.Value = column.Value;

            command.Parameters.Add(parameter);
        }

        public DbDataReader Select(string table) {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;

            command.CommandText = string.Format("SELECT * FROM `{0}`", table);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        DbDataReader IDbManager.SelectList(IPersistableList obj) {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;

            bool startComma = false;
            string where = "";
            foreach (DbColumn column in obj.DbPrimaryKey) {
                if (startComma)
                    where += " AND ";
                else
                    startComma = true;

                where += string.Format("{0}=@{0}", column.Column);
            }

            command.CommandText = string.Format("SELECT * FROM `{0}_list` WHERE {1}", obj.DbTable, where);

            foreach (DbColumn column in obj.DbPrimaryKey)
                addParameter(command, column, DataRowVersion.Original);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        DbDataReader IDbManager.SelectList(string table, params DbColumn[] primaryKeyValues) {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;

            bool startComma = false;
            string where = "";
            foreach (DbColumn column in primaryKeyValues) {
                if (startComma)
                    where += " AND ";
                else
                    startComma = true;

                where += string.Format("{0}=@{0}", column.Column);
            }

            command.CommandText = string.Format("SELECT * FROM `{0}` WHERE {1}", table, where);

            foreach (DbColumn column in primaryKeyValues)
                addParameter(command, column, DataRowVersion.Original);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public DbDataReader ReaderQuery(string query) {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();
            
            command.Connection = connection;
            command.CommandText = query;

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void Query(string query) {
            if (paused) return;

            MySqlCommand command;
            bool transactional = persistantTransaction != null;
            MySqlDbTransaction transaction;

            if (!transactional)
            {
                transaction = CreateTransaction();
                MySqlConnection connection = GetConnection();
                command = connection.CreateCommand();
                command.Connection = connection;               
            }
            else
            {
                transaction = persistantTransaction;
                InitPersistantTransaction();
            }

            command = (transaction.transaction as MySqlTransaction).Connection.CreateCommand();
            command.Connection = (transaction.transaction as MySqlTransaction).Connection;
            command.Transaction = (transaction.transaction as MySqlTransaction);

            command.CommandText = query;

            ExecuteNonQuery(command);

            if (!transactional) {
                (transaction.transaction as MySqlTransaction).Commit();
                Close(command.Connection);
            }
        }

        void ExecuteNonQuery(MySqlCommand command) {
            if (Config.database_verbose) {
                StringWriter sqlwriter = new StringWriter();
                foreach (MySqlParameter param in command.Parameters) {
                    sqlwriter.Write(param.ToString() + "=" + ((param.Value != null) ? param.Value.ToString() : "NULL") + ",");
                }

                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") " + command.CommandText + " {" + sqlwriter.ToString() + "}");
            }

            for (int i = 0; i < 100; i++) {
                try {
                    command.ExecuteNonQuery();
                    return;
                }
                catch (MySqlException e) {
                    /*
                    if (e.Number == 1213) //deadlock
                    {
                        Global.DbLogger.Warn("(" + Thread.CurrentThread.ManagedThreadId + ") Dead lock detected at attempt " + i + " on command " + command.CommandText);
                        continue;
                    }
                    */
                    HandleGeneralException(e, command);
                    return;
                }
                catch (Exception e) {
                    HandleGeneralException(e, command);
                    return;
                }
            }

            throw new Exception("Maximum deadlock reached");
        }

        public void HandleGeneralException(Exception e, MySqlCommand command) {
            StringWriter writer = new StringWriter();
            foreach (MySqlParameter param in command.Parameters) {
                writer.Write(param.ToString() + "=" + param.Value.ToString() + ",");
            }

            Global.DbLogger.Error("(" + Thread.CurrentThread.ManagedThreadId + ") " + writer.ToString(), e);
            throw e;
        }

        public void Rollback() {
            if (persistantTransaction == null)
                throw new Exception("Not inside of a transaction");

            if (Config.database_verbose)
                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction rollback");

            persistantTransaction.Rollback();
        }

        public uint LastInsertId()
        {
            if (paused) return 0;
            MySqlCommand command;
            bool transactional = persistantTransaction != null;
            MySqlDbTransaction transaction;

            if (!transactional) {
                transaction = CreateTransaction();
                MySqlConnection connection = GetConnection();
                command = connection.CreateCommand();
                command.Connection = connection;
            }
            else {
                transaction = persistantTransaction;
                InitPersistantTransaction();
            }

            command = (transaction.transaction as MySqlTransaction).Connection.CreateCommand();
            command.Connection = (transaction.transaction as MySqlTransaction).Connection;
            command.Transaction = (transaction.transaction as MySqlTransaction);

            command.CommandText = "SELECT LAST_INSERT_ID()";

            long id = (long)command.ExecuteScalar();

            if (!transactional) {
                (transaction.transaction as MySqlTransaction).Commit();
                Close(command.Connection);
            }

            return (uint)id;
        }

        public void Pause() {
            paused = true;
        }

        public void Resume() {
            paused = false;
        }

        public void EmptyDatabase() {
            
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.CommandText = 
                "TRUNCATE TABLE `reference_stubs`;" +
                "TRUNCATE TABLE `chain_actions`;" +
                "TRUNCATE TABLE `active_actions`;" +
                "TRUNCATE TABLE `passive_actions`;" +
                "TRUNCATE TABLE `cities`;" +
                "TRUNCATE TABLE `players`;" +
                "TRUNCATE TABLE `structures`;" +
                "TRUNCATE TABLE `structure_properties`;" +
                "TRUNCATE TABLE `structure_properties_list`;" +
                "TRUNCATE TABLE `reference_stubs`;" +
                "TRUNCATE TABLE `technologies`;" +
                "TRUNCATE TABLE `technologies_list`;" +
                "TRUNCATE TABLE `troops`;" +
                "TRUNCATE TABLE `troop_stubs`;" +
                "TRUNCATE TABLE `troop_stubs_list`;" +
                "TRUNCATE TABLE `unit_templates`;" +
                "TRUNCATE TABLE `unit_templates_list`;" +
                "TRUNCATE TABLE `battles`;" +
                "TRUNCATE TABLE `battle_managers`;" +
                "TRUNCATE TABLE `battle_reports`;" +
                "TRUNCATE TABLE `battle_report_objects`;" +
                "TRUNCATE TABLE `battle_report_troops`;" +
                "TRUNCATE TABLE `reported_troops`;" +
                "TRUNCATE TABLE `reported_troops_list`;" +
                "TRUNCATE TABLE `reported_objects`;" +
                "TRUNCATE TABLE `reported_objects_list`;" +
                "TRUNCATE TABLE `combat_structures`;" +
                "TRUNCATE TABLE `combat_units`;" +
                "TRUNCATE TABLE `notifications`;" +
                "TRUNCATE TABLE `notifications_list`;" +
                "TRUNCATE TABLE `market`;"
                ;

            command.Connection = connection;

            command.ExecuteNonQuery();

            Close(connection);
        }
    }
}
