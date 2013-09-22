#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common;
using Game.Data;
using Game.Logic.Formulas;

#endregion

namespace Game.Setup
{
    public enum DataType : byte
    {
        Byte = 0,

        UShort = 1,

        UInt = 2,

        Int = 3,

        Float = 4,

        String = 10
    }

    public enum Visibility : byte
    {
        Private = 0,

        Public = 1,
    }

    public class Property
    {
        private readonly ISystemVariableManager systemVariableManager;

        public Property(string name, DataType type, PropertyOrigin origin, Visibility visibility, ISystemVariableManager systemVariableManager)
        {
            this.systemVariableManager = systemVariableManager;
            Name = name;
            Type = type;
            Origin = origin;
            Visibility = visibility;
        }

        public string Name { get; private set; }

        public DataType Type { get; private set; }

        public PropertyOrigin Origin { get; private set; }

        public Visibility Visibility { get; private set; }

        public object GetValue(IStructure structure)
        {
            if (!structure.Properties.Contains(Name))
            {
                return GetDefaultValue();
            }

            if (Origin == PropertyOrigin.Formula)
            {
                MethodInfo method = typeof(Formula).GetMethod(Name);
                return method.Invoke(null, new object[] {structure});
            }

            if (Origin == PropertyOrigin.System)
            {
                return systemVariableManager[Name].Value;
            }

            return structure[Name];
        }

        public object GetDefaultValue()
        {
            switch(Type)
            {
                case DataType.Byte:
                    return Byte.MaxValue;
                case DataType.UShort:
                    return UInt16.MaxValue;
                case DataType.UInt:
                    return UInt32.MaxValue;
                case DataType.String:
                    return "N/A";
                case DataType.Int:
                    return Int16.MaxValue;
                case DataType.Float:
                    return float.MaxValue;
            }

            return null;
        }
    }

    public enum PropertyOrigin
    {
        Structure,

        Formula,

        System
    }

    public class PropertyFactory
    {
        private readonly ISystemVariableManager systemVariableManager;
        private readonly Dictionary<int, List<Property>> dict = new Dictionary<int, List<Property>>();
        public PropertyFactory(ISystemVariableManager systemVariableManager)
        {
            this.systemVariableManager = systemVariableManager;
        }

        public void Init(string filename)
        {
            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                List<Property> properties;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                {
                    col.Add(reader.Columns[i], i);
                }

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                    {
                        continue;
                    }

                    int index = int.Parse(toks[col["Type"]]);

                    if (!dict.TryGetValue(index, out properties))
                    {
                        properties = new List<Property>();
                        dict[index] = properties;
                    }

                    Property prop;
                    var type = (DataType)Enum.Parse(typeof(DataType), toks[col["DataType"]], true);
                    var visibility = (Visibility)Enum.Parse(typeof(Visibility), toks[col["Visibility"]], true);

                    if (toks[col["Name"]].Contains("Formula."))
                    {
                        prop = new Property(toks[col["Name"]].Substring(toks[col["Name"]].LastIndexOf('.') + 1),
                                            type,
                                            PropertyOrigin.Formula,
                                            visibility,
                                            systemVariableManager);
                    }
                    else if (toks[col["Name"]].Contains("System."))
                    {
                        prop = new Property(toks[col["Name"]].Substring(toks[col["Name"]].LastIndexOf('.') + 1),
                                            type,
                                            PropertyOrigin.System,
                                            visibility,
                                            systemVariableManager);
                    }
                    else
                    {
                        prop = new Property(toks[col["Name"]], type, PropertyOrigin.Structure, visibility, systemVariableManager);
                    }

                    properties.Add(prop);
                }
            }
        }

        public IEnumerable<Property> GetProperties(int type)
        {
            if (dict == null)
            {
                return null;
            }

            List<Property> list;
            return !dict.TryGetValue(type, out list) ? new List<Property>() : list;
        }

        public IEnumerable<Property> GetProperties(int type, Visibility visibility)
        {
            List<Property> list;
            if (dict == null || !dict.TryGetValue(type, out list))
            {
                return new List<Property>();
            }

            return list.Where(prop => prop.Visibility == visibility);
        }
    }
}