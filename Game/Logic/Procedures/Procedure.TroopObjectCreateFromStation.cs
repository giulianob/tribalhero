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
        public virtual bool TroopObjectCreateFromStation(ITroopStub stub, out ITroopObject troopObject)
        {
            if (stub.State != TroopState.Stationed)
            {
                troopObject = null;
                return false;
            }

            uint x = stub.Station.X;
            uint y = stub.Station.Y;

            if (!stub.Station.Troops.RemoveStationed(stub.StationTroopId))
            {
                troopObject = null;
                return false;
            }

            troopObject = new TroopObject(stub) {X = x, Y = y + 1};

            stub.City.Add(troopObject);

            troopObject.BeginUpdate();
            troopObject.Stats = new TroopStats(Formula.Current.GetTroopRadius(stub, null), Formula.Current.GetTroopSpeed(stub));
            World.Current.Regions.Add(troopObject);
            troopObject.EndUpdate();

            return true;
        }
    }
}