#region

using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public static void AdjustCityResourceRates(Structure structure, int laborDelta)
        {
            if (ObjectTypeFactory.IsStructureType("Crop", structure))
                structure.City.Resource.Crop.Rate = Formula.GetCropRate(structure.City);
            else if (ObjectTypeFactory.IsStructureType("Iron", structure))
                structure.City.Resource.Iron.Rate = Formula.GetIronRate(structure);
        }

        public static void OnStructureUpgrade(Structure structure)
        {
            structure.City.BeginUpdate();
            SetResourceCap(structure.City);
            structure.City.EndUpdate();
        }
    }
}