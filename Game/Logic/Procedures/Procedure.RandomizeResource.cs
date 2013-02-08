#region

using System;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private bool Work(uint ox, uint oy, uint x, uint y, object custom)
        {
            var city = (ICity)custom;
            if (Config.Random.Next() % 4 == 0)
            {
                world.Regions.LockRegion(x, y);
                IStructure structure = Config.Random.Next() % 2 == 0
                                               ? Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2106, 1)
                                               : Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2107, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                if (!world.Regions.Add(structure))
                {
                    city.ScheduleRemove(structure, false);
                    world.Regions.UnlockRegion(x, y);
                    return true;
                }
                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                world.Regions.UnlockRegion(x, y);
            }
            return true;
        }

        public virtual bool RandomizeResource(ICity city)
        {
            using (locker.Lock(city))
            {
                byte radius = city.Radius;
                tileLocator.ForeachObject(city.X, city.Y, (byte)Math.Max(radius - 1, 0), false, Work, city);
            }

            return true;
        }
    }
}