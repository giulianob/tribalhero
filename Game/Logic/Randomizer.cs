#region

using System;
using Game.Data;
using Game.Database;
using Game.Map;
using Game.Setup;

#endregion

namespace Game.Logic {
    public class Randomizer {
        public static Random random = Config.Random;

        public static uint MapX {
            get { return (uint) random.Next(Config.width_margin, /*Config.map_width*/50 - Config.width_margin); }
        }

        public static uint MapY {
            get { return (uint) random.Next(Config.height_margin, /*Config.map_height*/50 - Config.height_margin); }
        }

        public static bool MainBuilding(out Structure structure) {
            structure = StructureFactory.GetStructure(2000, 1);
            uint x, y;
            if (!MapFactory.NextLocation(out x, out y)) {
                structure = null;
                return false;
            }
            structure.X = x;
            structure.Y = y;
            return true;
        }

        private class Random_foreach {
            public DbTransaction transaction;
            public City city;

            public Random_foreach(DbTransaction transaction, City city) {
                this.transaction = transaction;
                this.city = city;
            }
        }

        private static bool RandomizeNpcResourceWork(uint ox, uint oy, uint x, uint y, object custom) {
            Random_foreach feObj = (Random_foreach) custom;
            if (Config.Random.Next()%4 == 0) {
                Structure structure;
                Global.World.LockRegion(x, y);
                if (Config.Random.Next()%2 == 0)
                    structure = StructureFactory.GetStructure(2402, 1);
                else
                    structure = StructureFactory.GetStructure(2402, 1);
                structure.X = x;
                structure.Y = y;
                feObj.city.Add(structure);
                if (!Global.World.Add(structure))
                    feObj.city.Remove(structure);
                InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                Global.dbManager.Save(structure);
                Global.World.UnlockRegion(x, y);
            }
            return true;
        }

        public static void RandomizeNpcResource(City city, DbTransaction transaction) {
            byte radius = city.Radius;
            Structure structure = city.MainBuilding;
            Random_foreach feObject = new Random_foreach(transaction, city);
            RadiusLocator.foreach_object(structure.X, structure.Y, (byte) Math.Max(radius - 1, 0), false,
                                         RandomizeNpcResourceWork, feObject);
        }
    }
}