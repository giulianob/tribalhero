using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Map;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace MapGenerator {
    class Program {
        static List<uint> cityLocations = new List<uint>();
        static Random random = new Random();
        static StreamWriter sw;

        static int width = 3808;
        static int height = 6944;
        static int region_width = 34;
        static int region_height = 62;
        static int region_column = (int)(width / region_width);
        static int region_row = (int)(height / region_height);
        static int numberOfTiles = 10;
        static int numberOfFarm = 2;
        static int numberOfWoodland = 2;

        const ushort FARM_TILE = 208;
        const ushort WOODLAND_TILE = 209;
        const ushort CITY_TILE = 209;
        const byte radius = 3;

        static List<Region> regions = new List<Region>();

        class Region {
            public ushort[] map = new ushort[region_width * region_height];
            public List<Vector2D> cityLocations = new List<Vector2D>();
        }

        struct Vector2D {
            uint x;
            public uint X { get { return x; } set { x = value; } }

            uint y;
            public uint Y { get { return y; } set { y = value; } }

            public Vector2D(uint x, uint y) { this.x = x; this.y = y; }
        }

        static bool AreaClear(uint ox, uint oy, uint x, uint y, object custom) {
            ushort[] map = custom as ushort[];
            while (map[y * region_width + x] == FARM_TILE || map[y * region_width + x] == WOODLAND_TILE) {
                map[y * region_width + x] = (ushort)random.Next(1, numberOfTiles);
            }
            return true;
        }

        public static int distance(uint x, uint y, uint x_1, uint y_1) {
            /***********************************************************
             *   1,1  |  2,1  |  3,1  |  4,1  |
             *       1,2  |  2,2  |  3,2  |  4,2
             *   1,3  |  2,3  |  3,3  |  4,3  |
             *       1,4  |  2,4  |  3,4  |  4,4
             * 
             * *********************************************************/
            uint offset = 0;
            if (y % 2 == 1 && y_1 % 2 == 0 && x_1 <= x) offset = 1;
            if (y % 2 == 0 && y_1 % 2 == 1 && x_1 >= x) offset = 1;
            int dist = (int)((x_1 > x ? x_1 - x : x - x_1) + (y_1 > y ? y_1 - y : y - y_1) / 2 + offset);
            double real = Math.Sqrt(Math.Pow(x - x_1, 2) + Math.Pow(y - y_1, 2));

            return dist;
        }

        static void GenerateResource(Vector2D cityLocation, ushort[] map) {
            uint x;
            uint y;

            TileLocator.foreach_object(cityLocation.X, cityLocation.Y, radius, true, AreaClear, map);
            for (int i = 0; i < numberOfFarm; ++i) {
                do {
                    TileLocator.random_point(cityLocation.X, cityLocation.Y, radius, false, out x, out y);
                } while (map[y * region_width + x] == FARM_TILE || map[y * region_width + x] == WOODLAND_TILE);
                map[y * region_width + x] = FARM_TILE;
            }

            for (int i = 0; i < numberOfWoodland; ++i) {
                do {
                    TileLocator.random_point(cityLocation.X, cityLocation.Y, radius, false, out x, out y);
                } while (map[y * region_width + x] == FARM_TILE || map[y * region_width + x] == WOODLAND_TILE);
                map[y * region_width + x] = WOODLAND_TILE;
            }              
        }

        static void LoadRegions() {
            for (int mapCnt = 1; File.Exists(string.Format("map/map{0}.tmx", mapCnt)); mapCnt++) {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ProhibitDtd = false;
                settings.XmlResolver = null;

                using (XmlReader str = XmlReader.Create(File.OpenRead(string.Format("map/map{0}.tmx", mapCnt)), settings)) {
                    XPathDocument reader = new XPathDocument(str);
                    XPathNavigator nav = reader.CreateNavigator();

                    XPathNodeIterator nodeIter = nav.Select("/map/layer/data");

                    Region region = new Region();

                    //load tile info
                    while (nodeIter.MoveNext()) {
                        byte[] zippedMap = Convert.FromBase64String(nodeIter.Current.Value);
                        using (GZipStream gzip = new GZipStream(new MemoryStream(zippedMap), CompressionMode.Decompress)) {
                            byte[] tmpMap = new byte[region_width * region_height * sizeof(int)];
                            gzip.Read(tmpMap, 0, region_width * region_height * sizeof(int));

                            int cnt = 0;
                            for (int i = 0; i < tmpMap.Length; i += sizeof(int)) {
                                ushort tileId = (ushort)(BitConverter.ToInt32(tmpMap, i) - 1);
                                region.map[cnt++] = tileId;
                            }
                        }

                        break; //not supporting multi layer
                    }

                    //scan and generate tile list
                    for (uint y = 0; y < region_height; ++y) {
                        for (uint x = 0; x < region_width; ++x) {
                            ushort tileId = region.map[y * region_width + x];

                            if (tileId != CITY_TILE) continue;

                            region.cityLocations.Add(new Vector2D(x, y));
                        }
                    }

                    regions.Add(region);
                }
            }
        }

        static ushort[] GenerateRegion(uint xOffset, uint yOffset) {
            Region region = regions[random.Next(regions.Count)];

            ushort[] map = new ushort[region_width * region_height];
            Buffer.BlockCopy(region.map, 0, map, 0, region.map.Length * sizeof(ushort));

            foreach (Vector2D cityLocation in region.cityLocations) {
                sw.WriteLine("{0},{1}", xOffset + cityLocation.X, yOffset + cityLocation.Y);
                GenerateResource(cityLocation, map);
            }

            return map;
        }

        static void Main(string[] args) {
            LoadRegions();

            if (regions.Count == 0) {
                Console.Out.WriteLine("No maps were found. They should be in the /map/ folder");
                Environment.Exit(-1);
            }

            using (BinaryWriter bw = new BinaryWriter(File.Open("map.dat", FileMode.Create, FileAccess.Write))) {
                using (sw = new StreamWriter(File.Open("CityLocations.txt", FileMode.Create))) {                    
                    for (int row = 0; row < region_row; ++row) {
                        for (int col = 0; col < region_column; ++col) {
                            uint xOffset = (uint)(col * region_width);
                            uint yOffset = (uint)(row * region_height);

                            ushort[] data = GenerateRegion(xOffset, yOffset);
                            
                            for (int y = 0; y < region_height; ++y) {
                                for (int x = 0; x < region_width; ++x) {
                                    bw.Write(data[y * region_width + x]);
                                }
                            }
                        }
                    }
                }
            }            
        }
    }
}
