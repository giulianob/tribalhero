using System;
using System.Collections.Generic;
using Game.Data.Tribe;
using Game.Data.Tribe.EventArguments;
using Game.Data.Troop;
using Game.Setup;

namespace Game.Data.Stronghold
{
    public interface IStrongholdManager : IEnumerable<IStronghold>
    {
        int Count { get; }

        void DbLoaderAdd(IStronghold stronghold);

        bool TryGetStronghold(uint id, out IStronghold stronghold);

        bool TryGetStronghold(string name, out IStronghold stronghold);

        void Generate(int count);

        void Activate(IStronghold stronghold);

        void TransferTo(IStronghold stronghold, ITribe tribe);

        IEnumerable<Unit> GenerateNeutralStub(IStronghold stronghold);

        IEnumerable<IStronghold> StrongholdsForTribe(ITribe tribe);

        void RemoveStrongholdsFromTribe(ITribe tribe);

        Error RepairGate(IStronghold stronghold);

        Error UpdateGate(IStronghold stronghold);

        IEnumerable<IStronghold> OpenStrongholdsForTribe(ITribe tribe);

        void Probe(out int neutralStrongholds, out int capturedStrongholds);

        event EventHandler<StrongholdGainedEventArgs> StrongholdGained;

        event EventHandler<StrongholdLostEventArgs> StrongholdLost;

        void TribeFailedToTakeStronghold(IStronghold stronghold, ITribe attackingTribe);
    }
}