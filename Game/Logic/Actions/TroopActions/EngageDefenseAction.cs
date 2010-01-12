#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class EngageDefenseAction : PassiveAction {
        private byte stubId;
        private uint cityId;
        private int originalHP, remainingHP;

        public EngageDefenseAction(uint cityId, byte stubId) {
            this.cityId = cityId;
            this.stubId = stubId;
        }

        public EngageDefenseAction(ushort id, bool isVisible, Dictionary<string, string> properties)
            : base(id, isVisible) {
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);
            originalHP = int.Parse(properties["original_hp"]);
            remainingHP = int.Parse(properties["remaining_hp"]);

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
            city.Battle.ExitBattle += new BattleBase.OnBattle(Battle_ExitBattle);
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        public override Error execute() {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                return Error.OBJECT_NOT_FOUND;

            if (city.Battle == null) {
                stateChange(ActionState.COMPLETED);
                return Error.OK;
            }

            List<TroopStub> list = new List<TroopStub>();
            list.Add(stub);
            originalHP = remainingHP = stub.TotalHP;

            city.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
            city.Battle.ExitBattle += new BattleBase.OnBattle(Battle_ExitBattle);
            city.Battle.addToDefense(list);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(cityId);
            stub.TroopObject.EndUpdate();
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopStub.TroopState.BATTLE;
            stub.TroopObject.Stub.EndUpdate();

            return Error.OK;
        }

        private void Battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            if (target.City.CityId == cityId) {
                AttackCombatUnit unit = target as AttackCombatUnit;
                if (unit != null) {
                    City city;
                    TroopStub stub;
                    if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                        throw new Exception();

                    if (unit.TroopStub == stub) {
                        remainingHP -= damage;
                        if (remainingHP <= 0) {
                            List<TroopStub> list = new List<TroopStub>();
                            list.Add(stub);
                            city.Battle.removeFromAttack(list, ReportState.Dying);
                            city.Battle.ActionAttacked -= new BattleBase.OnAttack(Battle_ActionAttacked);
                            city.Battle.ExitBattle -= new BattleBase.OnBattle(Battle_ExitBattle);

                            stub.TroopObject.BeginUpdate();
                            stub.TroopObject.State = GameObjectState.NormalState();
                            stub.TroopObject.Stub.State = TroopStub.TroopState.IDLE;
                            stub.TroopObject.EndUpdate();
                            stateChange(ActionState.COMPLETED);
                        }
                    }
                }
            }
        }

        private void Battle_ExitBattle(CombatList atk, CombatList def) {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                throw new Exception();

            city.Battle.ActionAttacked -= new BattleBase.OnAttack(Battle_ActionAttacked);
            city.Battle.ExitBattle -= new BattleBase.OnBattle(Battle_ExitBattle);

            stub.TroopObject.BeginUpdate();
            stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.State = TroopStub.TroopState.IDLE;
            stub.EndUpdate();
            stub.TroopObject.EndUpdate();

            stateChange(ActionState.COMPLETED);
        }

        public override void interrupt(ActionInterrupt state) {
            return;
        }

        public override ActionType Type {
            get { return ActionType.ENGAGE_DEFENSE; }
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new XMLKVPair[] {
                                                                new XMLKVPair("troop_city_id", cityId), new XMLKVPair("troop_id", stubId),
                                                                new XMLKVPair("original_hp", originalHP),
                                                                new XMLKVPair("remaining_hp", remainingHP)
                                                            });
            }
        }

        #endregion
    }
}