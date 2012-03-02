#region

using System.Collections.Generic;

#endregion

namespace Persistance
{
    public interface IPersistable
    {
        string DbTable { get; }
        DbColumn[] DbPrimaryKey { get; }
        DbDependency[] DbDependencies { get; }
        DbColumn[] DbColumns { get; }
        //list of values. For IPersistableList this is the "header". Should contain values.
    }

    public interface IPersistableObject : IPersistable
    {
        bool DbPersisted { get; set; }
    }

    public interface IPersistableList : IPersistableObject
    {
        DbColumn[] DbListColumns { get; } //Meta data of list columns. No value.

        IEnumerable<DbColumn[]> DbListValues();
    }
}