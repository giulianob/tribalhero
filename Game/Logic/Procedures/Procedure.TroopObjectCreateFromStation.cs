#region

using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        /// <summary>
        ///     Creates a troop object from the specified stationed troop and removes the stub from being stationed.
        ///     NOTE: This will fail if the stub is in a state besides Stationed (e.g. troop is in battle)
        /// </summary>
        /// <returns></returns>
        public virtual bool TroopObjectCreateFromStation(ITroopStub stub, out ITroopObject troopObject)
        {
            if (stub.State != TroopState.Stationed)
            {
                troopObject = null;
                return false;
            }

            uint x = stub.Station.PrimaryPosition.X;
            uint y = stub.Station.PrimaryPosition.Y;

            if (!stub.Station.Troops.RemoveStationed(stub.StationTroopId))
            {
                troopObject = null;
                return false;
            }

            troopObject = stub.City.CreateTroopObject(stub, x, y + 1);

            troopObject.BeginUpdate();
            troopObject.Stats = new TroopStats(formula.GetTroopRadius(stub, null),
                                               formula.GetTroopSpeed(stub));
            world.Regions.Add(troopObject);
            troopObject.EndUpdate();

            return true;
        }
    }
}