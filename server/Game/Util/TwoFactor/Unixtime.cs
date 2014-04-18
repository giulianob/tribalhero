using System;

namespace Game.Util.TwoFactor
{
    class Unixtime
    {
        public long ToTimeStamp()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
    }
}
