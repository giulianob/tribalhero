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
        public static bool TroopStubCreate(City city, TroopStub stub) {
            if (!RemoveFromNormal(city.DefaultTroop, stub))
                return false;

            city.Troops.Add(stub);
            return true;
        }

        public static bool TroopStubDelete(City city, TroopStub stub)
        {
            AddToNormal(stub, city.DefaultTroop);
            city.Troops.Remove(stub.TroopId);
            return true;
        }

        public static bool TroopObjectCreate(City city, TroopStub stub)
        {
            var troop = new TroopObject(stub) { X = city.X, Y = city.Y };
            city.Add(troop);

            troop.BeginUpdate();
            troop.Stats = new TroopStats(Formula.GetTroopRadius(stub, null), Formula.GetTroopSpeed(stub));
            Global.World.Add(troop);
            troop.EndUpdate();
            return true;
        }

        public static bool TroopObjectCreateFromCity(City city, TroopStub stub, uint x, uint y)
        {
            if (!RemoveFromNormal(city.DefaultTroop, stub))
                return false;

            var troop = new TroopObject(stub) {X = x, Y = y + 1};

            city.Troops.Add(stub);
            city.Add(troop);

            troop.BeginUpdate();
            troop.Stats = new TroopStats(Formula.GetTroopRadius(stub, null), Formula.GetTroopSpeed(stub));
            Global.World.Add(troop);
            troop.EndUpdate();

            return true;
        }

        private static bool RemoveFromNormal(TroopStub source, TroopStub target)
        {
            if (!target.HasFormation(FormationType.Normal) || !source.HasFormation(FormationType.Normal))
                return false;

            foreach (var unit in target[FormationType.Normal])
            {
                ushort count;
                if (!source[FormationType.Normal].TryGetValue(unit.Key, out count) || count < unit.Value)
                    return false;
            }            

            source.BeginUpdate();
            foreach (var unit in target[FormationType.Normal])
            {
                source[FormationType.Normal].Remove(unit.Key, unit.Value);
            }            
            source.EndUpdate();

            return true;
        }
    }
}