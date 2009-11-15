using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static bool TroopObjectCreateFromStation(TroopStub stub, uint x, uint y) {
            if (!stub.StationedCity.Troops.RemoveStationed(stub.StationedTroopId)) {
                return false;
            }

            TroopObject troop = new TroopObject(stub);
            troop.X = x;
            troop.Y = y + 1;

            stub.City.add(troop);
            Global.World.add(troop);
            return true;
        }
    }
}
