using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using Game.Data;
using Game.Setup;

namespace CSVToXML
{
    public class Program
    {
        static string directory = Config.csv_compiled_folder;
        static string data_output_folder = Config.data_folder;

        #region WriteToConstant
        static void write_to_constant(string constant_as) {
            FileStream xmlfile = new FileStream(directory + "data.xml", FileMode.Open, FileAccess.Read);
            StreamReader xml_stream = new StreamReader(xmlfile);
            string new_xml = xml_stream.ReadToEnd();
            xml_stream.Close();
            xmlfile.Close();
            
            try
            {
                File.Copy(constant_as, constant_as + ".bak", true);
            }
            catch (Exception) { }

            FileStream file = new FileStream(constant_as, FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(file);
            string s = sr.ReadToEnd();
            sr.Close();
            file.Close();

            int begin  = s.IndexOf("<Data>");
            int end = s.LastIndexOf("</Data>")+ 7;
            string trimmed = s.Remove(begin, end - begin);

            Console.Out.WriteLine(s.Length);
            string final = trimmed.Insert(begin, new_xml);
            Console.Out.WriteLine(s.Length);

            FileStream new_file = new FileStream(constant_as, FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(new_file);
            sw.Write(final);
            sw.Close();
            new_file.Close();
        }
        #endregion

        #region Property
        class Property
        {
            public string type;
            public string name;
            public string datatype;
        }

        private static List<Property> findAllProperty(List<Property> properties, int type)
        {
            List<Property> ret = new List<Property>();
            foreach (Property prop in properties)
            {
                if (Int32.Parse(prop.type) == type)
                    ret.Add(prop);
            }

            return ret;
        }
        #endregion

        #region Layout
        class Layout
        {
            public string layout;
            public string type;
            public string level;
            public string reqtype;
            public string minlevel;
            public string maxlevel;
            public string mindist;
            public string maxdist;
            public string compare;
        }

        private static List<Layout> findAllLayout(List<Layout> layouts, int type, int level)
        {
            List<Layout> ret = new List<Layout>();
            foreach (Layout layout in layouts)
            {
                if (Int32.Parse(layout.type) == type && Int32.Parse(layout.level) == level)
                    ret.Add(layout);
            }

            return ret;
        }
        #endregion

        #region Worker Actions
        class WorkerActions : IComparable
        {
            public string type;
            public string action;
            public string param1,
                          param2,
                          param3,
                          param4,
                          param5;
            public string max;
            public string index;
            public string effectReq;
            public string effectReqInherit;

            public int CompareTo(object obj)
            {
                if (obj is WorkerActions)
                {
                    WorkerActions obj2 = obj as WorkerActions;

                    int typeDelta = Int32.Parse(type) - Int32.Parse(obj2.type);
                    if (typeDelta != 0)
                        return typeDelta;

                    int indexDelta = Int32.Parse(index) - Int32.Parse(obj2.index);
                    return indexDelta;
                }
                else
                {
                    return 1;
                }
            }
        }
        #endregion

        #region Technology Effects
        class TechnologyEffects
        {
            public string techid;
            public string lvl;
            public string effect;
            public string location;
            public string isprivate;
            public string param1,
                          param2,
                          param3,
                          param4,
                          param5;
        }

        private static List<TechnologyEffects> findAllTechEffects(List<TechnologyEffects> techEffects, int techid, int lvl)
        {
            List<TechnologyEffects> ret = new List<TechnologyEffects>();
            foreach (TechnologyEffects prop in techEffects)
            {
                if (Int32.Parse(prop.techid) == techid && Int32.Parse(prop.lvl) == lvl)
                    ret.Add(prop);
            }

            return ret;
        }
        #endregion

        public static string CleanCSVParam(string param)
        {
            char[] args = new char[] { '=' };
            string[] split = param.Split(args);
            
            return split[split.Length - 1].Trim();
        }

        public static void Main(string[] args)
        {
            XmlTextWriter writer = new XmlTextWriter(new StreamWriter(File.Open(data_output_folder + "data.xml", FileMode.Create)));
            writer.Formatting = Formatting.Indented;

            List<Property> properties = new List<Property>();
            using (CSVReader propReader = new CSVReader(new StreamReader(File.Open(directory + "property.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                while (true)
                {
                   string[] obj = propReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0].Length <= 0) continue;
                        Property prop = new Property();
                        prop.type = obj[0];
                        prop.name = obj[1];
                        prop.datatype = obj[2];

                        properties.Add(prop);
                    }
                    else
                        break;
                }
            }


            List<TechnologyEffects> techEffects = new List<TechnologyEffects>();

            using (CSVReader techEffectsReader = new CSVReader(new StreamReader(File.Open(directory + "technology_effects.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {                
                while (true)
                {
                    string[] obj = techEffectsReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0].Length <= 0) continue;

                        TechnologyEffects techEffect = new TechnologyEffects();
                        techEffect.techid = obj[0];
                        techEffect.lvl = obj[1];
                        techEffect.effect = ((int)((EffectCode) Enum.Parse(typeof(EffectCode), obj[2], true))).ToString();
                        techEffect.location = ((int)((EffectLocation)Enum.Parse(typeof(EffectLocation), obj[3], true))).ToString();
                        techEffect.isprivate = obj[4];
                        techEffect.param1 = obj[5];
                        techEffect.param2 = obj[6];
                        techEffect.param3 = obj[7];
                        techEffect.param4 = obj[8];
                        techEffect.param5 = obj[9];
                        
                        techEffects.Add(techEffect);
                    }
                    else
                        break;
                }
            }

            List<Layout> layouts = new List<Layout>();            
            
            using (CSVReader layoutReader = new CSVReader(new StreamReader(File.Open(directory+"layout.csv", FileMode.Open,FileAccess.Read,FileShare.ReadWrite))))
            {
                while (true)
                {                
                    string[] obj = layoutReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0].Length <= 0) continue;
                        Layout layout = new Layout();
                        layout.type = obj[0];
                        layout.level = obj[1];
                        layout.layout = obj[2];
                        layout.reqtype = obj[3];
                        layout.minlevel = obj[4];
                        layout.maxlevel = obj[5];
                        layout.mindist = obj[6];
                        layout.maxdist = obj[7];
                        layout.compare = obj[8];

                        layouts.Add(layout);                                                 
                    }
                    else
                        break;
                }
            }

