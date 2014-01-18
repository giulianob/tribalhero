using System;

namespace Game.Util.TwoFactor
{
    class Unixtime
    {
        private readonly DateTime date;

        public Unixtime()
        {
            this.date = DateTime.Now;
        }

        public Unixtime(int timestamp)
        {
            DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime newDateTime = converted.AddSeconds(timestamp);
            this.date = newDateTime.ToLocalTime();
        }

        public DateTime ToDateTime()
        {
            return this.date;
        }

        public long ToTimeStamp()
        {
            TimeSpan span = (this.date - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
            return Convert.ToInt64(span.TotalSeconds);
        }
    }
}
