#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Comm.Channel;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Ninject;
using Ninject.Parameters;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    class EngageAttackPassiveAction : PassiveAction
    {
        private readonly Resource bonus;
        private readonly uint cityId;
        private readonly AttackMode mode;
        private readonly byte stubId;
        private readonly uint targetCityId;
        private int originalUnitCount;
        private int remainingUnitCount;

        public EngageAttackPassiveAction(uint cityId, byte stubId, uint targetCityId, AttackMode mode)
        {
            this.cityId = cityId;
            this.stubId = stubId;
            this.targetCityId = targetCityId;
            this.mode = mode;
            bonus = new Resource();
        }

        public EngageAttackPassiveAction(uint id, bool isVisible, IDictionary<string, string> properties) : base(id, isVisible)
        {
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);

            mode = (AttackMode)(byte.Parse(properties["mode"]));
            originalUnitCount = int.Parse(properties["original_count"]);

            targetCityId = uint.Parse(properties["target_city_id"]);

            bonus = new Resource(int.Parse(properties["crop"]),
                                 int.Parse(properties["gold"]),
                                 int.Parse(properties["iron"]),
                                 int.Parse(properties["wood"]),
                                 int.Parse(properties["labor"]));
            City targetCity;
            Global.World.TryGetObjects(targetCityId, out targetCity);
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
                                                        new XmlKvPair("target_city_id", targetCityId), new XmlKvPair("troop_city_id", cityId),
                                                        new XmlKvPair("troop_id", stubId), new XmlKvPair("mode", (byte)mode),
                                                        new XmlKvPair("original_count", originalUnitCount), new XmlKvPair("crop", bonus.Crop),
                                                        new XmlKvPair("gold", bonus.Gold), new XmlKvPair("iron", bonus.Iron), new XmlKvPair("wood", bonus.Wood),
                                                        new XmlKvPair("labor", bonus.Labor),
                                                });
            }
        }

        private void RegisterBattleListeners(City targetCity)
        {
            targetCity.Battle.ActionAttacked += BattleActionAttacked;
            targetCity.Battle.ExitBattle += BattleExitBattle;
            targetCity.Battle.WithdrawAttacker += BattleWithdrawAttacker;
            targetCity.Battle.EnterRound += BattleEnterRound;
            targetCity.Battle.UnitRemoved += BattleUnitRemoved;
            targetCity.Battle.ExitTurn += BattleExitTurn;
        }

        private void DeregisterBattleListeners(City targetCity)
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

        private static IEnumerable<Structure> GetStructuresInRadius(IEnumerable<Structure> structures, TroopObject obj)
        {
            return
                    structures.Where(
                                     structure =>
                                     SimpleGameObject.RadiusToPointFiveStyle(structure.RadiusDistance(obj)) <=
                                     SimpleGameObject.RadiusToPointFiveStyle(obj.Stats.AttackRadius) +
                                     SimpleGameObject.RadiusToPointFiveStyle(structure.Stats.Base.Radius));
        }

        public override Error Execute()
        {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                return Error.ObjectNotFound;

            var list = new List<TroopStub> {stub};
            originalUnitCount = stub.TotalCount;

            if (targetCity.Battle != null)
            {
                RegisterBattleListeners(targetCity);

                Procedure.AddLocalToBattle(targetCity.Battle, targetCity, ReportState.Reinforced);

                targetCity.Battle.AddToLocal(GetStructuresInRadius(targetCity, stub.TroopObject));

                targetCity.Battle.AddToAttack(list);
            }
            else
            {
                targetCity.Battle = Ioc.Kernel.Get<BattleManager.Factory>()(targetCity);

                RegisterBattleListeners(targetCity);

                var ba = new BattlePassiveAction(targetCityId);

                targetCity.Battle.AddToLocal(GetStructuresInRadius(targetCity, stub.TroopObject));

                targetCity.Battle.AddToAttack(list);
                targetCity.Worker.DoPassive(targetCity, ba, false);
            }

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(targetCity.Id);
            stub.TroopObject.Stats.Stamina = BattleFormulas.GetStamina(stub, targetCity);
            stub.TroopObject.EndUpdate();

            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopState.Battle;
            stub.TroopObject.Stub.EndUpdate();

            return Error.Ok;
        }

        private void BattleWithdrawAttacker(IEnumerable<CombatObject> list)
        {
            TroopStub stub;
            City targetCity;
            City city;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            bool retreat = list.Any(co => co is AttackCombatUnit && ((AttackCombatUnit)co).TroopStub == stub);

            if (!retreat)
                return;

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
        private void BattleUnitRemoved(CombatObject co)
        {
            TroopStub stub;
            City targetCity;
            City city;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            // If this combat object is ours and all the units are dead, then remove it
            if (!(co is AttackCombatUnit) || ((AttackCombatUnit)co).TroopStub != stub || ((AttackCombatUnit)co).TroopStub.TotalCount > 0)
                return;

            DeregisterBattleListeners(targetCity);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.Completed);
        }

        private void SetLootedResources(IBattleManager battle, TroopStub stub)
        {
            if (!battle.BattleStarted)
                return;
            
            // Calculate bonus
            Resource resource = BattleFormulas.GetBonusResources(stub.TroopObject, originalUnitCount, remainingUnitCount);

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

        private void BattleExitTurn(CombatList atk, CombatList def, int turn)
        {
            City city;            
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                throw new ArgumentException();

            // Remove troop from battle if he is out of stamina, we need to check here because he might have lost
            // some stamina after knocking down a building
            if (stub.TroopObject.Stats.Stamina == 0)
            {
                City targetCity;
                if (!Global.World.TryGetObjects(targetCityId, out targetCity))
                    throw new ArgumentException();

                targetCity.Battle.RemoveFromAttack(new List<TroopStub> {stub}, ReportState.OutOfStamina);
            }
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            var unit = target as AttackCombatUnit;
            if (unit == null)
            {
                if (target.ClassType == BattleClass.Structure && target.IsDead)
                {
                    // if our troop knocked down a building, we get the bonus.
                    if (((AttackCombatUnit)source).TroopStub == stub)
                    {
                        bonus.Add(Ioc.Kernel.Get<StructureFactory>().GetCost(target.Type, target.Lvl)/2);

                        Structure structure = ((CombatStructure)target).Structure;
                        object value;
                        if(structure.Properties.TryGet("Crop",out value))
                            bonus.Crop+=(int)value;
                        if (structure.Properties.TryGet("Gold", out value))
                            bonus.Gold += (int)value;
                        if (structure.Properties.TryGet("Iron", out value))
                            bonus.Iron += (int)value;
                        if (structure.Properties.TryGet("Wood", out value))
                            bonus.Wood += (int)value;
                        if (structure.Properties.TryGet("Labor", out value))
                            bonus.Labor += (int)value;

                        Ioc.Kernel.Get<IDbManager>().Save(this);
                    }

                    ReduceStamina(stub, BattleFormulas.GetStaminaStructureDestroyed(stub.TroopObject.Stats.Stamina));
                }                
            }
            // Check if the unit being attacked belongs to us
            else if (unit.TroopStub == stub && unit.TroopStub.TroopObject == stub.TroopObject)
            {
                // Check to see if player should retreat
                remainingUnitCount = stub.TotalCount;

                // Don't return if we haven't fulfilled the minimum rounds or not below the threshold
                if (unit.RoundsParticipated < Config.battle_min_rounds || remainingUnitCount == 0 || remainingUnitCount > Formula.GetAttackModeTolerance(originalUnitCount, mode))
                    return;

                targetCity.Battle.RemoveFromAttack(new List<TroopStub> {stub}, ReportState.Retreating);
            }
        }

        private void BattleExitBattle(CombatList atk, CombatList def)
        {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

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
        
        private void BattleEnterRound(CombatList atk, CombatList def, uint round)
        {
            City city;
            TroopStub stub;
            City targetCity;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            // if battle lasts more than 5 rounds, attacker gets 3 attack points.
            if(round==5)
            {
                stub.TroopObject.BeginUpdate();
                stub.TroopObject.Stats.AttackPoint += 3;
                stub.TroopObject.EndUpdate();
            }

            // Reduce stamina and check if we need to remove this stub
            ReduceStamina(stub, (short)(stub.TroopObject.Stats.Stamina - 1));

            if (stub.TroopObject.Stats.Stamina == 0)
                targetCity.Battle.RemoveFromAttack(new List<TroopStub> { stub }, ReportState.OutOfStamina);
        }

        private static void ReduceStamina(TroopStub stub, short stamina)
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