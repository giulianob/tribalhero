#region

using System.Collections.Generic;
using System.Linq;

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
        public virtual bool TroopStubCreate(ICity city, ITroopStub stub, TroopState initialState = TroopState.Idle, params FormationType[] formations) {
            if (!RemoveFromNormal(city.DefaultTroop, stub, formations))
            {
                return false;
            }

            stub.State = initialState;
            stub.City = city;
            city.Troops.Add(stub);

            dbManager.Save(stub);
            
            return true;
        }

        public virtual void TroopStubDelete(ICity city, ITroopStub stub)
        {
            AddToNormal(stub, city.DefaultTroop);
            city.Troops.Remove(stub.TroopId);
        }

        public virtual void TroopObjectCreate(ICity city, ITroopStub stub)
        {
            var troop = new TroopObject(stub) { X = city.X, Y = city.Y };
            city.Add(troop);

            troop.BeginUpdate();
            troop.Stats = new TroopStats(Formula.Current.GetTroopRadius(stub, null), Formula.Current.GetTroopSpeed(stub));
            World.Current.Add(troop);
            troop.EndUpdate();            
        }

        public virtual bool TroopObjectCreateFromCity(ICity city, TroopStub stub, uint x, uint y)
        {
            if (stub.TotalCount == 0 || !RemoveFromNormal(city.DefaultTroop, stub))
                return false;

            stub.City = city;
            var troop = new TroopObject(stub) {X = x, Y = y + 1};

            city.Troops.Add(stub);
            city.Add(troop);

            troop.BeginUpdate();
            troop.Stats = new TroopStats(Formula.Current.GetTroopRadius(stub, null), Formula.Current.GetTroopSpeed(stub));
            World.Current.Add(troop);
            troop.EndUpdate();

            return true;
        }

        private bool RemoveFromNormal(ITroopStub source, IEnumerable<Formation> unitsToRemove, params FormationType[] formations)
        {
            if (!source.HasFormation(FormationType.Normal))
            {
                return false;
            }

            var acceptableUnits = unitsToRemove.Where(formation => formations == null || formations.Length == 0 || formations.Contains(formation.Type)).ToList();

            // Make sure there are enough units
            foreach (var unit in acceptableUnits.SelectMany(formation => formation))
            {
                ushort count;
                if (!source[FormationType.Normal].TryGetValue(unit.Key, out count) || count < unit.Value)
                {
                    return false;
                }
            }

            // Remove them, shouldnt fail since we've already checked
            source.BeginUpdate();
            foreach (var unit in acceptableUnits.SelectMany(formation => formation))
            {
                source[FormationType.Normal].Remove(unit.Key, unit.Value);
            }
            source.EndUpdate();

            return true;
        }
    }
}