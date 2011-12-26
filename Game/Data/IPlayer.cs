using System;
using System.Collections.Generic;
using Game.Comm;
using Game.Util.Locking;
using Persistance;

namespace Game.Data
{
    public interface IPlayer : ILockable, IPersistableObject
    {
        Session Session { get; set; }

        string Name { get; set; }

        uint PlayerId { get; }

        DateTime Created { get; }

        DateTime LastLogin { get; set; }

        string SessionId { get; set; }

        bool Admin { get; set; }

        DateTime ChatFloodTime { get; set; }

        int ChatFloodCount { get; set; }

        string Description { get; set; }

        Tribe.ITribesman Tribesman { get; set; }

        uint TribeRequest { get; set; }

        int AttackPoint { get; }

        int DefensePoint { get; }

        bool IsLoggedIn { get; }

        void Add(ICity city);

        int GetCityCount(bool includeDeleted = false);

        IEnumerable<ICity> GetCityList(bool includeDeleted = false);

        ICity GetCity(uint id);

        string ToString();

        void SendSystemMessage(IPlayer from, String subject, String message);

        void TribeUpdate();
    }
}