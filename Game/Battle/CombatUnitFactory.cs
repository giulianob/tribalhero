using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Fighting;

namespace Game.Battle {
    public class CombatUnitFactory {
        public static AttackCombatUnit[] CreateAttackCombatUnit(BattleManager owner, TroopObject troop, FormationType formation, ushort type, ushort count) {
            UnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.TroopTemplate[type];
            AttackCombatUnit[] units = new AttackCombatUnit[(count - 1) / stats.GroupSize + 1];
            AttackCombatUnit newUnit;
            int i = 0;
            do {
                ushort size = stats.GroupSize > count ? count : stats.GroupSize;
                newUnit = new AttackCombatUnit(owner, troop.Stub, formation, type, template.lvl, size);
               /* switch (formation) {
                    case FormationType.Attack:
                        newUnit.Stats.Atk = (byte)(newUnit.Stats.Atk * 0.5);
                        break;
                    case FormationType.Defense:
                        newUnit.Stats.Def = (byte)(newUnit.Stats.Def * 0.5);
                        break;
                    case FormationType.Scout:
                        newUnit.Stats.ModVision = 10;
                        break;
                }*/
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

        public static DefenseCombatUnit[] CreateDefenseCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, ushort count) {
            UnitStats template = stub.City.Template[type];
            BattleStats stats = stub.TroopTemplate[type];
            DefenseCombatUnit[] units = new DefenseCombatUnit[(count - 1) / stats.GroupSize + 1];
            DefenseCombatUnit newUnit;
            int i = 0;
            do {
                ushort size = stats.GroupSize > count ? count : stats.GroupSize;
                newUnit = new DefenseCombatUnit(owner, stub, formation, type, template.lvl, size);
              /*  switch (formation) {
                    case FormationType.Attack:
                        newUnit.Stats.ModAtk = (byte)(newUnit.Stats.Atk * 0.5);
                        break;
                    case FormationType.Defense:
                        newUnit.Stats.ModDef = (byte)(newUnit.Stats.Def * 0.5);
                        break;
                    case FormationType.Scout:
                        newUnit.Stats.ModVision = 10;
                        break;
                }*/
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

    }
}
