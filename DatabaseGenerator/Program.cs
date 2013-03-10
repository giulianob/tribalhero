#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace DatabaseGenerator
{
    class Program
    {
        private static IEnumerable<ushort> structureTypes;

        private static IEnumerable<uint> technologyTypes;

        private static IEnumerable<ushort> unitTypes;

        private static readonly Dictionary<string, string> lang = new Dictionary<string, string>();

        private static string output = "output";

        private static IKernel kernel;

        private static void Main()
        {
            string settings = string.Empty;
            try
            {
                var p = new OptionSet {{"output=", v => output = v}, {"settings=", v => settings = v}};

                p.Parse(Environment.GetCommandLineArgs());
            }
            catch(Exception)
            {
            }

            Config.LoadConfigFile(settings);            
            kernel = Engine.CreateDefaultKernel();
            kernel.Get<FactoriesInitializer>().CompileAndInit();

            LoadLanguages();

            // Get types
            structureTypes = from structure in kernel.Get<StructureFactory>().AllStructures()
                             group structure by structure.Type
                             into types orderby types.Key select types.Key;

            technologyTypes = from technology in kernel.Get<TechnologyFactory>().AllTechnologies()
                              group technology by technology.Techtype
                              into types orderby types.Key select types.Key;

            unitTypes = from unit in kernel.Get<UnitFactory>().AllUnits()
                        group unit by unit.Type
                        into types orderby types.Key select types.Key;

            // Process structures            
            Directory.CreateDirectory(output);
            using (var writer = new StreamWriter(File.Create(Path.Combine(output, "structure_listing.inc.php"))))
            {
                writer.Write(@"<?php
                    $structures = array(
                ");

                foreach (var type in structureTypes)
                {
                    if (kernel.Get<ObjectTypeFactory>().IsObjectType("DatabaseIgnoreStructures", type))
                    {
                        continue;
                    }

                    ProcessStructure(type);

                    StructureBaseStats stats = kernel.Get<StructureFactory>().GetBaseStats(type, 1);

                    var sprite = stats.SpriteClass;

                    // Sorry this is a bit of a hack, it's a CropField then we append the Mature status to it :)
                    if (kernel.Get<ObjectTypeFactory>().IsObjectType("CropField", type))
                    {
                        sprite =
                                kernel.Get<StructureFactory>()
                                   .AllStructures()
                                   .First(structure => structure.Lvl == 1 && structure.Name == "MATURE_" + stats.Name)
                                   .SpriteClass;
                    }

                    writer.WriteLine("'{2}_STRUCTURE' => array('name' => '{1}', 'sprite' => '{0}'),",
                                     sprite,
                                     lang[stats.Name + "_STRUCTURE_NAME"],
                                     stats.Name);
                }

                writer.Write(@");");
            }

            // Process units
            using (var writer = new StreamWriter(File.Create(Path.Combine(output, "unit_listing.inc.php"))))
            {
                writer.Write(@"<?php
                    $units = array(
                ");
                foreach (var type in unitTypes)
                {
                    ProcessUnit(type);

                    IBaseUnitStats stats = kernel.Get<UnitFactory>().GetUnitStats(type, 1);
                    writer.WriteLine("'{2}_UNIT' => array('name' => '{1}', 'sprite' => '{0}'),",
                                     stats.SpriteClass,
                                     lang[stats.Name + "_UNIT"],
                                     stats.Name);
                }
                writer.Write(@");");
            }

            // Process technologies
            // Process units
            using (var writer = new StreamWriter(File.Create(Path.Combine(output, "technology_listing.inc.php"))))
            {
                writer.Write(@"<?php
                    $technologies = array(
                ");
                foreach (var type in technologyTypes)
                {
                    if (kernel.Get<ObjectTypeFactory>().IsObjectType("DatabaseIgnoreTech", (ushort)type))
                    {
                        continue;
                    }

                    ProcessTechnology(type);

                    TechnologyBase stats = kernel.Get<TechnologyFactory>().GetTechnologyBase(type, 1);
                    writer.WriteLine("'{0}_TECHNOLOGY' => array('name' => '{1}'),",
                                     stats.Name,
                                     lang[stats.Name + "_TECHNOLOGY_NAME"]);
                }
                writer.Write(@");");
            }
        }

        // Technologies        
        private static void ProcessTechnology(uint type)
        {
            string generalTemplate = @"<?php
                // Generated by DatabaseGenerator program on #DATE#
	            $techKey = '#TECH#';
	            $techName = '#TECH_NAME#';
	
	            $trainedBy = array('name' => '#TRAINED_BY_NAME#', 'key' => '#TRAINED_BY#', 'level' => '#TRAINED_BY_LEVEL#');
	
	            // Levels array should contain:
	            $levels = array(
		            #LEVELS#
	            );

                include $includeLocation . 'technology_view.ctp';
            ";

            const string levelTemplate =
                    @"array('description' => '#DESCRIPTION#', 'time' => '#TIME#', 'gold' => #GOLD#, 'crop' => #CROP#, 'iron' => #IRON#, 'labor' => #LABOR#, 'wood' => #WOOD#, 'requirements' => array(#REQUIREMENTS#)),";

            string requirementTemplate = @"'#REQUIREMENT#',";

            // Get basic information
            TechnologyBase tech = kernel.Get<TechnologyFactory>().GetTechnologyBase(type, 1);

            generalTemplate = generalTemplate.Replace("#DATE#", DateTime.Now.ToString());
            generalTemplate = generalTemplate.Replace("#TECH#", tech.Name + "_TECHNOLOGY");
            generalTemplate = generalTemplate.Replace("#TECH_NAME#", lang[tech.Name + "_TECHNOLOGY_NAME"]);

            // Builder info
            StructureBaseStats trainer;
            FindTechnologyTrainer(type, 1, out trainer);
            generalTemplate = generalTemplate.Replace("#TRAINED_BY_NAME#",
                                                      trainer != null ? lang[trainer.Name + "_STRUCTURE_NAME"] : "");
            generalTemplate = generalTemplate.Replace("#TRAINED_BY#", trainer != null ? trainer.Name + "_STRUCTURE" : "");
            generalTemplate = generalTemplate.Replace("#TRAINED_BY_LEVEL#",
                                                      trainer != null ? trainer.Lvl.ToString() : "");

            // Level info
            byte level = 1;
            var levelsWriter = new StringWriter(new StringBuilder());
            TechnologyBase currentStats = tech;
            do
            {
                string currentLevel = levelTemplate.Replace("#DESCRIPTION#",
                                                            lang[currentStats.Name + "_TECHNOLOGY_LVL_" + level].Replace
                                                                    ("'", "\\'"));
                currentLevel = currentLevel.Replace("#TIME#", currentStats.Time.ToString());
                currentLevel = currentLevel.Replace("#GOLD#", currentStats.Resources.Gold.ToString());
                currentLevel = currentLevel.Replace("#CROP#", currentStats.Resources.Crop.ToString());
                currentLevel = currentLevel.Replace("#IRON#", currentStats.Resources.Iron.ToString());
                currentLevel = currentLevel.Replace("#LABOR#", currentStats.Resources.Labor.ToString());
                currentLevel = currentLevel.Replace("#WOOD#", currentStats.Resources.Wood.ToString());

                // Get requirements
                var requirementsWriter = new StringWriter(new StringBuilder());

                if (trainer != null)
                {
                    foreach (var requirement in GetTechnologyRequirements(type, level, trainer))
                    {
                        requirementsWriter.WriteLine(requirementTemplate.Replace("#REQUIREMENT#", requirement));
                    }
                }

                currentLevel = currentLevel.Replace("#REQUIREMENTS#", requirementsWriter.ToString());

                levelsWriter.WriteLine(currentLevel);

                FindTechnologyTrainer(type, level, out trainer);
                level++;
            }
            while ((currentStats = kernel.Get<TechnologyFactory>().GetTechnologyBase(type, level)) != null);

            generalTemplate = generalTemplate.Replace("#LEVELS#", levelsWriter.ToString());

            using (
                    var writer =
                            new StreamWriter(
                                    File.Create(Path.Combine(output, string.Format("{0}_TECHNOLOGY.ctp", tech.Name)))))
            {
                writer.Write(generalTemplate);
            }
        }

        private static void FindTechnologyTrainer(uint type, byte level, out StructureBaseStats trainer)
        {
            foreach (var builderType in structureTypes)
            {
                byte structureLevel = 1;
                StructureBaseStats stats;

                while ((stats = kernel.Get<StructureFactory>().GetBaseStats(builderType, structureLevel)) != null)
                {
                    structureLevel++;

                    ActionRequirementFactory.ActionRecord record =
                            kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(stats.WorkerId);

                    if (record == null)
                    {
                        continue;
                    }

                    if (
                            record.List.Any(
                                            action =>
                                            action.Type == ActionType.TechnologyUpgradeActive &&
                                            ushort.Parse(action.Parms[0]) == type &&
                                            byte.Parse(action.Parms[1]) >= level))
                    {
                        trainer = stats;
                        return;
                    }
                }
            }

            trainer = null;
        }

        private static IEnumerable<string> GetTechnologyRequirements(uint type, byte level, StructureBaseStats trainer)
        {
            ActionRequirementFactory.ActionRecord record =
                    kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(trainer.WorkerId);

            ActionRequirement foundAction =
                    record.List.FirstOrDefault(
                                               action =>
                                               action.Type == ActionType.TechnologyUpgradeActive &&
                                               ushort.Parse(action.Parms[0]) == type &&
                                               ushort.Parse(action.Parms[1]) >= level);

            if (foundAction != null)
            {
                var requirements =
                        kernel.Get<EffectRequirementFactory>()
                           .GetEffectRequirementContainer(foundAction.EffectReqId);
                foreach (var requirement in requirements)
                {
                    if (requirement.WebsiteDescription != string.Empty)
                    {
                        yield return requirement.WebsiteDescription;
                    }
                }
            }
        }

        // Units
        private static void ProcessUnit(ushort type)
        {
            string generalTemplate = @"<?php
                // Generated by DatabaseGenerator program on #DATE#
	            $unitKey = '#UNIT#';
	            $unitName = '#UNIT_NAME#';
	            $description = '#DESCRIPTION#';
	
	            $trainedBy = array('name' => '#TRAINED_BY_NAME#', 'key' => '#TRAINED_BY#', 'level' => '#TRAINED_BY_LEVEL#');
	
	            // Levels array should contain:
	            $levels = array(
		            #LEVELS#
	            );

                include $includeLocation . 'unit_view.ctp';
            ";

            const string levelTemplate =
                    @"array('time' => #TIME#, 'carry' => #CARRY#, 'speed' => #SPEED#, 'upkeep' => #UPKEEP#, 'splash' => #SPLASH#, 'trainTime' => #TRAIN_TIME#, 'trainGold' => #TRAIN_GOLD#, 'trainCrop' => #TRAIN_CROP#, 'trainIron' => #TRAIN_IRON#, 'trainLabor' => #TRAIN_LABOR#, 'trainWood' => #TRAIN_WOOD#, 'gold' => #GOLD#, 'crop' => #CROP#, 'iron' => #IRON#, 'labor' => #LABOR#, 'wood' => #WOOD#, 'hp' => #HP#, 'attack' => #ATTACK#, 'range' => #RANGE#, 'stealth' => #STEALTH#, 'armor' => '#ARMOR#', 'weapon' => '#WEAPON#', 'weaponClass' => '#WEAPONCLASS#', 'unitClass' => '#UNITCLASS#', 'requirements' => array(#REQUIREMENTS#)),";

            string requirementTemplate = @"'#REQUIREMENT#',";

            // Get basic information
            IBaseUnitStats stats = kernel.Get<UnitFactory>().GetUnitStats(type, 1);

            generalTemplate = generalTemplate.Replace("#DATE#", DateTime.Now.ToString());
            generalTemplate = generalTemplate.Replace("#UNIT#", stats.Name + "_UNIT");
            generalTemplate = generalTemplate.Replace("#UNIT_NAME#", lang[stats.Name + "_UNIT"]);
            generalTemplate = generalTemplate.Replace("#DESCRIPTION#",
                                                      lang[stats.Name + "_UNIT_DESC"].Replace("'", "\\'"));

            // Builder info
            StructureBaseStats trainer;
            FindUnitTrainer(type, 1, out trainer);
            generalTemplate = generalTemplate.Replace("#TRAINED_BY_NAME#",
                                                      trainer != null ? lang[trainer.Name + "_STRUCTURE_NAME"] : "");
            generalTemplate = generalTemplate.Replace("#TRAINED_BY#", trainer != null ? trainer.Name + "_STRUCTURE" : "");
            generalTemplate = generalTemplate.Replace("#TRAINED_BY_LEVEL#",
                                                      trainer != null ? trainer.Lvl.ToString() : "");

            // Level info
            byte level = 1;
            var levelsWriter = new StringWriter(new StringBuilder());
            IBaseUnitStats currentStats = stats;
            do
            {
                string currentLevel = levelTemplate.Replace("#TIME#", currentStats.UpgradeTime.ToString());
                currentLevel = currentLevel.Replace("#GOLD#", currentStats.UpgradeCost.Gold.ToString());
                currentLevel = currentLevel.Replace("#CROP#", currentStats.UpgradeCost.Crop.ToString());
                currentLevel = currentLevel.Replace("#IRON#", currentStats.UpgradeCost.Iron.ToString());
                currentLevel = currentLevel.Replace("#LABOR#", currentStats.UpgradeCost.Labor.ToString());
                currentLevel = currentLevel.Replace("#WOOD#", currentStats.UpgradeCost.Wood.ToString());
                currentLevel = currentLevel.Replace("#TRAIN_TIME#", currentStats.BuildTime.ToString());
                currentLevel = currentLevel.Replace("#TRAIN_GOLD#", currentStats.Cost.Gold.ToString());
                currentLevel = currentLevel.Replace("#TRAIN_CROP#", currentStats.Cost.Crop.ToString());
                currentLevel = currentLevel.Replace("#TRAIN_IRON#", currentStats.Cost.Iron.ToString());
                currentLevel = currentLevel.Replace("#TRAIN_LABOR#", currentStats.Cost.Labor.ToString());
                currentLevel = currentLevel.Replace("#TRAIN_WOOD#", currentStats.Cost.Wood.ToString());
                currentLevel = currentLevel.Replace("#HP#", currentStats.Battle.MaxHp.ToString());
                currentLevel = currentLevel.Replace("#SPLASH#", currentStats.Battle.Splash.ToString());
                currentLevel = currentLevel.Replace("#ATTACK#", currentStats.Battle.Atk.ToString());
                currentLevel = currentLevel.Replace("#RANGE#", currentStats.Battle.Rng.ToString());
                currentLevel = currentLevel.Replace("#STEALTH#", currentStats.Battle.Stl.ToString());
                currentLevel = currentLevel.Replace("#WEAPON#", currentStats.Battle.Weapon.ToString());
                currentLevel = currentLevel.Replace("#CARRY#", currentStats.Battle.Carry.ToString());
                currentLevel = currentLevel.Replace("#WEAPONCLASS#", currentStats.Battle.WeaponClass.ToString());
                currentLevel = currentLevel.Replace("#ARMOR#", currentStats.Battle.Armor.ToString());
                currentLevel = currentLevel.Replace("#UNITCLASS#", currentStats.Battle.ArmorClass.ToString());
                currentLevel = currentLevel.Replace("#SPEED#", currentStats.Battle.Spd.ToString());
                currentLevel = currentLevel.Replace("#UPKEEP#", currentStats.Upkeep.ToString());

                // Get requirements
                var requirementsWriter = new StringWriter(new StringBuilder());

                if (trainer != null)
                {
                    foreach (var requirement in GetUnitRequirements(type, level, trainer))
                    {
                        requirementsWriter.WriteLine(requirementTemplate.Replace("#REQUIREMENT#", requirement));
                    }
                }

                currentLevel = currentLevel.Replace("#REQUIREMENTS#", requirementsWriter.ToString());

                levelsWriter.WriteLine(currentLevel);

                FindUnitTrainer(type, level, out trainer);
                level++;
            }
            while ((currentStats = kernel.Get<UnitFactory>().GetUnitStats(type, level)) != null);

            generalTemplate = generalTemplate.Replace("#LEVELS#", levelsWriter.ToString());

            using (
                    var writer =
                            new StreamWriter(File.Create(Path.Combine(output, string.Format("{0}_UNIT.ctp", stats.Name))))
                    )
            {
                writer.Write(generalTemplate);
            }
        }

        private static void FindUnitTrainer(ushort type, byte level, out StructureBaseStats trainer)
        {
            foreach (var builderType in structureTypes)
            {
                byte structureLevel = 1;
                StructureBaseStats stats;

                while ((stats = kernel.Get<StructureFactory>().GetBaseStats(builderType, structureLevel)) != null)
                {
                    structureLevel++;

                    ActionRequirementFactory.ActionRecord record =
                            kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(stats.WorkerId);

                    if (record == null)
                    {
                        continue;
                    }

                    if (level == 1)
                    {
                        if (
                                record.List.Any(
                                                action =>
                                                action.Type == ActionType.UnitTrainActive &&
                                                ushort.Parse(action.Parms[0]) == type))
                        {
                            trainer = stats;
                            return;
                        }
                    }
                    else
                    {
                        if (
                                record.List.Any(
                                                action =>
                                                action.Type == ActionType.UnitUpgradeActive &&
                                                ushort.Parse(action.Parms[0]) == type &&
                                                byte.Parse(action.Parms[1]) >= level))
                        {
                            trainer = stats;
                            return;
                        }
                    }
                }
            }

            trainer = null;
        }

        private static IEnumerable<string> GetUnitRequirements(ushort type, byte level, StructureBaseStats trainer)
        {
            ActionRequirementFactory.ActionRecord record =
                    kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(trainer.WorkerId);

            ActionRequirement foundAction;

            if (level == 1)
            {
                foundAction =
                        record.List.FirstOrDefault(
                                                   action =>
                                                   action.Type == ActionType.UnitTrainActive &&
                                                   ushort.Parse(action.Parms[0]) == type);
            }
            else
            {
                foundAction =
                        record.List.FirstOrDefault(
                                                   action =>
                                                   action.Type == ActionType.UnitUpgradeActive &&
                                                   ushort.Parse(action.Parms[0]) == type &&
                                                   byte.Parse(action.Parms[1]) >= level);
            }

            if (foundAction == null)
            {
                yield break;
            }

            var requirements =
                    kernel.Get<EffectRequirementFactory>().GetEffectRequirementContainer(foundAction.EffectReqId);

            foreach (
                    var requirement in requirements.Where(requirement => requirement.WebsiteDescription != string.Empty)
                    )
            {
                yield return requirement.WebsiteDescription;
            }
        }

        // Structure Database
        private static void ProcessStructure(ushort type)
        {
            string generalTemplate = @"<?php
                // Generated by DatabaseGenerator program on #DATE#
	            $structureKey = '#STRUCTURE#';
	            $structureName = '#STRUCTURE_NAME#';
	            $description = '#DESCRIPTION#';
	
	            $converted = '#CONVERTED#';
	            $builtBy = array('name' => '#BUILT_BY_NAME#', 'key' => '#BUILT_BY#', 'level' => '#BUILT_BY_LEVEL#');
	
	            // Levels array should contain:
	            $levels = array(
		            #LEVELS#
	            );

                include $includeLocation . 'structure_view.ctp';
            ";

            const string levelTemplate =
                    @"array('description' => '#DESCRIPTION#', 'time' => #TIME#, 'splash' => #SPLASH#, 'gold' => #GOLD#, 'crop' => #CROP#, 'iron' => #IRON#, 'labor' => #LABOR#, 'wood' => #WOOD#, 'hp' => #HP#, 'attack' => #ATTACK#, 'range' => #RANGE#, 'stealth' => #STEALTH#, 'weapon' => '#WEAPON#', 'maxLabor' => #MAXLABOR#, 'requirements' => array(#REQUIREMENTS#)),";

            string requirementTemplate = @"'#REQUIREMENT#',";

            // Get basic information
            StructureBaseStats stats = kernel.Get<StructureFactory>().GetBaseStats(type, 1);

            generalTemplate = generalTemplate.Replace("#DATE#", DateTime.Now.ToString());
            generalTemplate = generalTemplate.Replace("#STRUCTURE#", stats.Name + "_STRUCTURE");
            generalTemplate = generalTemplate.Replace("#STRUCTURE_NAME#", lang[stats.Name + "_STRUCTURE_NAME"]);
            generalTemplate = generalTemplate.Replace("#DESCRIPTION#",
                                                      lang[stats.Name + "_STRUCTURE_DESCRIPTION"].Replace("'", "\\'"));

            // Builder info
            StructureBaseStats builder;
            bool converted;
            FindStructureBuilder(type, 1, out builder, out converted);
            generalTemplate = generalTemplate.Replace("#CONVERTED#", converted ? "1" : "0");
            generalTemplate = generalTemplate.Replace("#BUILT_BY_NAME#",
                                                      builder != null ? lang[builder.Name + "_STRUCTURE_NAME"] : "");
            generalTemplate = generalTemplate.Replace("#BUILT_BY#", builder != null ? builder.Name + "_STRUCTURE" : "");
            generalTemplate = generalTemplate.Replace("#BUILT_BY_LEVEL#", builder != null ? builder.Lvl.ToString() : "");

            // Level info
            byte level = 1;
            var levelsWriter = new StringWriter(new StringBuilder());
            StructureBaseStats currentStats = stats;
            do
            {
                string currentLevel = levelTemplate.Replace("#DESCRIPTION#",
                                                            lang[currentStats.Name + "_STRUCTURE_LVL_" + level].Replace(
                                                                                                                        "'",
                                                                                                                        "\\'"));
                currentLevel = currentLevel.Replace("#TIME#", currentStats.BuildTime.ToString());                
                currentLevel = currentLevel.Replace("#GOLD#", currentStats.Cost.Gold.ToString());
                currentLevel = currentLevel.Replace("#CROP#", currentStats.Cost.Crop.ToString());
                currentLevel = currentLevel.Replace("#IRON#", currentStats.Cost.Iron.ToString());
                currentLevel = currentLevel.Replace("#LABOR#", currentStats.Cost.Labor.ToString());
                currentLevel = currentLevel.Replace("#WOOD#", currentStats.Cost.Wood.ToString());
                currentLevel = currentLevel.Replace("#HP#", currentStats.Battle.MaxHp.ToString());
                currentLevel = currentLevel.Replace("#ATTACK#", currentStats.Battle.Atk.ToString());
                currentLevel = currentLevel.Replace("#RANGE#", currentStats.Battle.Rng.ToString());
                currentLevel = currentLevel.Replace("#STEALTH#", currentStats.Battle.Stl.ToString());
                currentLevel = currentLevel.Replace("#SPLASH#", currentStats.Battle.Splash.ToString());
                currentLevel = currentLevel.Replace("#WEAPON#", currentStats.Battle.Weapon.ToString());
                currentLevel = currentLevel.Replace("#MAXLABOR#", currentStats.MaxLabor.ToString());

                // Get requirements
                var requirementsWriter = new StringWriter(new StringBuilder());

                if (builder != null)
                {
                    foreach (var requirement in GetStructureRequirements(type, level, builder))
                    {
                        requirementsWriter.WriteLine(requirementTemplate.Replace("#REQUIREMENT#", requirement));
                    }
                }

                currentLevel = currentLevel.Replace("#REQUIREMENTS#", requirementsWriter.ToString());

                levelsWriter.WriteLine(currentLevel);

                level++;
                FindStructureBuilder(type, level, out builder, out converted);
            }
            while ((currentStats = kernel.Get<StructureFactory>().GetBaseStats(type, level)) != null);

            generalTemplate = generalTemplate.Replace("#LEVELS#", levelsWriter.ToString());

            using (
                    var writer =
                            new StreamWriter(
                                    File.Create(Path.Combine(output, string.Format("{0}_STRUCTURE.ctp", stats.Name)))))
            {
                writer.Write(generalTemplate);
            }
        }

        private static IEnumerable<string> GetStructureRequirements(ushort type, byte level, StructureBaseStats builder)
        {
            ActionRequirementFactory.ActionRecord record =
                    kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(builder.WorkerId);

            ActionRequirement foundAction = null;
            foreach (var action in record.List)
            {
                if (level == 1 && action.Type == ActionType.StructureBuildActive &&
                    ushort.Parse(action.Parms[0]) == type)
                {
                    foundAction = action;
                    break;
                }

                if (level == 1 && action.Type == ActionType.StructureChangeActive &&
                    ushort.Parse(action.Parms[0]) == type)
                {
                    foundAction = action;
                    break;
                }

                if (action.Type == ActionType.StructureUpgradeActive && byte.Parse(action.Parms[0]) >= level)
                {
                    foundAction = action;
                    break;
                }
            }

            if (foundAction != null)
            {
                var requirements =
                        kernel.Get<EffectRequirementFactory>()
                           .GetEffectRequirementContainer(foundAction.EffectReqId);
                foreach (var requirement in requirements)
                {
                    if (requirement.WebsiteDescription != string.Empty)
                    {
                        yield return requirement.WebsiteDescription;
                    }
                }
            }
        }

        private static void FindStructureBuilder(ushort type,
                                                 byte level,
                                                 out StructureBaseStats builder,
                                                 out bool convert)
        {
            if (level > 1)
            {
                builder = kernel.Get<StructureFactory>().GetBaseStats(type, (byte)(level - 1));
                convert = false;
                return;
            }

            foreach (var builderType in structureTypes)
            {
                if (builderType == type)
                {
                    continue;
                }

                byte structureLevel = 1;
                StructureBaseStats stats;

                while ((stats = kernel.Get<StructureFactory>().GetBaseStats(builderType, structureLevel)) != null)
                {
                    structureLevel++;

                    ActionRequirementFactory.ActionRecord record =
                            kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(stats.WorkerId);

                    if (record == null)
                    {
                        continue;
                    }

                    foreach (var action in record.List)
                    {
                        if (level == 1)
                        {
                            if (action.Type == ActionType.StructureBuildActive && ushort.Parse(action.Parms[0]) == type)
                            {
                                builder = stats;
                                convert = false;
                                return;
                            }

                            if (action.Type == ActionType.StructureChangeActive && ushort.Parse(action.Parms[0]) == type)
                            {
                                builder = stats;
                                convert = true;
                                return;
                            }
                        }
                    }
                }
            }

            builder = null;
            convert = false;
        }

        private static void LoadLanguages()
        {
            string[] files = Directory.GetFiles(Config.csv_folder, "lang.*.csv", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                using (var langReader = new CsvReader(new StreamReader(File.OpenRead(file))))
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
    }
}