using System;
using System.Collections.Generic;
using System.Text;
using Game.Battle;
using Game.Setup;
using Game.Data;
using Game.Logic.Procedures;
using Game.Fighting;
using Game.Util;

namespace Game.Logic.Actions {
    class BattleAction : ScheduledPassiveAction {
        BattleViewer viewer;
        uint cityId;

        public BattleAction(uint cityId) {
            this.cityId = cityId;

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
        }

        public BattleAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += new BattleBase.OnAttack(Battle_ActionAttacked);
            viewer = new BattleViewer(city.Battle);
        }

        #region ISchedule Members
        public override void callback(object custom) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception();
            }

            using (new MultiObjectLock(city.Battle.LockList)) {
                if (!city.Battle.executeTurn()) {
                    city.Battle.ActionAttacked -= new BattleBase.OnAttack(Battle_ActionAttacked);
                    Global.dbManager.Delete(city.Battle);
                    city.Battle = null;

                    city.DefaultTroop.BeginUpdate();
                    Procedure.MoveUnitFormation(city.DefaultTroop, FormationType.InBattle, FormationType.Normal);
                    city.DefaultTroop.EndUpdate();

                    foreach (TroopStub stub in city.Troops) {
                        //only set back the state to the local troop or the ones stationed in this city
                        if (stub != city.DefaultTroop && stub.StationedCity != city)
                            continue;

                        stub.BeginUpdate();
                        if (stub.State == TroopStub.TroopState.BATTLE_STATIONED)
                            stub.State = TroopStub.TroopState.STATIONED;
                        else if (stub.State == TroopStub.TroopState.BATTLE)
                            stub.State = TroopStub.TroopState.IDLE;
                        stub.EndUpdate();
                    }

                    stateChange(ActionState.COMPLETED);
                    return;
                }
                else
                    Global.dbManager.Save(city.Battle);              

                endTime = DateTime.Now.AddSeconds(Config.battle_turn_interval);
                stateChange(ActionState.FIRED);
            }
        }

        #endregion

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        public override Error execute() {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.OBJECT_NOT_FOUND;

            Global.dbManager.Save(city.Battle);

            //Add local troop
            Procedure.AddLocalToBattle(city.Battle, city, ReportState.Entering);

            List<TroopStub> list = new List<TroopStub>();

            //Add reinforcement
            foreach (TroopStub stub in city.Troops) {
                if (stub == city.DefaultTroop || stub.State != TroopStub.TroopState.STATIONED || stub.StationedCity != city)
                    continue; //skip if troop is the default troop or isn't stationed here
                
                stub.BeginUpdate();
                stub.State = TroopStub.TroopState.BATTLE_STATIONED;
                stub.EndUpdate();
                
                list.Add(stub);
            }

            city.Battle.addToDefense(list);
            viewer = new BattleViewer(city.Battle);
            beginTime = DateTime.Now;
            endTime = DateTime.Now.AddSeconds(Config.battle_turn_interval);

            return Error.OK;
        }

        void Battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            DefenseCombatUnit cu = target as DefenseCombatUnit;
         
            if (cu == null) return;

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return;

            if (cu.TroopStub.StationedCity == city && cu.TroopStub.TotalCount == 0) { //takes care of killing out stationed troops
                List<TroopStub> list = new List<TroopStub>(1);
                list.Add(cu.TroopStub);
                city.Battle.removeFromDefense(list, ReportState.Dying);

                city.Troops.RemoveStationed(cu.TroopStub.StationedTroopId);
                cu.TroopStub.City.Troops.Remove(cu.TroopStub.TroopId);
            }
        }

        public override void interrupt(ActionInterrupt state) {
            Global.Scheduler.del(this);
        }

        public override ActionType Type {
            get { return ActionType.BATTLE; }
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                        new XMLKVPair("city_id", cityId)
                    }
                );
            }
        }

        #endregion
    }
}
