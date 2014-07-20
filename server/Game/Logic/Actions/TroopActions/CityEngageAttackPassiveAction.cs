#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class CityEngageAttackPassiveAction : PassiveAction
    {
        private readonly IBattleFormulas battleFormula;

        private Resource bonus;

        private uint cityId;

        private readonly IDbManager dbManager;

        private readonly IStaminaMonitorFactory staminaMonitorFactory;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly CityBattleProcedure cityBattleProcedure;

        private readonly IStructureCsvFactory structureCsvFactory;

        private uint targetCityId;

        private uint troopObjectId;

        private uint groupId;

        private int originalUnitCount;

        public CityEngageAttackPassiveAction(IBattleFormulas battleFormula,
                                             IGameObjectLocator gameObjectLocator,
                                             CityBattleProcedure cityBattleProcedure,
                                             IStructureCsvFactory structureCsvFactory,
                                             IDbManager dbManager,
                                             IStaminaMonitorFactory staminaMonitorFactory)
        {
            this.battleFormula = battleFormula;
            this.gameObjectLocator = gameObjectLocator;
            this.cityBattleProcedure = cityBattleProcedure;
            this.structureCsvFactory = structureCsvFactory;
            this.dbManager = dbManager;
            this.staminaMonitorFactory = staminaMonitorFactory;

            bonus = new Resource();
        }

        public CityEngageAttackPassiveAction(uint cityId,
                                             uint troopObjectId,
                                             uint targetCityId,
                                             IBattleFormulas battleFormula,
                                             IGameObjectLocator gameObjectLocator,
                                             CityBattleProcedure cityBattleProcedure,
                                             IStructureCsvFactory structureCsvFactory,
                                             IDbManager dbManager,
                                             IStaminaMonitorFactory staminaMonitorFactory)
            : this(battleFormula, gameObjectLocator, cityBattleProcedure, structureCsvFactory, dbManager, staminaMonitorFactory)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.targetCityId = targetCityId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["troop_city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            groupId = uint.Parse(properties["group_id"]);

            originalUnitCount = int.Parse(properties["original_count"]);

            targetCityId = uint.Parse(properties["target_city_id"]);

            bonus = new Resource(int.Parse(properties["crop"]),
                                 int.Parse(properties["gold"]),
                                 int.Parse(properties["iron"]),
                                 int.Parse(properties["wood"]),
                                 int.Parse(properties["labor"]));
            ICity targetCity;
            gameObjectLocator.TryGetObjects(targetCityId, out targetCity);
            RegisterBattleListeners(targetCity);

            var combatGroup = targetCity.Battle.GetCombatGroup(groupId);
            ITroopObject troopObject;
            ICity city;
            gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject);

            StaminaMonitor = staminaMonitorFactory.CreateStaminaMonitor(targetCity.Battle, combatGroup, short.Parse(properties["stamina"]));
            StaminaMonitor.PropertyChanged += (sender, args) => dbManager.Save(this);

            AttackModeMonitor = new AttackModeMonitor(targetCity.Battle, combatGroup, troopObject.Stub);
        }

        private StaminaMonitor StaminaMonitor { get; set; }

        private AttackModeMonitor AttackModeMonitor { get; set; }

        public override ActionType Type
        {
            get
            {
                return ActionType.CityEngageAttackPassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("target_city_id", targetCityId), 
                                new XmlKvPair("troop_city_id", cityId),
                                new XmlKvPair("troop_object_id", troopObjectId), 
                                new XmlKvPair("original_count", originalUnitCount), 
                                new XmlKvPair("crop", bonus.Crop),
                                new XmlKvPair("gold", bonus.Gold), 
                                new XmlKvPair("iron", bonus.Iron),
                                new XmlKvPair("wood", bonus.Wood), 
                                new XmlKvPair("labor", bonus.Labor),
                                new XmlKvPair("group_id", groupId), 
                                new XmlKvPair("stamina", StaminaMonitor.Stamina)
                        });
            }
        }

        private void RegisterBattleListeners(ICity targetCity)
        {
            targetCity.Battle.ActionAttacked += BattleActionAttacked;
            targetCity.Battle.WithdrawAttacker += BattleWithdrawAttacker;
            targetCity.Battle.GroupKilled += BattleGroupKilled;
        }

        private void DeregisterBattleListeners(ICity targetCity)
        {
            targetCity.Battle.ActionAttacked -= BattleActionAttacked;
            targetCity.Battle.GroupKilled -= BattleGroupKilled;
            targetCity.Battle.WithdrawAttacker -= BattleWithdrawAttacker;
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            ICity targetCity;
            ITroopObject troopObject;

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                return Error.ObjectNotFound;
            }

            // Save original unit count to know when to bail out of battle
            originalUnitCount = troopObject.Stub.TotalCount;

            // Create the group in the battle
            uint battleId;
            ICombatGroup combatGroup;
            cityBattleProcedure.JoinOrCreateCityBattle(targetCity, troopObject, dbManager, out combatGroup, out battleId);
            groupId = combatGroup.Id;

            // Register the battle listeners
            RegisterBattleListeners(targetCity);

            // Create stamina monitor
            StaminaMonitor = staminaMonitorFactory.CreateStaminaMonitor(targetCity.Battle,
                                                                        combatGroup,
                                                                        battleFormula.GetStamina(troopObject.Stub, targetCity));            
            StaminaMonitor.PropertyChanged += (sender, args) => dbManager.Save(this);

            // Create attack mode monitor
            AttackModeMonitor = new AttackModeMonitor(targetCity.Battle, combatGroup, troopObject.Stub);

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
            ICity targetCity;
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            if (group.Id != groupId)
            {
                return;
            }

            DeregisterBattleListeners(targetCity);

            troopObject.BeginUpdate();
            SetLootedResources(targetCity.Battle, troopObject, group);
            troopObject.Stub.BeginUpdate();
            troopObject.State = GameObjectStateFactory.NormalState();
            troopObject.Stub.State = TroopState.Idle;
            troopObject.Stub.EndUpdate();
            troopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        /// <summary>
        ///     Takes care of finishing this action up if all our units are killed
        /// </summary>
        private void BattleGroupKilled(IBattleManager battle, ICombatGroup group)
        {
            // Ignore if not our group
            if (group.Id != groupId)
            {
                return;
            }

            ICity targetCity;
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            DeregisterBattleListeners(targetCity);

            troopObject.BeginUpdate();
            troopObject.State = GameObjectStateFactory.NormalState();
            troopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        private void SetLootedResources(IBattleManager battle, ITroopObject troopObject, ICombatGroup combatGroup)
        {
            if (!battle.BattleStarted)
            {
                return;
            }

            // Calculate bonus
            Resource resource = battleFormula.GetBonusResources(troopObject,
                                                                originalUnitCount,
                                                                troopObject.Stub.TotalCount);

            // Destroyed Structure bonus
            resource.Add(bonus);

            // Copy looted resources since we'll be modifying the troop's loot variable
            var looted = new Resource(troopObject.Stats.Loot);

            // Add bonus to troop object            
            Resource returning;
            Resource actual;
            Resource cap = new Resource(troopObject.Stub.Carry / 1,
                                        troopObject.Stub.Carry / 2,
                                        troopObject.Stub.Carry / Config.battle_loot_resource_iron_ratio,
                                        troopObject.Stub.Carry / 1,
                                        troopObject.Stub.Carry / Config.battle_loot_resource_labor_ratio);

            troopObject.Stats.Loot.Add(resource, cap, out actual, out returning);

            // Update battle report view with actual received bonus            
            battle.BattleReport.SetLootedResources(combatGroup.Owner, combatGroup.Id, battle.BattleId, looted, actual);
        }

        private void BattleActionAttacked(IBattleManager battle,
                                          BattleManager.BattleSide attackingSide,
                                          ICombatGroup attackerGroup,
                                          ICombatObject attacker,
                                          ICombatGroup targetGroup,
                                          ICombatObject target,
                                          decimal damage,
                                          int attackerCount,
                                          int targetCount)
        {
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
            {
                throw new ArgumentException();
            }

            if (attackerGroup.Id == groupId && target.ClassType == BattleClass.Structure && target is ICombatStructure &&
                target.IsDead)
            {
                // if our troop knocked down a building, we get the bonus.
                bonus.Add(structureCsvFactory.GetCost(target.Type, target.Lvl) / 2);

                IStructure structure = ((ICombatStructure)target).Structure;
                object value;
                if (structure.Properties.TryGet("Crop", out value))
                {
                    if (value is int)
                        bonus.Crop += (int)value;
                }
                if (structure.Properties.TryGet("Gold", out value))
                {
                    if (value is int)
                        bonus.Gold += (int)value;
                }
                if (structure.Properties.TryGet("Iron", out value))
                {
                    if (value is int)
                        bonus.Iron += (int)value;
                }
                if (structure.Properties.TryGet("Wood", out value))
                {
                    if (value is int) 
                        bonus.Wood += (int)value;
                }
                if (structure.Properties.TryGet("Labor", out value))
                {
                    if (value is int)
                        bonus.Labor += (int)value;
                }

                dbManager.Save(this);
            }
        }
        
        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }
    }
}