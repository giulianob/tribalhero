package src.Util {
    import com.adobe.utils.DateUtil;

    import flash.globalization.*;

    public class DateUtil {

        /**
         * Returns a time formatted such as "01:20:09". If it's negative, time will be "--:--:--"
         * @param time
         * @return
         */
        public static function formatTime(time: int): String
        {
            if (time <= 0) return "--:--:--";

            var hours: int = int(time / (60 * 60));
            time -= hours * 60 * 60;
            var minutes: int = int(time / 60);
            time -= minutes * 60;
            var seconds: int = time;

            return (hours <= 9 ? "0" + hours : hours) + ":" + (minutes <= 9 ? "0" + minutes : minutes) + ":" + (seconds <= 9 ? "0" + seconds : seconds);
        }

        /**
         * Returns a time such as "1d 20h 43m"
         * @param time
         * @return
         */
        public static function simpleTime(time: int): String
        {
            if ( time < 60 ) return "1 min";
            var days: int = int(time / (60 * 60 * 24));
            time -= days * 60 * 60 * 24;
            var hours: int = int(time / (60 * 60));
            time -= hours * 60 * 60;
            var minutes: int = int(time / 60);
            time -= minutes * 60;
            var seconds: int = time;

            var simple: String = "";

            if ( days > 0 )
                simple += days + "d ";
            if ( days > 0 || hours > 0)
                simple += hours + "h ";
            simple += minutes + "m";
            return simple;
        }

        /**
         * Returns a string such as "3 days" or "less than 1 day" if less than a day
         * @param time
         * @return
         */
        public static function niceDays(time: int): String
        {
            time /= 86400;
            if (time > 1) {
                return time.toString() + " days";
            } else if ( time == 1) {
                return "1 day";
            }

            return "less than 1 day";
        }

        /**
         * Returns a time such as "1 hour and 5 minutes"
         * @param time
         * @return
         */
        public static function niceTime(time: int): String
        {
            if (time < 60) {
                return "less than 1 minute";
            }

            var hours: int = int(time / (60 * 60));
            time -= hours * 60 * 60;
            var minutes: int = int(time / 60);
            time -= minutes * 60;
            var seconds: int = time;

            if (seconds > 30) {
                minutes++;
            }

            if (minutes == 60) { // No idea why we "add 1 minute if seconds is greater than 30", but if it's added up to 60 minutes, increments the hour.
                minutes = 0;
                hours++;
            }

            var nice: String = "";

            if (hours > 0) {
                if (hours > 1)
                    nice += hours + " hours";
                else
                    nice += "1 hour";

                if (minutes > 0) {
                    nice += " and ";
                }
            }

            if (minutes > 1) {
                nice += minutes + " minutes";
            }
            else if (minutes == 1) {
                nice += "1 minute";
            }

            return nice;
        }

        /**
         * Returns a date in the format "Today, 1:00pm" or "Jun 2 13"
         * @return
         */
        public static function niceShort(unixTime: int): String {
            var formatter: DateTimeFormatter = new DateTimeFormatter(LocaleID.DEFAULT);
            var dateTime: Date = new Date(unixTime * 1000);
            var now: Date = new Date();
            var beginningOfToday: Date = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);

            if (dateTime >= beginningOfToday) {
                formatter.setDateTimePattern("h:mm a");
                return StringHelper.localize("STR_NICE_SHORT_TODAY", formatter.format(dateTime));
            }

            var beginningOfYesterday: Date = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 1, 0, 0, 0, 0);
            if (dateTime >= beginningOfYesterday) {
                formatter.setDateTimePattern("h:mm a");
                return StringHelper.localize("STR_NICE_SHORT_YESTERDAY", formatter.format(dateTime));
            }

            if (dateTime.getFullYear() === now.getFullYear()) {
                formatter.setDateTimePattern("MMM d, h:mm a")
            }
            else {
                formatter.setDateTimePattern("MMM d yyyy, h:mm a")
            }

            return formatter.format(dateTime);
        }

        private static const SECONDS_PER_MINUTE:uint = 60;
        private static const SECONDS_PER_TWO_MINUTES:uint = 120;
        private static const SECONDS_PER_HOUR:uint = 3600;
        private static const SECONDS_PER_TWO_HOURS:uint = 7200;
        private static const SECONDS_PER_DAY:uint = 86400;
        private static const SECONDS_PER_TWO_DAYS:uint = 172800;
        private static const SECONDS_PER_THREE_DAYS:uint = 259200;

        /**
         * Creates a human-readable String representing the difference
         * in time from the date provided and now. This method handles
         * dates in both the past and the future (e.g. "2 hours ago"
         * and "2 hours from now". For any date beyond 3 days difference
         * from now, then a standard format is returned.
         *
         * @param date The date for which to compare against.
         *
         * @return Human-readable String representing the time elapsed.
         */
        public static function getRelativeFromTimestamp(unixTime:int, capitalizeFirstLetter:Boolean = false):String
        {
            return getRelativeDate(new Date(unixTime * 1000), new Date(), capitalizeFirstLetter);
        }

        /**
         * Creates a human-readable String representing the difference
         * in time from the first date provided with respect to the
         * second date provided. If no second date is provided, then
         * the relative date will be calcluated with respect to "now".
         * This method handles dates in both the past and the
         * future (e.g. "2 hours ago" and "2 hours from now". For
         * any date beyond 3 days difference from now, then a
         * standard format is returned.
         *
         * @param firstDate The date for which to compare against.
         * @param secondDate The date to use as "present" when comparing against firstDate.
         *
         * @return Human-readable String representing the time elapsed.
         */
        public static function getRelativeDate(firstDate:Date, secondDate:Date = null, capitalizeFirstLetter:Boolean = false):String
        {
            var relativeDate:String;
            var isFuture:Boolean = false;

            if (secondDate == null)
            {
                secondDate = new Date();
            }

            // the difference between the passed-in date and now, in seconds
            var secondsElapsed:Number = (secondDate.getTime() - firstDate.getTime()) / 1000;

            if (secondsElapsed < 0)
            {
                isFuture = true;
                secondsElapsed = Math.abs(secondsElapsed);
            }

            switch(true)
            {
                case secondsElapsed < SECONDS_PER_MINUTE:
                    relativeDate = "just now";
                    break;
                case secondsElapsed < SECONDS_PER_TWO_MINUTES:
                    relativeDate = "1 minute " + ((isFuture) ? "from now" : "ago");
                    break;
                case secondsElapsed < SECONDS_PER_HOUR:
                    relativeDate = int(secondsElapsed / SECONDS_PER_MINUTE) + " minutes " + ((isFuture) ? "from now" : "ago");
                    break;
                case secondsElapsed < SECONDS_PER_TWO_HOURS:
                    relativeDate = "about an hour " + ((isFuture) ? "from now" : "ago");
                    break;
                case secondsElapsed < SECONDS_PER_DAY:
                    relativeDate = int(secondsElapsed / SECONDS_PER_HOUR) + " hours " + ((isFuture) ? "from now" : "ago");
                    break;
                case secondsElapsed < SECONDS_PER_TWO_DAYS:
                    relativeDate = ((isFuture) ? "tomorrow" : "yesterday") + " at " + com.adobe.utils.DateUtil.getShortHour(firstDate) + ":" + getMinutesString(firstDate) + com.adobe.utils.DateUtil.getAMPM(firstDate).toLowerCase();
                    break;
                case secondsElapsed < SECONDS_PER_THREE_DAYS:
                    relativeDate = com.adobe.utils.DateUtil.getFullDayName(firstDate) + " at " + com.adobe.utils.DateUtil.getShortHour(firstDate) + ":" + getMinutesString(firstDate) + com.adobe.utils.DateUtil.getAMPM(firstDate).toLowerCase();
                    break;
                default:
                    relativeDate = com.adobe.utils.DateUtil.getFullMonthName(firstDate) + " " + firstDate.getDate() + " at " + com.adobe.utils.DateUtil.getShortHour(firstDate) + ":" + getMinutesString(firstDate) + com.adobe.utils.DateUtil.getAMPM(firstDate).toLowerCase()
                    break;
            }

            return ((capitalizeFirstLetter) ? relativeDate.substring(0, 1).toUpperCase() + relativeDate.substring(1, relativeDate.length) : relativeDate);
        }

        /**
         * @private
         */
        private static function getMinutesString(date:Date):String
        {
            return ((date.minutes < 10) ? "0" : "") + date.minutes;
        }
    }
}