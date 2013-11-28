using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Requirements
{
    class DistributedPointSystemRequirement
    {
        public Error Validate(IStructure structure, uint techType)
        {
            var total = (1 + structure.Lvl) * structure.Lvl / 2;
            var used = structure.Technologies.GetEffects(EffectCode.PointSystemValue, EffectInheritance.Invisible).Sum(tech => (int)tech.Value[0]);
            var technology = structure.Technologies.FirstOrDefault(x => x.Type == techType);
            if (technology == null)
            {
                // if technology does not currently exists, it requires at least 1 point
                if (total - used <= 0)
                    return Error.Unexpected;
                return Error.Ok;
            }

            if (total - used < technology.Level + 1)
                return Error.Unexpected;
            return Error.Ok;
        }
    }
}
