#region

using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///   Sets the resource caps for the given city
        /// </summary>
        /// <param name = "city"></param>
        public static void SetResourceCap(City city)
        {            
            if (Config.resource_cap)
            {
                city.Resource.SetLimits(Formula.ResourceCropCap(city.Lvl),
                                        0,
                                        Formula.ResourceIronCap(city.Lvl),
                                        Formula.ResourceWoodCap(city.Lvl),
                                        0);
            }
            else
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        }
    }
}