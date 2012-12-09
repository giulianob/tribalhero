#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

#endregion

namespace Game.Util
{
    public class XmlKvPair
    {
        public string Key { get; set; }

        public string Value { get; set; }

        #region Constructors

        public XmlKvPair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public XmlKvPair(string key, bool value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, byte value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, char value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, DateTime value)
        {
            Key = key;
            Value = XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc);
        }

        public XmlKvPair(string key, decimal value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, double value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, float value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, Guid value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, int value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, long value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, sbyte value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, short value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, TimeSpan value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, uint value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, ulong value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        public XmlKvPair(string key, ushort value)
        {
            Key = key;
            Value = XmlConvert.ToString(value);
        }

        #endregion
    }

    public class XmlSerializer
    {
        public static String SerializeComplexList(IEnumerable<object[]> list)
        {
            var writerSettings = new XmlWriterSettings
            {
                    OmitXmlDeclaration = true,
                    Indent = false,
                    NewLineOnAttributes = false
            };

            using (var sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, writerSettings))
                {
                    writer.WriteStartElement("List");

                    foreach (var variables in list)
                    {
                        writer.WriteStartElement("Properties");

                        foreach (var variable in variables)
                        {
                            if (variable is byte)
                            {
                                writer.WriteStartElement("Byte");
                            }
                            else if (variable is short)
                            {
                                writer.WriteStartElement("Short");
                            }
                            else if (variable is int)
                            {
                                writer.WriteStartElement("Int");
                            }
                            else if (variable is ushort)
                            {
                                writer.WriteStartElement("UShort");
                            }
                            else if (variable is uint)
                            {
                                writer.WriteStartElement("UInt");
                            }
                            else if (variable is string)
                            {
                                writer.WriteStartElement("String");
                            }
                            else
                            {
                                throw new Exception("Unsupported variable type " + variable.GetType().Name);
                            }

                            writer.WriteAttributeString("value", variable.ToString());
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.Flush();

                    return sw.GetStringBuilder().ToString();
                }
            }
        }

        public static String SerializeList(params object[] variables)
        {
            var writerSettings = new XmlWriterSettings
            {
                    OmitXmlDeclaration = true,
                    Indent = false,
                    NewLineOnAttributes = false
            };

            using (var sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, writerSettings))
                {
                    writer.WriteStartElement("Properties");

                    foreach (var variable in variables)
                    {
                        if (variable is byte)
                        {
                            writer.WriteStartElement("Byte");
                        }
                        else if (variable is short)
                        {
                            writer.WriteStartElement("Short");
                        }
                        else if (variable is int)
                        {
                            writer.WriteStartElement("Int");
                        }
                        else if (variable is ushort)
                        {
                            writer.WriteStartElement("UShort");
                        }
                        else if (variable is uint)
                        {
                            writer.WriteStartElement("UInt");
                        }
                        else if (variable is string)
                        {
                            writer.WriteStartElement("String");
                        }
                        else
                        {
                            throw new Exception("Unsupported variable type " + variable.GetType().Name);
                        }

                        writer.WriteAttributeString("value", variable.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.Flush();

                    return sw.GetStringBuilder().ToString();
                }
            }
        }

        public static String Serialize(params XmlKvPair[] variables)
        {
            var writerSettings = new XmlWriterSettings
            {
                    OmitXmlDeclaration = true,
                    Indent = false,
                    NewLineOnAttributes = false
            };

            using (var sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, writerSettings))
                {
                    writer.WriteStartElement("Properties");

                    foreach (var variable in variables)
                    {
                        writer.WriteStartElement("Property");
                        writer.WriteAttributeString("name", variable.Key);
                        writer.WriteAttributeString("value", variable.Value);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.Flush();

                    return sw.GetStringBuilder().ToString();
                }
            }
        }

        public static List<object[]> DeserializeComplexList(String xml)
        {
            var ret = new List<object[]>();

            using (var stringReader = new StringReader(xml))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    List<object> properties = null;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch(reader.Name.ToLower())
                            {
                                case "list":
                                    continue;
                                case "properties":
                                    if (properties != null)
                                    {
                                        ret.Add(properties.ToArray());
                                    }
                                    properties = new List<object>();
                                    break;
                                case "string":
                                    properties.Add(reader["value"]);
                                    break;
                                case "short":
                                    properties.Add(short.Parse(reader["value"]));
                                    break;
                                case "int":
                                    properties.Add(int.Parse(reader["value"]));
                                    break;
                                case "ushort":
                                    properties.Add(ushort.Parse(reader["value"]));
                                    break;
                                case "uint":
                                    properties.Add(uint.Parse(reader["value"]));
                                    break;
                                case "byte":
                                    properties.Add(byte.Parse(reader["value"]));
                                    break;
                                default:
                                    throw new Exception("Unsupported variable type");
                            }
                        }
                    }

                    if (properties != null)
                    {
                        ret.Add(properties.ToArray());
                    }

                    return ret;
                }
            }
        }

        public static List<object> DeserializeList(String xml)
        {
            var ret = new List<object>();

            using (var stringReader = new StringReader(xml))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch(reader.Name)
                            {
                                case "Properties":
                                    continue;
                                case "String":
                                    ret.Add(reader["value"]);
                                    break;
                                case "Short":
                                    ret.Add(short.Parse(reader["value"]));
                                    break;
                                case "Int":
                                    ret.Add(int.Parse(reader["value"]));
                                    break;
                                case "UShort":
                                    ret.Add(ushort.Parse(reader["value"]));
                                    break;
                                case "UInt":
                                    ret.Add(uint.Parse(reader["value"]));
                                    break;
                                case "Byte":
                                    ret.Add(byte.Parse(reader["value"]));
                                    break;
                                default:
                                    throw new Exception("Unsupported variable type");
                            }
                        }
                    }

                    return ret;
                }
            }
        }

        public static Dictionary<String, String> Deserialize(String xml)
        {
            var ret = new Dictionary<string, string>();

            using (var stringReader = new StringReader(xml))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    while (reader.Read())
                    {
                        string key = string.Empty;
                        string value = string.Empty;
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Property")
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                switch(reader.Name)
                                {
                                    case "name":
                                        key = reader.Value;
                                        break;
                                    case "value":
                                        value = reader.Value;
                                        break;
                                }
                            }
                            ret[key] = value;
                        }
                    }

                    return ret;
                }
            }
        }
    }
}