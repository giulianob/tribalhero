using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Battle;
using Game.Database;
using Game.Util;
using Game.Setup;
using Game.Logic.Procedures;

namespace Game.Logic.Actions {
    class EngageAttackAction : PassiveAction {
        uint cityId;
        byte stubId;        
        uint targetCityId;
        AttackMode mode;
        int originalHP, remainingHP;

        public EngageAttackAction(uint cityId, byte stubId, uint targetCityId, AttackMode mode) {
            this.cityId = cityId;
            this.stubId = stubId;
            this.targetCityId = targetCityId;
            this.mode = mode;
        }

        public EngageAttackAction(ushort id, bool isVisible, Dictionary<string, string> properties)
            : base(id, isVisible) {
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);

            mode = (AttackMode)(byte.Parse(properties["mode"]));
            originalHP = int.Parse(properties["original_hp"]);
            remainingHP = int.Parse(properties["remaining_hp"]);

            targetCityId = uint.Parse(properties["target_city_id"]);

            City targetCity;
            Global.World.TryGetObjects(targetCityId, out targetCity);
            targetCity.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
            targetCity.Battle.ExitBattle += new BattleBase.OnBattle(Battle_ExitBattle);
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        private List<Structure> getStructuresInRadius(City city, TroopObject obj) {
            List<Structure> listStruct = new List<Structure>();
            foreach (Structure structure in city) {
                if (structure.distance(obj) <= obj.Stats.TotalAttackRadius || structure.distance(obj) <= structure.Stats.Base.Battle.Rng)
                    listStruct.Add(structure);
            }

            return listStruct;
        }

        public override Error execute() {

            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, out targetCity)) {
                return Error.OBJECT_NOT_FOUND;
            }

            List<TroopStub> list = new List<TroopStub>();
            list.Add(stub);
            originalHP = remainingHP = stub.TotalHP;

            if (targetCity.Battle != null) {
                targetCity.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
                targetCity.Battle.ExitBattle += new BattleBase.OnBattle(Battle_ExitBattle);
                Procedure.AddLocalToBattle(targetCity.Battle, targetCity, ReportState.Reinforced);
                targetCity.Battle.AddToLocal(getStructuresInRadius(targetCity, stub.TroopObject));
                targetCity.Battle.addToAttack(list);
            }
            else {
                targetCity.Battle = new BattleManager(targetCity);
                targetCity.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
                targetCity.Battle.ExitBattle += new BattleBase.OnBattle(Battle_ExitBattle);
                BattleAction ba = new BattleAction(targetCityId);
                targetCity.Battle.AddToLocal(getStructuresInRadius(targetCity, stub.TroopObject));
                targetCity.Battle.addToAttack(list);
                targetCity.Worker.doPassive(targetCity, ba, false);
            }

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(targetCity.CityId);
            stub.TroopObject.EndUpdate();

            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopStub.TroopState.BATTLE;
            stub.TroopObject.Stub.EndUpdate();

            return Error.OK;
        }

        void Battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            AttackCombatUnit unit = target as AttackCombatUnit;
            if (unit == null) return;

            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, out targetCity)) {
                throw new NotImplementedException();
            }

            if (unit.TroopStub == stub && unit.TroopStub.TroopObject == stub.TroopObject) {
                remainingHP -= damage;
                if (remainingHP <= Formula.GetAttackModeTolerance(originalHP, mode)) {
                    List<TroopStub> list = new List<TroopStub>();
                    list.Add(stub);
                    targetCity.Battle.removeFromAttack(list, remainingHP == 0 ? ReportState.Dying : ReportState.Retreating);
                    targetCity.Battle.ActionAttacked -= new BattleBase.OnAttack(Battle_ActionAttacked);
                    targetCity.Battle.ExitBattle -= new BattleBase.OnBattle(Battle_ExitBattle);

                    stub.TroopObject.BeginUpdate();
                    stub.TroopObject.State = GameObjectState.NormalState();
                    stub.TroopObject.EndUpdate();

                    stateChange(ActionState.COMPLETED);
                }
            }
        }

        void Battle_ExitBattle(CombatList atk, CombatList def) {
            City city;
            City targetCity;
            TroopStub stub;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, out targetCity)) {
                throw new NotImplementedException();
            }

            targetCity.Battle.ActionAttacked -= new BattleBase.OnAttack(Battle_ActionAttacked);
            targetCity.Battle.ExitBattle -= new BattleBase.OnBattle(Battle_ExitBattle);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.Stub.State = TroopStub.TroopState.IDLE;
            stub.TroopObject.Stub.EndUpdate();
            stub.TroopObject.EndUpdate();

            stateChange(ActionState.COMPLETED);
        }        

        public override void interrupt(ActionInterrupt state) {
            return;
        }

        public override ActionType Type {
            get { return ActionType.ENGAGE_ATTACK; }
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                        new XMLKVPair("target_city_id", targetCityId),
                        new XMLKVPair("troop_city_id", cityId),
                        new XMLKVPair("troop_id", stubId),
                        new XMLKVPair("mode", (byte)mode),
                        new XMLKVPair("original_hp", originalHP),
                        new XMLKVPair("remaining_hp", remainingHP)
                    }
                );
            }
        }

        #endregion
    }
}
