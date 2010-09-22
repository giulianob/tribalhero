#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Comm;
using Game.Data;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Setup {
    public enum DataType : byte {
        BYTE = 0,
        USHORT = 1,
        UINT = 2,
        INT = 3,
        FLOAT = 4,
        STRING = 10
    }

    public enum Visibility : byte {
        PRIVATE = 0,
        PUBLIC = 1,
    }

    public class Property {
        public string Name { get; private set; }
        public DataType Type { get; private set; }
        public PropertyOrigin Origin { get; private set; }
        public Visibility Visibility { get; private set; }

        public Property(string name, DataType type, PropertyOrigin origin, Visibility visibility) {
            Name = name;
            Type = type;
            Origin = origin;
            Visibility = visibility;
        }

        public object GetValue(Structure structure) {
            if (!structure.Properties.Contains(Name))
                return GetDefaultValue();

            if (Origin == PropertyOrigin.FORMULA) {
                MethodInfo method = typeof (Formula).GetMethod(Name);
                return method.Invoke(null, new object[] {structure});
            }

            if (Origin == PropertyOrigin.SYSTEM)
                return Global.SystemVariables[Name].Value;
            
            return structure[Name];
        }

        public object GetDefaultValue() {
            switch (Type) {
                case DataType.BYTE:
                    return Byte.MaxValue;
                case DataType.USHORT:
                    return UInt16.MaxValue;
                case DataType.UINT:
                    return UInt32.MaxValue;
                case DataType.STRING:
                    return "N/A";
                case DataType.INT:
                    return Int16.MaxValue;
                case DataType.FLOAT:
                    return float.MaxValue;
            }

            return null;
        }
    }

    public enum PropertyOrigin {
        STRUCTURE,
        FORMULA,
        SYSTEM
    }

    public class PropertyFactory {
        private static Dictionary<int, List<Property>> dict;

        public static void Init(string filename) {
            if (dict != null)
                return;

            dict = new Dictionary<int, List<Property>>();

            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                List<Property> properties;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0)
                        continue;

                    int index = int.Parse(toks[col["Type"]]);

                    if (!dict.TryGetValue(index, out properties)) {
                        properties = new List<Property>();
                        dict[index] = properties;
                    }

                    Property prop;
                    DataType type = (DataType)Enum.Parse(typeof(DataType), toks[col["DataType"]], true);
                    Visibility visibility = (Visibility)Enum.Parse(typeof(Visibility), toks[col["Visibility"]], true);

                    if (toks[col["Name"]].Contains("Formula.")) {
                        prop = new Property(toks[col["Name"]].Substring(toks[col["Name"]].LastIndexOf('.') + 1), type, PropertyOrigin.FORMULA, visibility);
                    }
                    else if (toks[col["Name"]].Contains("System.")) {
                        prop = new Property(toks[col["Name"]].Substring(toks[col["Name"]].LastIndexOf('.') + 1), type, PropertyOrigin.SYSTEM, visibility);
                    }
                    else {
                        prop = new Property(toks[col["Name"]], type, PropertyOrigin.STRUCTURE, visibility);
                    }

                    properties.Add(prop);
                }
            }
        }

        public static IEnumerable<Property> GetProperties(int type) {
            if (dict == null)
                return null;

            List<Property> list;
            return !dict.TryGetValue(type, out list) ? new List<Property>() : list;
        }

        public static IEnumerable<Property> GetProperties(int type, Visibility visibility) {
            List<Property> list;
            if (dict == null || !dict.TryGetValue(type, out list)) 
                return new List<Property>();

            return list.Where(prop => prop.Visibility == visibility);            
        }
    }
}