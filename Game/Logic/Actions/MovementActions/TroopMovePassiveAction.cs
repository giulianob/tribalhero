#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class TroopMovePassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;

        private readonly Boolean isAttacking;

        private readonly Formula formula;

        private readonly ITileLocator tileLocator;

        private readonly IGameObjectLocator world;

        private readonly ILocker locker;

        private readonly Boolean isReturningHome;

        private readonly uint troopObjectId;

        private readonly uint x;

        private readonly uint y;

        private int distanceRemaining;

        private double moveTime;

        private uint nextX;

        private uint nextY;

        public TroopMovePassiveAction(uint cityId,
                                      uint troopObjectId,
                                      uint x,
                                      uint y,
                                      bool isReturningHome,
                                      bool isAttacking,
                                      Formula formula,
                                      ITileLocator tileLocator,
                                      IGameObjectLocator world,
                                      ILocker locker)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.x = x;
            this.y = y;
            this.isReturningHome = isReturningHome;
            this.isAttacking = isAttacking;
            this.formula = formula;
            this.tileLocator = tileLocator;
            this.world = world;
            this.locker = locker;
        }

        public TroopMovePassiveAction(uint id,
                                      DateTime beginTime,
                                      DateTime nextTime,
                                      DateTime endTime,
                                      bool isVisible,
                                      string nlsDescription,
                                      Dictionary<string, string> properties,
                                      Formula formula,
                                      ITileLocator tileLocator,
                                      IGameObjectLocator world,
                                      ILocker locker)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.formula = formula;
            this.tileLocator = tileLocator;
            this.world = world;
            this.locker = locker;
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            nextX = uint.Parse(properties["next_x"]);
            nextY = uint.Parse(properties["next_y"]);
            distanceRemaining = int.Parse(properties["distance_remaining"]);
            isReturningHome = Boolean.Parse(properties["returning_home"]);
            moveTime = double.Parse(properties["move_time"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.TroopMovePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("troop_id", troopObjectId),
                                new XmlKvPair("x", x), new XmlKvPair("y", y), new XmlKvPair("next_x", nextX),
                                new XmlKvPair("next_y", nextY), new XmlKvPair("distance_remaining", distanceRemaining),
                                new XmlKvPair("returning_home", isReturningHome), new XmlKvPair("move_time", moveTime)
                        });
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        private bool CalculateNextPosition(ITroopObject obj)
        {
            if (distanceRemaining <= 0)
            {
                return false;
            }

            var recordForeach = new RecordForeach {ShortestDistance = int.MaxValue, IsShortestDistanceDiagonal = false};
            foreach (var position in tileLocator.ForeachTile(obj.X, obj.Y, 1, false))
            {
                int distance = tileLocator.TileDistance(position.X, position.Y, 1, x, y, 1);

                if (distance < recordForeach.ShortestDistance)
                {
                    recordForeach.ShortestDistance = distance;
                    recordForeach.X = position.X;
                    recordForeach.Y = position.Y;
                    recordForeach.IsShortestDistanceDiagonal = IsDiagonal(position.Y, obj.X, obj.Y);
                }
                else if (distance == recordForeach.ShortestDistance && !recordForeach.IsShortestDistanceDiagonal)
                {
                    recordForeach.ShortestDistance = distance;
                    recordForeach.X = position.X;
                    recordForeach.Y = position.Y;
                    recordForeach.IsShortestDistanceDiagonal = IsDiagonal(position.Y, obj.X, obj.Y);
                }
            }

            nextX = recordForeach.X;
            nextY = recordForeach.Y;
            return true;
        }

        public override Error Execute()
        {
            ICity city;
            ITroopObject troopObj;

            if (!world.TryGetObjects(cityId, troopObjectId, out city, out troopObj))
            {
                return Error.ObjectNotFound;
            }

            distanceRemaining = Math.Max(1, tileLocator.TileDistance(troopObj.X, troopObj.Y, troopObj.Size, x, y, 1));

            double moveTimeTotal = formula.MoveTimeTotal(troopObj.Stub, distanceRemaining, isAttacking);

            var actionConfigTime = ActionConfigTime();
            if (actionConfigTime != null)
            {
                moveTime = actionConfigTime.Value;
            }
            else
            {
                moveTime = moveTimeTotal / distanceRemaining;
            }

            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(moveTimeTotal);
            nextTime = DateTime.UtcNow.AddSeconds(moveTime);

            troopObj.Stub.BeginUpdate();
            troopObj.Stub.State = !isReturningHome ? TroopState.Moving : TroopState.ReturningHome;

            if (!CalculateNextPosition(troopObj))
            {
                troopObj.Stub.State = TroopState.Idle;
                StateChange(ActionState.Completed);
                troopObj.Stub.EndUpdate();
                return Error.Ok;
            }
            troopObj.Stub.EndUpdate();

            troopObj.BeginUpdate();
            troopObj.TargetX = x;
            troopObj.TargetY = y;
            troopObj.State = GameObjectStateFactory.MovingState();
            troopObj.EndUpdate();

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            using (locker.Lock(cityId, out city))
            {
                StateChange(ActionState.Failed);
            }
        }

        public override void Callback(object custom)
        {
            ICity city;
            ITroopObject troopObj;

            using (locker.Lock(cityId, troopObjectId, out city, out troopObj))
            {
                if (!IsValid())
                {
                    return;
                }

                --distanceRemaining;

                troopObj.BeginUpdate();
                troopObj.X = nextX;
                troopObj.Y = nextY;
                troopObj.EndUpdate();

                // Fire updated to force sending new position
                troopObj.Stub.BeginUpdate();
                troopObj.Stub.FireUpdated();
                troopObj.Stub.EndUpdate();

                if (!CalculateNextPosition(troopObj))
                {
                    troopObj.Stub.BeginUpdate();
                    troopObj.Stub.State = TroopState.Idle;
                    troopObj.Stub.EndUpdate();

                    troopObj.BeginUpdate();
                    troopObj.State = GameObjectStateFactory.NormalState();
                    troopObj.EndUpdate();
                    StateChange(ActionState.Completed);
                    return;
                }

                nextTime = DateTime.UtcNow.AddSeconds(moveTime);
                StateChange(ActionState.Fired);
            }
        }

        private bool IsDiagonal(uint y, uint x1, uint y1)
        {
            return y % 2 != y1 % 2;
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