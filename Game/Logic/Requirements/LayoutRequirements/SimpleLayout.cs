#region

using System;
using System.Collections.Generic;
using System.Globalization;
using Game.Data;
using Game.Map;

#endregion

namespace Game.Logic.Requirements.LayoutRequirements
{
    public class SimpleLayout : LayoutRequirement
    {
        private readonly ITileLocator tileLocator;

        public SimpleLayout(ITileLocator tileLocator)
        {
            this.tileLocator = tileLocator;
        }

        public override bool Validate(IStructure builder, ushort type, uint x, uint y, byte size)
        {
            foreach (var req in Requirements)
            {
                LayoutComparison comparison;
                if (!Enum.TryParse(req.Cmp.ToString(CultureInfo.InvariantCulture), out comparison))
                {
                    throw new Exception(string.Format("Invalid comparison type specified for SimpleLayout: {0}", req.Cmp));
                }

                switch((LayoutComparison)req.Cmp)
                {
                    case LayoutComparison.Contains:
                        if (!HaveBuilding(req, builder.City, x, y, size))
                        {
                            return false;
                        }
                        break;
                    case LayoutComparison.NotContains:
                        if (!HaveNoBuilding(req, builder.City, x, y, size))
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        private bool HaveNoBuilding(Requirement req, IEnumerable<IStructure> objects, uint x, uint y, byte size)
        {
            foreach (var obj in objects)
            {
                if (req.Type != obj.Type || obj.Lvl > req.MaxLvl || obj.Lvl < req.MinLvl)
                {
                    continue;
                }

                int dist = tileLocator.RadiusDistance(obj.X, obj.Y, obj.Size, x, y, size);

                if (dist > req.MaxDist || dist < req.MinDist)
                {
                    continue;
                }

                return false;
            }
            return true;
        }

        private bool HaveBuilding(Requirement req, IEnumerable<IStructure> objects, uint x, uint y, byte size)
        {
            foreach (var obj in objects)
            {
                if (req.Type != obj.Type || obj.Lvl > req.MaxLvl || obj.Lvl < req.MinLvl)
                {
                    continue;
                }

                int dist = tileLocator.RadiusDistance(obj.X, obj.Y, obj.Size, x, y, size);

                if (dist > req.MaxDist || dist < req.MinDist)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}