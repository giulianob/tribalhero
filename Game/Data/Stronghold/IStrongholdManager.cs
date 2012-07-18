using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Stronghold
{
    interface IStrongholdManager : IEnumerable<IStronghold>
    {
        int Count { get; }
        void Add(IStronghold stronghold);
        void Generate(int count);
    }
}
