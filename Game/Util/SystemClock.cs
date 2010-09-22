using System;

namespace Game.Util {

    public class SystemClock {

        private static bool customTime;
        private static DateTime clock = DateTime.MinValue;

        public static void SetClock(DateTime value) {
            customTime = true;
            clock = value;
        }

        public static DateTime Now {
            get { return !customTime ? DateTime.UtcNow : clock; }
        }

        public static void ResyncClock() {
            customTime = false;
        }
    }


}
