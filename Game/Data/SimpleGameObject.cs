#region

using System;
using Game.Data.Events;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Data
{
    public abstract class SimpleGameObject : ISimpleGameObject
    {
        public event EventHandler<SimpleGameObjectArgs> ObjectUpdated;

        public enum SystemGroupIds : uint
        {
            NewCityStartTile = 10000001,

            Forest = 10000002,

            Stronghold = 10000003,

            BarbarianTribe = 10000004
        }

        public enum Types : ushort
        {
            Troop = 100,

            Forest = 200,

            Stronghold = 300,

            BarbarianTribe = 400,
        }

        protected uint objectId;

        protected uint x;

        protected uint y;

        #region Properties

        private bool inWorld;

        private GameObjectState state = GameObjectState.NormalState();

        public ushort CityRegionRelX
        {
            get
            {
                return (ushort)(x % Config.city_region_width);
            }
        }

        public ushort CityRegionRelY
        {
            get
            {
                return (ushort)(y % Config.city_region_height);
            }
        }

        public bool InWorld
        {
            get
            {
                return inWorld;
            }
            set
            {
                CheckUpdateMode();
                inWorld = value;
                SaveOrigPos();
            }
        }

        public GameObjectState State
        {
            get
            {
                return state;
            }
            set
            {
                CheckUpdateMode();
                state = value;
            }
        }

        public abstract ushort Type { get; }

        public abstract uint GroupId { get; }

        public virtual uint ObjectId
        {
            get
            {
                return objectId;
            }
            set
            {
                CheckUpdateMode();
                objectId = value;
            }
        }

        public uint X
        {
            get
            {
                return x;
            }
            set
            {
                CheckUpdateMode();

                if (inWorld)
                {
                    origX = x;
                }

                x = value;
            }
        }

        public uint Y
        {
            get
            {
                return y;
            }
            set
            {
                CheckUpdateMode();

                if (inWorld)
                {
                    origY = y;
                }

                y = value;
            }
        }

        public uint RelX
        {
            get
            {
                return x % Config.region_width;
            }
        }

        public uint RelY
        {
            get
            {
                return y % Config.region_height;
            }
        }

        #endregion

        #region Constructors

        protected SimpleGameObject(uint x, uint y)
        {
            this.x = x;
            this.y = y;
            this.state = GameObjectState.NormalState();
        }

        #endregion

        #region Update Events

        private uint origX;

        private uint origY;

        protected bool Updating;

        public virtual void BeginUpdate()
        {
            if (Updating)
            {
                throw new Exception("Nesting beginupdate");
            }

            Updating = true;

            SaveOrigPos();
        }

        protected abstract void CheckUpdateMode();

        public void EndUpdate()
        {
            if (!Updating)
            {
                throw new Exception("Called endupdate without first calling begin update");
            }

            Updating = false;
            Update();
        }

        protected virtual bool Update()
        {
            if (!Global.Current.FireEvents)
            {
                return false;
            }

            if (Updating)
            {
                return false;
            }

            ObjectUpdated.Raise(this, new SimpleGameObjectArgs(this) {OriginalX = origX, OriginalY = origY});

            return true;
        }

        #endregion

        #region Methods

        private void SaveOrigPos()
        {
            if (InWorld)
            {
                origX = x;
                origY = y;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} x[{1}] y[{2}] type[{3}] groupId[{4}] objId[{5}]", base.ToString(), X, Y, Type, GroupId, ObjectId);
        }

        public int TileDistance(uint x1, uint y1)
        {
            return TileDistance(x, y, x1, y1);
        }

        public int TileDistance(ISimpleGameObject obj)
        {
            return TileDistance(obj.X, obj.Y);
        }

        public int RadiusDistance(uint x1, uint y1)
        {
            return RadiusDistance(x, y, x1, y1);
        }

        public int RadiusDistance(ISimpleGameObject obj)
        {
            return RadiusDistance(obj.X, obj.Y);
        }

        /// <summary>
        ///     Returns whether the two tiles are diagonal to one another.
        ///     NOTE: This function only handles case where the distance between both tiles is 1. If the distance is greater, you will get invalid results.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static bool IsDiagonal(uint x, uint y, uint x1, uint y1)
        {
            return y % 2 != y1 % 2;
        }

        /// <summary>
        ///     Returns whether two tiles are perpendicular. This means that they are on the same lines if you were to just draw
        ///     lines going up/down and left/right from a tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static bool IsPerpendicular(uint x, uint y, uint x1, uint y1)
        {
            return y == y1 || (x == x1 && y % 2 == y1 % 2);
        }

        public static int TileDistance(uint x, uint y, uint x1, uint y1)
        {
            return TileLocator.Current.TileDistance(x, y, x1, y1);
        }

        public static int RadiusDistance(uint x, uint y, uint x1, uint y1)
        {
            return RadiusLocator.Current.RadiusDistance(x, y, x1, y1);
        }

        #endregion
    }
}