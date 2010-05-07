#region

using System;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {

        private static bool Work(uint ox, uint oy, uint x, uint y, object custom) {
            City city = (City) custom;
            if (Config.Random.Next()%4 == 0) {
                Global.World.LockRegion(x, y);
                Structure structure = Config.Random.Next()%2 == 0 ? StructureFactory.GetStructure(2106, 1) : StructureFactory.GetStructure(2107, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                if (!Global.World.Add(structure)) {
                    city.ScheduleRemove(structure, false);
                    Global.World.UnlockRegion(x, y);
                    return true;
                }
                InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                Global.World.UnlockRegion(x, y);
            }
            return true;
        }

        public static bool RandomizeResource(City city) {
            using (new MultiObjectLock(city)) {
                byte radius = city.Radius;
                Structure structure = city.MainBuilding;
                TileLocator.foreach_object(structure.X, structure.Y, (byte) Math.Max(radius - 1, 0), false, Work, city);
            }

            return true;
        }
    }
}