#region

using System;
using Game.Map;
using Game.Setup;

#endregion

namespace Game.Data
{
    public abstract class SimpleGameObject
    {
        public enum Types : ushort
        {
            Troop = 100,
            Forest = 200,
        }

        public enum SystemGroupIds : uint
        {
            NewCityStartTile = 10000001,
            Forest = 10000002,
        }

        protected uint objectId;
        protected uint x;
        protected uint y;

        #region Properties

        private bool inWorld;

        private GameObjectState state = GameObjectState.NormalState();

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
                return x%Config.region_width;
            }
        }

        public uint RelY
        {
            get
            {
                return y%Config.region_height;
            }
        }

        public ushort CityRegionRelX
        {
            get
            {
                return (ushort)(x%Config.city_region_width);
            }
        }

        public ushort CityRegionRelY
        {
            get
            {
                return (ushort)(y%Config.city_region_height);
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

        public void BeginUpdate()
        {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;
            origX = x;
            origY = y;
        }

        public abstract void CheckUpdateMode();
        public abstract void EndUpdate();

        protected void Update()
        {
            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            World.Current.ObjectUpdateEvent(this, origX, origY);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return base.ToString() + "[" + X + "," + Y + "]";
        }

        public static bool IsDiagonal(uint x, uint y, uint x1, uint y1)
        {
            return y%2 != y1%2;
        }

        public static int TileDistance(uint x, uint y, uint x1, uint y1)
        {
            return TileLocator.Current.TileDistance(x, y, x1, y1);
        }

        public int TileDistance(uint x1, uint y1)
        {
            return TileDistance(x, y, x1, y1);
        }

        public int TileDistance(SimpleGameObject obj)
        {
            return TileDistance(obj.x, obj.y);
        }

        public int RadiusDistance(uint x1, uint y1)
        {
            return RadiusDistance(x, y, x1, y1);
        }

        public int RadiusDistance(SimpleGameObject obj)
        {
            return RadiusDistance(obj.x, obj.y);
        }

        public static int RadiusDistance(uint x, uint y, uint x1, uint y1)
        {
            return RadiusLocator.Current.RadiusDistance(x, y, x1, y1);
        }

        #endregion
    }
}