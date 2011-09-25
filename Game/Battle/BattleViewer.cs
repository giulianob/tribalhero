#region

using Game.Data;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Battle
{
    public class BattleViewer
    {
        private IBattleManager battle;

        public BattleViewer(IBattleManager battle)
        {
            this.battle = battle;
            battle.EnterBattle += BattleEnterBattle;
            battle.ExitBattle += BattleExitBattle;
            battle.EnterTurn += BattleEnterTurn;
            battle.ExitTurn += BattleExitTurn;
            battle.UnitRemoved += BattleUnitRemoved;
            battle.ActionAttacked += BattleActionAttacked;
            battle.SkippedAttacker += BattleSkippedAttacker;
            battle.EnterRound += BattleEnterRound;
        }

        void BattleEnterRound(CombatList atk, CombatList def, uint round) {
            Append("Round[" + round + "] Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
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
                Append("Team[Atk] List[" + co.CombatList.Id + "] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" + Ioc.Kernel.Get<UnitFactory>().GetName(unit.Type, 1) +
                       "] HP[" + unit.Hp + "]");
            }
            else if(co is DefenseCombatUnit)
            {
                var unit = co as DefenseCombatUnit;
                Append("Team[Def] List[" + co.CombatList.Id + "] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" + Ioc.Kernel.Get<UnitFactory>().GetName(unit.Type, 1) +
                       "] HP[" + unit.Hp + "]");
            } 
            else if (co is CombatStructure)
            {
                var cs = co as CombatStructure;
                Append("Team[Def] List[" + co.CombatList.Id + "] Structure[" + co.Id + "] Type[" + Ioc.Kernel.Get<StructureFactory>().GetName(cs.Structure.Type,(byte)cs.Structure.Lvl) + "] HP[" + cs.Hp + "]");
            }
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {
            Append("**************************************");
            Append("Attacker: ");
            PrintCombatobject(source);
            Append("Defender: ");
            PrintCombatobject(target);
            Append("**************************************\n");
        }

        private void BattleUnitRemoved(CombatObject obj)
        {
            Append("**************************************");
            Append("Removing: ");
            PrintCombatobject(obj);
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

        void BattleSkippedAttacker(CombatObject obj) {
            Append("**************************************");
            Append("Skipping: ");
            PrintCombatobject(obj);
            Append("**************************************\n");
        }
    }
}