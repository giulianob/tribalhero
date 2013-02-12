#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Common;
using Game.Data;
using Game.Logic;
using Game.Setup;
using Game.Util;
using Ninject;

#endregion

namespace CSVToXML
{
    public class Converter
    {
        private static XmlTextWriter writer;

        private static String dataOutputFolder;

        private static String csvDataFolder;

        private static String langDataFolder;

        #region Layout

        private static List<Layout> FindAllLayout(List<Layout> layouts, int type, int level)
        {
            List<Layout> ret = new List<Layout>();
            foreach (Layout layout in layouts)
            {
                if (Int32.Parse(layout.type) == type && Int32.Parse(layout.level) == level)
                {
                    ret.Add(layout);
                }
            }

            return ret;
        }

        private class Layout
        {
            public string compare;

            public string layout;

            public string level;

            public string maxdist;

            public string maxlevel;

            public string mindist;

            public string minlevel;

            public string reqtype;

            public string type;
        }

        #endregion

        #region Technology Effects

        private static IEnumerable<TechnologyEffects> FindAllTechEffects(IEnumerable<TechnologyEffects> techEffects,
                                                                         int techid,
                                                                         int lvl)
        {
            return
                    techEffects.Where(effect => Int32.Parse(effect.techid) == techid && Int32.Parse(effect.lvl) == lvl)
                               .ToList();
        }

        private class TechnologyEffects
        {
            public string effect;

            public string isprivate;

            public string location;

            public string lvl;

            public string param1, param2, param3, param4, param5;

            public string techid;
        }

        #endregion

        public static string CleanCsvParam(string param)
        {
            char[] args = new[] {'='};
            string[] split = param.Split(args);

            return split[split.Length - 1].Trim();
        }

        public static void Go(string dataOutputFolderIn, string csvDataFolderIn, string langDataFolderIn)
        {
            dataOutputFolder = dataOutputFolderIn;
            csvDataFolder = csvDataFolderIn;
            langDataFolder = langDataFolderIn;

            #region Data XML

            writer = new XmlTextWriter(new StreamWriter(FileExtension.OverwriteLockedFile(dataOutputFolderIn + "data.xml")))
            {
                    Formatting = Formatting.None
            };
            writer.WriteStartElement("Data");

            WriteStructures();
            WriteUnits();
            WriteObjectTypes();
            WriteWorkers();
            WriteTechnologies();
            WriteProperties();
            WriteEffectRequirements();

            writer.WriteEndElement();
            writer.Close();

            #endregion

            WriteLanguageFile();
        }

        private static void WriteStructures()
        {
            #region Get Layouts

            List<Layout> layouts = new List<Layout>();

            using (
                    CsvReader layoutReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "layout.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = layoutReader.ReadRow();
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj[0].Length <= 0)
                    {
                        continue;
                    }
                    Layout layout = new Layout
                    {
                            type = obj[0],
                            level = obj[1],
                            layout = obj[2],
                            reqtype = obj[3],
                            minlevel = obj[4],
                            maxlevel = obj[5],
                            mindist = obj[6],
                            maxdist = obj[7],
                            compare = obj[8]
                    };

                    layouts.Add(layout);
                }
            }

            #endregion

