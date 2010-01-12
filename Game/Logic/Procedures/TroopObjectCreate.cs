#region

using System.Collections.Generic;
using Game.Data;
using Game.Fighting;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static bool TroopObjectCreate(City city, TroopStub stub, uint x, uint y) {
            if (!remove_from_normal(city.DefaultTroop, stub))
                return false;

            TroopObject troop = new TroopObject(stub);
            troop.X = x;
            troop.Y = y + 1;

            city.Troops.Add(stub);
            city.Add(troop);

            Global.World.Add(troop);

            return true;
        }

        private static bool remove_from_normal(TroopStub source, TroopStub target) {
            foreach (
                KeyValuePair<FormationType, Formation> kvp in
                    (IEnumerable<KeyValuePair<FormationType, Formation>>) target) {
                foreach (KeyValuePair<ushort, ushort> unit in kvp.Value) {
                    ushort count;
                    if (!source[FormationType.Normal].TryGetValue(unit.Key, out count))
                        return false;
                    if (count < unit.Value)
                        return false;
                }
            }

            source.BeginUpdate();
            foreach (
                KeyValuePair<FormationType, Formation> kvp in
                    (IEnumerable<KeyValuePair<FormationType, Formation>>) target) {
                foreach (KeyValuePair<ushort, ushort> unit in kvp.Value) {
                    if (source[FormationType.Normal].remove(unit.Key, unit.Value) != unit.Value)
                        return false;
                }
            }
            source.EndUpdate();

            return true;
        }
    }
}