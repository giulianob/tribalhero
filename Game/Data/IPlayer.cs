using System;
using System.Collections.Generic;
using Game.Comm;
using Game.Data.Tribe;
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

        PlayerRights Rights { get; set; }

        DateTime ChatFloodTime { get; set; }

        DateTime ChatLastMessage { get; set; }

        bool IsIdle { get; }

        int ChatFloodCount { get; set; }

        string Description { get; set; }

        ITribesman Tribesman { get; set; }

        uint TribeRequest { get; set; }

        int AttackPoint { get; }

        int DefensePoint { get; }

        int Value { get; }

        bool IsLoggedIn { get; }

        bool Muted { get; set; }

        bool IsInTribe { get; }

        void Add(ICity city);

        int GetCityCount(bool includeDeleted = false);

        IEnumerable<ICity> GetCityList(bool includeDeleted = false);

        ICity GetCity(uint id);

        string ToString();

        void SendSystemMessage(IPlayer from, String subject, String message);

        void TribeUpdate();
    }
}