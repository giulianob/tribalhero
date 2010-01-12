#region

using System;
using Game.Data.Stats;

#endregion

namespace Game.Data {
    public enum ResourceType : byte {
        Gold = 0,
        Crop = 1,
        Wood = 2,
        Iron = 3
    }

    public class Resource : BaseStats, IComparable {
        private int gold = 0;

        public int Gold {
            get { return gold; }
            set {
                gold = value;
                fireStatsUpdate();
            }
        }

        private int wood = 0;

        public int Wood {
            get { return wood; }
            set {
                wood = value;
                fireStatsUpdate();
            }
        }

        private int iron = 0;

        public int Iron {
            get { return iron; }
            set {
                iron = value;
                fireStatsUpdate();
            }
        }

        private int crop = 0;

        public int Crop {
            get { return crop; }
            set {
                crop = value;
                fireStatsUpdate();
            }
        }

        private int labor = 0;

        public int Labor {
            get { return labor; }
            set {
                labor = value;
                fireStatsUpdate();
            }
        }

        public Resource() {}

        public Resource(Resource copy) {
            crop = copy.Crop;
            gold = copy.Gold;
            wood = copy.Wood;
            labor = copy.Labor;
            Iron = copy.Iron;
        }

        public Resource(int crop, int gold, int iron, int wood, int labor) {
            this.crop = crop;
            this.gold = gold;
            this.iron = iron;
            this.wood = wood;
            this.labor = labor;
        }

        #region IComparable Members

        public int CompareTo(object obj) {
            Resource obj2 = obj as Resource;

            if (gold == obj2.gold && iron == obj2.iron && wood == obj2.wood && crop == obj2.crop && labor == obj2.labor)
                return 0;
            else
                return -1;
        }

        public static Resource operator -(Resource obj, Resource obj2) {
            return new Resource(obj.crop - obj2.crop, obj.gold - obj2.gold, obj.iron - obj2.iron, obj.wood - obj2.wood,
                                obj.labor - obj2.labor);
        }

        public static Resource operator +(Resource obj, Resource obj2) {
            return new Resource(obj.crop + obj2.crop, obj.gold + obj2.gold, obj.iron + obj2.iron, obj.wood + obj2.wood,
                                obj.labor + obj2.labor);
        }

        public static Resource operator *(Resource obj, double multiplier) {
            return new Resource((int) (obj.crop*multiplier), (int) (obj.gold*multiplier), (int) (obj.iron*multiplier),
                                (int) (obj.wood*multiplier), (int) (obj.labor*multiplier));
        }

        public static Resource operator *(Resource obj, int count) {
            return new Resource(obj.crop*count, obj.gold*count, obj.iron*count, obj.wood*count, obj.labor*count);
        }

        public static Resource operator /(Resource obj, int count) {
            return new Resource(obj.crop/count, obj.gold/count, obj.iron/count, obj.wood/count, obj.labor/count);
        }

        #endregion

        public int maxAffordable(Resource costPerUnit) {
            int cropDelta;
            if (costPerUnit.crop == 0)
                cropDelta = int.MaxValue;
            else
                cropDelta = (int) (crop/costPerUnit.crop);

            int goldDelta;
            if (costPerUnit.gold == 0)
                goldDelta = int.MaxValue;
            else
                goldDelta = (int) (gold/costPerUnit.gold);

            int ironDelta;
            if (costPerUnit.iron == 0)
                ironDelta = int.MaxValue;
            else
                ironDelta = (int) (iron/costPerUnit.iron);

            int woodDelta;
            if (costPerUnit.wood == 0)
                woodDelta = int.MaxValue;
            else
                woodDelta = (int) (wood/costPerUnit.wood);

            return Math.Min(cropDelta, Math.Min(goldDelta, Math.Min(woodDelta, ironDelta)));
        }

        public bool hasEnough(Resource cost) {
            if (crop < cost.crop)
                return false;
            if (gold < cost.gold)
                return false;
            if (wood < cost.wood)
                return false;
            if (iron < cost.iron)
                return false;
            if (labor < cost.labor)
                return false;
            return true;
        }

        public void subtract(Resource cost, out Resource actual) {
            actual = new Resource();
            crop -= (actual.crop = crop > cost.crop ? cost.crop : crop);
            gold -= (actual.gold = gold > cost.gold ? cost.gold : gold);
            iron -= (actual.iron = iron > cost.iron ? cost.iron : crop);
            wood -= (actual.wood = wood > cost.wood ? cost.wood : wood);
            labor -= (actual.labor = labor > cost.labor ? cost.labor : labor);
            fireStatsUpdate();
        }

        public void add(Resource cost) {
            crop += cost.crop;
            gold += cost.gold;
            iron += cost.iron;
            wood += cost.wood;
            labor += cost.labor;
            fireStatsUpdate();
        }

        internal void Clear() {
            crop = 0;
            gold = 0;
            iron = 0;
            wood = 0;
            labor = 0;
            fireStatsUpdate();
        }
    }
}