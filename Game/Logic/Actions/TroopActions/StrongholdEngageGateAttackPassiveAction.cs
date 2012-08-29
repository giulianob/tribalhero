#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class StrongholdEngageGateAttackPassiveAction : PassiveAction
    {
        private StaminaMonitor StaminaMonitor { get; set; }

        private readonly BattleFormulas battleFormula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly BattleProcedure battleProcedure;

        private readonly IDbManager dbManager;

        private readonly uint cityId;

        private uint groupId;

        private readonly uint troopObjectId;

        private readonly uint targetStrongholdId;

        public StrongholdEngageGateAttackPassiveAction(uint cityId,
                                         uint troopObjectId,
                                         uint targetStrongholdId,
                                         BattleFormulas battleFormula,
                                         IGameObjectLocator gameObjectLocator,
                                         BattleProcedure battleProcedure,
                                         IDbManager dbManager)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.targetStrongholdId = targetStrongholdId;
            this.battleFormula = battleFormula;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.dbManager = dbManager;
        }

        public StrongholdEngageGateAttackPassiveAction(uint id,
                                         bool isVisible,
                                         IDictionary<string, string> properties,
                                         BattleFormulas battleFormula,
                                         IGameObjectLocator gameObjectLocator,
                                         BattleProcedure battleProcedure,
                                         IDbManager dbManager)
                : base(id, isVisible)
        {
            this.battleFormula = battleFormula;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.dbManager = dbManager;

            cityId = uint.Parse(properties["troop_city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            groupId = uint.Parse(properties["group_id"]);

            targetStrongholdId = uint.Parse(properties["target_stronghold_id"]);

            IStronghold targetStronghold;
            gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold);
            RegisterBattleListeners(targetStronghold);

            StaminaMonitor = new StaminaMonitor(targetStronghold.Battle, targetStronghold.Battle.GetCombatGroup(groupId), short.Parse(properties["stamina"]), battleFormula);
            StaminaMonitor.PropertyChanged += (sender, args) => dbManager.Save(this);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StrongholdEngageGateAttackPassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[]
                {
                        new XmlKvPair("target_stronghold_id", targetStrongholdId), new XmlKvPair("troop_city_id", cityId),
                        new XmlKvPair("troop_object_id", troopObjectId), new XmlKvPair("group_id", groupId), new XmlKvPair("stamina", StaminaMonitor.Stamina)
                });
            }
        }

        private void RegisterBattleListeners(IStronghold targetStronghold)
        {
            targetStronghold.Battle.WithdrawAttacker += BattleWithdrawAttacker;
        }

        private void DeregisterBattleListeners(IStronghold targetStronghold)
        {
            targetStronghold.Battle.WithdrawAttacker -= BattleWithdrawAttacker;
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

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
            {
                return Error.ObjectNotFound;
            }

            // Create the group in the battle
            uint battleId;
            ICombatGroup combatGroup;
            battleProcedure.JoinOrCreateStrongholdGateBattle(targetStronghold, troopObject, out combatGroup, out battleId);
            groupId = combatGroup.Id;

            // Register the battle listeners
            RegisterBattleListeners(targetStronghold);

            // Create stamina monitor
            StaminaMonitor = new StaminaMonitor(targetStronghold.Battle, combatGroup, battleFormula.GetStamina(troopObject.Stub, targetStronghold), battleFormula);
            StaminaMonitor.PropertyChanged += (sender, args) => dbManager.Save(this);

            // Set the attacking troop object to the correct state and stamina
            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.BattleState(battleId);
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

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
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
            troopObject.State = GameObjectState.NormalState();
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