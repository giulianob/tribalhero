#region

using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static bool TroopObjectCreate(City city, TroopStub stub, uint x, uint y) {
            if (!RemoveFromNormal(city.DefaultTroop, stub))
                return false;

            TroopObject troop = new TroopObject(stub) {
                                                          X = x,
                                                          Y = y + 1
                                                      };

            city.Troops.Add(stub);
            city.Add(troop);

            troop.BeginUpdate();
            Global.World.Add(troop);
            troop.EndUpdate();

            return true;
        }

        private static bool RemoveFromNormal(TroopStub source, TroopStub target) {
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