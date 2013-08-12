#region

using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Formulas;

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

        public virtual void TroopStubDelete(ICity city, ITroopStub stub)
        {
            AddToNormal(stub, city.DefaultTroop);
            city.Troops.Remove(stub.TroopId);
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