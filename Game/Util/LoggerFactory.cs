using Ninject.Extensions.Logging;
using Ninject.Extensions.Logging.Log4net.Infrastructure;

namespace Game.Util
{
    public class LoggerFactory
    {
        private static ILoggerFactory loggerFactory;

        public static ILoggerFactory Current
        {
            get
            {
                return loggerFactory ?? (loggerFactory = new Log4NetLoggerFactory());
            }
        }
    }
}
