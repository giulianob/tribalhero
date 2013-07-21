#region

using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Setup;
using Game.Util;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Battle
{
    public class BattleViewer
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public BattleViewer(IBattleManager battle)
        {
            battle.EnterBattle += BattleEnterBattle;
            battle.ExitBattle += BattleExitBattle;
            battle.ExitTurn += BattleExitTurn;
            battle.UnitKilled += BattleUnitKilled;
            battle.ActionAttacked += BattleActionAttacked;
            battle.SkippedAttacker += BattleSkippedAttacker;
            battle.EnterRound += BattleEnterRound;
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            Append("Round[" + round + "] Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void Append(string str)
        {
            logger.Info(str);
        }

        private void PrintCombatobject(ICombatObject co)
        {
            if (co is AttackCombatUnit)
            {
                var unit = co as AttackCombatUnit;
                Append("Team[Atk] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" +
                       Ioc.Kernel.Get<UnitFactory>().GetName(unit.Type, 1) + "] HP[" + unit.Hp + "]");
            }
            else if (co is DefenseCombatUnit)
            {
                var unit = co as DefenseCombatUnit;
                Append("Team[Def] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" +
                       Ioc.Kernel.Get<UnitFactory>().GetName(unit.Type, 1) + "] HP[" + unit.Hp + "]");
            }
            else if (co is CombatStructure)
            {
                var cs = co as CombatStructure;
                Append("Team[Def] Structure[" + co.Id + "] Type[" +
                       Ioc.Kernel.Get<IStructureCsvFactory>().GetName(cs.Structure.Type, cs.Structure.Lvl) + "] HP[" + cs.Hp +
                       "]");
            }
        }

        private void BattleActionAttacked(IBattleManager battle,
                                          BattleManager.BattleSide attackingSide,
                                          ICombatGroup attackerGroup,
                                          ICombatObject source,
                                          ICombatGroup defenderGroup,
                                          ICombatObject target,
                                          decimal damage,
                                          int attackerCount,
                                          int targetCount)
        {
            Append("**************************************");
            Append("Attacker: ");
            PrintCombatobject(source);
            Append("Defender: ");
            PrintCombatobject(target);
            Append("**************************************\n");
        }

        private void BattleUnitKilled(IBattleManager battle,
                                      BattleManager.BattleSide objSide,
                                      ICombatGroup combatGroup,
                                      ICombatObject obj)
        {
            Append("**************************************");
            Append("Removing: ");
            PrintCombatobject(obj);
            Append("**************************************\n");
        }

        private void BattleExitTurn(IBattleManager battle, ICombatList atk, ICombatList def, int turn)
        {
            Append("Turn[" + turn + "] Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleEnterTurn(IBattleManager battle, ICombatList atk, ICombatList def, int turn)
        {
            Append("Turn[" + turn + "] Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            Append("Battle Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleEnterBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            Append("Battle Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        private void BattleSkippedAttacker(IBattleManager battle,
                                           BattleManager.BattleSide objSide,
                                           ICombatGroup combatGroup,
                                           ICombatObject obj)
        {
            Append("**************************************");
            Append("Skipping: ");
            PrintCombatobject(obj);
            Append("**************************************\n");
        }
    }
}