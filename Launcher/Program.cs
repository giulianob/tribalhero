#region

using System;
using Game.Setup;

#endregion

namespace Launcher {
    public class Program {
        public static void Main(string[] args) {
            Factory.CompileConfigFiles();
            CSVToXML.Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);

            if (!Game.Engine.Start()) {
                throw new Exception("Failed to load server");
            }           

            while (true) {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key != ConsoleKey.Q || key.Modifiers != ConsoleModifiers.Alt)
                    continue;

                Game.Engine.Stop();
                return;
            }
        }
    }
}