#region

using System.IO;
using Game.Battle.CombatObjects;
using Game.Data;
using Ninject;

#endregion

namespace Game.Setup
{
    public class Factory
    {
        internal static void BuildFiles(string outputFile, string dir, string pattern)
        {
            outputFile = Path.GetFullPath(outputFile);
            Global.Logger.Info("Building CSV file " + outputFile);

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

                    using (
                            var sr =
                                    new StreamReader(new FileStream(fullFilename,
                                                                    FileMode.Open,
                                                                    FileAccess.Read,
                                                                    FileShare.ReadWrite)))
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

        public static void CompileConfigFiles()
        {
            Directory.CreateDirectory(Config.csv_compiled_folder);

            BuildFiles(Path.Combine(Config.csv_compiled_folder, "structure.csv"), Config.csv_folder, "*structure.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "action.csv"), Config.csv_folder, "*action.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"),
                       Config.csv_folder,
                       "*effect_requirement.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "init.csv"), Config.csv_folder, "*init.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "property.csv"), Config.csv_folder, "*property.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "layout.csv"), Config.csv_folder, "*layout.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "technology.csv"), Config.csv_folder, "*technology.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "technology_effects.csv"),
                       Config.csv_folder,
                       "*technology_effects.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "unit.csv"), Config.csv_folder, "*unit.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "unit_modifier.csv"),
                       Config.csv_folder,
                       "*unit_modifier.csv");
            BuildFiles(Path.Combine(Config.csv_compiled_folder, "object_type.csv"), Config.csv_folder, "object_type.csv");
        }

        public static void InitAll()
        {
            // These can be removed once they are injected as dependencies
            Ioc.Kernel.Get<ActionRequirementFactory>();
            Ioc.Kernel.Get<StructureFactory>();
            Ioc.Kernel.Get<EffectRequirementFactory>();
            Ioc.Kernel.Get<InitFactory>();
            Ioc.Kernel.Get<PropertyFactory>();
            Ioc.Kernel.Get<RequirementFactory>();
            Ioc.Kernel.Get<TechnologyFactory>();
            Ioc.Kernel.Get<UnitFactory>();
            Ioc.Kernel.Get<ObjectTypeFactory>();
            Ioc.Kernel.Get<UnitModFactory>();
            Ioc.Kernel.Get<MapFactory>();
            Ioc.Kernel.Get<CombatUnitFactory>();
        }
    }
}