using System;
using System.Collections.Generic;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Tribe
{
    public interface ITribe : ILockable, IPersistableObject
    {
        uint Id { get; }

        IPlayer Owner { get; }

        string Name { get; set; }

        byte Level { get; set; }

        int AttackPoint { get; set; }

        int DefensePoint { get; set; }

        string Description { get; set; }

        Resource Resource { get; }

        short AssignmentCount { get; }

        IEnumerable<Assignment> Assignments { get; }

        IEnumerable<ITribesman> Tribesmen { get; }

        int Count { get; }

        bool IsOwner(IPlayer player);

        Error AddTribesman(ITribesman tribesman, bool save = true);

        Error RemoveTribesman(uint playerId);

        bool TryGetTribesman(uint playerId, out ITribesman tribesman);

        Error SetRank(uint playerId, byte rank);

        Error Contribute(uint playerId, Resource resource);

        IEnumerable<Tribe.IncomingListItem> GetIncomingList();

        bool HasRight(uint playerId, string action);

        Error CreateAssignment(ITroopStub stub, uint x, uint y, ICity targetCity, DateTime time, AttackMode mode, string description, out int id);

        Error JoinAssignment(int id, ITroopStub stub);

        void RemoveAssignment(Assignment assignment);

        void DbLoaderAddAssignment(Assignment assignment);

        void Upgrade();

        void SendUpdate();
    }
}