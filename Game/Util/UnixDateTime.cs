using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Util {

    class UnixDateTime {
        public static readonly DateTime UnixBase = new DateTime(1970, 1, 1);
        public static DateTime UnixToDateTime(uint ts) {
            return UnixBase.AddSeconds(ts);
        }
        public static uint DateTimeToUnix(DateTime dtm) {
            return (uint)((TimeSpan)dtm.Subtract(UnixBase)).TotalSeconds;
        }
    }
}
