#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class TroopMoveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;
        private readonly Boolean isReturningHome;
        private readonly uint troopObjectId;
        private readonly uint x;
        private readonly uint y;
        private int distanceRemaining;
        private uint nextX;
        private uint nextY;
        private double speedMod;

        // non-persist variable
        private Boolean isAttacking;

        public TroopMoveAction(uint cityId, uint troopObjectId, uint x, uint y, bool isReturningHome, bool isAttacking)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.x = x;
            this.y = y;
            this.isReturningHome = isReturningHome;
            this.isAttacking = isAttacking;
        }

        public TroopMoveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible)
        {
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            nextX = uint.Parse(properties["next_x"]);
            nextY = uint.Parse(properties["next_y"]);
            distanceRemaining = int.Parse(properties["distance_remaining"]);
            isReturningHome = Boolean.Parse(properties["returning_home"]);
            speedMod = double.Parse(properties["speed_mod"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.TroopMove;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                                                {
                                                        new XmlKvPair("city_id", cityId), new XmlKvPair("troop_id", troopObjectId), new XmlKvPair("x", x),
                                                        new XmlKvPair("y", y), new XmlKvPair("next_x", nextX), new XmlKvPair("next_y", nextY),
                                                        new XmlKvPair("distance_remaining", distanceRemaining), new XmlKvPair("returning_home", isReturningHome),
                                                        new XmlKvPair("speed_mod", speedMod)
                                                });
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        private bool Work(uint ox, uint oy, uint x, uint y, object custom)
        {
            var recordForeach = (RecordForeach)custom;
            int distance = SimpleGameObject.TileDistance(x, y, this.x, this.y);

            if (distance < recordForeach.ShortestDistance)
            {
                recordForeach.ShortestDistance = distance;
                recordForeach.X = x;
                recordForeach.Y = y;
                recordForeach.IsShortestDistanceDiagonal = SimpleGameObject.IsDiagonal(x, y, ox, oy);
            }
            else if (distance == recordForeach.ShortestDistance && !recordForeach.IsShortestDistanceDiagonal)
            {
                recordForeach.ShortestDistance = distance;
                recordForeach.X = x;
                recordForeach.Y = y;
                recordForeach.IsShortestDistanceDiagonal = SimpleGameObject.IsDiagonal(x, y, ox, oy);
            }
            return true;
        }

        private bool CalculateNext(TroopObject obj)
        {
            int distance = obj.TileDistance(x, y);
            if (distance == 0)
                return false;
            RecordForeach recordForeach = new RecordForeach {shortestDistance = int.MaxValue, isShortestDistanceDiagonal = false};
            TileLocator.foreach_object(obj.X, obj.Y, 1, false, Work, recordForeach);
            nextX = recordForeach.x;
            nextY = recordForeach.y;
            nextTime = DateTime.UtcNow.AddSeconds(Math.Max(1, Formula.MoveTime(obj.Stats.Speed) * Config.seconds_per_unit * speedMod / 100));

            --distanceRemaining;
            return true;
        }

        public override Error Execute()
        {
            City city;
            TroopObject troopObj;

            if (!Global.World.TryGetObjects(cityId, troopObjectId, out city, out troopObj))
                return Error.ObjectNotFound;


            distanceRemaining = troopObj.TileDistance(x, y);

            speedMod = Formula.MoveTimeMod(city, distanceRemaining, isAttacking);

            endTime = DateTime.UtcNow.AddSeconds(Math.Max(1, Formula.MoveTime(troopObj.Stats.Speed) * Config.seconds_per_unit * speedMod / 100) * distanceRemaining);
            beginTime = DateTime.UtcNow;

            troopObj.Stub.BeginUpdate();
            troopObj.Stub.State = !isReturningHome ? TroopState.Moving : TroopState.ReturningHome;

            if (!CalculateNext(troopObj))
            {
                troopObj.Stub.State = TroopState.Idle;
                StateChange(ActionState.Completed);
                troopObj.Stub.EndUpdate();
                return 0;
            }
            troopObj.Stub.EndUpdate();

            troopObj.BeginUpdate();
            troopObj.TargetX = x;
            troopObj.TargetY = y;
            troopObj.State = GameObjectState.MovingState(x, y);
            troopObj.EndUpdate();

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            StateChange(ActionState.Failed);
        }

        public override void Callback(object custom)
        {
            City city;
            TroopObject troopObj;

            using (new MultiObjectLock(cityId, troopObjectId, out city, out troopObj))
            {
                troopObj.BeginUpdate();
                troopObj.X = nextX;
                troopObj.Y = nextY;
                --distanceRemaining;
                troopObj.EndUpdate();

                // Fire updated to force sending new position
                troopObj.Stub.BeginUpdate();
                troopObj.Stub.FireUpdated();
                troopObj.Stub.EndUpdate();

                if (!CalculateNext(troopObj))
                {
                    troopObj.Stub.BeginUpdate();
                    troopObj.Stub.State = TroopState.Idle;
                    troopObj.Stub.EndUpdate();

                    troopObj.BeginUpdate();
                    troopObj.State = GameObjectState.NormalState();
                    troopObj.EndUpdate();
                    StateChange(ActionState.Completed);
                    return;
                }

                StateChange(ActionState.Fired);
            }
        }

        #region Nested type: RecordForeach

        private class RecordForeach
        {
            public bool IsShortestDistanceDiagonal { get; set; }
            public int ShortestDistance { get; set; }
            public uint X { get; set; }
            public uint Y { get; set; }
     }

        #endregion
    }
}