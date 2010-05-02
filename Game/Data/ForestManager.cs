using System.Collections.Generic;
using System.Linq;
using Game.Setup;
using Game.Util;

namespace Game.Data {
    public class ForestManager : ILockable {

        public static readonly object ForestLock = new object();

        Dictionary<uint, Forest> forests;

        public ForestManager() {
            forests = new Dictionary<uint, Forest>();            
        }

        public void DbLoaderAdd(Forest forest) {
            forests.Add(forest.ObjectId, forest);
        }

        public void CreateForest(byte lvl, int capacity, int rate) {
            CreateForestAt(lvl, capacity, rate, 0, 0);
        }

        public void CreateForestAt(byte lvl, int capacity, int rate, uint x, uint y) {
            Forest forest = new Forest(lvl, capacity, rate);

            if (x == 0 || y == 0) {
                while (true) {
                    x = (uint) Config.Random.Next(15, (int) Config.map_width - 15);
                    y = (uint) Config.Random.Next(15, (int) Config.map_height - 15);

                    Global.World.LockRegion(x, y);

                    // check if near a city
                    if (Global.World.GetObjectsWithin(x, y, 4).Exists(obj => !(obj is TroopObject))) {
                        Global.World.UnlockRegion(x, y);
                        continue;
                    }

                    break;
                }
            } else {
                Global.World.LockRegion(x, y);
            }

            forest.X = x;
            forest.Y = y;           

            Global.World.Add(forest);
            Global.World.UnlockRegion(x, y);

            forests.Add(forest.ObjectId, forest);

            forest.BeginUpdate();
            forest.RecalculateForest();
            forest.EndUpdate();
        }


        public void RemoveForest(Forest forest) {
            forests.Remove(forest.ObjectId);

            Global.World.Remove(forest);            
            Global.DbManager.Delete(forest);
        }

        /// <summary>
        /// Locks all cities participating in this forest. 
        /// Proper usage would be to lock the forest manager and the main city in the base objects. 
        /// The custom[0] parameter should a uint with the forestId.
        /// Once inside of the lock, a call to ForestManager.TryGetValue should be used to get the forest.
        /// </summary>
        /// <param name="custom">custom[0] should contain the forestId to lock</param>
        /// <returns>List of cities to lock for the forest.</returns>
        public ILockable[] CallbackLockHandler(object[] custom) {
            Forest forest;
            
            if (!forests.TryGetValue((uint)custom[0], out forest)) return new ILockable[] { };
            
            return forest.Select(obj => obj.City).ToArray();
        }

        public bool TryGetValue(uint id, out Forest forest) {
            return forests.TryGetValue(id, out forest);
        }

        public int Hash {
            get { return (int)Global.Locks.FOREST; }
        }

        public object Lock {
            get { return ForestLock; }
        }
    }
}
