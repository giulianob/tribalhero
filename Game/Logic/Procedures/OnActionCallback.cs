#region

using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        internal static void OnLaborUpdate(Structure structure, int laborDelta) {
            if (ObjectTypeFactory.IsStructureType("Wood", structure)) {
                structure.City.Resource.Wood.Rate += laborDelta;
            }

            if (ObjectTypeFactory.IsStructureType("Crop", structure)) {
                structure.City.Resource.Wood.Rate += laborDelta;
            }
        }

        internal static void OnStructureUpgrade(Structure structure) {
            structure.City.BeginUpdate();
            Formula.ResourceCap(structure.City);
            structure.City.EndUpdate();
        }
    }
}