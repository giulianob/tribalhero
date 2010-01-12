#region

using System;
using Game.Setup;

#endregion

namespace Game.Battle {
    class BattleViewer {
        private BattleManager battle;

        protected virtual void Append(string str) {
            Console.Out.WriteLine(str);
        }

        public BattleViewer(BattleManager battle) {
            this.battle = battle;
            battle.EnterBattle += new BattleBase.OnBattle(battle_EnterBattle);
            battle.ExitBattle += new BattleBase.OnBattle(battle_ExitBattle);
            battle.EnterTurn += new BattleBase.OnTurn(battle_EnterTurn);
            battle.ExitTurn += new BattleBase.OnTurn(battle_ExitTurn);
            battle.UnitRemoved += new BattleBase.OnUnitUpdate(battle_UnitRemoved);
            battle.ActionAttacked += new BattleBase.OnAttack(battle_ActionAttacked);
        }

        private void print_combatobject(CombatObject co) {
            if (co is AttackCombatUnit) {
                AttackCombatUnit unit = co as AttackCombatUnit;
                Append("List[" + co.CombatList.Id + "] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" +
                       UnitFactory.getName(unit.Type, 1) + "] HP[" + unit.Hp + "]");
            } else if (co is CombatStructure) {
                CombatStructure cs = co as CombatStructure;
                Append("List[" + co.CombatList.Id + "] Structure[" + co.Id + "] Type[" +
                       StructureFactory.getName(cs.Structure) + "] HP[" + cs.Hp + "]");
            }
        }

        private void battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            Append("**************************************");
            Append("Attacker: ");
            print_combatobject(source);
            Append("\n");
            Append("Defender: ");
            print_combatobject(target);
            Append("\n");
            Append("**************************************\n");
        }

        private void battle_UnitRemoved(CombatObject obj) {
            Append("**************************************");
            Append("Removing: ");
            print_combatobject(obj);
            Append("\n");
            Append("**************************************\n");
        }

        private void battle_ExitTurn(CombatList atk, CombatList def, int turn) {
            Append("Turn[" + turn + "] Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void battle_EnterTurn(CombatList atk, CombatList def, int turn) {
            Append("Turn[" + turn + "] Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void battle_ExitBattle(CombatList atk, CombatList def) {
            Append("Battle Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void battle_EnterBattle(CombatList atk, CombatList def) {
            Append("Battle Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }
    }
}