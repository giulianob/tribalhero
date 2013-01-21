using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Extensions;

namespace Common.Testing
{
    public class CsvDataAttribute : DataAttribute
    {
        private bool hasHeader = true;

        public CsvDataAttribute(string filename, bool hasHeader = true)
        {
            HasHeader = hasHeader;
            Filename = filename;
        }

        public bool HasHeader
        {
            get
            {
                return hasHeader;
            }
            set
            {
                hasHeader = value;
            }
        }

        public string Filename { get; set; }

        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            using (
                    var csvReader =
                            new CsvReader(new StreamReader(new FileStream(Filename, FileMode.Open, FileAccess.Read)),
                                          hasHeader))
            {
                string[] toks;
                while ((toks = csvReader.ReadRow()) != null)
                {
                    yield return ConvertParameters(toks, parameterTypes);
                }
            }
        }

        private object[] ConvertParameters(IList<string> values, IList<Type> parameterTypes)
        {
            object[] result = new object[values.Count];

            for (int idx = 0; idx < values.Count; idx++)
            {
                result[idx] = ConvertParameter(values[idx], idx >= parameterTypes.Count ? null : parameterTypes[idx]);
            }

            return result;
        }

        protected virtual object ConvertParameter(string parameter, Type parameterType)
        {
            switch(parameterType.Name)
            {
                case "Int32":
                    return int.Parse(parameter);
                case "UInt32":
                    return uint.Parse(parameter);
                case "Double":
                    return double.Parse(parameter);
                case "Float":
                    return float.Parse(parameter);
                case "String":
                    return parameter;
                case "Int16":
                    return short.Parse(parameter);
                case "UInt16":
                    return ushort.Parse(parameter);
                case "Boolean":
                    return bool.Parse(parameter);
                case "Byte":
                    return byte.Parse(parameter);
                default:
                    throw new Exception(string.Format("Unable to convert to type {0}", parameterType.Name));
            }
        }
    }
}