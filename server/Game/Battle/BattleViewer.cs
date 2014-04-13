#region

using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Setup;
using Game.Util;
using ILogger = Common.ILogger;

#endregion

namespace Game.Battle
{
    public class BattleViewer
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<BattleViewer>();

        private readonly IBattleManager battleManager;

        private readonly UnitFactory unitFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        public BattleViewer(IBattleManager battle, UnitFactory unitFactory, IStructureCsvFactory structureCsvFactory)
        {
            battle.EnterBattle += BattleEnterBattle;
            battle.ExitBattle += BattleExitBattle;
            battle.ExitTurn += BattleExitTurn;
            battle.UnitKilled += BattleUnitKilled;
            battle.ActionAttacked += BattleActionAttacked;
            battle.SkippedAttacker += BattleSkippedAttacker;
            battle.EnterRound += BattleEnterRound;            
            battleManager = battle;
            this.unitFactory = unitFactory;
            this.structureCsvFactory = structureCsvFactory;
        }

        private int TotalUpkeep(ICombatList combatList)
        {
            return combatList.AllCombatObjects().Sum(p => p.Upkeep);
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            Append("Round[" + round + "] Started with atk_upkeep[" + TotalUpkeep(atk) + "] def_upkeep[" + TotalUpkeep(def) + "]\n");
        }

        private void Append(string str)
        {
            logger.Info("ID[" + battleManager.BattleId + "] " + str);
        }

        private void PrintCombatobject(ICombatObject co)
        {
            if (co is AttackCombatUnit)
            {
                var unit = co as AttackCombatUnit;
                Append("Team[Atk] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" +
                       unitFactory.GetName(unit.Type, 1) + "] HP[" + unit.Hp + "]");
            }
            else if (co is DefenseCombatUnit)
            {
                var unit = co as DefenseCombatUnit;
                Append("Team[Def] Unit[" + co.Id + "] Formation[" + unit.Formation + "] Type[" +
                       unitFactory.GetName(unit.Type, 1) + "] HP[" + unit.Hp + "]");
            }
            else if (co is CombatStructure)
            {
                var cs = co as CombatStructure;
                Append("Team[Def] Structure[" + co.Id + "] Type[" +
                       structureCsvFactory.GetName(cs.Structure.Type, cs.Structure.Lvl) + "] HP[" + cs.Hp +
                       "]");
            }
            else if (co is BarbarianTribeCombatUnit)
            {
                var unit = co as BarbarianTribeCombatUnit;
                Append("Team[Def] Unit[" + co.Id + "] Type[" +
                       unitFactory.GetName(unit.Type, 1) + "] HP[" + unit.Hp + "]");               
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
            Append("*************Attacking****************");
            PrintCombatobject(source);
            PrintCombatobject(target);
            //Append("**************************************\n");
        }

        private void BattleUnitKilled(IBattleManager battle,
                                      BattleManager.BattleSide objSide,
                                      ICombatGroup combatGroup,
                                      ICombatObject obj)
        {
            Append("**************Removing****************");
            PrintCombatobject(obj);
        }

        private void BattleExitTurn(IBattleManager battle, ICombatList atk, ICombatList def, uint turn)
        {
            Append("Turn[" + turn + "] Ended with atk_upkeep[" + TotalUpkeep(atk) + "] def_upkeep[" + TotalUpkeep(def) + "]");
            Append("Turn[" + turn + "] Ended with atk_upkeep_active[" + atk.UpkeepNotParticipatedInRound(battle.Round) + "] def_upkeep_active[" + def.UpkeepNotParticipatedInRound(battle.Round) + "]\n");
        }

        private void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            Append("Battle Ended with atk_upkeep[" + TotalUpkeep(atk) + "] def_upkeep[" + TotalUpkeep(def) + "]\n");
        }

        private void BattleEnterBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            Append("Battle Started with atk_upkeep[" + TotalUpkeep(atk) + "] def_upkeep[" + TotalUpkeep(def) + "]\n");
        }

        private void BattleSkippedAttacker(IBattleManager battle,
                                           BattleManager.BattleSide objSide,
                                           ICombatGroup combatGroup,
                                           ICombatObject obj)
        {
            Append("***************Skipping***************");
            PrintCombatobject(obj);
            Append("**************************************\n");
        }
    }
}