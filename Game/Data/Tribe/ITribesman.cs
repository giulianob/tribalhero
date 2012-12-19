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
    }
}