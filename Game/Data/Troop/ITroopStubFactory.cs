using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Troop
{
    public interface ITroopStubFactory
    {
        TroopStub CreateTroopStub(byte troopId);
    }
}
