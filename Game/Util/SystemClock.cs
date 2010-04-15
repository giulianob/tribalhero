using System;

namespace Game.Util {

    public class SystemClock {

        private static DateTime clock = DateTime.MinValue;

        public static void SetClock(DateTime value) {
            clock = value;
        }

        public static DateTime Now {
            get { return clock == DateTime.MinValue ? DateTime.Now : clock; }
        }

        public static void ResyncClock() {
            clock = DateTime.MinValue;
        }
    }


}
