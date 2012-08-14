using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Tribe;
using Game.Data.Troop;

namespace Game.Data.Stronghold
{
    public interface IStrongholdManager : IEnumerable<IStronghold>
    {
        int Count { get; }
        void Add(IStronghold stronghold);
        void DbLoaderAdd(IStronghold stronghold);
        bool TryGetValue(uint id, out IStronghold stronghold);
        void Generate(int count);

        void Activate(IStronghold stronghold);
        void TransferTo(IStronghold stronghold, ITribe tribe);
    }
}
