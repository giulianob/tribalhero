/*
 * Generates a graphviz file of the game's tree
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CSVToXML;
using Game.Data;
using Game.Data.Stats;
using Game.Logic;
using Game.Setup;
using NDesk.Options;

namespace GraphGenerator
{
    class Program
    {
        private const ushort MAIN_BUILDING = 2000;

        static StringWriter nodeConnections;

        static string[] imageDirectories = new[]
                                               {
                                                   @"c:\source\game\graphics\buildings",
                                                   @"c:\source\game\graphics\units"
                                                   
                                               };

        static string gvPath = @"C:\Program Files (x86)\Graphviz2.26.3\bin\dot.exe";
        static string gvArgs = @"-Tpng -o{0} {1}";

        static string output = "output";

        static List<int> processedStructures = new List<int>();
        static List<int> processedUnits = new List<int>();
        static List<int> processedTechnologies = new List<int>();
        static Dictionary<string, string> nodeDefintions = new Dictionary<string, string>();
        static Dictionary<string, string> lang = new Dictionary<string, string>();

        static List<List<string>> ranks = new List<List<string>>();

        const string TEMPLATE_LARGE = @"digraph g {	
    graph [fontsize=32 labelloc=""t"" bgcolor=""transparent"" splines=true overlap=false rankdir=""LR"" ranksep=""equally""];
    node [shape=none, fontsize=12];			
    edge [fontsize=12];

    %content%

}";

        const string TEMPLATE = @"digraph g {	
    graph [size=34 fontsize=32 labelloc=""t"" bgcolor=""transparent"" splines=true overlap=false rankdir=""LR"" ranksep=""equally""];
    node [shape=none, fontsize=15];			
    edge [fontsize=15];

    %content%

}";

        enum Result
        {
            OK,
            EMPTY,
            ALREADY_PROCESSED
        }

        static void ParseArgs() {
            List<string> imgDirList = new List<string>();

            var p = new OptionSet
                        {
                            {"img=", v => imgDirList.Add(v)},
                            {"gv-path=", v => gvPath = v },     
                            {"output=", v => output = v },
                        };

            p.Parse(Environment.GetCommandLineArgs());

            if (imgDirList.Count > 0)
                imageDirectories = imgDirList.ToArray();
        }

        static void Main()
        {
            ParseArgs();
            Factory.CompileConfigFiles();
            Factory.InitAll();

            LoadLanguages();

            var rawPath = Path.Combine(Path.GetFullPath(output), "raw");
            Directory.CreateDirectory(rawPath);            

            //Copy all images from image folders to output
            foreach (string directory in imageDirectories) {
                foreach (string file in Directory.GetFiles(directory, "*.png")) {                                        
                    File.Copy(file, Path.Combine(rawPath, Path.GetFileName(file)), true);
                }
            }

            // Process
            nodeConnections = new StringWriter(new StringBuilder());            
            
            ProcessStructure(StructureFactory.GetBaseStats(MAIN_BUILDING, 1), false);

            using (StringWriter b = new StringWriter(new StringBuilder())) {
                WriteDefinitions(b);
                b.Write(nodeConnections.ToString());
                WriteRankings(b);

                // Write small
                using (StreamWriter outStream = new StreamWriter(File.Create(Path.Combine(rawPath, "tree.gv"))))
                {
                    string outStr = TEMPLATE.Replace("%content%", b.ToString());

                    outStream.Write(outStr);
                }

                RunGv(Path.Combine(output, "game-tree.png"), "tree.gv");
                
                // Write large                
                using (StreamWriter outStream = new StreamWriter(File.Create(Path.Combine(rawPath, "tree-large.gv"))))
                {
                    string outStr = TEMPLATE_LARGE.Replace("%content%", b.ToString());

                    outStream.Write(outStr);
                }

                RunGv(Path.Combine(output, "game-tree-large.png"), "tree-large.gv");
            }


            nodeConnections.Close();
        }

        private static void RunGv(string outImg, string gvFile) {
            ProcessStartInfo info = new ProcessStartInfo {
                                            FileName = gvPath,
                                            Arguments = string.Format(gvArgs, outImg, gvFile),
                                            WorkingDirectory = Path.Combine(Path.GetFullPath(output), "raw"),
                                            CreateNoWindow = true,                                            
                                        };         
            Process proc = Process.Start(info);
            proc.WaitForExit();
        }

        private static void LoadLanguages() {
            string[] files = Directory.GetFiles(Config.csv_folder, "lang.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string fullFilename = file;
                using (CsvReader langReader = new CsvReader(new StreamReader(File.Open(fullFilename, FileMode.Open))))
                {
                    while (true)
                    {
                        string[] obj = langReader.ReadRow();
                        if (obj == null)
                            break;

                        if (obj[0] == string.Empty)
                            continue;

                        lang[obj[0]] = obj[1];
                    }
                }
            }
        }

        private static void WriteDefinitions(TextWriter outStream) {
            foreach (var kvp in nodeDefintions)
            {
                outStream.WriteLine("{0} {1}", kvp.Key, kvp.Value);
            }            
        }

        private static void WriteRankings(TextWriter outStream)
        {
            foreach (List<String> ranking in ranks) {
                outStream.Write("{ rank=\"same\"; ");
                foreach (string str in ranking)
                    outStream.Write("{0}; ", str);
                outStream.WriteLine("}");
            }
        }

        private static Result ProcessStructure(StructureBaseStats structureBaseStats, bool skipUpgrades)
        {

            int hash = structureBaseStats.Type * 100 + structureBaseStats.Lvl;
            if (processedStructures.Contains(hash))
            {
                return Result.ALREADY_PROCESSED;
            }

            Console.Out.WriteLine("Parsing " + structureBaseStats.Name + " " + structureBaseStats.Lvl);

            ActionRecord record = ActionFactory.GetActionRequirementRecord(structureBaseStats.WorkerId);

            processedStructures.Add(hash);            

            bool hadConnection = false;

            if (structureBaseStats.Lvl == 1)
                CreateDefinition(structureBaseStats);

            if (record == null)
                return Result.EMPTY;

            // First pass
            foreach (ActionRequirement action in record.list)
            {
                switch (action.type)
                {
                    case ActionType.STRUCTURE_BUILD:
                    case ActionType.STRUCTURE_CHANGE:
                        StructureBaseStats building = StructureFactory.GetBaseStats(ushort.Parse(action.parms[0]), 1);
                        Result result = ProcessStructure(building, false);
                        if (result != Result.ALREADY_PROCESSED)
                        {
                            if (action.type == ActionType.STRUCTURE_BUILD)
                                WriteNode(structureBaseStats, building);
                            else if (action.type == ActionType.STRUCTURE_CHANGE)
                                WriteNode(structureBaseStats, building, "dashed", "Converts To");

                            hadConnection = true;
                        }
                        break;
                    case ActionType.UNIT_TRAIN:
                        BaseUnitStats training = UnitFactory.GetUnitStats(ushort.Parse(action.parms[0]), 1);
                        if (!processedUnits.Contains(training.UnitHash))
                        {
                            WriteNode(structureBaseStats, training);
                            CreateDefinition(training);
                            hadConnection = true;
                            processedUnits.Add(training.UnitHash);
                        }
                        break;
                    case ActionType.TECHNOLOGY_UPGRADE:
                        TechnologyBase tech = TechnologyFactory.GetTechnologyBase(ushort.Parse(action.parms[0]), 1);
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
            foreach (ActionRequirement action in record.list)
            {
                switch (action.type)
                {
                    case ActionType.STRUCTURE_UPGRADE:
                        if (!skipUpgrades)
                        {
                            byte maxLvl = byte.Parse(action.parms[0]);
                            StructureBaseStats from = structureBaseStats;
                            List<string> newRank = new List<String>
                                                       {
                                                           GetKey(from)
                                                       };
                            
                            for (int i = from.Lvl; i < maxLvl; i++)
                            {
                                StructureBaseStats to = StructureFactory.GetBaseStats(from.Type, (byte)(i + 1));
                                Result result = ProcessStructure(to, true);
                                if (result == Result.OK || i == maxLvl - 1)
                                {
                                    WriteNode(from, to);
                                    CreateDefinition(to);
                                    hadConnection = true;
                                    newRank.Add(GetKey(to));
                                    from = to;
                                }
                            }

                            if (newRank.Count > 1) ranks.Add(newRank);
                        }
                        break;
                }
            }

            return hadConnection ? Result.OK : Result.EMPTY;
        }

        private static string GetKey(StructureBaseStats stats) {
            return "STRUCTURE_" + stats.StructureHash;
        }

        private static string GetKey(BaseUnitStats unit)
        {
            return "UNIT_" + unit.UnitHash;
        }

        private static string GetKey(TechnologyBase tech)
        {
            return "TECH_" + tech.TechnologyHash;
        }

        private static void CreateDefinition(StructureBaseStats stats) {
            nodeDefintions[GetKey(stats)] = string.Format("[label=\"{0} (Level {1})\", labelloc=\"b\", height=1, shape=none, image=\"{2}.png\"]", lang[stats.Name + "_STRUCTURE_NAME"], stats.Lvl, stats.SpriteClass);
        }

        private static void CreateDefinition(BaseUnitStats stats)
        {
            nodeDefintions[GetKey(stats)] = string.Format("[label=\"{0}\", labelloc=\"b\", height=1, shape=none, image=\"{1}.png\"]", lang[stats.Name + "_UNIT"], stats.SpriteClass);
        }

        private static void CreateDefinition(TechnologyBase tech)
        {
            nodeDefintions[GetKey(tech)] = string.Format("[label=\"{0}\", shape=box]", lang[tech.name + "_TECHNOLOGY_NAME"]);
        }

        private static void WriteNode(StructureBaseStats from, StructureBaseStats to)
        {
            nodeConnections.WriteLine("{0} -> {1};", GetKey(from), GetKey(to));
        }

        private static void WriteNode(StructureBaseStats from, StructureBaseStats to, string style, string label)
        {
            // Causes graphviz to freak out and draw a lot of edge intersections
            //nodeConnections.WriteLine("{0} -> {1} [ label = \"{2}\" ];", GetKey(from), GetKey(to), label);
            nodeConnections.WriteLine("{0} -> {1} [style={2} label=\"{3}\"];", GetKey(from), GetKey(to), style, label);
        }

        private static void WriteNode(StructureBaseStats from, BaseUnitStats to)
        {
            nodeConnections.WriteLine("{0} -> {1};", GetKey(from), GetKey(to));
        }

        private static void WriteNode(StructureBaseStats from, TechnologyBase to)
        {
            nodeConnections.WriteLine("{0} -> {1};", GetKey(from), GetKey(to));
        }
    }
}
