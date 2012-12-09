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
        private readonly BattleFormulas battleFormula;

        private readonly BattleProcedure battleProcedure;

        private readonly Resource bonus;

        private readonly uint cityId;

        private readonly IDbManager dbManager;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly AttackMode mode;

        private readonly StructureFactory structureFactory;

        private readonly uint targetCityId;

        private readonly uint troopObjectId;

        private uint groupId;

        private int originalUnitCount;

        public CityEngageAttackPassiveAction(uint cityId,
                                             uint troopObjectId,
                                             uint targetCityId,
                                             AttackMode mode,
                                             BattleFormulas battleFormula,
                                             IGameObjectLocator gameObjectLocator,
                                             BattleProcedure battleProcedure,
                                             StructureFactory structureFactory,
                                             IDbManager dbManager)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.targetCityId = targetCityId;
            this.mode = mode;
            this.battleFormula = battleFormula;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.structureFactory = structureFactory;
            this.dbManager = dbManager;

            bonus = new Resource();
        }

        public CityEngageAttackPassiveAction(uint id,
                                             bool isVisible,
                                             IDictionary<string, string> properties,
                                             BattleFormulas battleFormula,
                                             IGameObjectLocator gameObjectLocator,
                                             BattleProcedure battleProcedure,
                                             StructureFactory structureFactory,
                                             IDbManager dbManager)
                : base(id, isVisible)
        {
            this.battleFormula = battleFormula;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.structureFactory = structureFactory;
            this.dbManager = dbManager;

            cityId = uint.Parse(properties["troop_city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            groupId = uint.Parse(properties["group_id"]);

            mode = (AttackMode)(byte.Parse(properties["mode"]));
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

            StaminaMonitor = new StaminaMonitor(targetCity.Battle,
                                                combatGroup,
                                                short.Parse(properties["stamina"]),
                                                battleFormula);
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
                                new XmlKvPair("target_city_id", targetCityId), new XmlKvPair("troop_city_id", cityId),
                                new XmlKvPair("troop_object_id", troopObjectId), new XmlKvPair("mode", (byte)mode),
                                new XmlKvPair("original_count", originalUnitCount), new XmlKvPair("crop", bonus.Crop),
                                new XmlKvPair("gold", bonus.Gold), new XmlKvPair("iron", bonus.Iron),
                                new XmlKvPair("wood", bonus.Wood), new XmlKvPair("labor", bonus.Labor),
                                new XmlKvPair("group_id", groupId), new XmlKvPair("stamina", StaminaMonitor.Stamina)
                        });
            }
        }

        private void RegisterBattleListeners(ICity targetCity)
        {
            targetCity.Battle.ActionAttacked += BattleActionAttacked;
            targetCity.Battle.WithdrawAttacker += BattleWithdrawAttacker;
            targetCity.Battle.EnterRound += BattleEnterRound;
            targetCity.Battle.GroupKilled += BattleGroupKilled;
        }

        private void DeregisterBattleListeners(ICity targetCity)
        {
            targetCity.Battle.ActionAttacked -= BattleActionAttacked;
            targetCity.Battle.GroupKilled -= BattleGroupKilled;
            targetCity.Battle.WithdrawAttacker -= BattleWithdrawAttacker;
            targetCity.Battle.EnterRound -= BattleEnterRound;
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
            battleProcedure.JoinOrCreateCityBattle(targetCity, troopObject, out combatGroup, out battleId);
            groupId = combatGroup.Id;

            // Register the battle listeners
            RegisterBattleListeners(targetCity);

            // Create stamina monitor
            StaminaMonitor = new StaminaMonitor(targetCity.Battle,
                                                combatGroup,
                                                battleFormula.GetStamina(troopObject.Stub, targetCity),
                                                battleFormula);
            StaminaMonitor.PropertyChanged += (sender, args) => dbManager.Save(this);

            // Create attack mode monitor
            AttackModeMonitor = new AttackModeMonitor(targetCity.Battle, combatGroup, troopObject.Stub);

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
            troopObject.State = GameObjectState.NormalState();
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
            troopObject.State = GameObjectState.NormalState();
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
            Resource cap = new Resource(troopObject.Stub.Carry / Config.resource_crop_ratio,
                                        troopObject.Stub.Carry / Config.resource_gold_ratio,
                                        troopObject.Stub.Carry / Config.resource_iron_ratio,
                                        troopObject.Stub.Carry / Config.resource_wood_ratio,
                                        troopObject.Stub.Carry / Config.resource_labor_ratio);

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
                                          decimal damage)
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
                bonus.Add(structureFactory.GetCost(target.Type, target.Lvl) / 2);

                IStructure structure = ((ICombatStructure)target).Structure;
                object value;
                if (structure.Properties.TryGet("Crop", out value))
                {
                    bonus.Crop += (int)value;
                }
                if (structure.Properties.TryGet("Gold", out value))
                {
                    bonus.Gold += (int)value;
                }
                if (structure.Properties.TryGet("Iron", out value))
                {
                    bonus.Iron += (int)value;
                }
                if (structure.Properties.TryGet("Wood", out value))
                {
                    bonus.Wood += (int)value;
                }
                if (structure.Properties.TryGet("Labor", out value))
                {
                    bonus.Labor += (int)value;
                }

                dbManager.Save(this);
            }
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            ICity city;

            ICity targetCity;

            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new Exception("City or troop not found");
            }

            // if battle lasts more than 5 rounds, attacker gets 3 attack points.
            if (battle.GetCombatGroup(groupId).Any(co => co.RoundsParticipated == 5))
            {
                troopObject.BeginUpdate();
                troopObject.Stats.AttackPoint += 3;
                troopObject.EndUpdate();
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