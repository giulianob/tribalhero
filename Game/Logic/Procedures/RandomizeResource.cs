using System;
using System.Collections.Generic;
using System.Text;
using Game.Map;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Database;

namespace Game.Logic.Procedures {
    public partial class Procedure {

        private static bool work(uint ox, uint oy, uint x, uint y, object custom) {
            City city = (City)custom;
            if (Config.Random.Next() % 4 == 0) {
                Structure structure;
                Global.World.lockRegion(x, y);
                if (Config.Random.Next() % 2 == 0) {
                    structure = StructureFactory.getStructure(2106, 1);
                }
                else {
                    structure = StructureFactory.getStructure(2107, 1);
                }
                structure.X = x;
                structure.Y = y;
                city.add(structure);
                if (!Global.World.add(structure)) {
                    city.remove(structure);
                    Global.World.unlockRegion(x, y);
                    return false;
                }
                InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                Global.World.unlockRegion(x, y);
            }
            return true;
        }

        public static bool RandomizeResource(City city) {
            using (new MultiObjectLock(city)) {
                byte radius = city.Radius;
                Structure structure = city.MainBuilding;
                RadiusLocator.foreach_object(structure.X, structure.Y, (byte)Math.Max(radius - 1, 0), false, work, city);
            }

            return true;
        }
    }
}
