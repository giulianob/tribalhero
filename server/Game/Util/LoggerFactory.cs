using Common;

namespace Game.Util
{
    public class LoggerFactory
    {
        private static LoggerFactory loggerFactory;

        public static LoggerFactory Current
        {
            get
            {
                return loggerFactory ?? (loggerFactory = new LoggerFactory());
            }
        }

        public ILogger GetLogger<T>()
        {
            return new Log4NetLogger(typeof(T));
        }
    }
}
