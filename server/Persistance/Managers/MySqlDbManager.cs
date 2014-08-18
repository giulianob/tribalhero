#region

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Common;
using MySql.Data.MySqlClient;

#endregion

namespace Persistance.Managers
{
    public class MySqlDbManager : IDbManager
    {
        [ThreadStatic]
        private static MySqlDbTransaction persistantTransaction;

        private readonly string connectionString;

        private readonly ConcurrentDictionary<Type, String> createCommands = new ConcurrentDictionary<Type, String>();

        private readonly ConcurrentDictionary<Type, String> createListCommands =
                new ConcurrentDictionary<Type, String>();

        private readonly ConcurrentDictionary<Type, String> deleteCommands = new ConcurrentDictionary<Type, String>();

        private readonly ConcurrentDictionary<Type, String> deleteListCommands =
                new ConcurrentDictionary<Type, String>();

        private readonly ILogger logger;

        private readonly ConcurrentDictionary<Type, String> saveCommands = new ConcurrentDictionary<Type, String>();

        private bool verbose;

        private DateTime lastProbe;

        private bool paused;

        private long queriesRan;

        private int openConnections;

        public MySqlDbManager(ILogger logger,
                              string hostname,
                              string username,
                              string password,
                              string database,
                              int timeout,
                              int maxConnections,
                              bool verbose)
        {
            connectionString =
                    string.Format(
                                  "Database={0};Host={1};User Id={2};Password={3};Connection Timeout={4};Default Command Timeout={4};Max Pool Size={5};Min Pool Size={5};CharSet=utf8;",
                                  database,
                                  hostname,
                                  username,
                                  password,
                                  timeout,
                                  maxConnections);
            this.logger = logger;
            this.verbose = verbose;
        }

        public void Close(DbConnection connection)
        {
            Interlocked.Decrement(ref openConnections);
            connection.Close();            
        }

        public void ClearThreadTransaction()
        {
            persistantTransaction = null;
        }

        public DbTransaction GetThreadTransaction(bool returnOnly = false)
        {
            if (returnOnly)
            {
                return persistantTransaction;
            }

            if (persistantTransaction != null)
            {
                persistantTransaction.ReferenceCount++;

                return persistantTransaction;
            }

            persistantTransaction = new MySqlDbTransaction(this, logger, verbose, null) {ReferenceCount = 1};

            return persistantTransaction;
        }

        public bool Save(params IPersistable[] objects)
        {
            if (paused)
            {
                return true;
            }

            InitPersistantTransaction();

            var mySqlTransaction = (persistantTransaction.Transaction as MySqlTransaction);

            if (mySqlTransaction == null)
            {
                throw new Exception("Invalid transaction specified");
            }

            if (objects.Length == 0)
            {
                return true;
            }

            foreach (var obj in objects)
            {
                MySqlCommand command;

                if (obj is IPersistableObject)
                {
                    var persistableObj = obj as IPersistableObject;
                    command = persistableObj.DbPersisted
                                      ? UpdateObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj)
                                      : CreateObject(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    if (command != null)
                    {
                        ExecuteNonQuery(command);
                    }

                    persistableObj.DbPersisted = true;
                }

                if (obj is IPersistableList)
                {
                    var persistableObj = obj as IPersistableList;

                    command = DeleteList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    if (command != null)
                    {
                        ExecuteNonQuery(command);
                    }

                    command = CreateList(mySqlTransaction.Connection, mySqlTransaction, persistableObj);

                    if (command != null)
                    {
                        ExecuteNonQuery(command);
                    }
                }

                Type objType = obj.GetType();
                foreach (var dependency in obj.DbDependencies.Where(dep => dep.AutoSave))
                {
                    Save((IPersistable)objType.GetProperty(dependency.Property).GetValue(obj, null));
                }
            }

            return true;
        }

        public bool Delete(params IPersistable[] objects)
        {
            if (paused)
            {
                return true;
            }

            InitPersistantTransaction();

            var mySqlTransaction = (persistantTransaction.Transaction as MySqlTransaction);
            if (mySqlTransaction == null)
            {
                throw new Exception("Invalid transaction specified");
            }

            if (objects.Length == 0)
            {
                return true;
            }

            foreach (var obj in objects)
            {
                if (obj is IPersistableObject && !(obj as IPersistableObject).DbPersisted)
                {
                    continue;
                }

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
                        {
                            Delete(persistable);
                        }
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
                {
                    continue;
                }

                PropertyInfo propInfo = objType.GetProperty(dependency.Property);
                var persistable = propInfo.GetValue(obj, null) as IPersistable;
                if (persistable != null)
                {
                    Delete(persistable);
                }
            }
        }

