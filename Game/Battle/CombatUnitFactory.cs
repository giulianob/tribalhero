using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Fighting;
using Game.Data.Stats;

namespace Game.Battle {
    public class CombatUnitFactory {
        public static AttackCombatUnit[] CreateAttackCombatUnit(BattleManager owner, TroopObject troop, FormationType formation, ushort type, ushort count) {
            BaseUnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.TroopTemplate[type];
            AttackCombatUnit[] units = new AttackCombatUnit[(count - 1) / stats.Base.GroupSize + 1];
            AttackCombatUnit newUnit;
            int i = 0;
            do {
                ushort size = stats.Base.GroupSize > count ? count : stats.Base.GroupSize;
                newUnit = new AttackCombatUnit(owner, troop.Stub, formation, type, template.Lvl, size);
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

        public static DefenseCombatUnit[] CreateDefenseCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, ushort count) {
            BaseUnitStats template = stub.City.Template[type];
            BattleStats stats = stub.TroopTemplate[type];
            DefenseCombatUnit[] units = new DefenseCombatUnit[(count - 1) / stats.Base.GroupSize + 1];
            DefenseCombatUnit newUnit;
            int i = 0;
            do {
                ushort size = stats.Base.GroupSize > count ? count : stats.Base.GroupSize;
                newUnit = new DefenseCombatUnit(owner, stub, formation, type, template.Lvl, size);
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

    }
}
