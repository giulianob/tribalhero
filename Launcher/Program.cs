#region

using System;
using CSVToXML;
using Game;
using Game.Setup;
using NDesk.Options;
using Ninject;
using Ninject.Extensions.Logging.Log4net.Infrastructure;
using log4net.Config;

#endregion

namespace Launcher
{
    public class Program
    {
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
            
            XmlConfigurator.Configure();
            Engine.AttachExceptionHandler();
            Config.LoadConfigFile(settingsFile);

            var kernel = Engine.CreateDefaultKernel();                        
            kernel.Get<FactoriesInitializer>().CompileAndInit();
            Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);
            
#if DEBUG
            // Always enable lock checking in debug mode
            Config.locks_check = true;

            // Empty db
            if (Config.database_empty)
            {
                Console.Out.Write("Are you sure you want to empty the database?(Y/N):");
                if (!Console.ReadKey().Key.ToString().ToLower().Equals("y"))
                {
                    return;
                }
            }
#endif

            // Start game engine
            var engine = kernel.Get<Engine>();

            engine.Start();

            // Quit if press alt+q
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key != ConsoleKey.Q || key.Modifiers != ConsoleModifiers.Alt)
                {
                    continue;
                }

                engine.Stop();
                return;
            }
        }
    }
}