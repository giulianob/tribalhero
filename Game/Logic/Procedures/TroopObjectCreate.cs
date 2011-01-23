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
        public static bool TroopObjectCreate(City city, TroopStub stub, uint x, uint y)
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
            foreach (var formation in target)
            {
                foreach (var unit in formation)
                {
                    ushort count;
                    if (!source[FormationType.Normal].TryGetValue(unit.Key, out count))
                        return false;
                    if (count < unit.Value)
                        return false;
                }
            }

            source.BeginUpdate();
            foreach (var formation in target)
            {
                foreach (var unit in formation)
                {
                    if (source[FormationType.Normal].Remove(unit.Key, unit.Value) != unit.Value)
                        return false;
                }
            }
            source.EndUpdate();

            return true;
        }
    }
}