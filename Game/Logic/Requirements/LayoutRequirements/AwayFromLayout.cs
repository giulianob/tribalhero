using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic {
    
    public class AwayFromLayout : LayoutRequirement {
        public override bool Validate(Structure builder, ushort type, uint x, uint y) {

            var effects = builder.Technologies.GetAllEffects(EffectInheritance.SELF_ALL);

            foreach (Reqirement req in requirements) {
                int radius = Formula.GetAwayFromRadius(effects, req.minDist, type);
                Reqirement req1 = req; // Copy for local closure
                if (builder.City.Any(o => o.Type == req1.type && o.Lvl>=req1.minLvl && o.Lvl<=req1.maxLvl && o.RadiusDistance(x, y) < radius+1)) {
                    return false;
                }
            }
            return true;
        }
    }
}
