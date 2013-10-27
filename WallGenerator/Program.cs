/*
 * Generates the wall arrays used in the client's wall manager file
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.XPath;

namespace WallGenerator
{
    class Program
    {
        private static int wall_width = 15;

        private static int wall_height = 29;
        
        private static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter(File.Create("output.txt"));

            output.WriteLine("[");

            foreach (string file in Directory.GetFiles("map", "*.tmx", SearchOption.TopDirectoryOnly))
            {                
                XmlReaderSettings settings = new XmlReaderSettings
                {
                        DtdProcessing = DtdProcessing.Ignore,
                        XmlResolver = null
                };

                using (XmlReader str = XmlReader.Create(File.OpenRead(file), settings))
                {
                    XPathDocument reader = new XPathDocument(str);
                    XPathNavigator nav = reader.CreateNavigator();

                    // Fetch the first tile id of the walls tileset                    
                    XPathNavigator tileIdIter = nav.SelectSingleNode("/map//tileset[2]");                    
                    var firstGid = ushort.Parse(tileIdIter.GetAttribute("firstgid", string.Empty));                    

                    // Fetch the 2nd layer which is the "Walls" layer
                    XPathNavigator nodeIter = nav.SelectSingleNode("/map/layer[2]/data");

                    ushort[] tiles = new ushort[wall_height * wall_width];

                    //load tile info
                    byte[] zippedMap = Convert.FromBase64String(nodeIter.Value);
                    using (GZipStream gzip = new GZipStream(new MemoryStream(zippedMap), CompressionMode.Decompress))
                    {
                        byte[] tmpMap = new byte[wall_width * wall_height * sizeof(int)];
                        gzip.Read(tmpMap, 0, wall_width * wall_height * sizeof(int));

                        int cnt = 0;
                        for (int i = 0; i < tmpMap.Length; i += sizeof(int))
                        {
                            ushort tileId = (ushort)(BitConverter.ToInt32(tmpMap, i) - 1);
                            if (tileId == ushort.MaxValue)
                            {
                                tiles[cnt++] = ushort.MaxValue;
                            }
                            else
                            {
                                tiles[cnt++] = (ushort)(tileId - firstGid + 1);
                            }
                        }
                    }


                    output.WriteLine("\t[");
                    //scan and generate tile list
                    for (uint y = 0; y < wall_height; ++y)
                    {
                        output.Write("\t\t[");
                        for (uint x = 0; x < wall_width; ++x)
                        {
                            ushort tileId = tiles[y * wall_width + x];
                            // Skip empty tiles
                            if (tileId == ushort.MaxValue)
                            {
                                output.Write("\"\", \t");
                            }
                            else
                            {
                                output.Write("\"" + tileId + "\", \t");
                            }
                        }
                        output.WriteLine("],");
                    }

                    output.WriteLine("],");
                }
            }

            output.WriteLine("]");

            output.Close();
        }
    }
}