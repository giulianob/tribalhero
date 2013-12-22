#region

using System.IO;
using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Setup
{
    public class FactoriesInitializer
    {        
        private readonly ActionRequirementFactory actionRequirementFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly EffectRequirementFactory effectRequirementFactory;

        private readonly InitFactory initFactory;

        private readonly PropertyFactory propertyFactory;

        private readonly IRequirementCsvFactory requirementCsvFactory;

        private readonly TechnologyFactory technologyFactory;

        private readonly UnitFactory unitFactory;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly UnitModFactory unitModFactory;

        private readonly MapFactory mapFactory;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public FactoriesInitializer(ActionRequirementFactory actionRequirementFactory,
                                    IStructureCsvFactory structureCsvFactory,
                                    EffectRequirementFactory effectRequirementFactory,
                                    InitFactory initFactory,
                                    PropertyFactory propertyFactory,
                                    IRequirementCsvFactory requirementCsvFactory,
                                    TechnologyFactory technologyFactory,
                                    UnitFactory unitFactory,
                                    IObjectTypeFactory objectTypeFactory,
                                    UnitModFactory unitModFactory,
                                    MapFactory mapFactory)
        {
            this.actionRequirementFactory = actionRequirementFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.effectRequirementFactory = effectRequirementFactory;
            this.initFactory = initFactory;
            this.propertyFactory = propertyFactory;
            this.requirementCsvFactory = requirementCsvFactory;
            this.technologyFactory = technologyFactory;
            this.unitFactory = unitFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.unitModFactory = unitModFactory;
            this.mapFactory = mapFactory;
        }

        private void BuildFiles(string outputFile, string dir, string pattern)
        {
            outputFile = Path.GetFullPath(outputFile);
            logger.Info("Building CSV file {0}", outputFile);

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            using (var sw = new StreamWriter(outputFile))
            {
                bool headerWritten = false;
                logger.Info("Searching {0} for {1}", dir, pattern);

                foreach (var filename in Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    string fullFilename = filename;

                    if (Global.Current.IsRunningOnMono())
                    {
                        fullFilename = Path.Combine(dir, filename);
                    }

                    logger.Info("Appending {0}", fullFilename);

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

            logger.Info("Finished building {0}", outputFile);
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

            logger.Info("Action requirement factory init");
            actionRequirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "action.csv"));
            
            logger.Info("Structure csv factory init");
            structureCsvFactory.Init(Path.Combine(Config.csv_compiled_folder, "structure.csv"));

            logger.Info("Effect requirement factory init");
            effectRequirementFactory.Init(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"));
            
            logger.Info("Init factory init");
            initFactory.Init(Path.Combine(Config.csv_compiled_folder, "init.csv"));

            logger.Info("Property factory init");
            propertyFactory.Init(Path.Combine(Config.csv_compiled_folder, "property.csv"));

            logger.Info("Requirement csv factory init");
            requirementCsvFactory.Init(Path.Combine(Config.csv_compiled_folder, "layout.csv"));

            logger.Info("Technology factory init");
            technologyFactory.Init(Path.Combine(Config.csv_compiled_folder, "technology.csv"),
                                   Path.Combine(Config.csv_folder, "technology_effects.csv"));

            logger.Info("Unit factory init");
            unitFactory.Init(Path.Combine(Config.csv_compiled_folder, "unit.csv"));

            logger.Info("Object type factory init");
            objectTypeFactory.Init(Path.Combine(Config.csv_compiled_folder, "object_type.csv"));

            logger.Info("Unit mod factory init");
            unitModFactory.Init(Path.Combine(Config.csv_compiled_folder, "unit_modifier.csv"));

            logger.Info("Map factory init");
            mapFactory.Init(Path.Combine(Config.maps_folder, "CityLocations.txt"));
        }
    }
}