#region

using System.ServiceProcess;

#endregion

namespace LauncherService
{
    static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] servicesToRun = new ServiceBase[] {new GameService()};
            ServiceBase.Run(servicesToRun);
        }
    }
}