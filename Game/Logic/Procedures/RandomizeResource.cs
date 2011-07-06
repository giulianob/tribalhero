#region

using System;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private static bool Work(uint ox, uint oy, uint x, uint y, object custom)
        {
            var city = (City)custom;
            if (Config.Random.Next()%4 == 0)
            {
                Global.World.LockRegion(x, y);
                Structure structure = Config.Random.Next()%2 == 0 ? StructureFactory.GetNewStructure(2106, 1) : StructureFactory.GetNewStructure(2107, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                if (!Global.World.Add(structure))
                {
                    city.ScheduleRemove(structure, false);
                    Global.World.UnlockRegion(x, y);
                    return true;
                }
                InitFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                Global.World.UnlockRegion(x, y);
            }
            return true;
        }

        public static bool RandomizeResource(City city)
        {
            using (new MultiObjectLock(city))
            {
                byte radius = city.Radius;
                TileLocator.ForeachObject(city.X, city.Y, (byte)Math.Max(radius - 1, 0), false, Work, city);
            }

            return true;
        }
    }
}