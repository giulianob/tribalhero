#region

using System;
using Game.Data;
using Game.Map;
using Game.Setup;
using Ninject;
using Persistance;

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
            structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2000, lvl);
            uint x, y;
            if (!Ioc.Kernel.Get<MapFactory>().NextLocation(out x, out y, radius))
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
                    structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2402, 1);
                else
                    structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2402, 1);
                structure.X = x;
                structure.Y = y;
                feObj.City.Add(structure);
                if (!Global.World.Add(structure))
                    feObj.City.ScheduleRemove(structure, false);
                Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                Ioc.Kernel.Get<IDbManager>().Save(structure);
                Global.World.UnlockRegion(x, y);
            }
            return true;
        }

        public static void RandomizeNpcResource(City city, DbTransaction transaction)
        {
            byte radius = city.Radius;
            var feObject = new RandomForeach(city);
            TileLocator.ForeachObject(city.X, city.Y, (byte)Math.Max(radius - 1, 0), false, RandomizeNpcResourceWork, feObject);
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