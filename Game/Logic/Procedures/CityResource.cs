using System;
using System.Collections.Generic;
using System.Text;
using Game.Util;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static void CityResource(City city) {
            using (new MultiObjectLock(city)) {
                Resource resource = new Resource();

                foreach (Structure structure in city) {
                    if (structure.Properties.Contains("Efficiency")) {
                        if (ObjectTypeFactory.IsStructureType("Wood", structure)) {
                            resource.Wood += Formula.GetResource(structure.Lvl, (ushort)structure["Efficiency"]);
                        }
                        if (ObjectTypeFactory.IsStructureType("Crop", structure)) {
                            resource.Crop += Formula.GetResource(structure.Lvl, (ushort)structure["Efficiency"]);
                        }
                    }
                }

                city.Resource.Add(resource);

                Resource Upkeep = city.Troops.Upkeep();
                /*   if (city.Resource.hasEnough(Upkeep)) {
                       city.Resource -= Upkeep;
                       city.Troops.Feed();
                   } else {
                       city.Resource.Crop=0;
                       city.Troops.Starve();
                   }*/
            }
        }
    }
}
