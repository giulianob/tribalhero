#region

using System;
using Game.Data;
using Game.Database;
using Game.Map;
using Game.Setup;
using Ninject;

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

        public static bool MainBuilding(out IStructure structure, byte radius, byte lvl)
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
            if (Config.Random.Next() % 4 == 0)
            {
                IStructure structure;
                World.Current.Regions.LockRegion(x, y);
                if (Config.Random.Next() % 2 == 0)
                {
                    structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2402, 1);
                }
                else
                {
                    structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2402, 1);
                }
                structure.X = x;
                structure.Y = y;
                feObj.City.Add(structure);
                if (!World.Current.Regions.Add(structure))
                {
                    feObj.City.ScheduleRemove(structure, false);
                }
                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                DbPersistance.Current.Save(structure);
                World.Current.Regions.UnlockRegion(x, y);
            }
            return true;
        }

        public static void RandomizeNpcResource(ICity city)
        {
            byte radius = city.Radius;
            var feObject = new RandomForeach(city);
            TileLocator.Current.ForeachObject(city.X,
                                              city.Y,
                                              (byte)Math.Max(radius - 1, 0),
                                              false,
                                              RandomizeNpcResourceWork,
                                              feObject);
        }

        #region Nested type: RandomForeach

        private class RandomForeach
        {
            public RandomForeach(ICity city)
            {
                City = city;
            }

            public ICity City { get; private set; }
        }

        #endregion
    }
}