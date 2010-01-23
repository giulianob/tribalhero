#region

using System;
using Game.Setup;

#endregion

namespace Game.Data {
    public class LazyValue {
        public delegate void OnResourcesUpdate();

        public event OnResourcesUpdate ResourcesUpdate;

        private DateTime last;

        public DateTime LastRealizeTime {
            get { return last; }
        }

        private int limit;

        public int Limit {
            get { return limit; }
            set {
                limit = value;
                CheckLimit();
                Update();
            }
        }

        private int value;

        public int Value {
            get {
                int delta = 0;

                if ((int) (rate*Config.seconds_per_unit) > 0) {
                    int elapsed = (int) DateTime.Now.Subtract(last).TotalMilliseconds;
                    delta = (int) (elapsed/(int) (rate*Config.seconds_per_unit));
                    if (delta < 0)
                        throw new Exception("Delta is negative?");
                }

                if (limit > 0 && (value + delta) > limit)
                    return limit;

                return value + delta;
            }
        }

        public int RawValue {
            get { return value; }
        }

        private int rate;

        public int Rate {
            get { return rate; }
            set {
                Realize();
                if (value < 0)
                    throw new Exception("Rate can not be negative");
                rate = value;
                Update();
            }
        }

        public LazyValue(int val) {
            value = val;
        }

        public LazyValue(int val, DateTime lastRealizeTime, int rate) {
            value = val;
            last = lastRealizeTime;
            this.rate = rate;
        }

        private void Update() {
            if (ResourcesUpdate != null)
                ResourcesUpdate();
        }

        public void Add(int val) {
            if (val == 0)
                return;
            Realize();
            value += val;
            CheckLimit();
            Update();
        }

        public void Subtract(int val) {
            if (val == 0)
                return;
            Realize();
            value -= val;
            CheckLimit();
            Update();
        }

        private void Realize() {
            if (rate > 0) {
                int elapsed = (int) DateTime.Now.Subtract(last).TotalMilliseconds;
                int delta = (int) (elapsed/(int) (rate*Config.seconds_per_unit));
                value += delta;
                int leftOver = elapsed%(int) (rate*Config.seconds_per_unit);
                DateTime now = DateTime.Now;
                last = now.Subtract(new TimeSpan(0, 0, 0, 0, leftOver));
                CheckLimit();
            } else
                last = DateTime.Now;
        }

        private void CheckLimit() {
            if (limit > 0 && value > limit)
                value = limit;
            if (value < 0)
                value = 0;
        }
    }

    public class LazyResource {
        public event LazyValue.OnResourcesUpdate ResourcesUpdate;

        private bool isUpdating;
        private bool isDirty;

        private LazyValue crop;

        public LazyValue Crop {
            get { return crop; }
        }

        private LazyValue wood;

        public LazyValue Wood {
            get { return wood; }
        }

        private LazyValue iron;

        public LazyValue Iron {
            get { return iron; }
        }

        private LazyValue gold;

        public LazyValue Gold {
            get { return gold; }
        }

        private LazyValue labor;

        public LazyValue Labor {
            get { return labor; }
        }

        public LazyResource(int crop, DateTime cropRealizeTime, int cropRate, int gold, DateTime goldRealizeTime,
                            int goldRate, int iron, DateTime ironRealizeTime, int ironRate, int wood,
                            DateTime woodRealizeTime, int woodRate, int labor, DateTime laborRealizeTime, int laborRate) {
            this.crop = new LazyValue(crop, cropRealizeTime, cropRate);
            this.gold = new LazyValue(gold, goldRealizeTime, goldRate);
            this.iron = new LazyValue(iron, ironRealizeTime, ironRate);
            this.wood = new LazyValue(wood, woodRealizeTime, woodRate);
            this.labor = new LazyValue(labor, laborRealizeTime, laborRate);
            SetEvents();
        }

        public LazyResource(int crop, int gold, int iron, int wood, int labor) {
            this.crop = new LazyValue(crop);
            this.gold = new LazyValue(gold);
            this.iron = new LazyValue(iron);
            this.wood = new LazyValue(wood);
            this.labor = new LazyValue(labor);
            SetEvents();
        }

        private void SetEvents() {
            crop.ResourcesUpdate += Update;
            gold.ResourcesUpdate += Update;
            wood.ResourcesUpdate += Update;
            iron.ResourcesUpdate += Update;
            labor.ResourcesUpdate += Update;
        }

