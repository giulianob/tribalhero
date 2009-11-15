using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Game.Setup {
    public class MapFactory {
        static Dictionary<int, List<uint>> dict = new Dictionary<int, List<uint>>();
        static int index = 0;
        static int region_index = 0;
        static uint region_width = Config.region_width*2;
        static uint region_height = Config.region_height*3;
        static int region_col = (int)(Config.map_width / region_width);
        static int region_row = (int)(Config.map_height / region_height);
        static int region_count = region_col * region_row;

        static int GetRegion(uint x, uint y) {
            return (int)(x / region_width + (y / region_height) * region_col);
        }
        public static void init(string filename) {
       //     if (Config.map_width % region_width != 0 || Config.map_height % region_height != 0) throw new Exception();
            for (int i = 0; i < region_count; ++i) dict[i] = new List<uint>();
            using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))) {
                String line;
                while ((line = reader.ReadLine()) != null) {
                    String[] strs = line.Split(',');
                    uint x = uint.Parse(strs[0]);
                    uint y = uint.Parse(strs[1]);
                    uint hash = x + y * Config.map_width;
                    int region = GetRegion(x,y);
                    List<uint> list;
                    if (!dict.TryGetValue(region, out list)) {
                        for (int i = region_count; i <= region; ++i ) {
                            list = new List<uint>();
                            dict[i] = list;
                        }
                        region_count = region + 1;
                    }
                    list.Add(hash);
                }
            }
        }

        public static bool NextLocation(out uint x, out uint y) {
            List<uint> list;
            do {
                if (!dict.TryGetValue(region_index, out list)) {
                    y = x = 0;
                    return false;
                }
                do {
                    if (index >= list.Count) {
                        index = 0;
                        ++region_index;
                        break;
                    }
                    uint hash = list[index++];
                    x = hash % Config.map_width;
                    y = hash / Config.map_width;
                    if (Global.World.getObjects(x, y).Count == 0) {
                        return true;
                    }
                } while (true);
            } while (true);
        }
    }
}
