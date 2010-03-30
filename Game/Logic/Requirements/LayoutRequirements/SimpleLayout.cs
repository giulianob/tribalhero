#region

using System.Collections.Generic;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    class Reqirement {
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
        private List<Reqirement> requirements = new List<Reqirement>();

        public override void Add(Reqirement req) {
            requirements.Add(req);
        }

        public override bool Validate(IEnumerable<Structure> objects, uint x, uint y) {
            List<Reqirement> list = new List<Reqirement>(requirements);
            List<Structure> gameObjects = new List<Structure>(objects);

            if (!ObjectTypeFactory.IsTileType("TileBuildable", Global.World.GetTileType(x, y)))
                return false;

            foreach (Reqirement req in list) {
                Structure lastObject = null;

                foreach (Structure obj in gameObjects) {
                    if (!Satisfy(req, obj, x, y))
                        continue;

                    lastObject = obj;
                    break;
                }
                
                if (lastObject == null)
                    return false;

                gameObjects.Remove(lastObject);
            }
            return true;
        }

        private static bool Satisfy(Reqirement req, GameObject obj, uint x, uint y) {
            if (req.type != obj.Type)
                return false;
            
            if (obj.Lvl > req.maxLvl)
                return false;

            if (obj.Lvl < req.minLvl)
                return false;

            int dist = obj.Distance(x, y);

            if (dist > req.maxDist)
                return false;

            if (dist < req.minDist)
                return false;

            return true;
        }
    }
}