        public DbDataReader Select(string table)
        {
            return ReaderQuery(string.Format("SELECT * FROM `{0}`", table));
        }

        DbDataReader IDbManager.SelectList(string table)
        {
            return ReaderQuery(string.Format("SELECT * FROM `{0}_list`", table));
        }

        DbDataReader IDbManager.SelectList(IPersistableList obj)
        {
            bool startComma = false;
            string where = "";
            foreach (var column in obj.DbPrimaryKey)
            {
                if (startComma)
                {
                    where += " AND ";
                }
                else
                {
                    startComma = true;
                }

                where += string.Format("`{0}`=@{0}", column.Column);
            }

            return ReaderQuery(string.Format("SELECT * FROM `{0}_list` WHERE {1}", obj.DbTable, where), obj.DbPrimaryKey);
        }

        public void LogCommand(DbCommand command, bool onlyIfVerbose = true)
        {
            if (command == null || (onlyIfVerbose && !verbose))
            {
                return;
            }

            logger.Info("{0}", GetCommandTextForLogging(command));
        }

        private string GetCommandTextForLogging(DbCommand command)
        {
            var sqlwriter = new StringWriter();
            sqlwriter.Write("({0} {1} (", Thread.CurrentThread.ManagedThreadId, command.CommandText);
            foreach (MySqlParameter param in command.Parameters)
            {
                sqlwriter.Write("{0}={1},", param, ((param.Value != null) ? param.Value.ToString() : "NULL"));
            }
            sqlwriter.Write(")");

            return sqlwriter.ToString();
        }

        DbDataReader IDbManager.SelectList(string table, params DbColumn[] primaryKeyValues)
        {                        
            string where = "1=1";
            foreach (var column in primaryKeyValues)
            {
                where += string.Format(" AND `{0}`=@{0}", column.Column);
            }

            return ReaderQuery(string.Format("SELECT * FROM `{0}` WHERE {1}", table, where), primaryKeyValues);
        }

        public DbDataReader ReaderQuery(string query, DbColumn[] parms = null)
        {
            if (parms == null)
            {
                parms = new DbColumn[] {};
            }

            MySqlCommand command;

            // If we are inside of a transaction use it, otherwise run by itself.
            var inTransaction = persistantTransaction != null;

            if (inTransaction)
            {
                InitPersistantTransaction();
                command = ((MySqlTransaction)persistantTransaction.Transaction).Connection.CreateCommand();
                command.Connection = ((MySqlTransaction)persistantTransaction.Transaction).Connection;
                command.Transaction = (persistantTransaction.Transaction as MySqlTransaction);
            }
            else
            {
                MySqlConnection connection = GetConnection(false);            
                command = connection.CreateCommand();
                command.Connection = connection;                
            }
            
            command.CommandText = query;
            foreach (var parm in parms)
            {
                AddParameter(command, parm);
            }

            LogCommand(command);

            return command.ExecuteReader(inTransaction ? CommandBehavior.Default : CommandBehavior.CloseConnection);
        }

        public void Query(string query, params DbColumn[] parms)
        {
            if (paused)
            {
                return;
            }

            if (persistantTransaction == null)
            {
                throw new Exception("Calling transactional query outside of transactional block");
            }

            InitPersistantTransaction();
            MySqlCommand command = ((MySqlTransaction)persistantTransaction.Transaction).Connection.CreateCommand();
            command.Connection = ((MySqlTransaction)persistantTransaction.Transaction).Connection;
            command.Transaction = (persistantTransaction.Transaction as MySqlTransaction);

            command.CommandText = query;
            if (parms != null)
            {
                foreach (var parm in parms)
                {
                    AddParameter(command, parm);
                }
            }

            ExecuteNonQuery(command);
        }

