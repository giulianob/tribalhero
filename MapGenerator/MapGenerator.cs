using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Game.Map;

namespace MapGenerator
{
    class MapGenerator
    {
        public const int TOTAL_PLAYERS = 13000;

        public const ushort CITY_TILE = 16;

        public const byte MIN_DIST_BETWEEN_CITIES = 11;

        public const int WIDTH = 3400;

        public const int HEIGHT = 6200;

        public const int TMX_REGION_WIDTH = 34;

        public const int TMX_REGION_HEIGHT = 62;

        private const int TMX_REGION_COLUMNS = WIDTH / TMX_REGION_WIDTH;

        private const int TMX_REGION_ROWS = HEIGHT / TMX_REGION_HEIGHT;

        private readonly CityResourceTileGenerator cityResourceTileGenerator;

        private readonly ITileLocator tileLocator;

        private readonly ushort[,] map = new ushort[WIDTH, HEIGHT];

        private readonly Random random = new Random();

        private readonly List<MapGenRegion> regions = new List<MapGenRegion>();

        public MapGenerator(CityResourceTileGenerator cityResourceTileGenerator, ITileLocator tileLocator)
        {
            this.cityResourceTileGenerator = cityResourceTileGenerator;
            this.tileLocator = tileLocator;
        }

        private void LoadRegions()
        {
            foreach (var mapFilePath in Directory.GetFiles("map", "*.tmx", SearchOption.AllDirectories))
            {
                using (var xmlReader = XmlReader.Create(File.OpenRead(mapFilePath), new XmlReaderSettings {DtdProcessing = DtdProcessing.Parse, XmlResolver = null}))
                {
                    var document = new XPathDocument(xmlReader);
                    var nav = document.CreateNavigator();

                    var nodeIter = nav.Select("/map/layer/data");

                    var region = new MapGenRegion();

                    //load tile info
                    while (nodeIter.MoveNext())
                    {
                        Debug.Assert(nodeIter.Current != null, "nodeIter.Current != null");

                        byte[] zippedMap = Convert.FromBase64String(nodeIter.Current.Value);
                        using (var gzip = new GZipStream(new MemoryStream(zippedMap), CompressionMode.Decompress))
                        {
                            var tmpMap = new byte[TMX_REGION_WIDTH * TMX_REGION_HEIGHT * sizeof(int)];
                            gzip.Read(tmpMap, 0, TMX_REGION_WIDTH * TMX_REGION_HEIGHT * sizeof(int));

                            int cnt = 0;
                            for (int i = 0; i < tmpMap.Length; i += sizeof(int))
                            {
                                var tileId = (ushort)(BitConverter.ToInt32(tmpMap, i) - 1);
                                region.Map[cnt++] = tileId;
                            }
                        }

                        break; //not supporting multi layer
                    }

                    regions.Add(region);
                }
            }

            if (regions.Count == 0)
            {
                Console.Out.WriteLine("No maps were found. They should be in the /map/ folder");
                Environment.Exit(-1);
            }
        }

        public void GenerateMap() {

            LoadRegions();

            GenerateMapArray();

            GenerateCityLocationsAndResources();

            WriteMapToFile();

            SortCityLocations();
        }

        private void GenerateCityLocationsAndResources()
        {
            var cityLocations = new HashSet<Position>();
            
            do
            {
                var newCityPosition = new Position((uint)random.Next(10, WIDTH - 10), (uint)random.Next(10, HEIGHT - 10));

                var tooCloseToOtherCity = tileLocator.ForeachTile(newCityPosition.X, newCityPosition.Y, MIN_DIST_BETWEEN_CITIES).Any(cityLocations.Contains);

                if (tooCloseToOtherCity)
                {
                    continue;
                }

                cityResourceTileGenerator.GenerateResource(newCityPosition, map);

                cityLocations.Add(newCityPosition);

                map[newCityPosition.X, newCityPosition.Y] = CITY_TILE;
            }
            while (cityLocations.Count < TOTAL_PLAYERS);
        }

        private void SortCityLocations()
        {
            using (StreamWriter sw = new StreamWriter(File.Open("out/CityLocations.txt", FileMode.Create)))
            {
                foreach (var cityLocation in new SpiralSorter().Sort(map))
                {
                    sw.WriteLine("{0},{1}", cityLocation.X, cityLocation.Y);
                }
            }
        }

        private void WriteMapToFile()
        {
            if (!Directory.Exists("out"))
            {
                Directory.CreateDirectory("out");
            }

            using (var bw = new BinaryWriter(File.Open("out/map.dat", FileMode.Create, FileAccess.Write)))
            {
                for (int row = 0; row < TMX_REGION_ROWS; ++row)
                {
                    for (int col = 0; col < TMX_REGION_COLUMNS; ++col)
                    {
                        for (int y = 0; y < TMX_REGION_HEIGHT; ++y)
                        {
                            for (int x = 0; x < TMX_REGION_WIDTH; ++x)
                            {
                                bw.Write(map[TMX_REGION_WIDTH * col + x, TMX_REGION_HEIGHT * row + y]);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateMapArray()
        {
            for (int row = 0; row < TMX_REGION_ROWS; ++row)
            {
                for (int col = 0; col < TMX_REGION_COLUMNS; ++col)
                {
                    MapGenRegion region = regions[random.Next(regions.Count)];

                    var regionMap = new ushort[TMX_REGION_WIDTH * TMX_REGION_HEIGHT];
                    Buffer.BlockCopy(region.Map, 0, regionMap, 0, region.Map.Length * sizeof(ushort));
                    ushort[] data = regionMap;

                    for (int y = 0; y < TMX_REGION_HEIGHT; ++y)
                    {
                        for (int x = 0; x < TMX_REGION_WIDTH; ++x)
                        {
                            map[TMX_REGION_WIDTH * col + x, TMX_REGION_HEIGHT * row + y] = data[y * TMX_REGION_WIDTH + x];
                        }
                    }
                }
            }
        }
    }
}