            writer.WriteStartElement("Structures");
            using (
                    CsvReader statsReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "structure.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = statsReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == "")
                        {
                            continue;
                        }

                        writer.WriteStartElement("Structure");

                        writer.WriteAttributeString("name", obj[0]);
                        writer.WriteAttributeString("type", obj[1]);
                        writer.WriteAttributeString("level", obj[2]);
                        writer.WriteAttributeString("class", obj[3]);
                        writer.WriteAttributeString("spriteclass", obj[4]);
                        writer.WriteAttributeString("hp", obj[5]);
                        writer.WriteAttributeString("attack", obj[6]);
                        writer.WriteAttributeString("splash", obj[7]);
                        writer.WriteAttributeString("defense", obj[8]);
                        writer.WriteAttributeString("radius", obj[9]);
                        writer.WriteAttributeString("stealth", obj[10]);
                        writer.WriteAttributeString("range", obj[11]);
                        writer.WriteAttributeString("speed", obj[12]);
                        writer.WriteAttributeString("gold", obj[13]);
                        writer.WriteAttributeString("crop", obj[14]);
                        writer.WriteAttributeString("wood", obj[15]);
                        writer.WriteAttributeString("iron", obj[16]);
                        writer.WriteAttributeString("labor", obj[17]);
                        writer.WriteAttributeString("time", obj[18]);
                        if (int.Parse(obj[19]) != 0)
                        {
                            writer.WriteAttributeString("workerid", obj[19]);
                        }
                        else
                        {
                            writer.WriteAttributeString("workerid",
                                                        byte.Parse(obj[2]) == 0
                                                                ? "0"
                                                                : Ioc.Kernel.Get<ActionRequirementFactory>()
                                                                     .GetActionRequirementRecordBestFit(
                                                                                                        int.Parse(obj[1]),
                                                                                                        byte.Parse(
                                                                                                                   obj[2
                                                                                                                           ]))
                                                                     .Id.ToString());
                        }
                        writer.WriteAttributeString("weapon", obj[20]);
                        writer.WriteAttributeString("weaponclass", obj[21]);
                        writer.WriteAttributeString("unitclass", obj[22]);
                        writer.WriteAttributeString("armor", obj[23]);
                        writer.WriteAttributeString("maxlabor", obj[24]);

                        writer.WriteStartElement("Layout");

                        List<Layout> objLayouts = FindAllLayout(layouts, Int32.Parse(obj[1]), Int32.Parse(obj[2]));
                        foreach (Layout layout in objLayouts)
                        {
                            writer.WriteStartElement(layout.layout);
                            //Type,Lvl,Layout,Rtype,Lmin,Lmax,Dmin,Dmax,Cmp
                            writer.WriteAttributeString("type", layout.reqtype);
                            writer.WriteAttributeString("minlevel", layout.minlevel);
                            writer.WriteAttributeString("maxlevel", layout.maxlevel);
                            writer.WriteAttributeString("mindist", layout.mindist);
                            writer.WriteAttributeString("maxdist", layout.maxdist);
                            writer.WriteAttributeString("compare", layout.compare);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            writer.WriteEndElement();
        }

        private static void WriteUnits()
        {
            writer.WriteStartElement("Units");
            using (
                    CsvReader statsReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "unit.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = statsReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == "")
                        {
                            continue;
                        }

                        writer.WriteStartElement("Unit");

                        writer.WriteAttributeString("name", obj[0]);
                        writer.WriteAttributeString("type", obj[1]);
                        writer.WriteAttributeString("level", obj[2]);
                        writer.WriteAttributeString("weapon", obj[3]);
                        writer.WriteAttributeString("weaponclass", obj[4]);
                        writer.WriteAttributeString("unitclass", obj[5]);
                        writer.WriteAttributeString("armor", obj[6]);
                        writer.WriteAttributeString("class", obj[7]);
                        writer.WriteAttributeString("spriteclass", obj[8]);
                        writer.WriteAttributeString("hp", obj[9]);
                        writer.WriteAttributeString("attack", obj[10]);
                        writer.WriteAttributeString("splash", obj[11]);
                        writer.WriteAttributeString("defense", obj[12]);
                        writer.WriteAttributeString("vision", obj[13]);
                        writer.WriteAttributeString("stealth", obj[14]);
                        writer.WriteAttributeString("range", obj[15]);
                        writer.WriteAttributeString("speed", obj[16]);
                        writer.WriteAttributeString("crop", obj[17]);
                        writer.WriteAttributeString("gold", obj[18]);
                        writer.WriteAttributeString("iron", obj[19]);
                        writer.WriteAttributeString("wood", obj[20]);
                        writer.WriteAttributeString("labor", obj[21]);
                        writer.WriteAttributeString("time", obj[22]);
                        writer.WriteAttributeString("upgradecrop", obj[23]);
                        writer.WriteAttributeString("upgradegold", obj[24]);
                        writer.WriteAttributeString("upgradeiron", obj[25]);
                        writer.WriteAttributeString("upgradewood", obj[26]);
                        writer.WriteAttributeString("upgradelabor", obj[27]);
                        writer.WriteAttributeString("upgradetime", obj[28]);
                        writer.WriteAttributeString("groupsize", obj[29]);
                        writer.WriteAttributeString("upkeep", obj[30]);
                        writer.WriteAttributeString("carry", obj[31]);

                        writer.WriteEndElement();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            writer.WriteEndElement();
        }

        private static void WriteObjectTypes()
        {
            writer.WriteStartElement("ObjectTypes");
            using (
                    CsvReader statsReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "object_type.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = statsReader.ReadRow();
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj[0] == string.Empty)
                    {
                        continue;
                    }

                    string name = obj[0];
                    for (int i = 1; i < obj.Length; i++)
                    {
                        if (obj[i] == string.Empty)
                        {
                            continue;
                        }

                        writer.WriteStartElement("ObjectType");
                        writer.WriteAttributeString("name", name);
                        writer.WriteAttributeString("type", obj[i]);
                        writer.WriteEndElement();
                    }
                }
            }
            writer.WriteEndElement();
        }

