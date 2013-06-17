using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;

namespace Game.Logic.Procedures
{
    public class StrongholdBattleProcedure
    {
        private readonly BattleProcedure battleProcedure;

        private readonly ICombatUnitFactory combatUnitFactory;

        private readonly ICombatGroupFactory combatGroupFactory;

        private readonly IBattleManagerFactory battleManagerFactory;

        private readonly IActionFactory actionFactory;

        private readonly Formula formula;

        [Obsolete("For testing only", true)]
        public StrongholdBattleProcedure()
        {
        }

        public StrongholdBattleProcedure(BattleProcedure battleProcedure,
                                         ICombatUnitFactory combatUnitFactory,
                                         ICombatGroupFactory combatGroupFactory,
                                         IBattleManagerFactory battleManagerFactory,
                                         IActionFactory actionFactory,
                                         Formula formula)
        {
            this.battleProcedure = battleProcedure;
            this.combatUnitFactory = combatUnitFactory;
            this.combatGroupFactory = combatGroupFactory;
            this.battleManagerFactory = battleManagerFactory;
            this.actionFactory = actionFactory;
            this.formula = formula;
        }

        public StrongholdBattleProcedure(Formula formula, BattleProcedure battleProcedure)
        {
            this.formula = formula;
            this.battleProcedure = battleProcedure;
        }

        public virtual void JoinOrCreateStrongholdMainBattle(IStronghold targetStronghold,
                                                             ITroopObject attackerTroopObject,
                                                             out ICombatGroup combatGroup,
                                                             out uint battleId)
        {
            // If battle already exists, then we just join it in also bringing any new units
            if (targetStronghold.MainBattle != null)
            {
                combatGroup = battleProcedure.AddAttackerToBattle(targetStronghold.MainBattle, attackerTroopObject);
            }
                    // Otherwise, the battle has to be created
            else
            {
                var battleOwner = targetStronghold.Tribe == null
                                          ? new BattleOwner(BattleOwnerType.Stronghold, targetStronghold.ObjectId)
                                          : new BattleOwner(BattleOwnerType.Tribe, targetStronghold.Tribe.Id);

                targetStronghold.MainBattle =
                        battleManagerFactory.CreateStrongholdMainBattleManager(
                                                                               new BattleLocation(BattleLocationType.Stronghold, targetStronghold.ObjectId),
                                                                               battleOwner,
                                                                               targetStronghold);

                targetStronghold.MainBattle.SetProperty("defense_stronghold_meter", formula.StrongholdMainBattleMeter(targetStronghold.Lvl));
                targetStronghold.MainBattle.SetProperty("offense_stronghold_meter", formula.StrongholdMainBattleMeter(targetStronghold.Lvl));

                combatGroup = battleProcedure.AddAttackerToBattle(targetStronghold.MainBattle, attackerTroopObject);

                var battlePassiveAction = actionFactory.CreateStrongholdMainBattlePassiveAction(targetStronghold.ObjectId);
                Error result = targetStronghold.Worker.DoPassive(targetStronghold, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
            }

            battleId = targetStronghold.MainBattle.BattleId;
        }

        public virtual Error CanStrongholdBeAttacked(ICity city, IStronghold stronghold, bool forceAttack)
        {
            if (stronghold.StrongholdState == StrongholdState.Inactive)
            {
                return Error.StrongholdStillInactive;
            }

            if (!city.Owner.IsInTribe)
            {
                return Error.TribesmanNotPartOfTribe;
            }

            if (!forceAttack)
            {
                if (city.Owner.Tribesman.Tribe == stronghold.Tribe)
                {
                    return Error.StrongholdCantAttackSelf;
                }

                if (stronghold.GateOpenTo != null && stronghold.GateOpenTo != city.Owner.Tribesman.Tribe)
                {
                    return Error.StrongholdGateNotOpenToTribe;
                }
            }

            return Error.Ok;
        }

        public virtual Error CanStrongholdBeDefended(ICity city, IStronghold stronghold)
        {
            if (stronghold.StrongholdState == StrongholdState.Inactive)
            {
                return Error.StrongholdStillInactive;
            }

            if (!city.Owner.IsInTribe)
            {
                return Error.TribesmanNotPartOfTribe;
            }

            if (city.Owner.Tribesman.Tribe != stronghold.Tribe)
            {
                return Error.StrongholdBelongsToOther;
            }

            return Error.Ok;
        }

        public virtual void JoinOrCreateStrongholdGateBattle(IStronghold targetStronghold,
                                                             ITroopObject attackerTroopObject,
                                                             out ICombatGroup combatGroup,
                                                             out uint battleId)
        {
            // If battle already exists, then we just join it in also bringing any new units
            if (targetStronghold.GateBattle != null)
            {
                combatGroup = battleProcedure.AddAttackerToBattle(targetStronghold.GateBattle, attackerTroopObject);
            }
                    // Otherwise, the battle has to be created
            else
            {
                var battleOwner = targetStronghold.Tribe == null
                                          ? new BattleOwner(BattleOwnerType.Stronghold, targetStronghold.ObjectId)
                                          : new BattleOwner(BattleOwnerType.Tribe, targetStronghold.Tribe.Id);

                targetStronghold.GateBattle =
                        battleManagerFactory.CreateStrongholdGateBattleManager(
                                                                               new BattleLocation(BattleLocationType.StrongholdGate,
                                                                                                  targetStronghold.ObjectId),
                                                                               battleOwner,
                                                                               targetStronghold);

                combatGroup = battleProcedure.AddAttackerToBattle(targetStronghold.GateBattle, attackerTroopObject);

                var battlePassiveAction = actionFactory.CreateStrongholdGateBattlePassiveAction(targetStronghold.ObjectId);
                Error result = targetStronghold.Worker.DoPassive(targetStronghold, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
                targetStronghold.BeginUpdate();
                targetStronghold.State = GameObjectState.BattleState(targetStronghold.GateBattle.BattleId);
                targetStronghold.EndUpdate();
            }

            battleId = targetStronghold.GateBattle.BattleId;
        }

        public virtual ICombatGroup AddStrongholdGateToBattle(IBattleManager battle, IStronghold stronghold)
        {
            var strongholdCombatGroup = combatGroupFactory.CreateStrongholdCombatGroup(battle.BattleId, battle.GetNextGroupId(), stronghold);
            if (stronghold.Gate == 0)
            {
                throw new Exception("Dead gate trying to join the battle");
            }

            strongholdCombatGroup.Add(combatUnitFactory.CreateStrongholdGateStructure(battle, stronghold, stronghold.Gate));

            battle.Add(strongholdCombatGroup, BattleManager.BattleSide.Defense, false);

            return strongholdCombatGroup;
        }

        public virtual ICombatGroup AddStrongholdUnitsToBattle(IBattleManager battle, IStronghold stronghold, IEnumerable<Unit> units)
        {
            var strongholdCombatGroup = combatGroupFactory.CreateStrongholdCombatGroup(battle.BattleId, battle.GetNextGroupId(), stronghold);

            foreach (var unit in units)
            {
                var combatUnits = combatUnitFactory.CreateStrongholdCombatUnit(battle,
                                                                               stronghold,
                                                                               unit.Type,
                                                                               (byte)Math.Max(1, stronghold.Lvl / 2),
                                                                               unit.Count);
                foreach (var obj in combatUnits)
                {
                    strongholdCombatGroup.Add(obj);
                }
            }

            battle.Add(strongholdCombatGroup, BattleManager.BattleSide.Defense, false);

            return strongholdCombatGroup;
        }
    }
}