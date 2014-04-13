#region

using System.Collections.Generic;

#endregion

namespace Persistance
{
    public interface IPersistable
    {
        IEnumerable<DbDependency> DbDependencies { get; }
    }

    public interface IPersistableObject : IPersistable
    {
        bool DbPersisted { get; set; }

        string DbTable { get; }

        DbColumn[] DbPrimaryKey { get; }

        DbColumn[] DbColumns { get; }
    }

    public interface IPersistableList : IPersistableObject
    {
        /// <summary>
        ///     Meta data of list columns. No values needed since they will be fetched in the <see cref="DbListValues" />.
        /// </summary>
        IEnumerable<DbColumn> DbListColumns { get; }

        /// <summary>
        ///     The values for each list item
        /// </summary>
        /// <returns></returns>
        IEnumerable<DbColumn[]> DbListValues();
    }
}