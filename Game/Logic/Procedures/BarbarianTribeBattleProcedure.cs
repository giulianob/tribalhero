using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Setup;

namespace Game.Logic.Procedures
{
    public class BarbarianTribeBattleProcedure
    {
        private readonly BattleProcedure battleProcedure;

        private readonly ICombatGroupFactory combatGroupFactory;

        private readonly ICombatUnitFactory combatUnitFactory;

        private readonly IActionFactory actionFactory;

        private readonly IBattleManagerFactory battleManagerFactory;
        
        [Obsolete("For testing only", true)]
        public BarbarianTribeBattleProcedure()
        {
        }

        public BarbarianTribeBattleProcedure(
                                   IBattleManagerFactory battleManagerFactory,
                                   IActionFactory actionFactory,
                                   BattleProcedure battleProcedure,
                                   ICombatGroupFactory combatGroupFactory,
                                   ICombatUnitFactory combatUnitFactory)
        {            
            this.battleManagerFactory = battleManagerFactory;
            this.actionFactory = actionFactory;
            this.battleProcedure = battleProcedure;
            this.combatGroupFactory = combatGroupFactory;
            this.combatUnitFactory = combatUnitFactory;
        }

        public virtual void JoinOrCreateBarbarianTribeBattle(IBarbarianTribe barbarianTribe,
                                                             ITroopObject attackerTroopObject,
                                                             out ICombatGroup combatGroup,
                                                             out uint battleId)
        {
            // If battle already exists, then we just join it in also bringing any new units
            if (barbarianTribe.Battle != null)
            {
                combatGroup = battleProcedure.AddAttackerToBattle(barbarianTribe.Battle, attackerTroopObject);
            }
            // Otherwise, the battle has to be created
            else
            {
                var battleOwner = new BattleOwner(BattleOwnerType.BarbarianTribe, barbarianTribe.ObjectId);

                barbarianTribe.Battle =
                        battleManagerFactory.CreateBarbarianBattleManager(new BattleLocation(BattleLocationType.BarbarianTribe, barbarianTribe.ObjectId),
                                                                          battleOwner,
                                                                          barbarianTribe);

                combatGroup = battleProcedure.AddAttackerToBattle(barbarianTribe.Battle, attackerTroopObject);

                var battlePassiveAction = actionFactory.CreateBarbarianTribeBattlePassiveAction(barbarianTribe.ObjectId);
                Error result = barbarianTribe.Worker.DoPassive(barbarianTribe, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }

                barbarianTribe.BeginUpdate();
                barbarianTribe.LastAttacked = DateTime.UtcNow;
                barbarianTribe.State = GameObjectState.BattleState(barbarianTribe.Battle.BattleId);
                barbarianTribe.EndUpdate();
            }

            battleId = barbarianTribe.Battle.BattleId;
        }

        public virtual ICombatGroup AddBarbarianTribeUnitsToBattle(IBattleManager battle, IBarbarianTribe barbarianTribe, IEnumerable<Unit> units)
        {
            var barbarianCombatGroup = combatGroupFactory.CreateBarbarianTribeCombatGroup(battle.BattleId, battle.GetNextGroupId(), barbarianTribe);

            foreach (var unit in units)
            {
                var combatUnits = combatUnitFactory.CreateBarbarianTribeCombatUnit(battle,
                                                                               barbarianTribe,
                                                                               unit.Type,
                                                                               (byte)Math.Max(1, barbarianTribe.Lvl / 2),
                                                                               unit.Count);
                foreach (var obj in combatUnits)
                {
                    barbarianCombatGroup.Add(obj);
                }
            }

            battle.Add(barbarianCombatGroup, BattleManager.BattleSide.Defense, false);

            return barbarianCombatGroup;
        }
    }
}