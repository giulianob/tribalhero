using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Data.Stats;
using Game.Logic;
using Game.Setup;

namespace GraphGenerator
{
    class Program
    {
        private const ushort MAIN_BUILDING = 2000;

        static StreamWriter output;

        static List<int> processedStructures = new List<int>();
        static List<int> processedUnits = new List<int>();
        static List<int> processedTechnologies = new List<int>();

        static List<List<string>> ranks = new List<List<string>>();

        enum Result
        {
            ERROR,
            OK,
            EMPTY,
            ALREADY_PROCESSED
        }

        static void Main(string[] args)
        {
            Factory.InitAll();

            using (output = new StreamWriter(File.Create("output.txt")))
            {
                ProcessStructure(StructureFactory.GetBaseStats(MAIN_BUILDING, 1), false);
                WriteRankings();
            }

        }

        private static void WriteRankings() {
            foreach (List<String> ranking in ranks) {
                output.Write("{ rank=\"same\"; ");
                foreach (string str in ranking)
                    output.Write("\"{0}\"; ", str);
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
                            WriteNode(structureBaseStats, building);
                            hadConnection = true;
                        }
                        break;
                    case ActionType.UNIT_TRAIN:
                        BaseUnitStats training = UnitFactory.GetUnitStats(ushort.Parse(action.parms[0]), 1);
                        if (!processedUnits.Contains(training.UnitHash))
                        {
                            WriteNode(structureBaseStats, training);
                            hadConnection = true;
                            processedUnits.Add(training.UnitHash);
                        }
                        break;
                    case ActionType.TECHNOLOGY_UPGRADE:
                        TechnologyBase tech = TechnologyFactory.GetTechnologyBase(ushort.Parse(action.parms[0]), 1);
                        if (!processedTechnologies.Contains(tech.TechnologyHash))
                        {
                            WriteNode(structureBaseStats, tech);
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
                                if (result == Result.OK)
                                {
                                    WriteNode(from, to);
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

        private static string GetKey(StructureBaseStats structureBaseStats) {
            return string.Format("{0} (Lvl {1})", structureBaseStats.Name, structureBaseStats.Lvl);
        }

        private static void WriteNode(StructureBaseStats from, StructureBaseStats to)
        {
            output.WriteLine("\"{0}\" -> \"{1}\";", GetKey(from), GetKey(to));
        }

        private static void WriteNode(StructureBaseStats from, BaseUnitStats to)
        {
            output.WriteLine("\"{0} (Lvl {2})\" -> \"{1} (Lvl {3})\";", from.Name, to.Name, from.Lvl, to.Lvl);
        }

        private static void WriteNode(StructureBaseStats from, TechnologyBase to)
        {
            output.WriteLine("\"{0} (Lvl {2})\" -> \"{1} (Lvl {3})\";", from.Name, to.name, from.Lvl, to.level);
        }
    }
}
