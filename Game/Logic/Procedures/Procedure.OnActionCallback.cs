#region

using Game.Data;
using Game.Logic.Formulas;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual void RecalculateCityResourceRates(ICity city)
        {
            city.Resource.Crop.Rate = formula.GetCropRate(city);
            city.Resource.Iron.Rate = formula.GetIronRate(city);
            city.Resource.Wood.Rate = formula.GetWoodRate(city);
            city.Resource.Gold.Rate = formula.GetGoldRate(city);
        }

        public virtual void OnStructureUpgradeDowngrade(IStructure structure)
        {
            SetResourceCap(structure.City);
            RecalculateCityResourceRates(structure.City);
        }

        public virtual void OnTechnologyChange(IStructure structure)
        {
            structure.City.BeginUpdate();
            SetResourceCap(structure.City);
            structure.City.EndUpdate();
        }
    }
}