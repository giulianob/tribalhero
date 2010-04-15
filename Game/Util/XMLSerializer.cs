#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

#endregion

namespace Game.Util {
    public class XMLKVPair {
        private string key;

        public string Key {
            get { return key; }
            set { key = value; }
        }

        private string value;

        public string Value {
            get { return value; }
            set { this.value = value; }
        }

        #region Constructors

        public XMLKVPair(string key, string value) {
            this.key = key;
            this.value = value;
        }

        public XMLKVPair(string key, bool value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, byte value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, char value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, DateTime value) {
            this.key = key;
            this.value = XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc);
        }

        public XMLKVPair(string key, decimal value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, double value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, float value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, Guid value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, int value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, long value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, sbyte value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, short value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, TimeSpan value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, uint value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, ulong value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        public XMLKVPair(string key, ushort value) {
            this.key = key;
            this.value = XmlConvert.ToString(value);
        }

        #endregion
    }

    public class XMLSerializer {
        public static String SerializeList(params object[] variables) {
            XmlWriterSettings writerSettings = new XmlWriterSettings {
                                                                         OmitXmlDeclaration = true,
                                                                         Indent = false,
                                                                         NewLineOnAttributes = false
                                                                     };

            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, writerSettings);

            writer.WriteStartElement("Properties");

            foreach (object variable in variables) {
                if (variable is byte)
                    writer.WriteStartElement("Byte");
                else if (variable is short)
                    writer.WriteStartElement("Short");
                else if (variable is int)
                    writer.WriteStartElement("Int");
                else if (variable is ushort)
                    writer.WriteStartElement("Ushort");
                else if (variable is uint)
                    writer.WriteStartElement("Uint");
                else if (variable is string)
                    writer.WriteStartElement("String");
                else
                    throw new Exception("Unsupported variable type " + variable.GetType().Name);

                writer.WriteAttributeString("value", variable.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Flush();

            return sw.GetStringBuilder().ToString();
        }

        public static String Serialize(params XMLKVPair[] variables) {
            XmlWriterSettings writerSettings = new XmlWriterSettings {
                                                                         OmitXmlDeclaration = true,
                                                                         Indent = false,
                                                                         NewLineOnAttributes = false
                                                                     };

            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, writerSettings);

            writer.WriteStartElement("Properties");

            foreach (XMLKVPair variable in variables) {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("name", variable.Key);
                writer.WriteAttributeString("value", variable.Value);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Flush();

            return sw.GetStringBuilder().ToString();
        }

        public static List<object> DeserializeList(String xml) {
            List<object> ret = new List<object>();

            StringReader stringReader = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stringReader);

            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element) {
                    switch (reader.Name) {
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
                        case "Ushort":
                            ret.Add(ushort.Parse(reader["value"]));
                            break;
                        case "Uint":
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

        public static Dictionary<String, String> Deserialize(String xml) {
            Dictionary<String, String> ret = new Dictionary<string, string>();

            StringReader stringReader = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stringReader);

            while (reader.Read()) {
                string key = string.Empty;
                string value = string.Empty;
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Property") {
                    while (reader.MoveToNextAttribute()) {
                        switch (reader.Name) {
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