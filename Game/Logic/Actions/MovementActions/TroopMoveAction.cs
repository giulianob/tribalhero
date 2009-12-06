using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Map;
using Game.Util;
using Game.Database;
using Game.Setup;

namespace Game.Logic.Actions {
    class TroopMoveAction : ScheduledPassiveAction {
        uint cityId;
        uint troopObjectId;
        int distanceRemaining;
        uint x, nextX;
        uint y, nextY;

        public TroopMoveAction(uint cityId, uint troopObjectId, uint x, uint y) {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.x = x;
            this.y = y;
        }

        public TroopMoveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            nextX = uint.Parse(properties["next_x"]);
            nextY = uint.Parse(properties["next_y"]);
            distanceRemaining = int.Parse(properties["distance_remaining"]);
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        class Record_Foreach {
            public int shortest_distance;
            public uint x;
            public uint y;
            public bool isShortestDistanceDiagonal;
        }

        bool work(uint ox, uint oy, uint x, uint y, object custom) {
            Record_Foreach record_foreach = (Record_Foreach)custom;
            int distance = GameObject.distance(x, y, this.x, this.y);

            if (distance < record_foreach.shortest_distance) {
                record_foreach.shortest_distance = distance;
                record_foreach.x = x;
                record_foreach.y = y;
                record_foreach.isShortestDistanceDiagonal = GameObject.isDiagonal(x, y, ox, oy);
            }
            else if (distance == record_foreach.shortest_distance && !record_foreach.isShortestDistanceDiagonal) {
                record_foreach.shortest_distance = distance;
                record_foreach.x = x;
                record_foreach.y = y;
                record_foreach.isShortestDistanceDiagonal = GameObject.isDiagonal(x, y, ox, oy);
            }
            return true;
        }

        bool calculateNext(TroopObject obj) {
            int distance = obj.distance(x, y);
            if (distance == 0) {
                return false;
            }
            Record_Foreach record_foreach = new Record_Foreach();
            record_foreach.shortest_distance = int.MaxValue;
            record_foreach.isShortestDistanceDiagonal = false;
            RadiusLocator.foreach_object(obj.X, obj.Y, 1, false, work, record_foreach);
            nextX = record_foreach.x;
            nextY = record_foreach.y;
            nextTime = DateTime.Now.AddSeconds(Math.Max(1, Formula.MoveTime(obj.Stats.BaseSpeed) * Setup.Config.seconds_per_unit));
            return true;
        }

        public override Error execute() {
            City city;
            TroopObject troopObj;

            if (!Global.World.TryGetObjects(cityId, troopObjectId, out city, out troopObj)) {
                return Error.OBJECT_NOT_FOUND;
            }

            distanceRemaining = troopObj.distance(x, y);
            endTime = DateTime.Now.AddSeconds(Math.Max(1, Formula.MoveTime(troopObj.Stats.BaseSpeed) * Setup.Config.seconds_per_unit) * distanceRemaining);
            beginTime = DateTime.Now;

            troopObj.Stub.BeginUpdate();
            troopObj.Stub.State = TroopStub.TroopState.MOVING;

            if (!calculateNext(troopObj)) {
                troopObj.Stub.State = TroopStub.TroopState.IDLE;
                stateChange(ActionState.COMPLETED);
                troopObj.Stub.EndUpdate();
                return 0;
            }
            troopObj.Stub.EndUpdate();

            return Error.OK;
        }

        public override void interrupt(ActionInterrupt state) {
            City city;
            TroopObject troopObj;

            using (new MultiObjectLock(cityId, troopObjectId, out city, out troopObj)) {
                switch (state) {
                    case ActionInterrupt.CANCEL:
                        x = city.MainBuilding.X;
                        y = city.MainBuilding.Y;
                        if (!calculateNext(troopObj)) {
                            Global.Scheduler.del(this);
                            stateChange(ActionState.COMPLETED);
                            return;
                        }
                        break;
                }
            }
        }

        public override ActionType Type {
            get { return ActionType.TROOP_MOVE; }
        }

        #region ISchedule Members

        public override void callback(object custom) {
            City city;
            TroopObject troopObj;

            using (new MultiObjectLock(cityId, troopObjectId, out city, out troopObj)) {
                troopObj.BeginUpdate();
                troopObj.X = nextX;
                troopObj.Y = nextY;
                --distanceRemaining;
                troopObj.EndUpdate();

                if (!calculateNext(troopObj)) {
                    troopObj.Stub.BeginUpdate();
                    troopObj.Stub.State = TroopStub.TroopState.IDLE;
                    troopObj.Stub.EndUpdate();
                    stateChange(ActionState.COMPLETED);
                    return;
                }

                stateChange(ActionState.FIRED);
            }
        }

        #endregion

        #region IPersistable Members

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                        new XMLKVPair("city_id", cityId),
                        new XMLKVPair("troop_id", troopObjectId),
                        new XMLKVPair("x", x),
                        new XMLKVPair("y", y),
                        new XMLKVPair("next_x", nextX),
                        new XMLKVPair("next_y", nextY),
                        new XMLKVPair("distance_remaining", distanceRemaining)
                    }
                );
            }
        }

        #endregion
    }
}
