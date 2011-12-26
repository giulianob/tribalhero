using System;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Tribe
{
    public interface ITribesman : IPersistableObject, ILockable
    {
        ITribe Tribe { get; }

        IPlayer Player { get; }

        DateTime JoinDate { get; }

        Resource Contribution { get; set; }

        byte Rank { get; set; }

        bool DbPersisted { get; set; }

        string DbTable { get; }

        DbColumn[] DbPrimaryKey { get; }

        DbDependency[] DbDependencies { get; }

        DbColumn[] DbColumns { get; }

        int Hash { get; }

        object Lock { get; }
    }
}