﻿#region

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
        public virtual void SetResourceCap(ICity city)
        {
            if (Config.resource_cap)
            {
                var bonus = city.Technologies.GetEffects(EffectCode.AtticStorageMod, EffectInheritance.All).Sum(x => (int)x.Value[0]);
                var resourceBonus = Formula.Current.HiddenResource(city) * (double)bonus / 100;

                city.Resource.SetLimits(Formula.Current.ResourceCropCap(city.Lvl) + resourceBonus.Crop,
                                        0,
                                        Formula.Current.ResourceIronCap(city.Lvl) + resourceBonus.Iron,
                                        Formula.Current.ResourceWoodCap(city.Lvl) + resourceBonus.Wood,
                                        0);
            }
            else
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        }
    }
}