using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Settlement
{
    public interface ISettlementFactory
    {
        Settlement CreateSettlement(uint id, string name, byte level, uint x, uint y, int count);
    }
}
