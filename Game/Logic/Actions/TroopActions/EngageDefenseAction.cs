#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class EngageDefenseAction : PassiveAction {
        private readonly byte stubId;
        private readonly uint cityId;
        private int originalHp;
        private int remainingHp;

        public EngageDefenseAction(uint cityId, byte stubId) {
            this.cityId = cityId;
            this.stubId = stubId;
        }

        public EngageDefenseAction(ushort id, bool isVisible, IDictionary<string, string> properties)
            : base(id, isVisible) {
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);
            originalHp = int.Parse(properties["original_hp"]);
            remainingHp = int.Parse(properties["remaining_hp"]);

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += Battle_ActionAttacked;
            city.Battle.ExitBattle += Battle_ExitBattle;
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                return Error.OBJECT_NOT_FOUND;

            if (city.Battle == null) {
                StateChange(ActionState.COMPLETED);
                return Error.OK;
            }

            List<TroopStub> list = new List<TroopStub> {stub};
            originalHp = remainingHp = stub.TotalHp;

            city.Battle.ActionAttacked += Battle_ActionAttacked;
            city.Battle.ExitBattle += Battle_ExitBattle;
            city.Battle.AddToDefense(list);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(cityId);
            stub.TroopObject.EndUpdate();
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopStub.TroopState.BATTLE;
            stub.TroopObject.Stub.EndUpdate();

            return Error.OK;
        }

        private void Battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            if (target.City.CityId != cityId)
                return;

            AttackCombatUnit unit = target as AttackCombatUnit;
            if (unit == null)
                return;

            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                throw new Exception();

            if (unit.TroopStub != stub)
                return;

            remainingHp -= damage;
            if (remainingHp > 0)
                return;

            List<TroopStub> list = new List<TroopStub> {stub};
            city.Battle.RemoveFromAttack(list, ReportState.DYING);
            city.Battle.ActionAttacked -= Battle_ActionAttacked;
            city.Battle.ExitBattle -= Battle_ExitBattle;

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.Stub.State = TroopStub.TroopState.IDLE;
            stub.TroopObject.EndUpdate();
            StateChange(ActionState.COMPLETED);
        }

        private void Battle_ExitBattle(CombatList atk, CombatList def) {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                throw new Exception();

            city.Battle.ActionAttacked -= Battle_ActionAttacked;
            city.Battle.ExitBattle -= Battle_ExitBattle;

            stub.TroopObject.BeginUpdate();
            stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.State = TroopStub.TroopState.IDLE;
            stub.EndUpdate();
            stub.TroopObject.EndUpdate();

            StateChange(ActionState.COMPLETED);
        }

        public override void Interrupt(ActionInterrupt state) {
            return;
        }

        public override ActionType Type {
            get { return ActionType.ENGAGE_DEFENSE; }
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                new XMLKVPair("troop_city_id", cityId), new XMLKVPair("troop_id", stubId),
                                                new XMLKVPair("original_hp", originalHp),
                                                new XMLKVPair("remaining_hp", remainingHp)
                    });
            }
        }

        #endregion
    }
}