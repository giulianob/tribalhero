using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Stronghold
{
    public interface IStrongholdFactory
    {
        IStronghold CreateStronghold(uint id, string name, byte level, uint x, uint y);
    }
}
