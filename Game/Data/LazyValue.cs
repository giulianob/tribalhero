using System;
using Game.Setup;
using Game.Util;

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

        public int RawValue { get; protected set; }

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

        protected virtual void CheckLimit()
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
}