        public void SetLimits(int cropLimit, int goldLimit, int ironLimit, int woodLimit, int laborLimit) {
            BeginUpdate();
            crop.Limit = cropLimit;
            gold.Limit = goldLimit;
            iron.Limit = ironLimit;
            wood.Limit = woodLimit;
            labor.Limit = laborLimit;
            EndUpdate();
        }

        public int FindMaxAffordable(Resource costPerUnit) {
            int cropDelta;
            if (costPerUnit.Crop == 0)
                cropDelta = int.MaxValue;
            else
                cropDelta = crop.Value/costPerUnit.Crop;

            int goldDelta;
            if (costPerUnit.Gold == 0)
                goldDelta = int.MaxValue;
            else
                goldDelta = gold.Value/costPerUnit.Gold;

            int ironDelta;
            if (costPerUnit.Iron == 0)
                ironDelta = int.MaxValue;
            else
                ironDelta = iron.Value/costPerUnit.Iron;

            int woodDelta;
            if (costPerUnit.Wood == 0)
                woodDelta = int.MaxValue;
            else
                woodDelta = wood.Value/costPerUnit.Wood;

            return Math.Min(cropDelta, Math.Min(goldDelta, Math.Min(woodDelta, ironDelta)));
        }

        public bool HasEnough(Resource cost) {
            if (crop.Value < cost.Crop)
                return false;
            if (gold.Value < cost.Gold)
                return false;
            if (wood.Value < cost.Wood)
                return false;
            if (iron.Value < cost.Iron)
                return false;
            if (labor.Value < cost.Labor)
                return false;
            return true;
        }

        public void Subtract(Resource resource) {
            BeginUpdate();

            if (resource.Crop > 0)
                crop.Subtract(resource.Crop);

            if (resource.Gold > 0)
                gold.Subtract(resource.Gold);

            if (resource.Wood > 0)
                wood.Subtract(resource.Wood);

            if (resource.Iron > 0)
                iron.Subtract(resource.Iron);

            if (resource.Labor > 0)
                labor.Subtract(resource.Labor);

            EndUpdate();
        }

        public void Subtract(Resource cost, out Resource actual) {
            BeginUpdate();
            actual = new Resource();
            crop.Subtract((actual.Crop = crop.Value > cost.Crop ? cost.Crop : crop.Value));
            gold.Subtract((actual.Gold = gold.Value > cost.Gold ? cost.Gold : gold.Value));
            iron.Subtract((actual.Iron = iron.Value > cost.Iron ? cost.Iron : crop.Value));
            wood.Subtract((actual.Wood = wood.Value > cost.Wood ? cost.Wood : wood.Value));
            labor.Subtract((actual.Labor = labor.Value > cost.Labor ? cost.Labor : labor.Value));
            EndUpdate();
        }

        public void Add(Resource resource) {
            Add(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor);
            Update();
        }

        public void Add(int crop, int gold, int iron, int wood, int labor) {
            BeginUpdate();
            this.crop.Add(crop);
            this.gold.Add(gold);
            this.wood.Add(wood);
            this.iron.Add(iron);
            this.labor.Add(labor);
            EndUpdate();
        }

        public Resource GetResource() {
            return new Resource(crop.Value, gold.Value, iron.Value, wood.Value, labor.Value);
        }

        public void BeginUpdate() {
            if (isUpdating)
                throw new Exception("Nesting beginupdate");
            isUpdating = true;
        }

        public void EndUpdate() {
            if (isUpdating) {
                isUpdating = false;
                
                if (isDirty)
                    Update();

                isDirty = false;
            }
        }

        private void Update() {
            if (!isUpdating) {
                if (ResourcesUpdate != null)
                    ResourcesUpdate();
            } else
                isDirty = true;
        }

        public override string ToString() {
            return "Gold " + gold.Value + "/" + gold.RawValue + "/" + gold.Rate + gold.LastRealizeTime +
                   Environment.NewLine + " Wood " + wood.Value + "/" + wood.RawValue + "/" + wood.Rate +
                   wood.LastRealizeTime + Environment.NewLine + " Iron " + iron.Value + "/" + iron.RawValue + "/" +
                   iron.Rate + iron.LastRealizeTime + Environment.NewLine + " Crop " + crop.Value + "/" + crop.RawValue +
                   "/" + crop.Rate + crop.LastRealizeTime + Environment.NewLine + " Labor " + labor.Value + "/" +
                   labor.RawValue + "/" + labor.Rate + labor.LastRealizeTime + Environment.NewLine;
        }
    }
}