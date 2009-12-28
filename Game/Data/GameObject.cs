using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Game.Logic;
using Game.Setup;
using Game.Module;
using Game.Database;
using Game.Util;

namespace Game.Data {

    public abstract class GameObject: ICanDo {
        #region Position Related Members
        protected uint objectID;
        protected City city;
        protected uint x = 0;
        protected uint y = 0;
        #endregion

        #region Properties
        GameObjectState state = GameObjectState.NormalState();

        public GameObjectState State {
            get { return state; }
            set { CheckUpdateMode(); state = value; }
        }

        public City City {
            get { return city; }
            set { city = value; }
        }

        public abstract ushort Type { get; }
        public abstract byte Lvl { get; }

        public virtual uint ObjectID {
            get { return objectID; }
            set { CheckUpdateMode();  objectID = value; }
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
            get { return (uint)(x % Setup.Config.region_width); }
        }
        
        public uint RelY {
            get { return (uint)(y % Setup.Config.region_height); }
        }

        public uint CityRegionRelX {
            get { return (uint)(x % Setup.Config.city_region_width); }
        }
        
        public uint CityRegionRelY {
            get { return (uint)(y % Setup.Config.city_region_height); }
        }

        #endregion

        #region Constructors
        public GameObject() {

        }

        public GameObject(uint x, uint y) {
            this.x = this.origX = x;
            this.y = this.origY = y;
        }
        #endregion

        #region Update Events
        protected bool updating = false;
        uint origX = 0;
        uint origY = 0;

        public void CheckUpdateMode() {
            //If city is null then we dont care about being inside of a begin/end update block
            if (!Global.FireEvents || city == null) return;
            
            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            if (!MultiObjectLock.IsLocked(city))
                throw new Exception("Object not locked");
        }

        public void BeginUpdate() {
            if (updating) throw new Exception("Nesting beginupdate");
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
                city.obj_UpdateEvent(this, origX, origY);
                Global.World.obj_UpdateEvent(this, origX, origY);
            }
        }
        #endregion

        #region Methods
        public override string ToString() {
            return base.ToString() + "[" + X + "," + Y + "]";
        }

        internal static bool isDiagonal(uint x, uint y, uint x_1, uint y_1) {
            return !(y % 2 == y_1 % 2);
        }
        
        public static int distance(uint x, uint y, uint x_1, uint y_1) {
            /***********************************************************
             *   1,1  |  2,1  |  3,1  |  4,1  |
             *       1,2  |  2,2  |  3,2  |  4,2
             *   1,3  |  2,3  |  3,3  |  4,3  |
             *       1,4  |  2,4  |  3,4  |  4,4
             * 
             * *********************************************************/
            uint offset = 0;
            if (y % 2 == 1 && y_1 % 2 == 0 && x_1 <= x) offset = 1;
            if (y % 2 == 0 && y_1 % 2 == 1 && x_1 >= x) offset = 1;
            int dist = (int)((x_1 > x ? x_1 - x : x - x_1) + (y_1 > y ? y_1 - y : y - y_1) / 2 + offset);
            double real = Math.Sqrt(Math.Pow(x - x_1, 2) + Math.Pow(y - y_1, 2));
      
            return dist;
        }

        public int distance(uint x_1, uint y_1) {
            return distance(x, y, x_1, y_1);
        }

        public int distance(GameObject obj) {
            return distance(obj.x, obj.y);
        }
        #endregion

        #region ICanDo Members


        public uint WorkerId {
            get { return objectID; }
        }

        #endregion
    }
}
