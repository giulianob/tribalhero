#region

using System;
using System.Linq;
using Game.Battle;
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
                var resourceBonus = Formula.Current.HiddenResource(city) * (double)bonus / 100;

                city.Resource.SetLimits(Formula.Current.ResourceCropCap(city.Lvl) + resourceBonus.Crop,
                                        0,
                                        Formula.Current.ResourceIronCap(city.Lvl) + resourceBonus.Iron,
                                        Formula.Current.ResourceWoodCap(city.Lvl) + resourceBonus.Wood,
                                        0);
            }
            else
            {
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            }
        }

        public int UpkeepForCity(City city, ITroopManager troops)
        {
            var upkeep = 0;
            var effects = city.Technologies.GetEffects(EffectCode.UpkeepReduce);
            foreach(var stub in troops)
            {
                if (stub.City != city) continue;
                foreach (var formation in stub)
                {
                    foreach( var kvp in formation)
                    {
                        var reduce = effects.Sum(x=> BattleFormulas.Current.UnitStatModCheck(city.Template[kvp.Key].Battle, TroopBattleGroup.Any, (string)x.Value[1])?(int)x.Value[0]:0);
                        upkeep += (int) Math.Ceiling((formation.Type == FormationType.Garrison ? 1.25f : 1f) * kvp.Value * city.Template[kvp.Key].Upkeep * (100f - Math.Max(reduce,70)) / 100);
                    }
                }
            }
            return upkeep;
        }
    }
}