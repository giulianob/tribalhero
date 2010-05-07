#region

using System;
using Game.Logic;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Data {
    public abstract class SimpleGameObject {

        public enum Types : ushort {
            TROOP = 100,
            FOREST = 200
        }

        #region Position Related Members

        protected uint objectId;        
        protected uint x;
        protected uint y;

        #endregion

        #region Properties

        private bool inWorld;
        public bool InWorld {
            get { return inWorld; }
            set {
                CheckUpdateMode();
                inWorld = value;
            }
        }

        private GameObjectState state = GameObjectState.NormalState();

        public GameObjectState State {
            get { return state; }
            set {
                CheckUpdateMode();
                state = value;
            }
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

        protected SimpleGameObject() {}

        protected SimpleGameObject(uint x, uint y) {
            this.x = origX = x;
            this.y = origY = y;
        }

        #endregion

        #region Update Events

        protected bool updating;
        protected uint origX;
        protected uint origY;

        public void BeginUpdate() {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;
            origX = x;
            origY = y;
        }

        public abstract void CheckUpdateMode();
        public abstract void EndUpdate();

        protected void Update() {
            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            Global.World.ObjectUpdateEvent(this, origX, origY);
        }

        #endregion

        #region Methods

        public override string ToString() {
            return base.ToString() + "[" + X + "," + Y + "]";
        }

        internal static bool IsDiagonal(uint x, uint y, uint x1, uint y1) {
            return y%2 != y1%2;
        }

        public static int GetOffset(uint x, uint y, uint x1, uint y1) {
            if (y % 2 == 1 && y1 % 2 == 0 && x1 <= x)
                return 1;
            if (y % 2 == 0 && y1 % 2 == 1 && x1 >= x)
                return 1;

            return 0;
        }

        public static int TileDistance(uint x, uint y, uint x1, uint y1) {
            /***********************************************************
					     13,12  |  14,12 
			        12,13  |  13,13  |  14,13  |  15,13
               12,14  | (13,14) |  14,14  |  15,14  | 16,14
          11,15  |  12,15  |  13,15  |  14,15
               12,16  |  13,16  |  14,16
                    12,17  |  13,17  | 14,17
			             13,18     14,18
             *********************************************************/
            int offset = GetOffset(x, y, x1, y1);
            int dist = (int) ((x1 > x ? x1 - x : x - x1) + (y1 > y ? y1 - y : y - y1)/2 + offset);

            return dist;
        }

        public int TileDistance(uint x1, uint y1) {
            return TileDistance(x, y, x1, y1);
        }

        public int TileDistance(SimpleGameObject obj) {
            return TileDistance(obj.x, obj.y);
        }

        public int RadiusDistance(uint x1, uint y1) {
            return RadiusDistance(x, y, x1, y1);
        }

        public int RadiusDistance(SimpleGameObject obj) {
            return RadiusDistance(obj.x, obj.y);
        }

        public static int RadiusDistance(uint x, uint y, uint x1, uint y1) {
            /***********************************************************
10,11  |  11,11  |  12,11  |  13,11  |  14,11  |  15,11
			   12,12  |	 13,12  |  14,12 
		  11,13     12,13  |  13,13  |  14,13  |  15,13
               12,14  | (13,14) |  (14,14)  |  15,14  | 16,14
          11,15  |  12,15  |  13,15  |  14,15
               12,16  |  13,16  |  14,16
                    12,17  |  13,17  | 14,17
			             13,18     14,18
             *********************************************************/
            // Calculate the x and y distances
            int offset = 0;
            int xoffset = 0;
            if (y%2 != y1%2) {
                if (y%2 == 0) {
                    if (x > x1) {
                        xoffset = 1;
                    }
                } else {
                    if (x1 > x) {
                        xoffset = 1;
                    }
                }
                offset = 1;
            }

            int xDistance = (int) MapMath.AbsDiff(x, x1) - xoffset;
            int yDistance = (int) MapMath.AbsDiff(y, y1);
            int yhalf = yDistance/2;
            int x05 = Math.Min(xDistance, yhalf);
            int x15 = xDistance > yhalf ? xDistance - yhalf : 0;
            float radius = x05*0.5f + x15*1.5f + yhalf*1.5f + offset;

            return (int) (radius*2) - 1;
        }

        #endregion
    }
}