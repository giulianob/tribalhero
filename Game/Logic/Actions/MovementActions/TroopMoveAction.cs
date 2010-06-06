#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class TroopMoveAction : ScheduledPassiveAction {
        private uint cityId;
        private uint troopObjectId;
        private int distanceRemaining;
        private uint x, nextX;
        private uint y, nextY;

        public TroopMoveAction(uint cityId, uint troopObjectId, uint x, uint y) {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.x = x;
            this.y = y;
        }

        public TroopMoveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                               Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            nextX = uint.Parse(properties["next_x"]);
            nextY = uint.Parse(properties["next_y"]);
            distanceRemaining = int.Parse(properties["distance_remaining"]);
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        private class RecordForeach {
            public int shortestDistance;
            public uint x;
            public uint y;
            public bool isShortestDistanceDiagonal;
        }

        private bool Work(uint ox, uint oy, uint x, uint y, object custom) {
            RecordForeach recordForeach = (RecordForeach) custom;
            int distance = SimpleGameObject.TileDistance(x, y, this.x, this.y);

            if (distance < recordForeach.shortestDistance) {
                recordForeach.shortestDistance = distance;
                recordForeach.x = x;
                recordForeach.y = y;
                recordForeach.isShortestDistanceDiagonal = SimpleGameObject.IsDiagonal(x, y, ox, oy);
            } else if (distance == recordForeach.shortestDistance && !recordForeach.isShortestDistanceDiagonal) {
                recordForeach.shortestDistance = distance;
                recordForeach.x = x;
                recordForeach.y = y;
                recordForeach.isShortestDistanceDiagonal = SimpleGameObject.IsDiagonal(x, y, ox, oy);
            }
            return true;
        }

        private bool CalculateNext(TroopObject obj) {
            int distance = obj.TileDistance(x, y);
            if (distance == 0)
                return false;
            RecordForeach recordForeach = new RecordForeach {shortestDistance = int.MaxValue, isShortestDistanceDiagonal = false};
            TileLocator.foreach_object(obj.X, obj.Y, 1, false, Work, recordForeach);
            nextX = recordForeach.x;
            nextY = recordForeach.y;

            int mod = 100;

            foreach (Effect effect in obj.City.Technologies.GetEffects(EffectCode.TroopSpeedMod, EffectInheritance.ALL))
                mod -= (int) effect.value[0];
            if (mod < 50)
                mod = 50;
            nextTime =
                DateTime.UtcNow.AddSeconds(Math.Max(1, Formula.MoveTime(obj.Stats.Speed)*Config.seconds_per_unit*mod/100));
            return true;
        }

        public override Error Execute() {
            City city;
            TroopObject troopObj;

            if (!Global.World.TryGetObjects(cityId, troopObjectId, out city, out troopObj))
                return Error.OBJECT_NOT_FOUND;

            distanceRemaining = troopObj.TileDistance(x, y);
            endTime =
                DateTime.UtcNow.AddSeconds(Math.Max(1, Formula.MoveTime(troopObj.Stats.Speed)*Config.seconds_per_unit)*
                                        distanceRemaining);
            beginTime = DateTime.UtcNow;

            troopObj.Stub.BeginUpdate();
            troopObj.Stub.State = TroopState.MOVING;

            if (!CalculateNext(troopObj)) {
                troopObj.Stub.State = TroopState.IDLE;
                StateChange(ActionState.COMPLETED);
                troopObj.Stub.EndUpdate();
                return 0;
            }
            troopObj.Stub.EndUpdate();

            return Error.OK;
        }

        public override void UserCancelled() {            
        }

        public override void WorkerRemoved(bool wasKilled) {
            StateChange(ActionState.FAILED);
        }

        public override ActionType Type {
            get { return ActionType.TROOP_MOVE; }
        }

        #region ISchedule Members

        public override void Callback(object custom) {
            City city;
            TroopObject troopObj;

            using (new MultiObjectLock(cityId, troopObjectId, out city, out troopObj)) {
                troopObj.BeginUpdate();
                troopObj.X = nextX;
                troopObj.Y = nextY;
                --distanceRemaining;
                troopObj.EndUpdate();

                if (!CalculateNext(troopObj)) {
                    troopObj.Stub.BeginUpdate();
                    troopObj.Stub.State = TroopState.IDLE;
                    troopObj.Stub.EndUpdate();
                    StateChange(ActionState.COMPLETED);
                    return;
                }

                StateChange(ActionState.FIRED);
            }
        }

        #endregion

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("city_id", cityId), new XMLKVPair("troop_id", troopObjectId),
                                                                new XMLKVPair("x", x), new XMLKVPair("y", y),
                                                                new XMLKVPair("next_x", nextX), new XMLKVPair("next_y", nextY),
                                                                new XMLKVPair("distance_remaining", distanceRemaining)
                                                            });
            }
        }

        #endregion
    }
}