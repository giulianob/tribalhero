using System.Collections.Generic;
using Game.Data.Tribe;
using Game.Data.Troop;

namespace Game.Data.Stronghold
{
    public interface IStrongholdManager : IEnumerable<IStronghold>
    {
        int Count { get; }
        void Add(IStronghold stronghold);
        void DbLoaderAdd(IStronghold stronghold);
        bool TryGetStronghold(uint id, out IStronghold stronghold);
        bool TryGetStronghold(string name, out IStronghold stronghold);
        void Generate(int count);

        void Activate(IStronghold stronghold);
        void TransferTo(IStronghold stronghold, ITribe tribe);
        IEnumerable<Unit> GenerateNeutralStub(IStronghold stronghold);
    }
}
