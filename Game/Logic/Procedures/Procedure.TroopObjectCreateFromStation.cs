#region

using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual bool TroopObjectCreateFromStation(TroopStub stub, uint x, uint y)
        {
            if (!stub.StationedCity.Troops.RemoveStationed(stub.StationedTroopId))
                return false;

            var troop = new TroopObject(stub) {X = x, Y = y + 1};

            stub.City.Add(troop);

            troop.BeginUpdate();
            troop.Stats = new TroopStats(Formula.Current.GetTroopRadius(stub, null), Formula.Current.GetTroopSpeed(stub));
            World.Current.Add(troop);
            troop.EndUpdate();

            return true;
        }
    }
}