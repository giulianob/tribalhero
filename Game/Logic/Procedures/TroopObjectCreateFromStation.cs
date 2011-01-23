#region

using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public static bool TroopObjectCreateFromStation(TroopStub stub, uint x, uint y)
        {
            if (!stub.StationedCity.Troops.RemoveStationed(stub.StationedTroopId))
                return false;

            var troop = new TroopObject(stub) {X = x, Y = y + 1};

            stub.City.Add(troop);

            troop.BeginUpdate();
            troop.Stats = new TroopStats(Formula.GetTroopRadius(stub, null), Formula.GetTroopSpeed(stub));
            Global.World.Add(troop);
            troop.EndUpdate();

            return true;
        }
    }
}