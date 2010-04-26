#region

using System;
using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util;

#endregion

namespace Launcher {
    public class Program {
        public static void Main(string[] args) {
            Factory.CompileConfigFiles();
            CSVToXML.Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);

            if (!Game.Engine.Start()) {
                throw new Exception("Failed to load server");
            }

            //Test Code
            using (new MultiObjectLock(Global.Forests)) {
                Global.Forests.CreateForestAt(1, 1000, 2, 5, 5);
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