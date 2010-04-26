#region

using Game.Data;
using Game.Data.Troop;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static bool TroopObjectCreateFromStation(TroopStub stub, uint x, uint y) {
            if (!stub.StationedCity.Troops.RemoveStationed(stub.StationedTroopId))
                return false;

            TroopObject troop = new TroopObject(stub);
            troop.X = x;
            troop.Y = y + 1;

            stub.City.Add(troop);
            Global.World.Add(troop);
            return true;
        }
    }
}