        public void Rollback()
        {
            if (persistantTransaction == null)
            {
                throw new Exception("Not inside of a transaction");
            }

            if (verbose)
            {
                logger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction rollback");
            }

            persistantTransaction.Rollback();
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
            using (MySqlConnection connection = GetConnection(false))
            {

                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;
                command.CommandText = "SHOW TABLES";

                using (MySqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read())
                    {
                        if ((String)reader[0] == "schema_migrations")
                        {
                            continue;
                        }

                        using (MySqlConnection truncateConnection = GetConnection(false))
                        {
                            MySqlCommand truncateCommand = connection.CreateCommand();
                            truncateCommand.CommandText = string.Format("TRUNCATE TABLE `{0}`", reader[0]);
                            truncateCommand.Connection = truncateConnection;
                            truncateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void Probe(out int queriesRanOut, out DateTime lastProbeOut)
        {
            queriesRanOut = (int)Interlocked.Read(ref queriesRan);
            Interlocked.Exchange(ref queriesRan, 0);
            lastProbeOut = lastProbe;
            lastProbe = DateTime.UtcNow;
        }

        private MySqlConnection GetConnection(bool increaseOpenConnections = true)
        {
            var connection = new MySqlConnection(connectionString);
            
            try
            {
                var openTime = DateTime.UtcNow;

                connection.Open();

                var timeToOpen = DateTime.UtcNow.Subtract(openTime).TotalMilliseconds;
                if (timeToOpen > 1000)
                {
                    logger.Warn("Database slow connection. Took {0}ms to open a connection", DateTime.UtcNow.Subtract(openTime).TotalMilliseconds);
                }

                if (increaseOpenConnections)
                {
                    Interlocked.Increment(ref openConnections);
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "An exception was encountered while attempting to open the connection.");
                Environment.Exit(-1);
            }
            return connection;
        }

        private void InitPersistantTransaction()
        {
            if (persistantTransaction == null || persistantTransaction.Transaction != null)
            {
                return;
            }

            MySqlConnection connection = GetConnection();

            if (connection == null)
            {
                HandleGeneralException(new Exception("Did not get a connection"));
                return;
            }

            if (verbose)
            {
                logger.Info("(" + Thread.CurrentThread.ManagedThreadId + ") Transaction started");
            }

            try
            {
                persistantTransaction.Transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                if (persistantTransaction.Transaction == null)
                {
                    throw new NullReferenceException("Failed to initialize persistant transaction");
                }
            }
            catch(Exception e)
            {
                HandleGeneralException(e);
                return;
            }

            var command = new MySqlCommand("SET AUTOCOMMIT = 0",
                                           (persistantTransaction.Transaction as MySqlTransaction).Connection,
                                           (persistantTransaction.Transaction as MySqlTransaction));
            ExecuteNonQuery(command);
        }

        private MySqlCommand UpdateObject(MySqlConnection connection,
                                          MySqlTransaction transaction,
                                          IPersistableObject obj)
        {
            string commandText;

            DbColumn[] columns = obj.DbColumns;
            if (columns.Length == 0)
            {
                return null;
            }

            if (!saveCommands.TryGetValue(obj.GetType(), out commandText))
            {
                bool startComma = false;
                string values = "";
                foreach (var column in obj.DbColumns)
                {
                    if (startComma)
                    {
                        values += ", ";
                    }
                    else
                    {
                        startComma = true;
                    }

                    values += string.Format("`{0}`=@{0}", column.Column);
                }

                startComma = false;
                string where = "";
                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                    {
                        where += " AND ";
                    }
                    else
                    {
                        startComma = true;
                    }

                    where += string.Format("{0}=@{0}", column.Column);
                }

                commandText = string.Format("UPDATE `{0}` SET {1} WHERE {2} LIMIT 1", obj.DbTable, values, where);

                saveCommands[obj.GetType()] = commandText;
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            foreach (var column in columns)
            {
                AddParameter(command, column);
            }

            foreach (var column in obj.DbPrimaryKey)
            {
                AddParameter(command, column);
            }

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
                    {
                        columns += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                }

                foreach (var column in obj.DbListColumns)
                {
                    if (startComma)
                    {
                        columns += ",";
                    }
                    startComma = true;
                    columns += string.Format("`{0}`", column.Column);
                }

                commandText = string.Format("INSERT INTO `{0}_list` ({1}) VALUES ", obj.DbTable, columns);

                createListCommands[obj.GetType()] = commandText;
            }

            var builder = new StringBuilder(commandText);

            bool startColumnsComma = false;
            bool empty = true;
            int row = 0;

            MySqlCommand command = connection.CreateCommand();
            
            foreach (var columns in obj.DbListValues())
            {
                empty = false;

                if (startColumnsComma)
                {
                    builder.Append(", (");
                }
                else
                {
                    builder.Append("(");
                    startColumnsComma = true;
                }

                bool startComma = false;

                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        startComma = true;
                    }

                    var param = string.Format("@{0}__{1}", column.Column, row);
                    builder.Append(param);                    
                    command.Parameters.AddWithValue(param, column.Value);
                }

                foreach (var column in columns)
                {
                    if (startComma)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        startComma = true;
                    }

                    var param = string.Format("@{0}__{1}", column.Column, row);
                    builder.Append(param);
                    command.Parameters.AddWithValue(param, column.Value);                    
                }
                builder.Append(")");

                row++;
            }

            if (empty)
            {
                return null;
            }

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandText = builder.ToString();

            return command;
        }

