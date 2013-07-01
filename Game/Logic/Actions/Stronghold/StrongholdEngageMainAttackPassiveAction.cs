#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    public class StrongholdEngageMainAttackPassiveAction : PassiveAction
    {
        private readonly StrongholdBattleProcedure strongholdBattleProcedure;

        private readonly uint cityId;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly AttackMode mode;

        private readonly uint targetStrongholdId;

        private readonly uint troopObjectId;

        private uint groupId;

        private int originalUnitCount;

        public StrongholdEngageMainAttackPassiveAction(uint cityId,
                                                       uint troopObjectId,
                                                       uint targetStrongholdId,
                                                       AttackMode mode,
                                                       IGameObjectLocator gameObjectLocator,
                                                       StrongholdBattleProcedure strongholdBattleProcedure)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.targetStrongholdId = targetStrongholdId;
            this.mode = mode;
            this.gameObjectLocator = gameObjectLocator;
            this.strongholdBattleProcedure = strongholdBattleProcedure;
        }

        public StrongholdEngageMainAttackPassiveAction(uint id,
                                                       bool isVisible,
                                                       IDictionary<string, string> properties,
                                                       IGameObjectLocator gameObjectLocator,
                                                       StrongholdBattleProcedure strongholdBattleProcedure)
                : base(id, isVisible)
        {
            this.gameObjectLocator = gameObjectLocator;
            this.strongholdBattleProcedure = strongholdBattleProcedure;

            cityId = uint.Parse(properties["troop_city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            groupId = uint.Parse(properties["group_id"]);
            mode = (AttackMode)uint.Parse(properties["mode"]);
            originalUnitCount = int.Parse(properties["original_count"]);

            targetStrongholdId = uint.Parse(properties["target_stronghold_id"]);

            IStronghold targetStronghold;
            gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold);
            RegisterBattleListeners(targetStronghold);

            var combatGroup = targetStronghold.MainBattle.GetCombatGroup(groupId);
            ITroopObject troopObject;
            ICity city;
            gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject);
            AttackModeMonitor = new AttackModeMonitor(targetStronghold.MainBattle, combatGroup, troopObject.Stub);
        }

        private AttackModeMonitor AttackModeMonitor { get; set; }

        public override ActionType Type
        {
            get
            {
                return ActionType.StrongholdEngageMainAttackPassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("target_stronghold_id", targetStrongholdId),
                                new XmlKvPair("troop_city_id", cityId), new XmlKvPair("troop_object_id", troopObjectId),
                                new XmlKvPair("group_id", groupId), new XmlKvPair("mode", (int)mode),
                                new XmlKvPair("original_count", originalUnitCount)
                        });
            }
        }

        private void RegisterBattleListeners(IStronghold targetStronghold)
        {
            targetStronghold.MainBattle.WithdrawAttacker += BattleWithdrawAttacker;
        }

        private void DeregisterBattleListeners(IStronghold targetStronghold)
        {
            targetStronghold.MainBattle.WithdrawAttacker -= BattleWithdrawAttacker;
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            ITroopObject troopObject;
            IStronghold targetStronghold;

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
            {
                return Error.ObjectNotFound;
            }

            // Create the group in the battle
            uint battleId;
            ICombatGroup combatGroup;
            strongholdBattleProcedure.JoinOrCreateStrongholdMainBattle(targetStronghold,
                                                             troopObject,
                                                             out combatGroup,
                                                             out battleId);
            groupId = combatGroup.Id;

            // Create attack mode monitor            
            originalUnitCount = troopObject.Stub.TotalCount;
            AttackModeMonitor = new AttackModeMonitor(targetStronghold.MainBattle, combatGroup, troopObject.Stub);

            // Register the battle listeners
            RegisterBattleListeners(targetStronghold);

            // Set the attacking troop object to the correct state and stamina
            troopObject.BeginUpdate();
            troopObject.State = GameObjectStateFactory.BattleState(battleId);
            troopObject.EndUpdate();

            // Set the troop stub to the correct state
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.State = TroopState.Battle;
            troopObject.Stub.EndUpdate();

            return Error.Ok;
        }

        private void BattleWithdrawAttacker(IBattleManager battle, ICombatGroup group)
        {
            ICity city;
            ITroopObject troopObject;
            IStronghold targetStronghold;

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
            {
                throw new ArgumentException();
            }

            if (group.Id != groupId)
            {
                return;
            }

            DeregisterBattleListeners(targetStronghold);

            troopObject.BeginUpdate();
            troopObject.Stub.BeginUpdate();
            troopObject.State = GameObjectStateFactory.NormalState();
            troopObject.Stub.State = TroopState.Idle;
            troopObject.Stub.EndUpdate();
            troopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }
    }
}