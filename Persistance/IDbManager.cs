#region

using System;
using System.Data.Common;

#endregion

namespace Persistance
{
    public interface IDbManager
    {
        void Pause();

        void Resume();

        void Close(DbConnection connection);

        void LogCommand(DbCommand command, bool onlyIfVerbose = true);

        DbTransaction GetThreadTransaction(bool returnOnly = false);

        void ClearThreadTransaction();

        bool Save(params IPersistable[] objects);

        bool Delete(params IPersistable[] objects);

        void DeleteDependencies(IPersistable obj);

        void Rollback();

        DbDataReader Select(string table);

        DbDataReader SelectList(IPersistableList obj);

        DbDataReader SelectList(string table, params DbColumn[] primaryKeyValues);

        void Query(string query, params DbColumn[] parms);

        DbDataReader ReaderQuery(string query, DbColumn[] parms = null);

        void EmptyDatabase();

        void Probe(out int queriesRanOut, out DateTime lastProbeOut);

        DbDataReader SelectList(string table);
    }
}