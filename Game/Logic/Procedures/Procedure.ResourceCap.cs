#region

using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///     Sets the resource caps for the given city
        /// </summary>
        /// <param name="city"></param>
        public virtual void SetResourceCap(ICity city)
        {
            if (Config.resource_cap)
            {
                var bonus =
                        city.Technologies.GetEffects(EffectCode.AtticStorageMod, EffectInheritance.All)
                            .Sum(x => (int)x.Value[0]);
                var resourceBonus = formula.HiddenResource(city) * (double)bonus / 100;

                city.Resource.SetLimits(formula.ResourceCropCap(city.Lvl) + resourceBonus.Crop,
                                        0,
                                        formula.ResourceIronCap(city.Lvl) + resourceBonus.Iron,
                                        formula.ResourceWoodCap(city.Lvl) + resourceBonus.Wood,
                                        0);
            }
            else
            {
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            }
        }

        public int UpkeepForCity(City city, ITroopManager troops)
        {
            return (int)(troops.Upkeep + city.DefaultTroop.UpkeepForFormation(FormationType.Garrison) * 0.25);
        }
    }
}