using System;
using System.Collections.Generic;
using Game.Data.Tribe.EventArguments;
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

        string PublicDescription { get; set; }

        Resource Resource { get; }

        short AssignmentCount { get; }

        IEnumerable<Assignment> Assignments { get; }

        IEnumerable<ITribesman> Tribesmen { get; }

        IEnumerable<ITribeRank> Ranks { get; }

        List<LeavingTribesmate> LeavingTribesmates { get; }

        ITribeRank DefaultRank { get; }

        ITribeRank ChiefRank { get; }

        int Count { get; }

        DateTime Created { get; }

        event EventHandler<TribesmanRemovedEventArgs> TribesmanRemoved;

        bool IsOwner(IPlayer player);

        void DbLoaderAdd(ITribesman tribesman);

        Error AddTribesman(ITribesman tribesman, bool ignoreRequirements = false);

        Error KickTribesman(IPlayer player, IPlayer kicker);

        Error LeaveTribesman(IPlayer player);

        Error RemoveTribesman(uint playerId, bool wasKicked, bool checkIfOwner = true);

        void CreateRank(byte id, string name, TribePermission permission);

        Error SetRank(uint playerId, byte rank);

        Error UpdateRank(byte rank, string name, TribePermission permission);

        Error Contribute(uint playerId, Resource resource);

        bool HasRight(uint playerId, TribePermission permission);

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

        void SendRanksUpdate();

        event EventHandler<EventArgs> RanksUpdated;

        event EventHandler<TribesmanEventArgs> TribesmanJoined;

        event EventHandler<TribesmanEventArgs> TribesmanLeft;

        event EventHandler<TribesmanKickedEventArgs> TribesmanKicked;

        event EventHandler<TribesmanContributedEventArgs> TribesmanContributed;

        event EventHandler<TribesmanEventArgs> TribesmanRankChanged;

        event EventHandler<EventArgs> Upgraded;

    }

}