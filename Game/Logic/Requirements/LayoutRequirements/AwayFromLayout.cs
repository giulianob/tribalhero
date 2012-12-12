#region

using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Requirements.LayoutRequirements;

#endregion

namespace Game.Logic
{
    public class AwayFromLayout : LayoutRequirement
    {
        public override bool Validate(IStructure builder, ushort type, uint x, uint y)
        {
            var effects = builder.Technologies.GetAllEffects(EffectInheritance.Upward);

            foreach (var req in requirements)
            {
                int radius = Formula.Current.GetAwayFromRadius(effects, req.MinDist, type);
                Requirement req1 = req; // Copy for local closure
                if (
                        builder.City.Any(
                                         o =>
                                         o.Type == req1.Type && o.Lvl >= req1.MinLvl && o.Lvl <= req1.MaxLvl &&
                                         o.RadiusDistance(x, y) < radius + 1))
                {
                    return false;
                }
            }
            return true;
        }
    }
}