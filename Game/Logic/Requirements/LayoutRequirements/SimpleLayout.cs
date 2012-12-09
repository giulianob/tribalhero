#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Logic.Requirements.LayoutRequirements
{
    class SimpleLayout : LayoutRequirement
    {
        public override bool Validate(IStructure builder, ushort type, uint x, uint y)
        {
            foreach (var req in requirements)
            {
                switch(req.Cmp)
                {
                    case 1:
                        if (!HaveBuilding(req, builder.City, x, y))
                        {
                            return false;
                        }
                        break;
                    case 2:
                        if (!HaveNoBuilding(req, builder.City, x, y))
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        private bool HaveNoBuilding(Requirement req, IEnumerable<IStructure> objects, uint x, uint y)
        {
            foreach (var obj in objects)
            {
                if (req.Type != obj.Type)
                {
                    continue;
                }

                if (obj.Lvl > req.MaxLvl)
                {
                    continue;
                }

                if (obj.Lvl < req.MinLvl)
                {
                    continue;
                }

                int dist = obj.RadiusDistance(x, y);

                if (dist > req.MaxDist)
                {
                    continue;
                }

                if (dist < req.MinDist)
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        private bool HaveBuilding(Requirement req, IEnumerable<IStructure> objects, uint x, uint y)
        {
            foreach (var obj in objects)
            {
                if (req.Type != obj.Type)
                {
                    continue;
                }

                if (obj.Lvl > req.MaxLvl)
                {
                    continue;
                }

                if (obj.Lvl < req.MinLvl)
                {
                    continue;
                }

                int dist = obj.RadiusDistance(x, y);

                if (dist > req.MaxDist)
                {
                    continue;
                }

                if (dist < req.MinDist)
                {
                    continue;
                }
                return true;
            }
            return false;
        }
    }
}