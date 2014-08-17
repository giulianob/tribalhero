#region

using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual bool TroopStubCreate(out ITroopStub troopStub,
                                            ICity city,
                                            ISimpleStub stub,
                                            TroopState initialState = TroopState.Idle)
        {
            if (!city.DefaultTroop.RemoveFromFormation(FormationType.Normal, stub))
            {
                troopStub = null;
                return false;
            }

            troopStub = city.CreateTroopStub();
            troopStub.BeginUpdate();
            troopStub.Add(stub);
            troopStub.State = initialState;
            troopStub.EndUpdate();
            return true;
        }

        public virtual Error TroopStubDelete(ICity city, ITroopStub stub)
        {
            if (city.TroopObjects.Any(t => t.Stub == stub))
            {
                return Error.TroopObjectExistsForStub;
            }

            AddToNormal(stub, city.DefaultTroop);
            city.Troops.Remove(stub.TroopId);

            return Error.Ok;
        }

        public virtual void TroopObjectCreate(ICity city, ITroopStub stub, out ITroopObject troopObject)
        {
            troopObject = city.CreateTroopObject(stub, city.PrimaryPosition.X, city.PrimaryPosition.Y);

            troopObject.BeginUpdate();
            troopObject.Stats = new TroopStats(formula.GetTroopRadius(stub, null), formula.GetTroopSpeed(stub));
            world.Regions.Add(troopObject);
            troopObject.EndUpdate();
        }
    }
}