        private static void WriteWorkers()
        {
            writer.WriteStartElement("Workers");

            #region write worker XML

            writer.WriteStartElement("Worker");
            writer.WriteAttributeString("type", "0");
            writer.WriteAttributeString("max", "0");
            writer.WriteEndElement();

            foreach (ActionRequirementFactory.ActionRecord record in Ioc.Kernel.Get<ActionRequirementFactory>())
            {
                writer.WriteStartElement("Worker");
                writer.WriteAttributeString("type", record.Id.ToString());
                foreach (ActionRequirement req in record.List)
                {
                    switch(req.Type)
                    {
                        case ActionType.ForestCampBuildActive:
                            writer.WriteStartElement("ForestCampBuild");
                            writer.WriteAttributeString("type", req.Parms[0]);
                            break;
                        case ActionType.ForestCampRemoveActive:
                            writer.WriteStartElement("ForestCampRemove");
                            break;
                        case ActionType.RoadBuildActive:
                            writer.WriteStartElement("RoadBuild");
                            break;
                        case ActionType.RoadDestroyActive:
                            writer.WriteStartElement("RoadDestroy");
                            break;
                        case ActionType.StructureBuildActive:
                            writer.WriteStartElement("StructureBuild");
                            writer.WriteAttributeString("type", req.Parms[0]);
                            writer.WriteAttributeString("tilerequirement", req.Parms[1]);
                            writer.WriteAttributeString("level",
                                                        req.Parms[2].Trim() == string.Empty ? "1" : req.Parms[2]);
                            break;
                        case ActionType.UnitTrainActive:
                            writer.WriteStartElement("TrainUnit");
                            writer.WriteAttributeString("type", req.Parms[0]);
                            break;
                        case ActionType.StructureChangeActive:
                            writer.WriteStartElement("StructureChange");
                            writer.WriteAttributeString("type", req.Parms[0]);
                            writer.WriteAttributeString("level", req.Parms[1]);
                            break;
                        case ActionType.StructureUpgradeActive:
                            writer.WriteStartElement("StructureUpgrade");
                            break;
                        case ActionType.UnitUpgradeActive:
                            writer.WriteStartElement("UnitUpgrade");
                            writer.WriteAttributeString("type", req.Parms[0]);
                            writer.WriteAttributeString("maxlevel", req.Parms[1]);
                            break;
                        case ActionType.TechnologyUpgradeActive:
                            writer.WriteStartElement("TechnologyUpgrade");
                            writer.WriteAttributeString("type", req.Parms[0]);
                            writer.WriteAttributeString("maxlevel", req.Parms[1]);
                            break;
                        case ActionType.ResourceSendActive:
                            writer.WriteStartElement("ResourceSend");
                            break;
                        case ActionType.ResourceSellActive:
                            writer.WriteStartElement("ResourceSell");
                            break;
                        case ActionType.ResourceBuyActive:
                            writer.WriteStartElement("ResourceBuy");
                            break;
                        case ActionType.LaborMoveActive:
                            writer.WriteStartElement("LaborMove");
                            break;
                        case ActionType.StructureDowngradeActive:
                            writer.WriteStartElement("StructureDowngrade");
                            break;
                        case ActionType.StructureSelfDestroyActive:
                            writer.WriteStartElement("StructureSelfDestroy");
                            break;
                        case ActionType.ResourceGatherActive:
                            writer.WriteStartElement("ResourceGather");
                            break;
                        case ActionType.TribeContributeActive:
                            writer.WriteStartElement("TribeContribute");
                            break;
                        default:
                            writer.WriteStartElement("MISSING_WORKER_ACTION");
                            writer.WriteAttributeString("name", Enum.GetName(typeof(ActionType), req.Type));
                            writer.WriteAttributeString("command", req.Parms[4]);
                            break;
                    }

                    writer.WriteAttributeString("index", req.Index.ToString());
                    writer.WriteAttributeString("option", ((int)req.Option).ToString());

                    if (req.EffectReqId != 0)
                    {
                        writer.WriteAttributeString("effectreq", req.EffectReqId.ToString());
                        writer.WriteAttributeString("effectreqinherit", req.EffectReqInherit.ToString().ToUpper());
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            #endregion

            writer.WriteEndElement();
        }

        private static void WriteTechnologies()
        {
            #region Get Tech Effects

            List<TechnologyEffects> techEffects = new List<TechnologyEffects>();

            using (
                    CsvReader techEffectsReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "technology_effects.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = techEffectsReader.ReadRow();
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj[0].Length <= 0)
                    {
                        continue;
                    }

                    TechnologyEffects techEffect = new TechnologyEffects
                    {
                            techid = obj[0],
                            lvl = obj[1],
                            effect = ((int)((EffectCode)Enum.Parse(typeof(EffectCode), obj[2], true))).ToString(),
                            location =
                                    ((int)((EffectLocation)Enum.Parse(typeof(EffectLocation), obj[3], true))).ToString(),
                            isprivate = obj[4],
                            param1 = obj[5],
                            param2 = obj[6],
                            param3 = obj[7],
                            param4 = obj[8],
                            param5 = obj[9]
                    };

                    techEffects.Add(techEffect);
                }
            }

            #endregion

            writer.WriteStartElement("Technologies");

            using (
                    CsvReader techReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "technology.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = techReader.ReadRow();
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj[0] == string.Empty)
                    {
                        continue;
                    }

                    writer.WriteStartElement("Tech");

                    TechnologyBase techBase = Ioc.Kernel.Get<TechnologyFactory>()
                                                 .GetTechnologyBase(uint.Parse(obj[0]), byte.Parse(obj[3]));

                    writer.WriteAttributeString("techtype", obj[0]);
                    writer.WriteAttributeString("name", obj[1]);
                    writer.WriteAttributeString("spriteclass", obj[2]);
                    writer.WriteAttributeString("level", obj[3]);
                    writer.WriteAttributeString("crop", techBase.Resources.Crop.ToString());
                    writer.WriteAttributeString("gold", techBase.Resources.Gold.ToString());
                    writer.WriteAttributeString("iron", techBase.Resources.Iron.ToString());
                    writer.WriteAttributeString("wood", techBase.Resources.Wood.ToString());
                    writer.WriteAttributeString("labor", techBase.Resources.Labor.ToString());
                    writer.WriteAttributeString("time", techBase.Time.ToString());

                    IEnumerable<TechnologyEffects> effects = FindAllTechEffects(techEffects,
                                                                                Int32.Parse(obj[0]),
                                                                                Int32.Parse(obj[3]));
                    foreach (TechnologyEffects effect in effects)
                    {
                        //TechType,Lvl,Effect,Location,IsPrivate,P1,P2,P3,P4,P5
                        writer.WriteStartElement("Effect");
                        writer.WriteAttributeString("effect", effect.effect);
                        writer.WriteAttributeString("location", effect.location);
                        writer.WriteAttributeString("private", effect.isprivate);
                        writer.WriteAttributeString("param1", effect.param1);
                        writer.WriteAttributeString("param2", effect.param2);
                        writer.WriteAttributeString("param3", effect.param3);
                        writer.WriteAttributeString("param4", effect.param4);
                        writer.WriteAttributeString("param5", effect.param5);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        private static void WriteProperties()
        {
            writer.WriteStartElement("Property");

            using (
                    CsvReader propReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "property.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = propReader.ReadRow();
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj[0].Length <= 0)
                    {
                        continue;
                    }

                    writer.WriteStartElement("Prop");
                    writer.WriteAttributeString("type", obj[0]);
                    writer.WriteAttributeString("index", obj[1]);
                    writer.WriteAttributeString("name", obj[2]);
                    writer.WriteAttributeString("datatype", obj[3].ToUpper());
                    writer.WriteAttributeString("visibility", obj[4].ToUpper());
                    writer.WriteAttributeString("perhour", obj[5]);
                    writer.WriteAttributeString("icon", obj[6]);
                    writer.WriteAttributeString("tooltip", obj[7]);
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        private static void WriteEffectRequirements()
        {
            writer.WriteStartElement("EffectRequirements");

            using (
                    CsvReader effectReqReader =
                            new CsvReader(
                                    new StreamReader(File.Open(csvDataFolder + "effect_requirement.csv",
                                                               FileMode.Open,
                                                               FileAccess.Read,
                                                               FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = effectReqReader.ReadRow();
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj[0] == string.Empty)
                    {
                        continue;
                    }

                    writer.WriteStartElement("EffectRequirement");

                    writer.WriteAttributeString("id", CleanCsvParam(obj[0]));
                    writer.WriteAttributeString("method", CleanCsvParam(obj[1]));
                    writer.WriteAttributeString("param1", CleanCsvParam(obj[2]));
                    writer.WriteAttributeString("param2", CleanCsvParam(obj[3]));
                    writer.WriteAttributeString("param3", CleanCsvParam(obj[4]));
                    writer.WriteAttributeString("param4", CleanCsvParam(obj[5]));
                    writer.WriteAttributeString("param5", CleanCsvParam(obj[6]));
                    writer.WriteAttributeString("description", CleanCsvParam(obj[7].Trim()));

                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        private static void WriteLanguageFile()
        {
            // TODO Hardcoded to just do English for now but later we can do all the languages            
            writer = new XmlTextWriter(new StreamWriter(FileExtension.OverwriteLockedFile(dataOutputFolder + "Game_en.xml")))
            {
                    Formatting = Formatting.Indented
            };

            writer.WriteStartElement("xliff");
            writer.WriteAttributeString("version", "1.0");
            writer.WriteAttributeString("xml:lang", "en");

            writer.WriteStartElement("file");

            writer.WriteAttributeString("datatype", "plaintext");
            writer.WriteAttributeString("source-language", "EN");

            writer.WriteStartElement("header");
            writer.WriteEndElement();

            writer.WriteStartElement("body");

            string[] files = Directory.GetFiles(langDataFolder, "lang.*.csv", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                String fullFilename = file;

                if (Global.IsRunningOnMono())
                {
                    fullFilename = Path.Combine(langDataFolder, file);
                }

                using (
                        CsvReader langReader =
                                new CsvReader(
                                        new StreamReader(File.Open((fullFilename),
                                                                   FileMode.Open,
                                                                   FileAccess.Read,
                                                                   FileShare.ReadWrite))))
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

                        if (obj[0].StartsWith("#"))
                        {
                            continue;
                        }

                        writer.WriteStartElement("trans-unit");
                        writer.WriteAttributeString("resname", obj[0]);
                        writer.WriteStartElement("source");
                        writer.WriteString(obj[1].Replace("\\n", "\n"));
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                }
            }

            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.Close();
        }
    }
}