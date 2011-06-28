#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Game.Data;
using Game.Setup;
using MySql.Data.MySqlClient;

#endregion

namespace Game.Database.Managers
{
    public class MySqlDbManager : IDbManager
    {
        [ThreadStatic]
        private static MySqlDbTransaction persistantTransaction;

        private readonly string connectionString;

        private readonly Dictionary<Type, String> createCommands = new Dictionary<Type, String>();
        private readonly Dictionary<Type, String> createListCommands = new Dictionary<Type, String>();
        private readonly Dictionary<Type, String> deleteCommands = new Dictionary<Type, String>();
        private readonly Dictionary<Type, String> deleteListCommands = new Dictionary<Type, String>();
        private readonly Dictionary<Type, String> saveCommands = new Dictionary<Type, String>();

        private DateTime lastProbe;

        private bool paused;
        private long queriesRan;

        public MySqlDbManager(string hostname, string username, string password, string database, int timeout)
        {
            connectionString = string.Format("Database={0};Host={1};User Id={2};Password={3};Connection Timeout={4}", database, hostname, username, password, timeout);            
        }

        #region IDbManager Members

        public void Close(DbConnection connection)
        {
            lock (this)
            {
                connection.Close();
            }
        }

        public void ClearThreadTransaction()
        {
            persistantTransaction = null;
        }

        public DbTransaction GetThreadTransaction()
        {
            if (persistantTransaction != null)
            {
                persistantTransaction.ReferenceCount++;

                return persistantTransaction;
            }

            persistantTransaction = new MySqlDbTransaction(this, null) {ReferenceCount = 1};

            return persistantTransaction;
        }

        public bool Save(params IPersistable[] objects)
        {
            if (paused)
                return true;

            InitPersistantTransaction();

            var mySqlTransaction = (persistantTransaction.Transaction as MySqlTransaction);

            if (mySqlTransaction == null)
                throw new Exception("Invalid transaction specified");

            if (objects.Length == 0)
                return true;

            foreach (var obj in objects)
            {
                MySqlCommand command;

                if (obj is IPersistableObject)
                {
                    var persistableObj = obj as IPersistableObject;
                    if (persistableObj.DbPersisted)
                        command = UpdateObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);
                    else
                        command = CreateObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    if (command != null)
                        ExecuteNonQuery(command);

                    persistableObj.DbPersisted = true;
                }

                if (obj is IPersistableList)
                {
                    var persistableObj = obj as IPersistableList;

                    command = DeleteList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    if (command != null)
                        ExecuteNonQuery(command);

                    command = CreateList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    if (command != null)
                        ExecuteNonQuery(command);
                }

                Type objType = obj.GetType();
                foreach (var dependency in obj.DbDependencies)
                {
                    if (dependency.AutoSave)
                    {
                        PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                        var persistable = propInfo.GetValue(obj, null) as IPersistable;
                        if (persistable != null)
                            Save(persistable);
                    }
                }
            }

            return true;
        }

        public bool Delete(params IPersistable[] objects)
        {
            if (paused)
                return true;

            InitPersistantTransaction();

            var mySqlTransaction = (persistantTransaction.Transaction as MySqlTransaction);
            if (mySqlTransaction == null)
                throw new Exception("Invalid transaction specified");

            if (objects.Length == 0)
                return true;

            foreach (var obj in objects)
            {
                if (obj is IPersistableObject && !(obj as IPersistableObject).DbPersisted)
                    continue;

                if (obj is IPersistableObject)
                {
                    var persistableObj = obj as IPersistableObject;

                    MySqlCommand command = DeleteObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    ExecuteNonQuery(command);

                    persistableObj.DbPersisted = false;
                }

                if (obj is IPersistableList)
                {
                    var persistableObj = obj as IPersistableList;

                    MySqlCommand command = DeleteList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    ExecuteNonQuery(command);
                }

                Type objType = obj.GetType();
                foreach (var dependency in obj.DbDependencies)
                {
                    if (dependency.AutoDelete)
                    {
                        PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                        var persistable = propInfo.GetValue(obj, null) as IPersistable;
                        if (persistable != null)
                            Delete(persistable);
                    }
                }
            }

            return true;
        }

