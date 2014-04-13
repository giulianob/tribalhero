using System;
using Game.Setup;

namespace Game.Data
{
    public class AggressiveLazyValue : LazyValue
    {
        public AggressiveLazyValue(int val)
                : base(val)
        {
        }

        public AggressiveLazyValue(int val, DateTime lastRealizeTime, int rate, int upkeep)
                : base(val, lastRealizeTime, rate, upkeep)
        {
        }

        protected override decimal GetCalculatedRate()
        {
            int deltaRate = Rate - Upkeep;
            if (deltaRate == 0)
            {
                return 0;
            }
            return (3600m / deltaRate) * (decimal)Config.seconds_per_unit;
        }
    }
}