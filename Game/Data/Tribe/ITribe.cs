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
        uint Id { get; set; }

        IPlayer Owner { get; }

        string Name { get; set; }

        byte Level { get; }

        int AttackPoint { get; set; }

        int DefensePoint { get; set; }

        decimal VictoryPoint { get; set; }

        string Description { get; set; }

        Resource Resource { get; }

        short AssignmentCount { get; }

        IEnumerable<Assignment> Assignments { get; }

        IEnumerable<ITribesman> Tribesmen { get; }

        int Count { get; }

        DateTime Created { get; }

        event EventHandler<TribesmanRemovedEventArgs> TribesmanRemoved;

        bool IsOwner(IPlayer player);

        Error AddTribesman(ITribesman tribesman, bool save = true);

        Error RemoveTribesman(uint playerId, bool wasKicked, bool checkIfOwner = true);

        Error SetRank(uint playerId, byte rank);

        Error Contribute(uint playerId, Resource resource);

        bool HasRight(uint playerId, string action);

        Error CreateAssignment(ICity city,
                               ISimpleStub stub,
                               uint x,
                               uint y,
                               ILocation target,
                               DateTime time,
                               AttackMode mode,
                               string description,
                               bool isAttack,
                               out int id);

        Error JoinAssignment(int id, ICity city, ISimpleStub stub);

        Error Transfer(uint newOwnerPlayerId);

        void DbLoaderAddAssignment(Assignment assignment);

        Error Upgrade();

        void SendUpdate();

        event EventHandler<EventArgs> Updated;
    }
}