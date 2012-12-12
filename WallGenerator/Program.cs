/*
 * Generates the wall arrays used in the client's wall manager file
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.XPath;

namespace WallGenerator
{
    class Program
    {
        private static int wall_width = 11;

        private static int wall_height = 24;

        private static readonly Dictionary<ushort, string> walls = new Dictionary<ushort, string>
        {
                {256, "O1"},
                {257, "O2"},
                {258, "E"},
                {259, "N"},
                {260, "NE"},
                {261, "SW"},
                {262, "NW"},
                {263, "SE"},
                {264, "NW"},
                {265, "S"},
                {266, "W"},
                {267, "O3"},
                {268, "O4"},
        };

        private static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter(File.Create("output.txt"));

            output.WriteLine("[");

            foreach (string file in Directory.GetFiles("map", "wall*", SearchOption.TopDirectoryOnly))
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

                    XPathNodeIterator nodeIter = nav.Select("/map/layer/data");

                    ushort[] tiles = new ushort[wall_height * wall_width];

                    //load tile info
                    while (nodeIter.MoveNext())
                    {
                        byte[] zippedMap = Convert.FromBase64String(nodeIter.Current.Value);
                        using (GZipStream gzip = new GZipStream(new MemoryStream(zippedMap), CompressionMode.Decompress)
                                )
                        {
                            byte[] tmpMap = new byte[wall_width * wall_height * sizeof(int)];
                            gzip.Read(tmpMap, 0, wall_width * wall_height * sizeof(int));

                            int cnt = 0;
                            for (int i = 0; i < tmpMap.Length; i += sizeof(int))
                            {
                                ushort tileId = (ushort)(BitConverter.ToInt32(tmpMap, i) - 1);
                                tiles[cnt++] = tileId;
                            }
                        }

                        break; //not supporting multi layer
                    }

                    output.WriteLine("\t[");
                    //scan and generate tile list
                    for (uint y = 0; y < wall_height; ++y)
                    {
                        output.Write("\t\t[");
                        for (uint x = 0; x < wall_width; ++x)
                        {
                            ushort tileId = tiles[y * wall_width + x];

                            string wallName;
                            if (walls.TryGetValue(tileId, out wallName))
                            {
                                output.Write("\"" + walls[tileId] + "\", ");
                            }
                            else
                            {
                                output.Write("\"\",   ");
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