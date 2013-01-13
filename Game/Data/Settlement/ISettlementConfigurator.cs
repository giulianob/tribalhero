using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Settlement
{
    public interface ISettlementConfigurator
    {
        bool Next(out string name, out byte level, out uint x, out uint y);
    }
}
