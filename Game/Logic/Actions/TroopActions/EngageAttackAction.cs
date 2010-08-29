#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class EngageAttackAction : PassiveAction {
        private readonly uint cityId;
        private readonly byte stubId;
        private readonly uint targetCityId;
        private readonly AttackMode mode;
        private int originalHp;
        private int remainingHp;

        public EngageAttackAction(uint cityId, byte stubId, uint targetCityId, AttackMode mode) {
            this.cityId = cityId;
            this.stubId = stubId;
            this.targetCityId = targetCityId;
            this.mode = mode;
        }

        public EngageAttackAction(uint id, bool isVisible, IDictionary<string, string> properties)
            : base(id, isVisible) {
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);

            mode = (AttackMode)(byte.Parse(properties["mode"]));
            originalHp = int.Parse(properties["original_hp"]);
            remainingHp = int.Parse(properties["remaining_hp"]);

            targetCityId = uint.Parse(properties["target_city_id"]);

            City targetCity;
            Global.World.TryGetObjects(targetCityId, out targetCity);
            RegisterBattleListeners(targetCity);
        }

        private void RegisterBattleListeners(City targetCity) {
            targetCity.Battle.ActionAttacked += Battle_ActionAttacked;
            targetCity.Battle.ExitBattle += Battle_ExitBattle;
            targetCity.Battle.WithdrawAttacker += Battle_WithdrawAttacker;            
        }

        private void DeregisterBattleListeners(City targetCity) {
            targetCity.Battle.ActionAttacked -= Battle_ActionAttacked;
            targetCity.Battle.ExitBattle -= Battle_ExitBattle;
            targetCity.Battle.WithdrawAttacker -= Battle_WithdrawAttacker;
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        private static List<Structure> GetStructuresInRadius(IEnumerable<Structure> structures, TroopObject obj) {
            List<Structure> listStruct = new List<Structure>();
            foreach (Structure structure in structures) {
                if (structure.RadiusDistance(obj) <= obj.Stats.AttackRadius ||
                    structure.RadiusDistance(obj) <= structure.Stats.Base.Radius)
                    listStruct.Add(structure);
            }

            return listStruct;
        }

        private static IEnumerable<Structure> GetProtectingStructures(IEnumerable<Structure> city, IEnumerable<Structure> structuresBeingAttacked, TroopObject attacker) {
            List<Structure> protectingStructures = new List<Structure>();
            foreach (Structure structure in city) {
                if (structure.Stats.Base.Radius == 0)
                    continue;

                bool found = false;

                foreach (Structure structureBeingAttacked in structuresBeingAttacked) {
                    if (structureBeingAttacked == structure) {
                        found = false;
                        break;
                    }
                    
                    if (structure.RadiusDistance(structureBeingAttacked) <= structure.Stats.Base.Radius) {
                        found = true;
                    }                                        
                }

                if (found)
                    protectingStructures.Add(structure);
            }

            return protectingStructures;
        }

        public override Error Execute() {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, out targetCity))
                return Error.OBJECT_NOT_FOUND;

            List<TroopStub> list = new List<TroopStub> { stub };
            originalHp = remainingHp = stub.TotalHp;

            if (targetCity.Battle != null) {                
                RegisterBattleListeners(targetCity);
                
                Procedure.AddLocalToBattle(targetCity.Battle, targetCity, ReportState.REINFORCED);
                
                List<Structure> defenders = GetStructuresInRadius(targetCity, stub.TroopObject);
                defenders.AddRange(GetProtectingStructures(targetCity, defenders, stub.TroopObject));
                targetCity.Battle.AddToLocal(defenders);                
                
                targetCity.Battle.AddToAttack(list);
            }
            else {
                targetCity.Battle = new BattleManager(targetCity);
                
                RegisterBattleListeners(targetCity);

                BattleAction ba = new BattleAction(targetCityId);
                
                List<Structure> defenders = GetStructuresInRadius(targetCity, stub.TroopObject);
                defenders.AddRange(GetProtectingStructures(targetCity, defenders, stub.TroopObject));
                targetCity.Battle.AddToLocal(defenders);                

                targetCity.Battle.AddToAttack(list);
                targetCity.Worker.DoPassive(targetCity, ba, false);
            }

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(targetCity.Id);
            stub.TroopObject.EndUpdate();

            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopState.BATTLE;
            stub.TroopObject.Stub.EndUpdate();

            return Error.OK;
        }

        private void Battle_WithdrawAttacker(IEnumerable<CombatObject> list) {
            TroopStub stub;
            City targetCity;
            City city;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            bool retreat = list.Any(co => co is AttackCombatUnit && ((AttackCombatUnit) co).TroopStub == stub);

            if (!retreat) return;

            DeregisterBattleListeners(targetCity);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            SetLootedResources(targetCity.Battle, stub);
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.COMPLETED);
        }

        private void SetLootedResources(BattleManager battle, TroopStub stub) {
            if (!battle.BattleStarted) return;

            // Calculate bonus
            Resource resource = BattleFormulas.GetBonusResources(stub.TroopObject);

            // Copy looted resources since we'll be modifying the troop's loot variable
            Resource looted = new Resource(stub.TroopObject.Stats.Loot);

            // Add bonus to troop object            
            Resource returning;
            Resource actual;
            stub.TroopObject.Stats.Loot.Add(resource, stub.Carry, out actual, out returning);            
            
            // Update battle report view with actual received bonus            
            battle.BattleReport.SetLootedResources(stub.City.Id, stub.TroopId, battle.BattleId, looted, actual);
        }

        private void Battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            AttackCombatUnit unit = target as AttackCombatUnit;
            if (unit == null)
                return;

            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            if (unit.TroopStub != stub || unit.TroopStub.TroopObject != stub.TroopObject)
                return;

            // Check to see if player should retreat
            remainingHp -= damage;
            if (unit.RoundsParticipated < Config.battle_min_rounds || remainingHp > Formula.GetAttackModeTolerance(originalHp, mode))
                return;

            List<TroopStub> list = new List<TroopStub> {
                                                           stub
                                                       };
            targetCity.Battle.RemoveFromAttack(list, remainingHp == 0 ? ReportState.DYING : ReportState.RETREATING);
        }

        private void Battle_ExitBattle(CombatList atk, CombatList def) {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            DeregisterBattleListeners(targetCity);

            stub.TroopObject.BeginUpdate();
            SetLootedResources(targetCity.Battle, stub);
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.Stub.State = TroopState.IDLE;
            stub.TroopObject.Stub.EndUpdate();
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.COMPLETED);
        }

        public override void UserCancelled() {            
        }

        public override void WorkerRemoved(bool wasKilled) {            
        }

        public override ActionType Type {
            get { return ActionType.ENGAGE_ATTACK; }
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("target_city_id", targetCityId),
                                                                new XMLKVPair("troop_city_id", cityId), new XMLKVPair("troop_id", stubId),
                                                                new XMLKVPair("mode", (byte) mode),
                                                                new XMLKVPair("original_hp", originalHp),
                                                                new XMLKVPair("remaining_hp", remainingHp)
                                                            });
            }
        }

        #endregion
    }
}