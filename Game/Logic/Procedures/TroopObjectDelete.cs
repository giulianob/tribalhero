using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static bool TroopObjectDelete(TroopObject troop) {
            return TroopObjectDelete(troop, true);
        }

        public static bool TroopObjectDelete(TroopObject troop, bool addBackToNormal) {
            if (addBackToNormal) {
                addToNormal(troop.Stub, troop.City.DefaultTroop);

                troop.City.BeginUpdate();
                troop.City.Resource.Add(troop.Stats.Loot);
                troop.City.EndUpdate();
            }

            troop.City.Troops.Remove(troop.Stub.TroopId);
            Global.World.remove(troop);
            troop.City.remove(troop);

            return true;
        }

        private static bool addToNormal(TroopStub source, TroopStub target) {
            target.BeginUpdate();
            foreach (KeyValuePair<FormationType, Formation> kvp in (IEnumerable<KeyValuePair<FormationType, Formation>>)source) {
                foreach (KeyValuePair<ushort, ushort> unit in kvp.Value) {
                    target.addUnit(FormationType.Normal, unit.Key, unit.Value);
                }
            }
            target.EndUpdate();

            return true;
        }
    }
}
