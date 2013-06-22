#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Common;
using Game;
using Game.Data;
using Game.Data.Stats;
using Game.Logic;
using Game.Setup;
using NDesk.Options;
using Ninject;

#endregion

namespace GraphGenerator
{
    class Program
    {
        private const ushort MAIN_BUILDING = 2000;

        private const string TEMPLATE_LARGE = @"digraph g {	
    graph [fontsize=32 labelloc=""t"" bgcolor=""transparent"" splines=true overlap=false rankdir=""LR"" ranksep=""equally""];
    node [shape=none, fontsize=12];			
    edge [fontsize=12];

    %content%

}";

        private const string TEMPLATE = @"digraph g {	
    graph [size=45 fontsize=32 labelloc=""t"" bgcolor=""transparent"" splines=true overlap=false rankdir=""LR"" ranksep=""equally""];
    node [shape=none, fontsize=15];			
    edge [fontsize=15];

    %content%

}";

        private const string GV_ARGS = @"-Tpng -o{0} {1}";

        private static StringWriter nodeConnections;

        private static string[] imageDirectories = new[]
        {
                @"c:\source\gameclient\graphics\buildings", @"c:\source\gameclient\graphics\units",
                @"c:\source\gameclient\graphics\icons\props"
        };

        private static string gvPath = @"C:\Program Files (x86)\Graphviz2.26.3\bin\dot.exe";

        private static string output = "output";

        private static readonly List<int> processedStructures = new List<int>();

        private static readonly List<int> processedUnits = new List<int>();

        private static readonly List<int> processedTechnologies = new List<int>();

        private static readonly Dictionary<string, string> nodeDefintions = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> lang = new Dictionary<string, string>();

        private static readonly List<List<string>> ranks = new List<List<string>>();

        private static string settings = string.Empty;

        private static IKernel kernel;

        private static void ParseArgs()
        {
            var imgDirList = new List<string>();

            var p = new OptionSet
            {
                    {"img=", v => imgDirList.Add(v)},
                    {"gv-path=", v => gvPath = v},
                    {"output=", v => output = v},
                    {"settings=", v => settings = v}
            };

            p.Parse(Environment.GetCommandLineArgs());

            if (imgDirList.Count > 0)
            {
                imageDirectories = imgDirList.ToArray();
            }
        }

        private static void Main()
        {
            ParseArgs();
            if (!string.IsNullOrEmpty(settings))
            {
                Config.LoadConfigFile(settings);
            }
            kernel = Engine.CreateDefaultKernel();
            kernel.Get<FactoriesInitializer>().CompileAndInit();

            LoadLanguages();

            var rawPath = Path.Combine(Path.GetFullPath(output), "raw");
            Directory.CreateDirectory(rawPath);

            //Copy all images from image folders to output
            foreach (var directory in imageDirectories)
            {
                foreach (var file in Directory.GetFiles(directory, "*.png"))
                {
                    File.Copy(file, Path.Combine(rawPath, Path.GetFileName(file)), true);
                }
            }

            // Process
            nodeConnections = new StringWriter(new StringBuilder());

            ProcessStructure(kernel.Get<IStructureCsvFactory>().GetBaseStats(MAIN_BUILDING, 1), false);

            using (var b = new StringWriter(new StringBuilder()))
            {
                WriteDefinitions(b);
                b.Write(nodeConnections.ToString());
                WriteRankings(b);

                // Write small
                using (var outStream = new StreamWriter(File.Create(Path.Combine(rawPath, "tree.gv"))))
                {
                    string outStr = TEMPLATE.Replace("%content%", b.ToString());

                    outStream.Write(outStr);
                }

                RunGv(Path.GetFullPath(Path.Combine(output, "game-tree.png")), "tree.gv");

                // Write large                
                using (var outStream = new StreamWriter(File.Create(Path.Combine(rawPath, "tree-large.gv"))))
                {
                    string outStr = TEMPLATE_LARGE.Replace("%content%", b.ToString());

                    outStream.Write(outStr);
                }

                RunGv(Path.GetFullPath(Path.Combine(output, "game-tree-large.png")), "tree-large.gv");
            }

            nodeConnections.Close();
        }

        private static void RunGv(string outImg, string gvFile)
        {
            var info = new ProcessStartInfo
            {
                    FileName = gvPath,
                    Arguments = string.Format(GV_ARGS, outImg, gvFile),
                    WorkingDirectory = Path.Combine(Path.GetFullPath(output), "raw"),
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
            };
            Process proc = Process.Start(info);
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                Console.Out.WriteLine(proc.StandardError.ReadToEnd());
                Environment.Exit(-1);
            }
        }

        private static void LoadLanguages()
        {
            string[] files = Directory.GetFiles(Config.csv_folder, "lang.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                string fullFilename = file;
                using (var langReader = new CsvReader(new StreamReader(File.Open(fullFilename, FileMode.Open))))
                {
                    while (true)
                    {
                        string[] obj = langReader.ReadRow();
                        if (obj == null)
                        {
                            break;
                        }

                        if (obj[0] == string.Empty)
                        {
                            continue;
                        }

                        lang[obj[0]] = obj[1];
                    }
                }
            }
        }

        private static void WriteDefinitions(TextWriter outStream)
        {
            foreach (var kvp in nodeDefintions)
            {
                outStream.WriteLine("{0} {1}", kvp.Key, kvp.Value);
            }
        }

        private static void WriteRankings(TextWriter outStream)
        {
            foreach (var ranking in ranks)
            {
                outStream.Write("{ rank=\"same\"; ");
                foreach (var str in ranking)
                {
                    outStream.Write("{0}; ", str);
                }
                outStream.WriteLine("}");
            }
        }

        private static Result ProcessStructure(IStructureBaseStats structureBaseStats, bool skipUpgrades)
        {
            int hash = structureBaseStats.Type * 100 + structureBaseStats.Lvl;
            if (processedStructures.Contains(hash))
            {
                return Result.AlreadyProcessed;
            }

            Console.Out.WriteLine("Parsing " + structureBaseStats.Name + " " + structureBaseStats.Lvl);

            ActionRequirementFactory.ActionRecord record =
                    kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(structureBaseStats.WorkerId);

            processedStructures.Add(hash);

            bool hadConnection = false;

            if (structureBaseStats.Lvl == 1)
            {
                CreateDefinition(structureBaseStats);
            }

            if (record == null)
            {
                return Result.Empty;
            }

            // First pass
            foreach (var action in record.List)
            {
                switch(action.Type)
                {
                    case ActionType.StructureBuildActive:
                    case ActionType.StructureChangeActive:
                        IStructureBaseStats building = kernel.Get<IStructureCsvFactory>().GetBaseStats(ushort.Parse(action.Parms[0]), 1);
                        Result result = ProcessStructure(building, false);
                        if (result != Result.AlreadyProcessed)
                        {
                            if (action.Type == ActionType.StructureBuildActive)
                            {
                                WriteNode(structureBaseStats, building);
                            }
                            else if (action.Type == ActionType.StructureChangeActive)
                            {
                                WriteNode(structureBaseStats, building, "dashed", "Converts To");
                            }

                            hadConnection = true;
                        }
                        break;
                    case ActionType.UnitTrainActive:
                        IBaseUnitStats training =
                                kernel.Get<UnitFactory>().GetUnitStats(ushort.Parse(action.Parms[0]), 1);
                        if (!processedUnits.Contains(training.UnitHash))
                        {
                            WriteNode(structureBaseStats, training);
                            CreateDefinition(training);
                            hadConnection = true;
                            processedUnits.Add(training.UnitHash);
                        }
                        break;
                    case ActionType.TechnologyUpgradeActive:
                        TechnologyBase tech =
                                kernel.Get<TechnologyFactory>().GetTechnologyBase(ushort.Parse(action.Parms[0]), 1);
                        if (!processedTechnologies.Contains(tech.TechnologyHash))
                        {
                            WriteNode(structureBaseStats, tech);
                            CreateDefinition(tech);
                            hadConnection = true;
                            processedTechnologies.Add(tech.TechnologyHash);
                        }
                        break;
                }
            }

            // Second pass
            foreach (var action in record.List)
            {
                switch(action.Type)
                {
                    case ActionType.StructureUpgradeActive:
                        if (!skipUpgrades)
                        {
                            byte maxLvl = byte.Parse(action.Parms[0]);
                            IStructureBaseStats from = structureBaseStats;
                            var newRank = new List<String> {GetKey(from)};

                            for (int i = from.Lvl; i < maxLvl; i++)
                            {
                                IStructureBaseStats to = kernel.Get<IStructureCsvFactory>()
                                                           .GetBaseStats(from.Type, (byte)(i + 1));
                                Result result = ProcessStructure(to, true);
                                if (result == Result.Ok || i == maxLvl - 1)
                                {
                                    WriteNode(from, to);
                                    CreateDefinition(to);
                                    hadConnection = true;
                                    newRank.Add(GetKey(to));
                                    from = to;
                                }
                            }

                            if (newRank.Count > 1)
                            {
                                ranks.Add(newRank);
                            }
                        }
                        break;
                }
            }

            return hadConnection ? Result.Ok : Result.Empty;
        }

        private static string GetKey(IStructureBaseStats stats)
        {
            return "STRUCTURE_" + stats.StructureHash;
        }

        private static string GetKey(IBaseUnitStats unit)
        {
            return "UNIT_" + unit.UnitHash;
        }

        private static string GetKey(TechnologyBase tech)
        {
            return "TECH_" + tech.TechnologyHash;
        }

        private static void CreateDefinition(IStructureBaseStats stats)
        {
            nodeDefintions[GetKey(stats)] =
                    string.Format(
                                  "[label=\"{0} (Level {1})\", labelloc=\"b\", height=1, shape=none, image=\"{2}.png\"]",
                                  lang[stats.Name + "_STRUCTURE_NAME"],
                                  stats.Lvl,
                                  stats.SpriteClass);
        }

        private static void CreateDefinition(IBaseUnitStats stats)
        {
            nodeDefintions[GetKey(stats)] =
                    string.Format("[label=\"{0}\", labelloc=\"b\", height=1, shape=none, image=\"{1}.png\"]",
                                  lang[stats.Name + "_UNIT"],
                                  stats.SpriteClass);
        }

        private static void CreateDefinition(TechnologyBase tech)
        {
            nodeDefintions[GetKey(tech)] =
                    string.Format(
                                  "[label=\"{0}\", labelloc=\"b\", height=0.8, shape=none, image=\"paper-scroll.png\"]",
                                  lang[tech.Name + "_TECHNOLOGY_NAME"]);
        }

        private static void WriteNode(IStructureBaseStats from, IStructureBaseStats to)
        {
            nodeConnections.WriteLine("{0} -> {1};", GetKey(from), GetKey(to));
        }

        private static void WriteNode(IStructureBaseStats from, IStructureBaseStats to, string style, string label)
        {
            // Causes graphviz to freak out and draw a lot of edge intersections
            //nodeConnections.WriteLine("{0} -> {1} [ label = \"{2}\" ];", GetKey(from), GetKey(to), label);
            nodeConnections.WriteLine("{0} -> {1} [style={2} label=\"{3}\"];", GetKey(from), GetKey(to), style, label);
        }

        private static void WriteNode(IStructureBaseStats from, IBaseUnitStats to)
        {
            nodeConnections.WriteLine("{0} -> {1};", GetKey(from), GetKey(to));
        }

        private static void WriteNode(IStructureBaseStats from, TechnologyBase to)
        {
            nodeConnections.WriteLine("{0} -> {1};", GetKey(from), GetKey(to));
        }

        #region Nested type: Result

        private enum Result
        {
            Ok,

            Empty,

            AlreadyProcessed
        }

        #endregion
    }
}