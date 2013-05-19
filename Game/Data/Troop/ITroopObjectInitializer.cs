using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Troop
{
    public interface ITroopObjectInitializer
    {
        bool GetTroopObject(out ITroopObject troopObject);
        void DeleteTroopObject(ITroopObject troopObject);
    }
}
