#region

using System;
using Game.Logic;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Data {
    public abstract class GameObject : ICanDo {
        #region Position Related Members

        protected uint objectId;
        protected City city;
        protected uint x;
        protected uint y;

        #endregion

        #region Properties

        private GameObjectState state = GameObjectState.NormalState();

        public GameObjectState State {
            get { return state; }
            set {
                CheckUpdateMode();
                state = value;
            }
        }

        public City City {
            get { return city; }
            set { city = value; }
        }

        public abstract ushort Type { get; }
        public abstract byte Lvl { get; }

        public virtual uint ObjectId {
            get { return objectId; }
            set {
                CheckUpdateMode();
                objectId = value;
            }
        }

        public uint X {
            get { return x; }
            set {
                CheckUpdateMode();
                origX = x;
                x = value;
            }
        }

        public uint Y {
            get { return y; }
            set {
                CheckUpdateMode();
                origY = y;
                y = value;
            }
        }

        public uint RelX {
            get { return x%Config.region_width; }
        }

        public uint RelY {
            get { return y%Config.region_height; }
        }

        public uint CityRegionRelX {
            get { return x%Config.city_region_width; }
        }

        public uint CityRegionRelY {
            get { return y%Config.city_region_height; }
        }

        #endregion

        #region Constructors

        protected GameObject() {}

        protected GameObject(uint x, uint y) {
            this.x = origX = x;
            this.y = origY = y;
        }

        #endregion

        #region Update Events

        protected bool updating;
        private uint origX;
        private uint origY;

        public void CheckUpdateMode() {
            //If city is null then we dont care about being inside of a begin/end update block
            if (!Global.FireEvents || city == null)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(city);
        }

        public void BeginUpdate() {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;
            origX = x;
            origY = y;
        }

        public abstract void EndUpdate();

        protected void Update() {
            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            if (city != null) {
                city.ObjUpdateEvent(this, origX, origY);
                Global.World.ObjUpdateEvent(this, origX, origY);
            }
        }

        #endregion

        #region Methods

        public override string ToString() {
            return base.ToString() + "[" + X + "," + Y + "]";
        }

        internal static bool IsDiagonal(uint x, uint y, uint x1, uint y1) {
            return !(y%2 == y1%2);
        }

        public static int Distance(uint x, uint y, uint x1, uint y1) {
            /***********************************************************
             *   1,1  |  2,1  |  3,1  |  4,1  |
             *       1,2  |  2,2  |  3,2  |  4,2
             *   1,3  |  2,3  |  3,3  |  4,3  |
             *       1,4  |  2,4  |  3,4  |  4,4
             * 
             * *********************************************************/
            uint offset = 0;
            if (y%2 == 1 && y1%2 == 0 && x1 <= x)
                offset = 1;
            if (y%2 == 0 && y1%2 == 1 && x1 >= x)
                offset = 1;
            int dist = (int) ((x1 > x ? x1 - x : x - x1) + (y1 > y ? y1 - y : y - y1)/2 + offset);

            return dist;
        }

        public int Distance(uint x1, uint y1) {
            return Distance(x, y, x1, y1);
        }

        public int Distance(GameObject obj) {
            return Distance(obj.x, obj.y);
        }

        #endregion

        #region ICanDo Members

        public uint WorkerId {
            get { return objectId; }
        }

        #endregion
    }
}