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

        private readonly byte stubId;

        private readonly uint targetCityId;

        private int originalUnitCount;

        private int remainingUnitCount;

        public EngageAttackPassiveAction(uint cityId,
                                         byte stubId,
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
            this.stubId = stubId;
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
            stubId = byte.Parse(properties["troop_id"]);
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
                                new XmlKvPair("target_city_id", targetCityId), new XmlKvPair("troop_city_id", cityId), new XmlKvPair("troop_id", stubId),
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
            ITroopStub stub;

            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                return Error.ObjectNotFound;
            }

            originalUnitCount = stub.TotalCount;

            battleProcedure.JoinOrCreateBattle(targetCity, stub.TroopObject, out groupId);

            RegisterBattleListeners(targetCity);

            // Set the attacking troop object to the correct state and stamina
            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(targetCity.Id);
            stub.TroopObject.Stats.Stamina = battleFormula.GetStamina(stub, targetCity);
            stub.TroopObject.EndUpdate();

            // Set the troop stub to the correct state
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopState.Battle;
            stub.TroopObject.Stub.EndUpdate();

            return Error.Ok;
        }

        private void BattleWithdrawAttacker(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            ITroopStub stub;
            ICity targetCity;
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            bool retreat = list.Any(co => co is AttackCombatUnit && co.TroopStub == stub);

            if (!retreat)
            {
                return;
            }

            DeregisterBattleListeners(targetCity);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            SetLootedResources(targetCity.Battle, stub);
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        /// <summary>
        /// Takes care of finishing this action up if all our units are killed
        /// </summary>
        private void BattleUnitRemoved(IBattleManager battle, CombatObject co)
        {
            ITroopStub stub;
            ICity targetCity;
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            // If this combat object is ours and all the units are dead, then remove it
            if (!(co is AttackCombatUnit) || co.TroopStub != stub || co.TroopStub.TotalCount > 0)
            {
                return;
            }

            DeregisterBattleListeners(targetCity);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        private void SetLootedResources(IBattleManager battle, ITroopStub stub)
        {
            if (!battle.BattleStarted)
            {
                return;
            }

            // Calculate bonus
            Resource resource = battleFormula.GetBonusResources(stub.TroopObject, originalUnitCount, remainingUnitCount);

            // Destroyed Structure bonus
            resource.Add(bonus);

            // Copy looted resources since we'll be modifying the troop's loot variable
            var looted = new Resource(stub.TroopObject.Stats.Loot);

            // Add bonus to troop object            
            Resource returning;
            Resource actual;
            Resource cap = new Resource(stub.Carry/Config.resource_crop_ratio,
                                        stub.Carry/Config.resource_gold_ratio,
                                        stub.Carry/Config.resource_iron_ratio,
                                        stub.Carry/Config.resource_wood_ratio,
                                        stub.Carry/Config.resource_labor_ratio);

            stub.TroopObject.Stats.Loot.Add(resource, cap, out actual, out returning);

            // Update battle report view with actual received bonus            
            battle.BattleReport.SetLootedResources(stub.City.Id, stub.TroopId, battle.BattleId, looted, actual);
        }

        private void BattleExitTurn(IBattleManager battle, ICombatList atk, ICombatList def, int turn)
        {
            ICity city;
            ITroopStub stub;
            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub))
            {
                throw new ArgumentException();
            }

            // Remove troop from battle if he is out of stamina, we need to check here because he might have lost
            // some stamina after knocking down a building
            if (stub.TroopObject.Stats.Stamina == 0)
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
            ITroopStub stub;

            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            var unit = target as AttackCombatUnit;
            if (unit == null)
            {
                if (target.ClassType == BattleClass.Structure && target.IsDead)
                {
                    // if our troop knocked down a building, we get the bonus.
                    if (source.TroopStub == stub)
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

                    ReduceStamina(stub, battleFormula.GetStaminaStructureDestroyed(stub.TroopObject.Stats.Stamina, target as CombatStructure));
                }
            }
            // Check if the unit being attacked belongs to us
            else if (unit.TroopStub == stub && unit.TroopStub.TroopObject == stub.TroopObject)
            {
                // Check to see if player should retreat
                remainingUnitCount = stub.TotalCount;

                // Don't return if we haven't fulfilled the minimum rounds or not below the threshold
                if (unit.RoundsParticipated < Config.battle_min_rounds || remainingUnitCount == 0 ||
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

            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            DeregisterBattleListeners(targetCity);

            stub.TroopObject.BeginUpdate();
            SetLootedResources(targetCity.Battle, stub);
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.Stub.State = TroopState.Idle;
            stub.TroopObject.Stub.EndUpdate();
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            ICity city;
            ITroopStub stub;
            ICity targetCity;

            if (!gameObjectLocator.TryGetObjects(cityId, stubId, out city, out stub) || !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                throw new ArgumentException();
            }

            // Find our guy
            var combatObject = atk.AllCombatObjects().First(co => co is AttackCombatUnit && co.TroopStub == stub);

            // if battle lasts more than 5 rounds, attacker gets 3 attack points.
            if (combatObject != null && combatObject.RoundsParticipated == 5)
            {
                stub.TroopObject.BeginUpdate();
                stub.TroopObject.Stats.AttackPoint += 3;
                stub.TroopObject.EndUpdate();
            }

            // Reduce stamina and check if we need to remove this stub
            ReduceStamina(stub, (short)(stub.TroopObject.Stats.Stamina - 1));

            if (stub.TroopObject.Stats.Stamina == 0)
            {
                CombatGroup combatGroup = targetCity.Battle.GetCombatGroup(groupId);
                if (combatGroup == null)
                {
                    throw new Exception("Cannot find group to be removed");
                }
                targetCity.Battle.Remove(combatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
            }
        }

        private static void ReduceStamina(ITroopStub stub, short stamina)
        {
            stub.TroopObject.BeginUpdate();
            stub.TroopObject.Stats.Stamina = Math.Max((short)0, stamina);
            stub.TroopObject.EndUpdate();
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }
    }
}