using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Tribe;

namespace Game.Data.Stronghold
{
    interface IStrongholdManager : IEnumerable<IStronghold>
    {
        int Count { get; }
        void Add(IStronghold stronghold);
        bool TryGetValue(uint id, out IStronghold stronghold);
        void Generate(int count);

        void Activate(IStronghold stronghold);
        void TransferTo(IStronghold stronghold, ITribe tribe);
    }
}
