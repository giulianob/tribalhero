using System;
using System.Collections.Generic;
using System.Text;
using Game.Util;
using System.IO;
using System.Reflection;
using Game.Logic;
using Game.Data;

namespace Game.Setup {
    public enum DataType : byte {
        Byte = 0,
        UShort = 1,
        UInt = 2,
        Int = 3,
        String = 10
    }

    public class Property {
        public string name;
        public DataType type;
        public PropertyOrigin origin;

        public Property(string name, DataType type, PropertyOrigin origin) {
            this.name = name;
            this.type = type;
            this.origin = origin;
        }

        public object getValue(Structure structure) {
            if (origin == PropertyOrigin.Formula) {
                MethodInfo method = typeof(Formula).GetMethod(name);
                return method.Invoke(null, new object[] { structure });
            }
            else if (origin == PropertyOrigin.System) {
                return Global.SystemVariables[name].Value;
            }
            else
                return structure[name];
        }
    }

    public enum PropertyOrigin {
        Structure,
        Formula,
        System
    }

    public class PropertyFactory {
        static Dictionary<int, List<Property>> dict;

        public static void init(string filename) {
            if (dict != null) return;
            dict = new Dictionary<int, List<Property>>();

            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                List<Property> properties;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i) {
                    col.Add(reader.Columns[i], i);
                }

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0) continue;
                    int index = int.Parse(toks[col["Type"]]);

                    if (!dict.TryGetValue(index, out properties)) {
                        properties = new List<Property>();
                        dict[index] = properties;
                    }
                    Property prop;
                    DataType type = (DataType)Enum.Parse(typeof(DataType), toks[col["DataType"]], true);

                    if (toks[col["Name"]].Contains("Formula.")) {
                        prop = new Property(toks[col["Name"]].Substring(toks[col["Name"]].LastIndexOf('.') + 1), type, PropertyOrigin.Formula);
                    }
                    else if (toks[col["Name"]].Contains("System.")) {
                        prop = new Property(toks[col["Name"]].Substring(toks[col["Name"]].LastIndexOf('.') + 1), type, PropertyOrigin.System);
                    }
                    else {
                        prop = new Property(toks[col["Name"]], type, PropertyOrigin.Structure);
                    }
                    
                    properties.Add(prop);
                }
            }
        }
        public static IEnumerable<Property> getProperties(int type) {
            if (dict == null) return null;
            List<Property> list;
            if (dict.TryGetValue(type, out list)) return list;
            return new List<Property>();
        }
        internal static void apply(ushort type, byte lvl, System.Collections.Specialized.ListDictionary listDictionary) {
            //    throw new Exception("The method or operation is not implemented.");
        }

    }
}
