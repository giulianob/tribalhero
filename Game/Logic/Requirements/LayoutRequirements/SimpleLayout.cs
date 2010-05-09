#region

using System.Collections.Generic;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public class Reqirement {
        public ushort type;
        public byte cmp;
        public byte minLvl;
        public byte maxLvl;

        public byte minDist;
        public byte maxDist;

        public Reqirement(ushort type, byte cmp, byte minLvl, byte maxLvl, byte minDist, byte maxDist) {
            this.type = type;
            this.cmp = cmp;
            this.minLvl = minLvl;
            this.maxLvl = maxLvl;
            this.minDist = minDist;
            this.maxDist = maxDist;
        }
    }

    class SimpleLayout : LayoutRequirement {
        public override bool Validate(Structure builder, ushort type, uint x, uint y) {
            foreach (Reqirement req in requirements) {
                switch (req.cmp) {
                    case 1:
                        if (!HaveBuilding(req, builder.City, x, y)) return false;
                        break;
                    case 2:
                        if (!HaveNoBuilding(req, builder.City, x, y)) return false;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        private bool HaveNoBuilding(Reqirement req, IEnumerable<Structure> objects, uint x, uint y) {
            foreach (Structure obj in objects) {
                if (req.type != obj.Type)
                    continue;

                if (obj.Lvl > req.maxLvl)
                    continue;

                if (obj.Lvl < req.minLvl)
                    continue;

                int dist = obj.RadiusDistance(x, y);

                if (dist > req.maxDist)
                    continue;

                if (dist < req.minDist)
                    continue;
                return false;
            }
            return true;
        }

        private bool HaveBuilding(Reqirement req, IEnumerable<Structure> objects, uint x, uint y) {
            foreach (Structure obj in objects) {
                if (req.type != obj.Type)
                    continue;

                if (obj.Lvl > req.maxLvl)
                    continue;

                if (obj.Lvl < req.minLvl)
                    continue;

                int dist = obj.RadiusDistance(x, y);

                if (dist > req.maxDist)
                    continue;

                if (dist < req.minDist)
                    continue;
                return true;
            }
            return false;
        }

    }
}