        private MySqlCommand CreateObject(MySqlConnection connection,
                                          MySqlTransaction transaction,
                                          IPersistableObject obj)
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
            {
                command.Transaction = transaction;
            }

            foreach (var column in obj.DbColumns)
            {
                AddParameter(command, column);
            }

            foreach (var column in obj.DbPrimaryKey)
            {
                AddParameter(command, column);
            }

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
                    {
                        where += " AND ";
                    }
                    else
                    {
                        startComma = true;
                    }

                    where += string.Format("`{0}`=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}_list` WHERE {1}", obj.DbTable, where);

                deleteListCommands[obj.GetType()] = commandText;
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            foreach (var column in obj.DbPrimaryKey)
            {
                AddParameter(command, column, DataRowVersion.Original);
            }

            return command;
        }

        private MySqlCommand DeleteObject(MySqlConnection connection,
                                          MySqlTransaction transaction,
                                          IPersistableObject obj)
        {
            string commandText;

            if (!deleteCommands.TryGetValue(obj.GetType(), out commandText))
            {
                bool startComma = false;
                string where = "";
                foreach (var column in obj.DbPrimaryKey)
                {
                    if (startComma)
                    {
                        where += " AND ";
                    }
                    else
                    {
                        startComma = true;
                    }

                    where += string.Format("`{0}`=@{0}", column.Column);
                }

                commandText = string.Format("DELETE FROM `{0}` WHERE {1} LIMIT 1", obj.DbTable, where);

                deleteCommands[obj.GetType()] = commandText;
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            foreach (var column in obj.DbPrimaryKey)
            {
                AddParameter(command, column, DataRowVersion.Original);
            }

            return command;
        }

        private static void AddParameter(MySqlCommand command,
                                         DbColumn column,
                                         DataRowVersion sourceVersion = DataRowVersion.Default)
        {
            MySqlParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@" + column.Column;

            if (column.Type != DbType.Object)
            {
                parameter.DbType = column.Type;
            }

            if (column.Size > 0)
            {
                parameter.Size = column.Size;
            }

            parameter.SourceVersion = sourceVersion;
            parameter.Value = column.Value;

            command.Parameters.Add(parameter);
        }

        private int ExecuteNonQuery(MySqlCommand command)
        {
            // ReSharper disable once RedundantAssignment
            int affectedRows = 0;

            Interlocked.Increment(ref queriesRan);

            LogCommand(command);

            do
            {
                try
                {
                    affectedRows = command.ExecuteNonQuery();
                    break;
                }
                catch(MySqlException e)
                {
                    switch(e.Number)
                    {
                        case 1213:
                            Thread.Sleep(0);
                            if (command.Transaction == persistantTransaction.Transaction)
                            {
                                persistantTransaction.RerunTransaction();
                            }
                            break;
                        case 1215:
                            Thread.Sleep(0);
                            continue;
                        default:
                            HandleGeneralException(e, command);
                            break;
                    }
                }
                catch(Exception e)
                {
                    HandleGeneralException(e, command);
                }
            }
            while (true);

            if (command.Transaction != null && command.Transaction == persistantTransaction.Transaction)
            {
                persistantTransaction.Commands.Add(command);
            }

            return affectedRows;
        }

        internal void HandleGeneralException(Exception e, DbCommand command = null)
        {
            logger.ErrorException(string.Format("General database error on thread {0} {1}", Thread.CurrentThread.ManagedThreadId, Environment.StackTrace), e);

            if (command != null)
            {
                logger.Error("Command that failed {0}", GetCommandTextForLogging(command));
            }

            throw e;
        }
    }
}