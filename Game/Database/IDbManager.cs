#region

using System;
using System.Data.Common;

#endregion

namespace Game.Database
{
    public interface IDbManager
    {
        void Pause();
        void Resume();

        void Close(DbConnection connection);

        DbTransaction GetThreadTransaction();
        void ClearThreadTransaction();

        bool Save(params IPersistable[] objects);
        bool Delete(params IPersistable[] objects);
        void DeleteDependencies(IPersistable obj);

        void Rollback();

        DbDataReader Select(string table);
        DbDataReader SelectList(IPersistableList obj);
        DbDataReader SelectList(string table, params DbColumn[] primaryKeyValues);

        int Query(string query, DbColumn[] parms);
        DbDataReader ReaderQuery(string query, DbColumn[] parms);
        uint LastInsertId();

        void EmptyDatabase();

        void Probe(out int queriesRan, out DateTime lastProbe);
    }
}