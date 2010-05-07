#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
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
            targetCity.Battle.ActionAttacked += Battle_ActionAttacked;
            targetCity.Battle.ExitBattle += Battle_ExitBattle;
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        private static List<Structure> GetStructuresInRadius(IEnumerable<Structure> structures, TroopObject obj) {
            List<Structure> listStruct = new List<Structure>();
            foreach (Structure structure in structures) {
                if (structure.TileDistance(obj) <= obj.Stats.AttackRadius ||
                    structure.TileDistance(obj) <= structure.Stats.Base.Radius)
                    listStruct.Add(structure);
            }

            return listStruct;
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
                targetCity.Battle.ActionAttacked += Battle_ActionAttacked;
                targetCity.Battle.ExitBattle += Battle_ExitBattle;
                Procedure.AddLocalToBattle(targetCity.Battle, targetCity, ReportState.REINFORCED);
                targetCity.Battle.AddToLocal(GetStructuresInRadius(targetCity, stub.TroopObject));
                targetCity.Battle.AddToAttack(list);
            }
            else {
                targetCity.Battle = new BattleManager(targetCity);
                targetCity.Battle.ActionAttacked += Battle_ActionAttacked;
                targetCity.Battle.ExitBattle += Battle_ExitBattle;
                BattleAction ba = new BattleAction(targetCityId);
                targetCity.Battle.AddToLocal(GetStructuresInRadius(targetCity, stub.TroopObject));
                targetCity.Battle.AddToAttack(list);
                targetCity.Worker.DoPassive(targetCity, ba, false);
            }

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(targetCity.Id);
            stub.TroopObject.EndUpdate();

            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopStub.TroopState.BATTLE;
            stub.TroopObject.Stub.EndUpdate();

            return Error.OK;
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
            targetCity.Battle.ActionAttacked -= Battle_ActionAttacked;
            targetCity.Battle.ExitBattle -= Battle_ExitBattle;

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.COMPLETED);
        }

        private void Battle_ExitBattle(CombatList atk, CombatList def) {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, out targetCity))
                throw new ArgumentException();

            targetCity.Battle.ActionAttacked -= Battle_ActionAttacked;
            targetCity.Battle.ExitBattle -= Battle_ExitBattle;

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.Stub.State = TroopStub.TroopState.IDLE;
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