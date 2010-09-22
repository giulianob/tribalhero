/*
 * Generates a graphviz file of the game's tree
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CSVToXML;
using Game.Data;
using Game.Data.Stats;
using Game.Logic;
using Game.Setup;

namespace GraphGenerator
{
    class Program
    {
        private const ushort MAIN_BUILDING = 2000;

        static StringWriter nodeConnections;                

        static List<int> processedStructures = new List<int>();
        static List<int> processedUnits = new List<int>();
        static List<int> processedTechnologies = new List<int>();
        static Dictionary<string, string> nodeDefintions = new Dictionary<string, string>();
        static Dictionary<string, string> lang = new Dictionary<string, string>();

        static List<List<string>> ranks = new List<List<string>>();

        const string template = @"digraph g {	
    graph [fontsize=32 labelloc=""t"" label=""TribalHero Tree"" splines=true overlap=false rankdir=""LR"" ranksep=""equally""];
    node [shape=none, fontsize=12];			
    edge [fontsize=12];

    %content%

}";

        enum Result
        {
            OK,
            EMPTY,
            ALREADY_PROCESSED
        }

        static void Main(string[] args)
        {
            Factory.CompileConfigFiles();
            Factory.InitAll();

            LoadLanguages();

            nodeConnections = new StringWriter(new StringBuilder());            
            
            ProcessStructure(StructureFactory.GetBaseStats(MAIN_BUILDING, 1), false);

            using (StreamWriter output = new StreamWriter(File.Create("output.txt"))) {
                using (StringWriter b = new StringWriter(new StringBuilder())) {
                    WriteDefinitions(b);
                    b.Write(nodeConnections.ToString());
                    WriteRankings(b);

                    string outStr = template.Replace("%content%", b.ToString());

                    output.Write(outStr);
                }
            }


            nodeConnections.Close();
        }

        private static void LoadLanguages() {
            string[] files = Directory.GetFiles(Config.csv_folder, "lang.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                using (CsvReader langReader = new CsvReader(new StreamReader(File.Open(file, FileMode.Open))))
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

        private static void WriteDefinitions(TextWriter output) {
            foreach (var kvp in nodeDefintions)
            {
                output.WriteLine("{0} {1}", kvp.Key, kvp.Value);
            }            
        }

        private static void WriteRankings(TextWriter output)
        {
            foreach (List<String> ranking in ranks) {
                output.Write("{ rank=\"same\"; ");
                foreach (string str in ranking)
                    output.Write("{0}; ", str);
                output.WriteLine("}");
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