        public void DeleteDependencies(IPersistable obj)
        {
            Type objType = obj.GetType();
            foreach (var dependency in obj.DbDependencies)
            {
                if (!dependency.AutoDelete)
                    continue;

                PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                var persistable = propInfo.GetValue(obj, null) as IPersistable;
                if (persistable != null)
                    Delete(persistable);
            }
        }

        public DbDataReader Select(string table)
        {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;

            command.CommandText = string.Format("SELECT * FROM `{0}`", table);

            LogCommand(command);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        DbDataReader IDbManager.SelectList(IPersistableList obj)
        {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;

            bool startComma = false;
            string where = "";
            foreach (var column in obj.DbPrimaryKey)
            {
                if (startComma)
                    where += " AND ";
                else
                    startComma = true;

                where += string.Format("`{0}`=@{0}", column.Column);
            }

            command.CommandText = string.Format("SELECT * FROM `{0}_list` WHERE {1}", obj.DbTable, where);

            foreach (var column in obj.DbPrimaryKey)
                AddParameter(command, column, DataRowVersion.Original);

            LogCommand(command);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private static void LogCommand(MySqlCommand command)
        {
            if (!Config.database_verbose)
                return;

            var sqlwriter = new StringWriter();
            foreach (MySqlParameter param in command.Parameters)
                sqlwriter.Write(param + "=" + ((param.Value != null) ? param.Value.ToString() : "NULL") + ",");

            Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") " + command.CommandText + " {" + sqlwriter + "}");
        }

        DbDataReader IDbManager.SelectList(string table, params DbColumn[] primaryKeyValues)
        {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;

            bool startComma = false;
            string where = "";
            foreach (var column in primaryKeyValues)
            {
                if (startComma)
                    where += " AND ";
                else
                    startComma = true;

                where += string.Format("`{0}`=@{0}", column.Column);
            }

            command.CommandText = string.Format("SELECT * FROM `{0}` WHERE {1}", table, where);

            foreach (var column in primaryKeyValues)
                AddParameter(command, column, DataRowVersion.Original);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public DbDataReader ReaderQuery(string query, DbColumn[] parms)
        {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();

            command.Connection = connection;
            command.CommandText = query;
            foreach (var parm in parms)
                AddParameter(command, parm);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public int Query(string query, DbColumn[] parms)
        {
            if (paused)
                return 0;

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

            command = ((MySqlTransaction)transaction.Transaction).Connection.CreateCommand();
            command.Connection = ((MySqlTransaction)transaction.Transaction).Connection;
            command.Transaction = (transaction.Transaction as MySqlTransaction);

            command.CommandText = query;
            foreach (var parm in parms)
                AddParameter(command, parm);

            int affected = ExecuteNonQuery(command);

            if (!transactional)
            {
                ((MySqlTransaction)transaction.Transaction).Commit();
                Close(command.Connection);
            }

            return affected;
        }

        public void Rollback()
        {
            if (persistantTransaction == null)
                throw new Exception("Not inside of a transaction");

            if (Config.database_verbose)
                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction rollback");

            persistantTransaction.Rollback();
        }

        public uint LastInsertId()
        {
            if (paused)
                return 0;
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

            command = ((MySqlTransaction)transaction.Transaction).Connection.CreateCommand();
            command.Connection = ((MySqlTransaction)transaction.Transaction).Connection;
            command.Transaction = (transaction.Transaction as MySqlTransaction);

            command.CommandText = "SELECT LAST_INSERT_ID()";

            var id = (long)command.ExecuteScalar();

            if (!transactional)
            {
                ((MySqlTransaction)transaction.Transaction).Commit();
                Close(command.Connection);
            }

            return (uint)id;
        }

        public void Pause()
        {
            paused = true;
        }

        public void Resume()
        {
            paused = false;
        }

        public void EmptyDatabase()
        {
            MySqlConnection connection = GetConnection();

            MySqlCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandText = "SHOW TABLES";

            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if ((String)reader[0] == "schema_migrations")
                    continue;

                MySqlConnection truncateConnection = GetConnection();
                MySqlCommand truncateCommand = connection.CreateCommand();
                truncateCommand.CommandText = string.Format("TRUNCATE TABLE `{0}`", reader[0]);
                truncateCommand.Connection = truncateConnection;
                truncateCommand.ExecuteNonQuery();
                Close(truncateConnection);
            }

            Close(connection);
        }

        public void Probe(out int queriesRan, out DateTime lastProbe)
        {
            queriesRan = (int)Interlocked.Read(ref this.queriesRan);
            Interlocked.Exchange(ref this.queriesRan, 0);
            lastProbe = this.lastProbe;
            this.lastProbe = DateTime.UtcNow;
        }

        #endregion

        private MySqlConnection GetConnection()
        {
            lock (this)
            {
                var connection = new MySqlConnection(connectionString);

                try
                {
                    connection.Open();
                }
                catch(Exception ex)
                {
                    Global.Logger.Error("An exception of type " + ex.GetType() + " was encountered while attempting to open the connection.", ex);
                    Environment.Exit(-1);
                }
                return connection;
            }
        }

        private void InitPersistantTransaction()
        {
            if (persistantTransaction == null || persistantTransaction.Transaction != null)
                return;

            MySqlConnection connection = GetConnection();

            if (connection == null)
            {
                HandleGeneralException(new Exception("Did not get a connection"), null);
                return;
            }

            if (Config.database_verbose)
                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction started");

            try
            {
                persistantTransaction.Transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch(Exception e)
            {
                HandleGeneralException(e, null);
                return;
            }

            var command = new MySqlCommand("SET AUTOCOMMIT = 0",
                                           (persistantTransaction.Transaction as MySqlTransaction).Connection,
                                           (persistantTransaction.Transaction as MySqlTransaction));
            ExecuteNonQuery(command);
        }

        private MySqlDbTransaction CreateTransaction()
        {
            MySqlConnection connection = GetConnection();

            if (connection == null)
            {
                HandleGeneralException(new Exception("Did not get a connection"), null);
                return null;
            }

            if (Config.database_verbose)
                Global.DbLogger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction started");

            var transaction = new MySqlDbTransaction(this, connection.BeginTransaction(IsolationLevel.RepeatableRead));

            var command = new MySqlCommand("SET AUTOCOMMIT = 0",
                                           ((MySqlTransaction)transaction.Transaction).Connection,
                                           (transaction.Transaction as MySqlTransaction));

            ExecuteNonQuery(command);

            return transaction;
        }

        private MySqlCommand UpdateObject(MySqlConnection connection, MySqlTransaction transaction, IPersistableObject obj)
        {
            string commandText;

            DbColumn[] columns = obj.DbColumns;
            if (columns.Length == 0)
                return null;

            if (!saveCommands.TryGetValue(obj.GetType(), out commandText))
            {
                bool startComma = false;
                string values = "";
                foreach (var column in obj.DbColumns)
                {
                    if (startComma)
                        values += ", ";
                    else
                        startComma = true;

                    values += string.Format("`{0}`=@{0}", column.Column);
                }

                startComma = false;
                string where = "";
                foreach (var column in obj.DbPrimaryKey)
                {
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

            foreach (var column in columns)
                AddParameter(command, column);

            foreach (var column in obj.DbPrimaryKey)
                AddParameter(command, column);

            return command;
        }

        private MySqlCommand CreateList(MySqlConnection connection, MySqlTransaction transaction, IPersistableList obj)
        {
            string commandText;

            if (!createListCommands.TryGetValue(obj.GetType(), out commandText))
            {
                string columns = "";

                bool startComma = false;

                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                        columns += ",";
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                }

                foreach (var column in obj.DbListColumns)
                {
                    if (startComma)
                        columns += ",";
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                }

                commandText = string.Format("INSERT INTO `{0}_list` ({1}) VALUES ", obj.DbTable, columns);

                createListCommands[obj.GetType()] = commandText;
            }

            var builder = new StringBuilder(commandText);

            bool startColumnsComma = false;
            bool empty = true;

            foreach (var columns in obj)
            {
                empty = false;

                if (startColumnsComma)
                    builder.Append(", (");
                else
                {
                    builder.Append("(");
                    startColumnsComma = true;
                }

                bool startComma = false;

                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                        builder.Append(", ");
                    else
                        startComma = true;

                    builder.Append("'" + column.Value + "'");
                }

                foreach (var column in columns)
                {
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

        private MySqlCommand CreateObject(MySqlConnection connection, MySqlTransaction transaction, IPersistableObject obj)
        {
            string commandText;

            if (!createCommands.TryGetValue(obj.GetType(), out commandText))
            {
                string columns = "";
                string values = "";

                bool startComma = false;

                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                    {
                        columns += ",";
                        values += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                    values += "@" + column.Column;
                }

                foreach (var column in obj.DbColumns)
                {
                    if (startComma)
                    {
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

            foreach (var column in obj.DbColumns)
                AddParameter(command, column);

            foreach (var column in obj.DbPrimaryKey)
                AddParameter(command, column);

            return command;
        }

        private MySqlCommand DeleteList(MySqlConnection connection, MySqlTransaction transaction, IPersistableList obj)
        {
            string commandText;

            if (!deleteListCommands.TryGetValue(obj.GetType(), out commandText))
            {
                bool startComma = false;
                string where = "";
                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                        where += " AND ";
                    else
                        startComma = true;

                    where += string.Format("`{0}`=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}_list` WHERE {1}", obj.DbTable, where);

                deleteListCommands[obj.GetType()] = commandText;
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (var column in obj.DbPrimaryKey)
                AddParameter(command, column, DataRowVersion.Original);

            return command;
        }

        private MySqlCommand DeleteObject(MySqlConnection connection, MySqlTransaction transaction, IPersistableObject obj)
        {
            string commandText;

            if (!deleteCommands.TryGetValue(obj.GetType(), out commandText))
            {
                bool startComma = false;
                string where = "";
                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                        where += " AND ";
                    else
                        startComma = true;

                    where += string.Format("`{0}`=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}` WHERE {1} LIMIT 1", obj.DbTable, where);

                deleteCommands[obj.GetType()] = commandText;
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
                command.Transaction = transaction;

            foreach (var column in obj.DbPrimaryKey)
                AddParameter(command, column, DataRowVersion.Original);

            return command;
        }

        private void AddParameter(MySqlCommand command, DbColumn column)
        {
            AddParameter(command, column, DataRowVersion.Default);
        }

        private void AddParameter(MySqlCommand command, DbColumn column, DataRowVersion sourceVersion)
        {
            MySqlParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@" + column.Column;

            if (column.Type != DbType.Object)
                parameter.DbType = column.Type;

            if (column.Size > 0)
                parameter.Size = column.Size;

            parameter.SourceVersion = sourceVersion;
            parameter.Value = column.Value;

            command.Parameters.Add(parameter);
        }

        private int ExecuteNonQuery(MySqlCommand command)
        {
            Interlocked.Increment(ref queriesRan);

            LogCommand(command);

            do
            {
                try
                {
                    return command.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    HandleGeneralException(e, command);
                    continue;
                }
            } while (true);
        }

        public static void HandleGeneralException(Exception e, MySqlCommand command)
        {
            var writer = new StringWriter();
            if (command != null)
            {
                writer.Write(command.CommandText);
                foreach (MySqlParameter param in command.Parameters)
                    writer.Write(param + "=" + param.Value + ",");
            }

            Global.DbLogger.Error("(" + Thread.CurrentThread.ManagedThreadId + ") " + writer, e);

            var mysqlException = e as MySqlException;
            if (mysqlException != null) {
                if (mysqlException.Number == 1213) {
                    Thread.Sleep(0);
                    return;
                }
            }

            Environment.Exit(-999);
        }
    }
}