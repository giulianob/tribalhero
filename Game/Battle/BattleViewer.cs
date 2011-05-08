#region

using Game.Data;
using Game.Setup;

#endregion

namespace Game.Battle
{
    class BattleViewer
    {
        private BattleManager battle;

        public BattleViewer(BattleManager battle)
        {
            this.battle = battle;
            battle.EnterBattle += BattleEnterBattle;
            battle.ExitBattle += BattleExitBattle;
            battle.EnterTurn += BattleEnterTurn;
            battle.ExitTurn += BattleExitTurn;
            battle.UnitRemoved += BattleUnitRemoved;
            battle.ActionAttacked += BattleActionAttacked;
        }

        protected virtual void Append(string str)
        {
            Global.Logger.Info(str);
        }

        private void PrintCombatobject(CombatObject co)
        {
            if (co is AttackCombatUnit)
            {
                var unit = co as AttackCombatUnit;
                Append("List[" + co.CombatList.Id + "] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" + UnitFactory.GetName(unit.Type, 1) +
                       "] HP[" + unit.Hp + "]");
            }
            else if (co is CombatStructure)
            {
                var cs = co as CombatStructure;
                Append("List[" + co.CombatList.Id + "] Structure[" + co.Id + "] Type[" + StructureFactory.GetName(cs.Structure) + "] HP[" + cs.Hp + "]");
            }
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {
            Append("**************************************");
            Append("Attacker: ");
            PrintCombatobject(source);
            Append("\n");
            Append("Defender: ");
            PrintCombatobject(target);
            Append("\n");
            Append("**************************************\n");
        }

        private void BattleUnitRemoved(CombatObject obj)
        {
            Append("**************************************");
            Append("Removing: ");
            PrintCombatobject(obj);
            Append("\n");
            Append("**************************************\n");
        }

        private void BattleExitTurn(CombatList atk, CombatList def, int turn)
        {
            Append("Turn[" + turn + "] Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleEnterTurn(CombatList atk, CombatList def, int turn)
        {
            Append("Turn[" + turn + "] Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleExitBattle(CombatList atk, CombatList def)
        {
            Append("Battle Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleEnterBattle(CombatList atk, CombatList def)
        {
            Append("Battle Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }
    }
}