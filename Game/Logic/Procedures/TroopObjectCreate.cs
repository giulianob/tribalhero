#region

using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
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
            foreach (Formation formation in target) {
                foreach (KeyValuePair<ushort, ushort> unit in formation) {
                    ushort count;
                    if (!source[FormationType.NORMAL].TryGetValue(unit.Key, out count))
                        return false;
                    if (count < unit.Value)
                        return false;
                }
            }

            source.BeginUpdate();
            foreach (Formation formation in target) {
                foreach (KeyValuePair<ushort, ushort> unit in formation) {
                    if (source[FormationType.NORMAL].Remove(unit.Key, unit.Value) != unit.Value)
                        return false;
                }
            }
            source.EndUpdate();

            return true;
        }
    }
}