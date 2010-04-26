#region

using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static void AdjustCityResourceRates(Structure structure, int laborDelta) {
            if (ObjectTypeFactory.IsStructureType("Crop", structure)) {
                structure.City.Resource.Crop.Rate += laborDelta;
            }            
        }

        public static void OnStructureUpgrade(Structure structure) {
            structure.City.BeginUpdate();
            Formula.ResourceCap(structure.City);
            structure.City.EndUpdate();
        }
    }
}