using System.Threading.Tasks;

namespace Game.Util
{
    public static class TaskExtension
    {
        public static Task CatchUnhandledException(this Task task)
        {
            return task.ContinueWith(continuation =>
                              Engine.UnhandledExceptionHandler(continuation.Exception),
                              TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}