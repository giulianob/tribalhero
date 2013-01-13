using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Map;
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
                var battleOwner = new BattleOwner(BattleOwnerType.BarbarianTribe, barbarianTribe.Id);

                barbarianTribe.Battle =
                        battleManagerFactory.CreateBarbarianBattleManager(new BattleLocation(BattleLocationType.BarbarianTribe, barbarianTribe.Id),
                                                                          battleOwner,
                                                                          barbarianTribe);

                combatGroup = battleProcedure.AddAttackerToBattle(barbarianTribe.Battle, attackerTroopObject);

                var battlePassiveAction = actionFactory.CreateBarbarianTribeBattlePassiveAction(barbarianTribe.Id);
                Error result = barbarianTribe.Worker.DoPassive(barbarianTribe, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
            }

            battleId = barbarianTribe.Battle.BattleId;
        }

    }
}