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
        /// <summary>
        /// Creates a troop object from the specified stationed troop and removes the stub from being stationed.
        /// NOTE: This will fail if the stub is in a state besides Stationed (e.g. troop is in battle)
        /// </summary>
        /// <returns></returns>
        public virtual bool TroopObjectCreateFromStation(ITroopStub stub)
        {
            if (stub.State != TroopState.Stationed)
            {
                return false;
            }

            uint x = stub.StationedCity.X;
            uint y = stub.StationedCity.Y;

            if (!stub.StationedCity.Troops.RemoveStationed(stub.StationedTroopId))
            {
                return false;
            }

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