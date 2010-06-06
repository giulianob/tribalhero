using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic {
    
    public class AwayFromLayout : LayoutRequirement {
        public override bool Validate(Structure builder, ushort type, uint x, uint y) {
            foreach (Reqirement req in requirements) {
                int radius = Formula.GetAwayFromRadius(builder.Technologies.GetAllEffects(EffectInheritance.SELF_ALL), req.minDist, type);
                if (builder.City.Any(o => o.Type == req.type && o.Lvl>=req.minLvl && o.Lvl<=req.maxLvl && o.RadiusDistance(x, y) < radius+1)) {
                    return false;
                }
            }
            return true;
        }
    }
}
