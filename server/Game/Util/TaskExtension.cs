using System.Threading.Tasks;
using Common;

namespace Game.Util
{
    public static class TaskExtension
    {
        private static readonly ILogger Logger = LoggerFactory.Current.GetLogger<Engine>();

        public static Task CatchUnhandledException(this Task task, string name)
        {
            return task.ContinueWith(continuation =>
            {
                Logger.Error("Caught exception in CatchUnhandledException for {0}, Status: {1}", name, continuation.Status);
                Engine.UnhandledExceptionHandler(continuation.Exception);
            }, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}