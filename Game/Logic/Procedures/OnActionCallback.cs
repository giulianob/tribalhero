using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        internal static void OnLaborUpdate(Structure structure, int laborDelta) {
            if (ObjectTypeFactory.IsStructureType("Wood", structure)) {
                //If rate is 0 it means that laborDelta must be positive and no other structures have labor in them, so a simple calculation can be performed
                //to get the rate
                if (structure.City.Resource.Wood.Rate == 0) {
                    structure.City.Resource.Wood.Rate = 3600000 / laborDelta;
                } else {
                    //Calculate the number of labors that represent this rate. We do this instead of looping through the whole city to add all of the labors
                    //that make up the rate. Then we can add the delta to get the new value.
                    int labor = ((3600000 / structure.City.Resource.Wood.Rate) + laborDelta);
                    if (labor == 0) {
                        structure.City.Resource.Wood.Rate = 0;
                    } else {
                        structure.City.Resource.Wood.Rate = 3600000 / labor; // get the new labor count first then divide the time with it
                    }
                }
            }

            if (ObjectTypeFactory.IsStructureType("Crop", structure)) {
                if (structure.City.Resource.Crop.Rate == 0) {
                    structure.City.Resource.Crop.Rate = 3600000 / laborDelta;
                } else {
                    int labor = ((3600000 / structure.City.Resource.Crop.Rate) + laborDelta);
                    if (labor == 0) {
                        structure.City.Resource.Crop.Rate = 0;
                    } else {
                        structure.City.Resource.Crop.Rate = 3600000 / labor; 
                    }
                }
            }
        }

        internal static void OnStructureUpgrade(Structure structure) {
            Formula.ResourceCap(structure.City);

        }
    }
}
