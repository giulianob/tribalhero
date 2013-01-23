#region

using System;
using Game.Map;
using Game.Setup;

#endregion

namespace Game.Data
{
    public abstract class SimpleGameObject : ISimpleGameObject
    {
        public enum SystemGroupIds : uint
        {
            NewCityStartTile = 10000001,

            Forest = 10000002,

            Stronghold = 10000003,

            Settlement = 10000004
        }

        public enum Types : ushort
        {
            Troop = 100,

            Forest = 200,

            Stronghold = 300,

            Settlement = 400,
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
                origX = x;
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
                origY = y;
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

        protected SimpleGameObject()
        {
        }

        protected SimpleGameObject(uint x, uint y)
        {
            this.x = origX = x;
            this.y = origY = y;
        }

        #endregion

        #region Update Events

        protected uint origX;

        protected uint origY;

        protected bool updating;

        public virtual void BeginUpdate()
        {
            if (updating)
            {
                throw new Exception("Nesting beginupdate");
            }

            updating = true;
            origX = x;
            origY = y;
        }

        public abstract void CheckUpdateMode();

        public abstract void EndUpdate();

        protected virtual void Update()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            if (updating)
            {
                return;
            }

            World.Current.Regions.ObjectUpdateEvent(this, origX, origY);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return base.ToString() + "[" + X + "," + Y + "]";
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