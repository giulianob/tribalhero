#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.XPath;
using Game;
using Game.Setup;
using NDesk.Options;
using Ninject;

#endregion

namespace MapGenerator
{
    static class Program
    {
        public const ushort CITY_TILE = 209;

        public const int WIDTH = 3400;

        public const int HEIGHT = 6200;

        public const int REGION_WIDTH = 34; // this region is not the same as the region in the game,

        public const int REGION_HEIGHT = 62;
                         // but they have same values because it guarantees cities will not be on the edge of the region.

        private const int REGION_COLUMN = WIDTH / REGION_WIDTH;

        private const int REGION_ROW = HEIGHT / REGION_HEIGHT;

        public static readonly Random Random = new Random();

        private static readonly List<Region> regions = new List<Region>();

        private static readonly ushort[,] map = new ushort[WIDTH,HEIGHT];

        private static IKernel kernel;

        private static void LoadRegions()
        {
            for (int mapCnt = 1; File.Exists(string.Format("map/map{0}.tmx", mapCnt)); mapCnt++)
            {
                var settings = new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse, XmlResolver = null};

                using (
                        XmlReader str = XmlReader.Create(File.OpenRead(string.Format("map/map{0}.tmx", mapCnt)),
                                                         settings))
                {
                    var reader = new XPathDocument(str);
                    XPathNavigator nav = reader.CreateNavigator();

                    XPathNodeIterator nodeIter = nav.Select("/map/layer/data");

                    var region = new Region();

                    //load tile info
                    while (nodeIter.MoveNext())
                    {
                        Debug.Assert(nodeIter.Current != null, "nodeIter.Current != null");

                        byte[] zippedMap = Convert.FromBase64String(nodeIter.Current.Value);
                        using (var gzip = new GZipStream(new MemoryStream(zippedMap), CompressionMode.Decompress))
                        {
                            var tmpMap = new byte[REGION_WIDTH * REGION_HEIGHT * sizeof(int)];
                            gzip.Read(tmpMap, 0, REGION_WIDTH * REGION_HEIGHT * sizeof(int));

                            int cnt = 0;
                            for (int i = 0; i < tmpMap.Length; i += sizeof(int))
                            {
                                var tileId = (ushort)(BitConverter.ToInt32(tmpMap, i) - 1);
                                region.Map[cnt++] = tileId;
                            }
                        }

                        break; //not supporting multi layer
                    }

                    //scan and generate tile list
                    for (uint y = 0; y < REGION_HEIGHT; ++y)
                    {
                        for (uint x = 0; x < REGION_WIDTH; ++x)
                        {
                            ushort tileId = region.Map[y * REGION_WIDTH + x];

                            if (tileId != CITY_TILE)
                            {
                                continue;
                            }

                            region.CityLocations.Add(new Vector2D(x, y));
                        }
                    }

                    regions.Add(region);
                }
            }
        }

        private static ushort[] GenerateRegion()
        {
            Region region = regions[Random.Next(regions.Count)];

            var regionMap = new ushort[REGION_WIDTH * REGION_HEIGHT];
            Buffer.BlockCopy(region.Map, 0, regionMap, 0, region.Map.Length * sizeof(ushort));

            foreach (var cityLocation in region.CityLocations)
            {
                CityResourceTileGenerator.GenerateResource(cityLocation, regionMap);
            }

            return regionMap;
        }

        private static void Main()
        {
            bool help = false;
            string settingsFile = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"settings=", v => settingsFile = v},};
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch(Exception e)
            {
                Console.Out.WriteLine(e.Message);
                Environment.Exit(0);
            }

            if (help)
            {
                Console.Out.WriteLine("Usage: mapgenerator [--settings=settings.ini]");
                Environment.Exit(0);
            }

            Config.LoadConfigFile(settingsFile);           
            kernel = Engine.CreateDefaultKernel();
            kernel.Get<FactoriesInitializer>().CompileAndInit();

            LoadRegions();

            if (regions.Count == 0)
            {
                Console.Out.WriteLine("No maps were found. They should be in the /map/ folder");
                Environment.Exit(-1);
            }

            using (var bw = new BinaryWriter(File.Open("map.dat", FileMode.Create, FileAccess.Write)))
            {
                for (int row = 0; row < REGION_ROW; ++row)
                {
                    for (int col = 0; col < REGION_COLUMN; ++col)
                    {
                        ushort[] data = GenerateRegion();

                        for (int y = 0; y < REGION_HEIGHT; ++y)
                        {
                            for (int x = 0; x < REGION_WIDTH; ++x)
                            {
                                bw.Write(data[y * REGION_WIDTH + x]);
                                map[REGION_WIDTH * col + x, REGION_HEIGHT * row + y] = data[y * REGION_WIDTH + x];
                            }
                        }
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(File.Open("CityLocations.txt", FileMode.Create)))
            {
                var sorter = new SpiralSorter();

                foreach (Vector2D loc in sorter.Sort(map))
                {
                    sw.WriteLine("{0},{1}", loc.X, loc.Y);
                }
            }
        }

        #region Nested type: Region

        private class Region
        {
            public readonly List<Vector2D> CityLocations = new List<Vector2D>();

            public readonly ushort[] Map = new ushort[REGION_WIDTH * REGION_HEIGHT];
        }

        #endregion
    }
}