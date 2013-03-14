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

        public static bool MainBuilding(ICity city, out IStructure structure, byte radius, byte lvl)
        {
            uint x, y;
            if (!Ioc.Kernel.Get<MapFactory>().NextLocation(out x, out y, radius))
            {
                structure = null;
                return false;
            }

            structure = city.CreateStructure(2000, lvl);

            if (structure.ObjectId != 1)
            {
                throw new Exception("Created main building but it did not have object id 1");
            }

            structure.BeginUpdate();
            structure.X = x;
            structure.Y = y;
            structure.EndUpdate();

            return true;
        }
    }
}