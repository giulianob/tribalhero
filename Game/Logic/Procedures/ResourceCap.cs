using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        /// Sets the resource caps for the given city
        /// </summary>
        /// <param name="city"></param>
        public static void SetResourceCap(City city)
        {
            if (Config.resource_cap)
            {
                city.Resource.SetLimits(Formula.ResourceCropCap(city.MainBuilding.Lvl), 0, Formula.ResourceIronCap(city.MainBuilding.Lvl), Formula.ResourceWoodCap(city.MainBuilding.Lvl), 0);
            }
            else
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        }
    }
}
