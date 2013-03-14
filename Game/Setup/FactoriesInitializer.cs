#region

using System.IO;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Util;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Setup
{
    public class FactoriesInitializer
    {
        private readonly ActionRequirementFactory actionRequirementFactory;

        private readonly StructureCsvFactory structureCsvFactory;

        private readonly EffectRequirementFactory effectRequirementFactory;

        private readonly InitFactory initFactory;

        private readonly PropertyFactory propertyFactory;

        private readonly RequirementFactory requirementFactory;

        private readonly TechnologyFactory technologyFactory;

        private readonly UnitFactory unitFactory;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly UnitModFactory unitModFactory;

        private readonly MapFactory mapFactory;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public FactoriesInitializer(ActionRequirementFactory actionRequirementFactory,
                                    StructureCsvFactory structureCsvFactory,
                                    EffectRequirementFactory effectRequirementFactory,
                                    InitFactory initFactory,
                                    PropertyFactory propertyFactory,
                                    RequirementFactory requirementFactory,
                                    TechnologyFactory technologyFactory,
                                    UnitFactory unitFactory,
                                    ObjectTypeFactory objectTypeFactory,
                                    UnitModFactory unitModFactory,
                                    MapFactory mapFactory)
        {
            this.actionRequirementFactory = actionRequirementFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.effectRequirementFactory = effectRequirementFactory;
            this.initFactory = initFactory;
            this.propertyFactory = propertyFactory;
            this.requirementFactory = requirementFactory;
            this.technologyFactory = technologyFactory;
            this.unitFactory = unitFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.unitModFactory = unitModFactory;
            this.mapFactory = mapFactory;
        }

        private void BuildFiles(string outputFile, string dir, string pattern)
        {
            outputFile = Path.GetFullPath(outputFile);
            logger.Info("Building CSV file " + outputFile);

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            using (var sw = new StreamWriter(outputFile))
            {
                bool headerWritten = false;
                foreach (var filename in Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    string fullFilename = filename;

                    if (Global.IsRunningOnMono())
                    {
                        fullFilename = Path.Combine(dir, filename);
                    }

                    using (var sr = new StreamReader(new FileStream(fullFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string header = sr.ReadLine();
                        if (!headerWritten)
                        {
                            sw.WriteLine(header);
                            headerWritten = true;
                        }
                        sw.Write(sr.ReadToEnd());
                    }
                }
            }
        }

        public void CompileAndInit()
        {
            Directory.CreateDirectory(Config.csv_compiled_folder);

            BuildFiles(Path.Combine(Config.csv_compiled_folder, "structure.csv"), Config.csv_folder, "*structure.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "action.csv"), Config.csv_folder, "*action.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"), Config.csv_folder, "*effect_requirement.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "init.csv"), Config.csv_folder, "*init.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "property.csv"), Config.csv_folder, "*property.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "layout.csv"), Config.csv_folder, "*layout.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "technology.csv"), Config.csv_folder, "*technology.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "technology_effects.csv"), Config.csv_folder, "*technology_effects.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "unit.csv"), Config.csv_folder, "*unit.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "unit_modifier.csv"), Config.csv_folder, "*unit_modifier.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "object_type.csv"), Config.csv_folder, "object_type.csv");

            actionRequirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "action.csv"));
            structureCsvFactory.Init(Path.Combine(Config.csv_compiled_folder, "structure.csv"));
            effectRequirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"));
            initFactory.Init(Path.Combine(Config.csv_compiled_folder, "init.csv"));
            propertyFactory.Init(Path.Combine(Config.csv_compiled_folder, "property.csv"));
            requirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "layout.csv"));
            technologyFactory.Init(Path.Combine(Config.csv_compiled_folder, "technology.csv"),
                                   Path.Combine(Config.csv_folder, "technology_effects.csv"));
            unitFactory.Init(Path.Combine(Config.csv_compiled_folder, "unit.csv"));
            objectTypeFactory.Init(Path.Combine(Config.csv_compiled_folder, "object_type.csv"));
            unitModFactory.Init(Path.Combine(Config.csv_compiled_folder, "unit_modifier.csv"));
            mapFactory.Init(Path.Combine(Config.maps_folder, "CityLocations.txt"));
        }
    }
}