            writer.WriteStartElement("Data");

            writer.WriteStartElement("Structures");
            using (CSVReader statsReader = new CSVReader(new StreamReader(File.Open(directory+"structure.csv", FileMode.Open,FileAccess.Read,FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = statsReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == "")
                            continue;

                        writer.WriteStartElement("Structure");
                        //Name,Type,Lvl,Class,SpriteClass,Hp,Atk,Matk,Def,Vsn,Stl,Rng,Spd,Crop,Gold,Iron,Wood,Labor,Time,WorkerId
                        writer.WriteAttributeString("name", obj[0]);
                        writer.WriteAttributeString("type", obj[1]);
                        writer.WriteAttributeString("level", obj[2]);
                        writer.WriteAttributeString("class", obj[3]);
                        writer.WriteAttributeString("spriteclass", obj[4]);
                        writer.WriteAttributeString("hp", obj[5]);
                        writer.WriteAttributeString("attack", obj[6]);
                        writer.WriteAttributeString("maxattack", obj[7]);
                        writer.WriteAttributeString("defense", obj[8]);
                        writer.WriteAttributeString("vision", obj[9]);
                        writer.WriteAttributeString("stealth", obj[10]);
                        writer.WriteAttributeString("range", obj[11]);
                        writer.WriteAttributeString("speed", obj[12]);
                        writer.WriteAttributeString("gold", obj[13]);
                        writer.WriteAttributeString("crop", obj[14]);
                        writer.WriteAttributeString("wood", obj[15]);
                        writer.WriteAttributeString("labor", obj[16]);
                        writer.WriteAttributeString("iron", obj[17]);
                        writer.WriteAttributeString("time", obj[18]);
                        writer.WriteAttributeString("workerid", obj[19]);
                        writer.WriteAttributeString("weapon", obj[20]);
                        writer.WriteAttributeString("maxlabor", obj[21]);

                        writer.WriteStartElement("Layout");

                        List<Layout> objLayouts = findAllLayout(layouts, Int32.Parse(obj[1]), Int32.Parse(obj[2]));
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
                        break;
                }
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Units");
            using (CSVReader statsReader = new CSVReader(new StreamReader(File.Open(directory + "unit.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = statsReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == "")
                            continue;

                        writer.WriteStartElement("Unit");
                        //Name,Type,Lvl,Class,SpriteClass,Hp,Atk,Matk,Def,Vsn,Stl,Rng,Spd,Crop,Gold,Iron,Wood,Labor,Time,UpgrdCrop,UpgrdGold,UpgrdIron,UpgrdWood,UpgrdLabor,UpgrdTime
                        writer.WriteAttributeString("name", obj[0]);
                        writer.WriteAttributeString("type", obj[1]);
                        writer.WriteAttributeString("level", obj[2]);
                        writer.WriteAttributeString("class", obj[5]);
                        writer.WriteAttributeString("spriteclass", obj[6]);
                        writer.WriteAttributeString("hp", obj[7]);
                        writer.WriteAttributeString("attack", obj[8]);
                        writer.WriteAttributeString("maxattack", obj[9]);
                        writer.WriteAttributeString("defense", obj[10]);
                        writer.WriteAttributeString("vision", obj[11]);
                        writer.WriteAttributeString("stealth", obj[12]);
                        writer.WriteAttributeString("range", obj[13]);
                        writer.WriteAttributeString("speed", obj[14]);
                        writer.WriteAttributeString("crop", obj[15]);
                        writer.WriteAttributeString("gold", obj[16]);
                        writer.WriteAttributeString("iron", obj[17]);
                        writer.WriteAttributeString("wood", obj[18]);
                        writer.WriteAttributeString("labor", obj[19]);
                        writer.WriteAttributeString("time", obj[20]);
                        writer.WriteAttributeString("upgradecrop", obj[21]);
                        writer.WriteAttributeString("upgradegold", obj[22]);
                        writer.WriteAttributeString("upgradeiron", obj[23]);
                        writer.WriteAttributeString("upgradewood", obj[24]);
                        writer.WriteAttributeString("upgradelabor", obj[25]);
                        writer.WriteAttributeString("upgradetime", obj[26]);

                        writer.WriteEndElement();
                    }
                    else
                        break;
                }
            }
            writer.WriteEndElement();


            writer.WriteStartElement("Workers");
            
            List<WorkerActions> workerActions = new List<WorkerActions>();

            using (CSVReader actionReader = new CSVReader(new StreamReader(File.Open(directory+"action.csv", FileMode.Open,FileAccess.Read,FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = actionReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == "")
                            continue;

                        WorkerActions workerAction = new WorkerActions();
                        workerAction.type = obj[0];
                        workerAction.index = obj[1];
                        workerAction.max = obj[2];
                        workerAction.action = obj[3];
                        workerAction.param1 = CleanCSVParam(obj[4]);
                        workerAction.param2 = CleanCSVParam(obj[5]);
                        workerAction.param3 = CleanCSVParam(obj[6]);
                        workerAction.param4 = CleanCSVParam(obj[7]);
                        workerAction.param5 = CleanCSVParam(obj[8]);
                        workerAction.effectReq = CleanCSVParam(obj[9]);
                        workerAction.effectReqInherit = CleanCSVParam(obj[10]).ToUpper();

                        workerActions.Add(workerAction);
                    }
                    else
                        break;
                }
            }

            workerActions.Sort();

            #region write worker XML
            int lastId = Int32.MinValue;
            writer.WriteStartElement("Worker");
            writer.WriteAttributeString("type", "0");
            writer.WriteAttributeString("max", "0");
            writer.WriteEndElement();
            foreach (WorkerActions workerAction in workerActions)
            {
                if (lastId != Int32.Parse(workerAction.type))
                {
                    if (lastId != Int32.MinValue)
                        writer.WriteEndElement();

                    writer.WriteStartElement("Worker");
                    writer.WriteAttributeString("type", workerAction.type);
                    writer.WriteAttributeString("max", workerAction.max);

                    if (Int32.Parse(workerAction.index) != 0)
                    {
                        throw new Exception("Error in actions.csv. Worker index 0 is not the first item in worker: " + workerAction.type);
                    }

                    lastId = Int32.Parse(workerAction.type);
                    continue;
                }

                switch (workerAction.action.ToLower())
                {
                    case "structure_build":
                        writer.WriteStartElement("StructureBuild");
                        writer.WriteAttributeString("type", workerAction.param1);
                        break;
                    case "unit_train":
                        writer.WriteStartElement("TrainUnit");
                        writer.WriteAttributeString("type", workerAction.param1);
                        break;
                    case "structure_change":
                        writer.WriteStartElement("StructureChange");
                        writer.WriteAttributeString("type", workerAction.param1);
                        writer.WriteAttributeString("level", workerAction.param2);
                        break;
                    case "structure_upgrade":
                        writer.WriteStartElement("StructureUpgrade");
                        writer.WriteAttributeString("type", workerAction.param1);                             
                        break;
                    case "unit_upgrade":
                        writer.WriteStartElement("UnitUpgrade");
                        writer.WriteAttributeString("type", workerAction.param1);
                        writer.WriteAttributeString("maxlevel", workerAction.param2);
                        break;
                    case "technology_upgrade":
                        writer.WriteStartElement("TechnologyUpgrade");
                        writer.WriteAttributeString("type", workerAction.param1);
                        writer.WriteAttributeString("maxlevel", workerAction.param2);
                        break;
                    case "resource_sell":
                        writer.WriteStartElement("ResourceSell");
                        break;
                    case "resource_buy":
                        writer.WriteStartElement("ResourceBuy");
                        break;
                    case "labor_move":
                        writer.WriteStartElement("LaborMove");
                        break;
                    default:
                        writer.WriteStartElement("MISSING_WORKER_ACTION");
                        writer.WriteAttributeString("name", workerAction.action);
                        writer.WriteAttributeString("param1", workerAction.param1);
                        writer.WriteAttributeString("param2", workerAction.param2);
                        writer.WriteAttributeString("param3", workerAction.param3);
                        writer.WriteAttributeString("param4", workerAction.param4);
                        writer.WriteAttributeString("param5", workerAction.param5);
                        break;
                }

                writer.WriteAttributeString("index", workerAction.index);
                writer.WriteAttributeString("max", workerAction.max);
                if (workerAction.effectReq != "")
                {
                    writer.WriteAttributeString("effectreq", workerAction.effectReq);
                    writer.WriteAttributeString("effectreqinherit", workerAction.effectReqInherit);
                }
                writer.WriteEndElement();
            }

            if (lastId != Int32.MinValue)
                writer.WriteEndElement();
            #endregion

            writer.WriteEndElement();

            #region Technologies

            writer.WriteStartElement("Technologies");

            using (CSVReader techReader = new CSVReader(new StreamReader(File.Open(directory + "technology.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = techReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == string.Empty)
                            continue;

                        writer.WriteStartElement("Tech");
                        
                        writer.WriteAttributeString("techtype", obj[0]);
                        writer.WriteAttributeString("spriteclass", obj[1]);
                        writer.WriteAttributeString("description", obj[2]);
                        writer.WriteAttributeString("level", obj[3]);
                        writer.WriteAttributeString("crop", obj[4]);
                        writer.WriteAttributeString("gold", obj[5]);
                        writer.WriteAttributeString("iron", obj[6]);
                        writer.WriteAttributeString("wood", obj[7]);
                        writer.WriteAttributeString("labor", obj[8]);
                        writer.WriteAttributeString("time", obj[9]);

                        List<TechnologyEffects> effects = findAllTechEffects(techEffects, Int32.Parse(obj[0]), Int32.Parse(obj[3]));
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
                    else
                        break;
                }
            }

            writer.WriteEndElement();
            #endregion

            #region Properties
            writer.WriteStartElement("Property");
            
            foreach (Property prop in properties)
            {
                writer.WriteStartElement("Prop");
                writer.WriteAttributeString("type", prop.type);
                writer.WriteAttributeString("datatype", prop.datatype.ToUpper());
                writer.WriteAttributeString("name", prop.name);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            #endregion

            #region Effect Requirement

            writer.WriteStartElement("EffectRequirements");

            using (CSVReader effectReqReader = new CSVReader(new StreamReader(File.Open(directory + "effect_requirement.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                while (true)
                {
                    string[] obj = effectReqReader.ReadRow();
                    if (obj != null)
                    {
                        if (obj[0] == string.Empty)
                            continue;

                        writer.WriteStartElement("EffectRequirement");

                        writer.WriteAttributeString("id", CleanCSVParam(obj[0]));
                        writer.WriteAttributeString("method", CleanCSVParam(obj[1]));
                        writer.WriteAttributeString("param1", CleanCSVParam(obj[2]));
                        writer.WriteAttributeString("param2", CleanCSVParam(obj[3]));
                        writer.WriteAttributeString("param3", CleanCSVParam(obj[4]));
                        writer.WriteAttributeString("param4", CleanCSVParam(obj[5]));
                        writer.WriteAttributeString("param5", CleanCSVParam(obj[6]));

                        writer.WriteEndElement();
                    }
                    else
                        break;
                }
            }

            writer.WriteEndElement();
            #endregion

            writer.WriteEndElement();

            writer.Close();

            //write_to_constant();
        }
    }
}
