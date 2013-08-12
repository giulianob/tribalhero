#region

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Game.Data;
using Game.Logic;
using Game.Logic.Requirements.LayoutRequirements;

#endregion

namespace Game.Setup
{
    public class RequirementCsvFactory : IRequirementCsvFactory
    {
        private readonly ILayoutRequirementFactory layoutRequirementFactory;

        private readonly Dictionary<int, ILayoutRequirement> dict = new Dictionary<int, ILayoutRequirement>();

        public RequirementCsvFactory(ILayoutRequirementFactory layoutRequirementFactory)
        {
            this.layoutRequirementFactory = layoutRequirementFactory;
        }

        public void Init(string filename)
        {
            using (var reader = new CsvReader(new StreamReader(new FileStream(filename,
                                                                    FileMode.Open,
                                                                    FileAccess.Read,
                                                                    FileShare.ReadWrite))))
            {
                String[] toks;
                var col = new Dictionary<string, int>();
                for (int i = 0; i < reader.Columns.Length; ++i)
                {
                    col.Add(reader.Columns[i], i);
                }

                while ((toks = reader.ReadRow()) != null)
                {
                    if (toks[0].Length <= 0)
                    {
                        continue;
                    }
                    int index = int.Parse(toks[col["Type"]]) * 100 + int.Parse(toks[col["Lvl"]]);
                    ILayoutRequirement layoutReq;
                    if (!dict.TryGetValue(index, out layoutReq))
                    {
                        switch(toks[col["Layout"]])
                        {
                            case "Simple":
                                layoutReq = layoutRequirementFactory.CreateSimpleLayout();
                                break;
                            case "AwayFrom":
                                layoutReq = layoutRequirementFactory.CreateAwayFromLayout();
                                break;
                            default:
                                layoutReq = layoutRequirementFactory.CreateSimpleLayout();
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
                    
                    dict[index] = layoutReq;
                }
            }
        }

        public ILayoutRequirement GetLayoutRequirement(ushort type, byte lvl)
        {
            if (dict == null)
            {
                return null;
            }

            ILayoutRequirement tmp;

            if (dict.TryGetValue(type * 100 + lvl, out tmp))
            {
                return tmp;
            }

            return layoutRequirementFactory.CreateSimpleLayout();
        }
    }
}