using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        internal static void OnLaborUpdate(Structure structure, int laborDelta) {
            if (ObjectTypeFactory.IsStructureType("Wood", structure)) {
                if (structure.City.Resource.Wood.Rate == 0) {
                    structure.City.Resource.Wood.Rate = 1800000 / laborDelta;
                } else {
                    int labor = ((1800000 / structure.City.Resource.Wood.Rate) + laborDelta);
                    if (labor == 0) {
                        structure.City.Resource.Wood.Rate = 0;
                    } else {
                        structure.City.Resource.Wood.Rate = 1800000 / ((1800000 / structure.City.Resource.Wood.Rate) + laborDelta); // get the new labor count first then divide the 1800sec with it
                    }
                }
            }
            if (ObjectTypeFactory.IsStructureType("Crop", structure)) {
                if (structure.City.Resource.Crop.Rate == 0) {
                    structure.City.Resource.Crop.Rate = 1800000 / laborDelta;
                } else {
                    int labor = ((1800000 / structure.City.Resource.Crop.Rate) + laborDelta);
                    if (labor == 0) {
                        structure.City.Resource.Crop.Rate = 0;
                    } else {
                        structure.City.Resource.Crop.Rate = 1800000 / ((1800000 / structure.City.Resource.Crop.Rate) + laborDelta); // get the new labor count first then divide the 1800sec with it
                    }
                }
            }
        }

        internal static void OnStructureUpgrade(Structure structure) {
            Formula.ResourceCap(structure.City);

        }
    }
}
