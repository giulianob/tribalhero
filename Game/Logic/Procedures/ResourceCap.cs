#region

using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using System.Linq;
using Ninject;

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
                var bonus = city.Technologies.GetEffects(EffectCode.AtticStorageMod, EffectInheritance.All).Sum(x => (int)x.Value[0]);
                var resourceBonus = Formula.HiddenResource(city) * (double)bonus / 100;

                city.Resource.SetLimits(Formula.ResourceCropCap(city.Lvl) + resourceBonus.Crop,
                                        0,
                                        Formula.ResourceIronCap(city.Lvl) + resourceBonus.Iron,
                                        Formula.ResourceWoodCap(city.Lvl) + resourceBonus.Wood,
                                        0);
            }
            else
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        }
    }
}