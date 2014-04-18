#region

using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Map;

#endregion

namespace Game.Logic.Requirements.LayoutRequirements
{
    public class AwayFromLayout : LayoutRequirement
    {
        private readonly Formula formula;

        private readonly ITileLocator tileLocator;

        public AwayFromLayout(Formula formula, ITileLocator tileLocator)
        {
            this.formula = formula;
            this.tileLocator = tileLocator;
        }

        public override bool Validate(IStructure builder, ushort type, uint x, uint y, byte size)
        {
            var effects = builder.Technologies.GetAllEffects(EffectInheritance.Upward).ToArray();

            foreach (var req in Requirements)
            {
                int radius = formula.GetAwayFromRadius(effects, req.MinDist, type);

                // Copy for local closure
                Requirement req1 = req;
                if (builder.City.Any(structure =>
                                     structure.Type == req1.Type &&
                                     structure.Lvl >= req1.MinLvl &&
                                     structure.Lvl <= req1.MaxLvl &&
                                     tileLocator.RadiusDistance(structure.PrimaryPosition, structure.Size, new Position(x, y), size) < radius + 1))
                {
                    return false;
                }
            }

            return true;
        }
    }
}