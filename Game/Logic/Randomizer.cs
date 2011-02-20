#region

using System;
using Game.Data;
using Game.Database;
using Game.Map;
using Game.Setup;

#endregion

namespace Game.Logic
{
    public class Randomizer
    {
        public static Random Random = Config.Random;

        public static uint MapX
        {
            get
            {
                return (uint)Random.Next(Config.width_margin, 50 - Config.width_margin);
            }
        }

        public static uint MapY
        {
            get
            {
                return (uint)Random.Next(Config.height_margin, 50 - Config.height_margin);
            }
        }

        public static bool MainBuilding(out Structure structure, byte radius, byte lvl)
        {
            structure = StructureFactory.GetNewStructure(2000, lvl);
            uint x, y;
            if (!MapFactory.NextLocation(out x, out y, radius))
            {
                structure = null;
                return false;
            }
            structure.X = x;
            structure.Y = y;
            return true;
        }

        private static bool RandomizeNpcResourceWork(uint ox, uint oy, uint x, uint y, object custom)
        {
            var feObj = (RandomForeach)custom;
            if (Config.Random.Next()%4 == 0)
            {
                Structure structure;
                Global.World.LockRegion(x, y);
                if (Config.Random.Next()%2 == 0)
                    structure = StructureFactory.GetNewStructure(2402, 1);
                else
                    structure = StructureFactory.GetNewStructure(2402, 1);
                structure.X = x;
                structure.Y = y;
                feObj.City.Add(structure);
                if (!Global.World.Add(structure))
                    feObj.City.ScheduleRemove(structure, false);
                InitFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                Global.DbManager.Save(structure);
                Global.World.UnlockRegion(x, y);
            }
            return true;
        }

        public static void RandomizeNpcResource(City city, DbTransaction transaction)
        {
            byte radius = city.Radius;
            Structure structure = city.MainBuilding;
            var feObject = new RandomForeach(city);
            TileLocator.ForeachObject(structure.X, structure.Y, (byte)Math.Max(radius - 1, 0), false, RandomizeNpcResourceWork, feObject);
        }

        #region Nested type: RandomForeach

        private class RandomForeach
        {
            public RandomForeach(City city)
            {
                City = city;
            }

            public City City { get; private set; }
        }

        #endregion
    }
}