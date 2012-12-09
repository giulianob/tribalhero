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
                World.Current.Regions.LockRegion(x, y);
                IStructure structure = Config.Random.Next() % 2 == 0
                                               ? Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2106, 1)
                                               : Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2107, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                if (!World.Current.Regions.Add(structure))
                {
                    city.ScheduleRemove(structure, false);
                    World.Current.Regions.UnlockRegion(x, y);
                    return true;
                }
                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                World.Current.Regions.UnlockRegion(x, y);
            }
            return true;
        }

        public virtual bool RandomizeResource(ICity city)
        {
            using (Concurrency.Current.Lock(city))
            {
                byte radius = city.Radius;
                TileLocator.Current.ForeachObject(city.X, city.Y, (byte)Math.Max(radius - 1, 0), false, Work, city);
            }

            return true;
        }
    }
}