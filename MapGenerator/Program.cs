#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.XPath;
using Game;
using Game.Data;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using NDesk.Options;

#endregion

namespace MapGenerator
{
    class Program
    {
        private const ushort FARM_TILE = 220;
        private const ushort WOODLAND_TILE = 220;
        private const ushort CITY_TILE = 209;        
        private static readonly Random random = new Random();
        private static StreamWriter sw;
        private static readonly List<Location> locations = new List<Location>();

        private static int width = 3400;
        private static int height = 6200;
        private static int region_width = 34;
        private static int region_height = 62;
        private static readonly int regionColumn = width/region_width;
        private static readonly int regionRow = height/region_height;
        private static int numberOfTiles = 10;
        private static int numberOfFarm = 2;
        private static int numberOfWoodland = 2;

        private static readonly List<Region> regions = new List<Region>();

        private static bool AreaClear(uint ox, uint oy, uint x, uint y, object custom)
        {
            var map = (ushort[])custom;
            while (map[y*region_width + x] == FARM_TILE || map[y*region_width + x] == WOODLAND_TILE)
                map[y*region_width + x] = (ushort)random.Next(1, numberOfTiles);
            return true;
        }

        public static int DistanceBetween(uint x, uint y, uint x1, uint y1)
        {
            /***********************************************************
             *   1,1  |  2,1  |  3,1  |  4,1  |
             *       1,2  |  2,2  |  3,2  |  4,2
             *   1,3  |  2,3  |  3,3  |  4,3  |
             *       1,4  |  2,4  |  3,4  |  4,4
             * 
             * *********************************************************/
            uint offset = 0;
            if (y%2 == 1 && y1%2 == 0 && x1 <= x)
                offset = 1;
            if (y%2 == 0 && y1%2 == 1 && x1 >= x)
                offset = 1;
            
            return (int)((x1 > x ? x1 - x : x - x1) + (y1 > y ? y1 - y : y - y1)/2 + offset);
        }

        private static void GenerateResource(Vector2D cityLocation, ushort[] map)
        {
            uint x;
            uint y;

            TileLocator.Current.ForeachObject(cityLocation.X, cityLocation.Y, Formula.Current.GetInitialCityRadius(), true, AreaClear, map);
            for (int i = 0; i < numberOfFarm; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(cityLocation.X, cityLocation.Y, (byte)(Formula.Current.GetInitialCityRadius() - 1), false, out x, out y);
                } while (map[y*region_width + x] == FARM_TILE || map[y*region_width + x] == WOODLAND_TILE ||
                         SimpleGameObject.TileDistance(x, y, cityLocation.X, cityLocation.Y) <= 1 ||
                         !SimpleGameObject.IsPerpendicular(x, y, cityLocation.X, cityLocation.Y));

                map[y*region_width + x] = FARM_TILE;
            }

            for (int i = 0; i < numberOfWoodland; ++i)
            {
                do
                {
                    TileLocator.Current.RandomPoint(cityLocation.X, cityLocation.Y, (byte)(Formula.Current.GetInitialCityRadius() - 1), false, out x, out y);
                } while (SimpleGameObject.TileDistance(x, y, cityLocation.X, cityLocation.Y) <= 1 ||
                         map[y * region_width + x] == FARM_TILE || map[y * region_width + x] == WOODLAND_TILE ||
                         !SimpleGameObject.IsPerpendicular(x, y, cityLocation.X, cityLocation.Y));
                map[y*region_width + x] = WOODLAND_TILE;
            }
        }

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
                            var tmpMap = new byte[region_width*region_height*sizeof(int)];
                            gzip.Read(tmpMap, 0, region_width*region_height*sizeof(int));

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
                    for (uint y = 0; y < region_height; ++y)
                    {
                        for (uint x = 0; x < region_width; ++x)
                        {
                            ushort tileId = region.Map[y*region_width + x];

                            if (tileId != CITY_TILE)
                                continue;

                            region.CityLocations.Add(new Vector2D(x, y));
                        }
                    }

                    regions.Add(region);
                }
            }
        }

        private static ushort[] GenerateRegion(uint xOffset, uint yOffset)
        {
            Region region = regions[random.Next(regions.Count)];

            var map = new ushort[region_width*region_height];
            Buffer.BlockCopy(region.Map, 0, map, 0, region.Map.Length*sizeof(ushort));

            foreach (var cityLocation in region.CityLocations)
            {
                locations.Add(new Location(xOffset + cityLocation.X, yOffset + cityLocation.Y, (uint)SimpleGameObject.TileDistance(xOffset + cityLocation.X, yOffset + cityLocation.Y, (uint)width / 2, (uint)height / 2)));
                GenerateResource(cityLocation, map);
            }

            return map;
        }

        private class Location
        {
            public uint X { get; private set; }
            public uint Y { get; private set; }
            public uint Distance { get; private set; }

            public Location(uint x, uint y, uint distance)

            {
                X = x;
                Y = y;
                Distance = distance;
            }
        }

        private static void Main()
        {
            bool help = false;
            string settingsFile = string.Empty;

            try
            {
                var p = new OptionSet { { "?|help|h", v => help = true }, { "settings=", v => settingsFile = v }, };
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch (Exception e)
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
            Factory.CompileConfigFiles();       
            Engine.CreateDefaultKernel();

            LoadRegions();

            if (regions.Count == 0)
            {
                Console.Out.WriteLine("No maps were found. They should be in the /map/ folder");
                Environment.Exit(-1);
            }

            using (var bw = new BinaryWriter(File.Open("map.dat", FileMode.Create, FileAccess.Write)))
            {
                for (int row = 0; row < regionRow; ++row)
                {
                    for (int col = 0; col < regionColumn; ++col)
                    {
                        var xOffset = (uint)(col*region_width);
                        var yOffset = (uint)(row*region_height);

                        ushort[] data = GenerateRegion(xOffset, yOffset);

                        for (int y = 0; y < region_height; ++y)
                        {
                            for (int x = 0; x < region_width; ++x)
                                bw.Write(data[y*region_width + x]);
                        }
                    }
                }


            }
            using (sw = new StreamWriter(File.Open("CityLocations.txt", FileMode.Create)))
            {
                locations.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                foreach(Location loc in locations)
                {
                    sw.WriteLine("{0},{1}", loc.X, loc.Y);
                }
            }

        }

        #region Nested type: Region

        private class Region
        {
            public readonly List<Vector2D> CityLocations = new List<Vector2D>();
            public readonly ushort[] Map = new ushort[region_width*region_height];
        }

        #endregion

        #region Nested type: Vector2D

        private struct Vector2D
        {
            public Vector2D(uint x, uint y) : this()
            {
                X = x;
                Y = y;
            }

            public uint X { get; private set; }

            public uint Y { get; private set; }
        }

        #endregion
    }
}