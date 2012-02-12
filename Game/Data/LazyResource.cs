#region

using System;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Data
{
    public class LazyValue
    {
        #region Delegates

        public delegate void OnResourcesUpdate();

        #endregion

        private int limit;
        private int rate;
        private int upkeep;

        public LazyValue(int val)
        {
            RawValue = val;
            LastRealizeTime = SystemClock.Now;
        }

        public LazyValue(int val, DateTime lastRealizeTime, int rate, int upkeep)
        {
            RawValue = val;
            LastRealizeTime = lastRealizeTime;
            this.rate = rate;
            this.upkeep = upkeep;
        }

        public DateTime LastRealizeTime { get; private set; }

        public int Limit
        {
            get
            {
                return limit;
            }
            set
            {
                limit = value;
                CheckLimit();
                Update();
            }
        }

        public int Value
        {
            get
            {
                int delta = 0;
                int calculatedRate = GetCalculatedRate();

                if (calculatedRate != 0)
                {
                    int elapsed = Math.Max(0, (int)SystemClock.Now.Subtract(LastRealizeTime).TotalMilliseconds);
                    delta = elapsed/calculatedRate;
                }

                if (limit > 0 && (RawValue + delta) > limit)
                    return limit;

                return Math.Max(0, RawValue + delta);
            }
        }

        public int RawValue { get; private set; }

        public int Rate
        {
            get
            {
                return rate;
            }
            set
            {
                Realize();
                if (value < 0)
                    throw new Exception("Rate can not be negative");
                rate = value;
                Update();
            }
        }

        public int Upkeep
        {
            get
            {
                return upkeep;
            }
            set
            {
                Realize();
                if (value < 0)
                    throw new Exception("Upkeep can not be negative");

                upkeep = value;
                Update();
            }
        }

        public event OnResourcesUpdate ResourcesUpdate;

        private void Update()
        {
            if (ResourcesUpdate != null)
                ResourcesUpdate();
        }

        public void Add(int val)
        {
            if (val == 0)
                return;
            Realize();
            RawValue += val;
            CheckLimit();
            Update();
        }

        public void Subtract(int val)
        {
            if (val == 0)
                return;
            Realize();
            RawValue -= val;
            CheckLimit();
            Update();
        }

        private void Realize()
        {
            int calculatedRate = GetCalculatedRate();

            if (calculatedRate != 0)
            {
                DateTime now = SystemClock.Now;
                int elapsed = Math.Max(0, (int)now.Subtract(LastRealizeTime).TotalMilliseconds);
                int delta = elapsed/calculatedRate;

                RawValue += delta;

                int leftOver = elapsed%calculatedRate;

                LastRealizeTime = now.Subtract(new TimeSpan(0, 0, 0, 0, leftOver));

                CheckLimit();
            }
            else
                LastRealizeTime = SystemClock.Now;
        }

        private void CheckLimit()
        {
            if (limit > 0 && RawValue > limit)
                RawValue = limit;

            if (RawValue < 0)
                RawValue = 0;

            // Cap to just limit something really bad from happening
            if (RawValue > 99999)
                RawValue = 99999;
        }

        protected virtual int GetCalculatedRate()
        {
            int deltaRate = rate - upkeep;
            if (deltaRate <= 0)
                return 0;
            return (int)((3600000f/deltaRate)*Config.seconds_per_unit);
        }

        /// <summary>
        /// Returns the amount of resources received for the given timeframe.
        /// NOTE: This can return a negative amount if upkeep is higher than rate.
        /// </summary>
        /// <param name="millisecondInterval"></param>
        /// <returns></returns>
        public int GetAmountReceived(int millisecondInterval)
        {
            int deltaRate = rate - upkeep;
            if (deltaRate == 0)
                return 0;

            double effectiveRate = (3600000f / deltaRate) * Config.seconds_per_unit;
            return (int)(millisecondInterval/effectiveRate);
        }
    }

    public class AggressiveLazyValue : LazyValue
    {
        public AggressiveLazyValue(int val) : base(val)
        {
        }

        public AggressiveLazyValue(int val, DateTime lastRealizeTime, int rate, int upkeep) : base(val, lastRealizeTime, rate, upkeep)
        {
        }

        protected override int GetCalculatedRate()
        {
            int deltaRate = Rate - Upkeep;
            if (deltaRate == 0)
                return 0;
            return (int)((3600000f/deltaRate)*Config.seconds_per_unit);
        }
    }

    public class LazyResource
    {
        private bool isDirty;
        private bool isUpdating;

        public LazyResource(int crop,
                            DateTime cropRealizeTime,
                            int cropRate,
                            int cropUpkeep,
                            int gold,
                            DateTime goldRealizeTime,
                            int goldRate,
                            int iron,
                            DateTime ironRealizeTime,
                            int ironRate,
                            int wood,
                            DateTime woodRealizeTime,
                            int woodRate,
                            int labor,
                            DateTime laborRealizeTime,
                            int laborRate)
        {
            Crop = new LazyValue(crop, cropRealizeTime, cropRate, cropUpkeep);
            Gold = new LazyValue(gold, goldRealizeTime, goldRate, 0);
            Iron = new LazyValue(iron, ironRealizeTime, ironRate, 0);
            Wood = new LazyValue(wood, woodRealizeTime, woodRate, 0);
            Labor = new LazyValue(labor, laborRealizeTime, laborRate, 0);
            SetEvents();
        }

        public LazyResource(int crop, int gold, int iron, int wood, int labor)
        {
            Crop = new LazyValue(crop);
            Gold = new LazyValue(gold);
            Iron = new LazyValue(iron);
            Wood = new LazyValue(wood);
            Labor = new LazyValue(labor);
            SetEvents();
        }

        public LazyValue Crop { get; private set; }
        public LazyValue Wood { get; private set; }
        public LazyValue Iron { get; private set; }
        public LazyValue Gold { get; private set; }
        public LazyValue Labor { get; private set; }
        public event LazyValue.OnResourcesUpdate ResourcesUpdate;

        private void SetEvents()
        {
            Crop.ResourcesUpdate += Update;
            Gold.ResourcesUpdate += Update;
            Wood.ResourcesUpdate += Update;
            Iron.ResourcesUpdate += Update;
            Labor.ResourcesUpdate += Update;
        }

        public void SetLimits(int cropLimit, int goldLimit, int ironLimit, int woodLimit, int laborLimit)
        {
            BeginUpdate();
            Crop.Limit = cropLimit;
            Gold.Limit = goldLimit;
            Iron.Limit = ironLimit;
            Wood.Limit = woodLimit;
            Labor.Limit = laborLimit;
            EndUpdate();
        }

        public int FindMaxAffordable(Resource costPerUnit)
        {
            int cropDelta;
            if (costPerUnit.Crop == 0)
                cropDelta = int.MaxValue;
            else
                cropDelta = Crop.Value/costPerUnit.Crop;

            int goldDelta;
            if (costPerUnit.Gold == 0)
                goldDelta = int.MaxValue;
            else
                goldDelta = Gold.Value/costPerUnit.Gold;

            int ironDelta;
            if (costPerUnit.Iron == 0)
                ironDelta = int.MaxValue;
            else
                ironDelta = Iron.Value/costPerUnit.Iron;

            int woodDelta;
            if (costPerUnit.Wood == 0)
                woodDelta = int.MaxValue;
            else
                woodDelta = Wood.Value/costPerUnit.Wood;

            int laborDelta;
            if (costPerUnit.Labor == 0)
                laborDelta = int.MaxValue;
            else
                laborDelta = Labor.Value/costPerUnit.Labor;

            return Math.Min(cropDelta, Math.Min(goldDelta, Math.Min(laborDelta, Math.Min(woodDelta, ironDelta))));
        }

        public bool HasEnough(Resource cost)
        {
            return Crop.Value >= cost.Crop && Gold.Value >= cost.Gold && Wood.Value >= cost.Wood && Iron.Value >= cost.Iron && Labor.Value >= cost.Labor;
        }

        public void Subtract(Resource resource)
        {
            BeginUpdate();

            if (resource.Crop > 0)
                Crop.Subtract(resource.Crop);

            if (resource.Gold > 0)
                Gold.Subtract(resource.Gold);

            if (resource.Wood > 0)
                Wood.Subtract(resource.Wood);

            if (resource.Iron > 0)
                Iron.Subtract(resource.Iron);

            if (resource.Labor > 0)
                Labor.Subtract(resource.Labor);

            EndUpdate();
        }

        public void Subtract(Resource cost, out Resource actual)
        {
            BeginUpdate();
            actual = new Resource();
            Crop.Subtract((actual.Crop = (Crop.Value > cost.Crop ? cost.Crop : Crop.Value)));
            Gold.Subtract((actual.Gold = (Gold.Value > cost.Gold ? cost.Gold : Gold.Value)));
            Iron.Subtract((actual.Iron = (Iron.Value > cost.Iron ? cost.Iron : Iron.Value)));
            Wood.Subtract((actual.Wood = (Wood.Value > cost.Wood ? cost.Wood : Wood.Value)));
            Labor.Subtract((actual.Labor = (Labor.Value > cost.Labor ? cost.Labor : Labor.Value)));
            EndUpdate();
        }

        public void Subtract(Resource cost, Resource hidden, out Resource actual)
        {
            BeginUpdate();
            actual = new Resource();
            Crop.Subtract((actual.Crop = (Crop.Value - hidden.Crop > cost.Crop ? cost.Crop : Math.Max(0, Crop.Value - hidden.Crop))));
            Gold.Subtract((actual.Gold = (Gold.Value - hidden.Gold > cost.Gold ? cost.Gold : Math.Max(0, Gold.Value - hidden.Gold))));
            Iron.Subtract((actual.Iron = (Iron.Value - hidden.Iron > cost.Iron ? cost.Iron : Math.Max(0, Iron.Value - hidden.Iron))));
            Wood.Subtract((actual.Wood = (Wood.Value - hidden.Wood > cost.Wood ? cost.Wood : Math.Max(0, Wood.Value - hidden.Wood))));
            Labor.Subtract((actual.Labor = (Labor.Value - hidden.Labor > cost.Labor ? cost.Labor : Math.Max(0, Labor.Value - hidden.Labor))));
            EndUpdate();
        }

        public void Add(Resource resource)
        {
            Add(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor);
            Update();
        }

        public void Add(int crop, int gold, int iron, int wood, int labor)
        {
            BeginUpdate();
            Crop.Add(crop);
            Gold.Add(gold);
            Wood.Add(wood);
            Iron.Add(iron);
            Labor.Add(labor);
            EndUpdate();
        }

        public Resource GetResource()
        {
            return new Resource(Crop.Value, Gold.Value, Iron.Value, Wood.Value, Labor.Value);
        }

        public void BeginUpdate()
        {
            if (isUpdating)
                throw new Exception("Nesting beginupdate");

            isUpdating = true;
        }

        public void EndUpdate()
        {
            if (!isUpdating)
                return;

            isUpdating = false;

            if (isDirty)
                Update();

            isDirty = false;
        }

        private void Update()
        {
            if (!isUpdating)
            {
                if (ResourcesUpdate != null)
                    ResourcesUpdate();
            }
            else
                isDirty = true;
        }

        public override string ToString()
        {
            return "Gold " + Gold.Value + "/" + Gold.RawValue + "/" + Gold.Rate + Gold.LastRealizeTime + Environment.NewLine + " Wood " + Wood.Value + "/" +
                   Wood.RawValue + "/" + Wood.Rate + Wood.LastRealizeTime + Environment.NewLine + " Iron " + Iron.Value + "/" + Iron.RawValue + "/" + Iron.Rate +
                   Iron.LastRealizeTime + Environment.NewLine + " Crop " + Crop.Value + "/" + Crop.RawValue + "/" + Crop.Rate + Crop.LastRealizeTime +
                   Environment.NewLine + " Labor " + Labor.Value + "/" + Labor.RawValue + "/" + Labor.Rate + Labor.LastRealizeTime + Environment.NewLine;
        }
    }
}