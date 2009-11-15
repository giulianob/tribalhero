using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Util;
using Game.Setup;

namespace Game.Logic {
    class Reqirement {
        public ushort type;
        public byte cmp;
        public byte min_lvl;
        public byte max_lvl;

        public byte min_dist;
        public byte max_dist;
        public Reqirement(ushort type, byte cmp, byte min_lvl, byte max_lvl, byte min_dist, byte max_dist) {
            this.type = type;
            this.cmp = cmp;
            this.min_lvl = min_lvl;
            this.max_lvl = max_lvl;
            this.min_dist = min_dist;
            this.max_dist = max_dist;
        }
    }

    class SimpleLayout : LayoutRequirement {
        List<Reqirement> requirements = new List<Reqirement>();
        public SimpleLayout() {
        }

        public override void add(Reqirement req) {
            requirements.Add(req);
        }

        public override bool validate(IEnumerable<Structure> objects, uint x, uint y) {
            List<Reqirement> list = new List<Reqirement>(requirements);
            List<Structure> game_objects = new List<Structure>(objects);

            if (ObjectTypeFactory.IsTileType("TileNonBuildable", Global.World.getTileType(x, y))) return false;
            foreach (Reqirement req in list) {
                Structure last_object = null;
                foreach (Structure obj in game_objects) {
                    if (satisfy(req, obj, x, y)) {
                        last_object = obj;
                        break;
                    }
                }
                if (last_object == null) {
                    return false;
                }
                else {
                    game_objects.Remove(last_object);
                }
            }
            return true;
        }

        private bool satisfy(Reqirement req, GameObject obj, uint x, uint y) {
            if (req.type != obj.Type) return false;
            if (obj.Lvl > req.max_lvl) return false;
            if (obj.Lvl < req.min_lvl) return false;

            int dist = obj.distance(x, y);
            if (dist > req.max_dist) return false;
            if (dist < req.min_dist) return false;
            return true;
        }

    }
}
