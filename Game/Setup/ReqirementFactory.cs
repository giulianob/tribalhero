using System;
using System.Collections.Generic;
using System.Text;
using Game.Logic;
using Game.Util;
using System.IO;

namespace Game.Setup {

    class ReqirementFactory {
        static Dictionary<int, LayoutRequirement> dict;

        public static void init(string filename) {
            if (dict != null) return;
            dict = new Dictionary<int, LayoutRequirement>();

            using (CSVReader reader = new CSVReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))) {
                String[] toks;
                Dictionary<string, int> col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i) {
                    col.Add(reader.Columns[i], i);
                }

                LayoutRequirement layout_req;
                Reqirement req;

                while ((toks = reader.ReadRow()) != null) {
                    if (toks[0].Length <= 0) continue;
                    int index = int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]);
                    if (!dict.TryGetValue(index, out layout_req)) {
                        switch (toks[col["Layout"]]) {
                            case "Simple":
                                layout_req = new SimpleLayout();
                                break;
                            default:
                                layout_req = new SimpleLayout();
                                break;
                        }
                    }
                    req = new Reqirement(ushort.Parse(toks[col["Rtype"]]),
                                         byte.Parse(toks[col["Cmp"]]),
                                         byte.Parse(toks[col["Lmin"]]),
                                         byte.Parse(toks[col["Lmax"]]),
                                         byte.Parse(toks[col["Dmin"]]),
                                         byte.Parse(toks[col["Dmax"]]));
                    layout_req.add(req);

                    Global.Logger.Info(string.Format("{0}", int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]])));
                    dict[index] = layout_req;
                }
            }
        }

        static public LayoutRequirement getLayoutRequirement(ushort type, byte lvl) {
            if (dict == null) return null;
            LayoutRequirement tmp;
            if (dict.TryGetValue(type * 100 + lvl, out tmp)) return (LayoutRequirement)tmp;
            return new SimpleLayout();
        }


    }
}
