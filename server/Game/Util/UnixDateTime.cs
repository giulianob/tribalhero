#region

using System;

#endregion

namespace Game.Util
{
    static class UnixDateTime
    {
        public static readonly DateTime UnixBase = new DateTime(1970, 1, 1);

        public static DateTime UnixToDateTime(uint ts)
        {
            return UnixBase.AddSeconds(ts);
        }

        public static uint DateTimeToUnix(DateTime dtm)
        {
            return (uint)(dtm.Subtract(UnixBase)).TotalSeconds;
        }

        public static uint ToUnixTime(this DateTime dtm)
        {
            return DateTimeToUnix(dtm);
        }
    }
}