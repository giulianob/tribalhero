#region

using System;
using CSVToXML;
using Game;
using Game.Data;
using Game.Setup;
using Game.Util;
using Mono.Unix;
using Mono.Unix.Native;
using NDesk.Options;
using log4net;
using Mono.Posix;

#endregion

namespace Launcher
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            bool help = false;
            string settingsFile = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"settings=", v => settingsFile = v},};
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch(Exception e)
            {
                Console.Out.WriteLine(e.Message);
                Environment.Exit(0);
            }

            if (help)
            {
                Console.Out.WriteLine("Usage: launcher [--settings=settings.ini]");
                Environment.Exit(0);
            }

            Log.Info("The game has begun");
            Engine.AttachExceptionHandler();
            Config.LoadConfigFile(settingsFile);

            var kernel = Engine.CreateDefaultKernel();                        
            kernel.Get<FactoriesInitializer>().CompileAndInit();
            Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);
            Converter.WatchLanguageFiles();
            
#if DEBUG
            // Always enable lock checking in debug mode
            Config.locks_check = true;

            // Empty db
            if (Config.database_empty)
            {
                Console.Out.Write("Are you sure you want to empty the database?(Y/N):");
                if (!Console.ReadKey().Key.ToString().ToLower().Equals("y"))
                {
                    Config.database_empty = false;
                }
            }

            if (Config.gc_monitor)
            {
                GcMonitor.MonitorGc();
            }
#endif

            // Start game engine
            var engine = kernel.Get<Engine>();
            engine.Start();

            // Loop and exit app if needed
            while (true)
            {
                if (Global.IsRunningOnMono())
                {
                    var unixExitSignals = new[]
                    {
                        new UnixSignal(Signum.SIGINT),
                        new UnixSignal(Signum.SIGTERM),
                    };
                    int id = UnixSignal.WaitAny(unixExitSignals);
                    if (id >= 0 && id < unixExitSignals.Length && unixExitSignals[id].IsSet)
                    {
                        break;
                    }
                }
                else
                {
                    var key = Console.ReadKey();
                    // Quit if press alt+q
                    if (key.Key == ConsoleKey.Q && key.Modifiers == ConsoleModifiers.Alt)
                    {
                        break;
                    }
                }
            }

            engine.Stop();
        }
    }
}