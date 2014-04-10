#region

using System;
using Game;
using Game.Setup;
using Game.Setup.DependencyInjection;
using NDesk.Options;

#endregion

namespace MapGenerator
{
    static class Program
    {
        private static IKernel kernel;

        private static void Main()
        {
            bool help = false;
            string settingsFile = string.Empty;

            string tmxFiles = "map";
            try
            {
                var p = new OptionSet
                {
                    {"?|help|h", v => help = true}, 
                    {"settings=", v => settingsFile = v},
                    {"tmxfiles=", v => tmxFiles = v},
                };
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch(Exception e)
            {
                Console.Out.WriteLine(e.Message);
                Environment.Exit(0);
            }

            if (help)
            {
                Console.Out.WriteLine("Usage: mapgenerator [--settings=settings.ini] [--tmxfiles=folderwithtmxmaps]");
                Environment.Exit(0);
            }

            Config.LoadConfigFile(settingsFile);
            kernel = Engine.CreateDefaultKernel(new MapGeneratorModule());

            kernel.Get<FactoriesInitializer>().CompileAndInit();
            kernel.Get<MapGenerator>().GenerateMap(tmxFiles);
        }
    }
}