#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class BattleAction : ScheduledPassiveAction {
        private uint cityId;

        public BattleAction(uint cityId) {
            this.cityId = cityId;

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += Battle_ActionAttacked;
        }

        public BattleAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                            IDictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += Battle_ActionAttacked;
        }

        #region ISchedule Members

        public override void Callback(object custom) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");                
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate {                
                List<ILockable> toBeLocked = new List<ILockable>();
                toBeLocked.AddRange(city.Battle.LockList);

                foreach (TroopStub stub in city.Troops.StationedHere())
                    toBeLocked.Add(stub.City);

                return toBeLocked.ToArray();
            };            

            using (new CallbackLock(lockHandler, null, city)) {
                if (!city.Battle.ExecuteTurn()) {
                    city.Battle.ActionAttacked -= Battle_ActionAttacked;
                    Global.DbManager.Delete(city.Battle);
                    city.Battle = null;

                    city.DefaultTroop.BeginUpdate();
                    city.DefaultTroop.Template.ClearStats();
                    Procedure.MoveUnitFormation(city.DefaultTroop, FormationType.IN_BATTLE, FormationType.NORMAL);
                    city.DefaultTroop.EndUpdate();

                    //Copy troop stubs from city since the foreach loop below will modify it during the loop
                    List<TroopStub> stubsCopy = new List<TroopStub>(city.Troops);

                    foreach (TroopStub stub in stubsCopy) {
                        //only set back the state to the local troop or the ones stationed in this city
                        if (stub != city.DefaultTroop && stub.StationedCity != city)
                            continue;

                        if (stub.StationedCity == city && stub.TotalCount == 0) {
                            city.Troops.RemoveStationed(stub.StationedTroopId);
                            stub.City.Troops.Remove(stub.TroopId);
                            continue;
                        }

                        stub.BeginUpdate();
                        switch (stub.State) {
                            case TroopState.BATTLE_STATIONED:
                                stub.State = TroopState.STATIONED;
                                break;
                            case TroopState.BATTLE:
                                stub.State = TroopState.IDLE;
                                break;
                        }
                        stub.EndUpdate();
                    }

                    StateChange(ActionState.COMPLETED);
                }
                else {
                    Global.DbManager.Save(city.Battle);
                    endTime = DateTime.Now.AddSeconds(Config.battle_turn_interval);
                    StateChange(ActionState.FIRED);
                }
            }
        }

        #endregion

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.OBJECT_NOT_FOUND;

            Global.DbManager.Save(city.Battle);

            //Add local troop
            Procedure.AddLocalToBattle(city.Battle, city, ReportState.ENTERING);

            List<TroopStub> list = new List<TroopStub>();

            //Add reinforcement
            foreach (TroopStub stub in city.Troops) {
                if (stub == city.DefaultTroop || stub.State != TroopState.STATIONED ||
                    stub.StationedCity != city)
                    continue; //skip if troop is the default troop or isn't stationed here

                stub.BeginUpdate();
                stub.State = TroopState.BATTLE_STATIONED;
                stub.EndUpdate();

                list.Add(stub);
            }

            city.Battle.AddToDefense(list);
            beginTime = DateTime.Now;
            endTime = DateTime.Now.AddSeconds(Config.battle_turn_interval);

            return Error.OK;
        }

        private void Battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            DefenseCombatUnit cu = target as DefenseCombatUnit;

            if (cu == null)
                return;

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return;

            if (cu.TroopStub.StationedCity == city && cu.TroopStub.TotalCount == 0) {
                //takes care of killing out stationed troops
                List<TroopStub> list = new List<TroopStub>(1) {cu.TroopStub};
                city.Battle.RemoveFromDefense(list, ReportState.DYING);
            }
        }

        public override void Interrupt(ActionInterrupt state) {
            throw new Exception("This action cannot be cancelled.");
        }

        public override ActionType Type {
            get { return ActionType.BATTLE; }
        }

        #region IPersistable Members

        public override string Properties {
            get { return XMLSerializer.Serialize(new[] {new XMLKVPair("city_id", cityId)}); }
        }

        #endregion
    }
}