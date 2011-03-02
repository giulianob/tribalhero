#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Logic;
using Game.Logic.Requirements.LayoutRequirements;
using Game.Util;

#endregion

namespace Game.Setup
{
    class RequirementFactory
    {
        private static Dictionary<int, LayoutRequirement> dict;

        public static void Init(string filename)
        {
            if (dict != null)
                return;

            dict = new Dictionary<int, LayoutRequirement>();

            using (var reader = new CsvReader(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                String[] toks;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                    col.Add(reader.Columns[i], i);

                LayoutRequirement layoutReq;

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                        continue;
                    int index = int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]]);
                    if (!dict.TryGetValue(index, out layoutReq))
                    {
                        switch(toks[col["Layout"]])
                        {
                            case "Simple":
                                layoutReq = new SimpleLayout();
                                break;
                            case "AwayFrom":
                                layoutReq = new AwayFromLayout();
                                break;
                            default:
                                layoutReq = new SimpleLayout();
                                break;
                        }
                    }

                    var req = new Requirement(ushort.Parse(toks[col["Rtype"]]),
                                              byte.Parse(toks[col["Cmp"]]),
                                              byte.Parse(toks[col["Lmin"]]),
                                              byte.Parse(toks[col["Lmax"]]),
                                              byte.Parse(toks[col["Dmin"]]),
                                              byte.Parse(toks[col["Dmax"]]));
                    layoutReq.Add(req);

                    Global.Logger.Info(string.Format("{0}", int.Parse(toks[col["Type"]])*100 + int.Parse(toks[col["Lvl"]])));
                    dict[index] = layoutReq;
                }
            }
        }

        public static LayoutRequirement GetLayoutRequirement(ushort type, byte lvl)
        {
            if (dict == null)
                return null;

            LayoutRequirement tmp;

            if (dict.TryGetValue(type*100 + lvl, out tmp))
                return tmp;

            return new SimpleLayout();
        }
    }
}