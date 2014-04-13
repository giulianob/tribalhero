#region

using System;
using Game.Data;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        [Obsolete("Use resource_rate_update in init.csv instead")]
        public virtual void RecalculateCityResourceRates(ICity city)
        {
            city.Resource.Crop.Rate = formula.GetCropRate(city);
            city.Resource.Iron.Rate = formula.GetIronRate(city);
            city.Resource.Wood.Rate = formula.GetWoodRate(city);
            city.Resource.Gold.Rate = formula.GetGoldRate(city);
        }
        
        [Obsolete("Use resource_rate_update and city_resource_cap_update in init.csv instead")]
        public virtual void OnStructureUpgradeDowngrade(IStructure structure)
        {
            SetResourceCap(structure.City);
            RecalculateCityResourceRates(structure.City);
        }
    }
}