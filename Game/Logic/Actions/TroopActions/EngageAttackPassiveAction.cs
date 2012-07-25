#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class EngageAttackPassiveAction : PassiveAction
    {
        private readonly BattleFormulas battleFormula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly BattleProcedure battleProcedure;

        private readonly Formula formula;

        private readonly StructureFactory structureFactory;

        private readonly IDbManager dbManager;

        private readonly Resource bonus;

        private readonly uint cityId;

        private uint groupId;

        private readonly AttackMode mode;

        private readonly uint troopObjectId;

        private readonly uint targetCityId;

        private int originalUnitCount;

        private int remainingUnitCount;

        public EngageAttackPassiveAction(uint cityId,
                                         uint troopObjectId,
                                         uint targetCityId,
                                         AttackMode mode,
                                         BattleFormulas battleFormula,
                                         IGameObjectLocator gameObjectLocator,
                                         BattleProcedure battleProcedure,
                                         Formula formula,
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
            this.formula = formula;
            this.structureFactory = structureFactory;
            this.dbManager = dbManager;

            bonus = new Resource();
        }

        public EngageAttackPassiveAction(uint id,
                                         bool isVisible,
                                         IDictionary<string, string> properties,
                                         BattleFormulas battleFormula,
                                         IGameObjectLocator gameObjectLocator,
                                         BattleProcedure battleProcedure,
                                         Formula formula,
                                         StructureFactory structureFactory,
                                         IDbManager dbManager)
                : base(id, isVisible)
        {
            this.battleFormula = battleFormula;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.formula = formula;
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
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.EngageAttackPassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("target_city_id", targetCityId), new XmlKvPair("troop_city_id", cityId), new XmlKvPair("troop_object_id", troopObjectId),
                                new XmlKvPair("mode", (byte)mode), new XmlKvPair("original_count", originalUnitCount), new XmlKvPair("crop", bonus.Crop),
                                new XmlKvPair("gold", bonus.Gold), new XmlKvPair("iron", bonus.Iron), new XmlKvPair("wood", bonus.Wood),
                                new XmlKvPair("labor", bonus.Labor), new XmlKvPair("group_id", groupId)
                        });
            }
        }

        private void RegisterBattleListeners(ICity targetCity)
        {
            targetCity.Battle.ActionAttacked += BattleActionAttacked;
            targetCity.Battle.ExitBattle += BattleExitBattle;
            targetCity.Battle.WithdrawAttacker += BattleWithdrawAttacker;
            targetCity.Battle.EnterRound += BattleEnterRound;
            targetCity.Battle.UnitRemoved += BattleUnitRemoved;
            targetCity.Battle.ExitTurn += BattleExitTurn;
        }

        private void DeregisterBattleListeners(ICity targetCity)
        {
            targetCity.Battle.ActionAttacked -= BattleActionAttacked;
            targetCity.Battle.ExitBattle -= BattleExitBattle;
            targetCity.Battle.UnitRemoved -= BattleUnitRemoved;
            targetCity.Battle.WithdrawAttacker -= BattleWithdrawAttacker;
            targetCity.Battle.EnterRound -= BattleEnterRound;
            targetCity.Battle.ExitTurn -= BattleExitTurn;
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

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                return Error.ObjectNotFound;
            }

            originalUnitCount = troopObject.Stub.TotalCount;

            battleProcedure.JoinOrCreateBattle(targetCity, troopObject, out groupId);

            RegisterBattleListeners(targetCity);

            // Set the attacking troop object to the correct state and stamina
            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.BattleState(targetCity.Id);
            troopObject.Stats.Stamina = battleFormula.GetStamina(troopObject.Stub, targetCity);
            troopObject.EndUpdate();

            // Set the troop stub to the correct state
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.State = TroopState.Battle;
            troopObject.Stub.EndUpdate();

            return Error.Ok;
        }

        private void BattleWithdrawAttacker(IBattleManager battle, IEnumerable<CombatObject> list)
        {            
            ICity targetCity;
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            bool retreat = list.Any(combatObject => combatObject is AttackCombatUnit && combatObject.TroopStub == troopObject.Stub);

            if (!retreat)
            {
                return;
            }

            DeregisterBattleListeners(targetCity);

            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.NormalState();
            SetLootedResources(targetCity.Battle, troopObject);
            troopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        /// <summary>
        /// Takes care of finishing this action up if all our units are killed
        /// </summary>
        private void BattleUnitRemoved(IBattleManager battle, CombatObject combatobject)
        {
            ICity targetCity;
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            // If this combat object is ours and all the units are dead, then remove it
            if (!(combatobject is AttackCombatUnit) || combatobject.TroopStub != troopObject.Stub || combatobject.TroopStub.TotalCount > 0)
            {
                return;
            }

            DeregisterBattleListeners(targetCity);

            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.NormalState();
            troopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        private void SetLootedResources(IBattleManager battle, ITroopObject troopObject)
        {
            if (!battle.BattleStarted)
            {
                return;
            }

            // Calculate bonus
            Resource resource = battleFormula.GetBonusResources(troopObject, originalUnitCount, remainingUnitCount);

            // Destroyed Structure bonus
            resource.Add(bonus);

            // Copy looted resources since we'll be modifying the troop's loot variable
            var looted = new Resource(troopObject.Stats.Loot);

            // Add bonus to troop object            
            Resource returning;
            Resource actual;
            Resource cap = new Resource(troopObject.Stub.Carry/Config.resource_crop_ratio,
                                        troopObject.Stub.Carry/Config.resource_gold_ratio,
                                        troopObject.Stub.Carry/Config.resource_iron_ratio,
                                        troopObject.Stub.Carry/Config.resource_wood_ratio,
                                        troopObject.Stub.Carry/Config.resource_labor_ratio);

            troopObject.Stats.Loot.Add(resource, cap, out actual, out returning);

            // Update battle report view with actual received bonus            
            battle.BattleReport.SetLootedResources(troopObject.City.Id, troopObject.Stub.TroopId, battle.BattleId, looted, actual);
        }

        private void BattleExitTurn(IBattleManager battle, ICombatList atk, ICombatList def, int turn)
        {
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
            {
                throw new ArgumentException();
            }

            // Remove troop from battle if he is out of stamina, we need to check here because he might have lost
            // some stamina after knocking down a building
            if (troopObject.Stats.Stamina == 0)
            {
                ICity targetCity;
                if (!gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
                {
                    throw new ArgumentException();
                }

                CombatGroup combatGroup = targetCity.Battle.GetCombatGroup(groupId);
                if (combatGroup == null)
                {
                    throw new Exception("Cannot find group to be removed");
                }
                targetCity.Battle.Remove(combatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
            }
        }

        private void BattleActionAttacked(IBattleManager battle, CombatObject source, CombatObject target, decimal damage)
        {
            ICity city;
            ICity targetCity;

            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            var targetCombatUnit = target as AttackCombatUnit;
            if (targetCombatUnit == null)
            {
                if (target.ClassType == BattleClass.Structure && target.IsDead)
                {
                    // if our troop knocked down a building, we get the bonus.
                    if (source.TroopStub == troopObject.Stub)
                    {
                        bonus.Add(structureFactory.GetCost(target.Type, target.Lvl)/2);

                        IStructure structure = ((CombatStructure)target).Structure;
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

                    ReduceStamina(troopObject, battleFormula.GetStaminaStructureDestroyed(troopObject.Stats.Stamina, target as CombatStructure));
                }
            }
            // Check if the unit being attacked belongs to us
            else if (targetCombatUnit.TroopStub == troopObject.Stub)
            {
                // Check to see if player should retreat
                remainingUnitCount = troopObject.Stub.TotalCount;

                // Don't return if we haven't fulfilled the minimum rounds or not below the threshold
                if (targetCombatUnit.RoundsParticipated < Config.battle_min_rounds || remainingUnitCount == 0 ||
                    remainingUnitCount > formula.GetAttackModeTolerance(originalUnitCount, mode))
                {
                    return;
                }

                CombatGroup combatGroup = targetCity.Battle.GetCombatGroup(groupId);
                if (combatGroup == null)
                {
                    throw new Exception("Cannot find group to be removed");
                }
                targetCity.Battle.Remove(combatGroup, BattleManager.BattleSide.Attack, ReportState.Retreating);
            }
        }

        private void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            ICity city;
            ICity targetCity;
            ITroopStub stub;

            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            DeregisterBattleListeners(targetCity);

            troopObject.BeginUpdate();
            SetLootedResources(targetCity.Battle, troopObject);
            troopObject.Stub.BeginUpdate();
            troopObject.State = GameObjectState.NormalState();
            troopObject.Stub.State = TroopState.Idle;
            troopObject.Stub.EndUpdate();
            troopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            ICity city;
            
            ICity targetCity;

            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            // Find our guy
            var combatObject = atk.AllCombatObjects().First(co => co is AttackCombatUnit && co.TroopStub == troopObject.Stub);

            // if battle lasts more than 5 rounds, attacker gets 3 attack points.
            if (combatObject != null && combatObject.RoundsParticipated == 5)
            {
                troopObject.BeginUpdate();
                troopObject.Stats.AttackPoint += 3;
                troopObject.EndUpdate();
            }

            // Reduce stamina and check if we need to remove this stub
            ReduceStamina(troopObject, (short)(troopObject.Stats.Stamina - 1));

            if (troopObject.Stats.Stamina == 0)
            {
                CombatGroup combatGroup = targetCity.Battle.GetCombatGroup(groupId);
                if (combatGroup == null)
                {
                    throw new Exception("Cannot find group to be removed");
                }
                targetCity.Battle.Remove(combatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
            }
        }

        private static void ReduceStamina(ITroopObject troopObject, short stamina)
        {
            troopObject.BeginUpdate();
            troopObject.Stats.Stamina = Math.Max((short)0, stamina);
            troopObject.EndUpdate();
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }
    }
}