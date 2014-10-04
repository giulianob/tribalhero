#region

using System;
using System.IO;
using Game.Data;
using Game.Map;
using Game.Module;
using Game.Util;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    class RegionCommandsLineModule : ICommandLineModule
    {

        private readonly IWorld world;

        private readonly MapDataExport mapDataExport;

        public RegionCommandsLineModule(IWorld world, MapDataExport mapDataExport)
        {
            this.world = world;
            this.mapDataExport = mapDataExport;
        }

        public void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("dumpregion", DumpRegion, PlayerRights.Bureaucrat);
            processor.RegisterCommand("dumpregioncache", DumpRegionCache, PlayerRights.Bureaucrat);
            processor.RegisterCommand("cleanregioncache", CleanRegionCache, PlayerRights.Bureaucrat);            
            processor.RegisterCommand("exportmapjson", ExportMapJson, PlayerRights.Bureaucrat);            
        }

        public string ExportMapJson(Session session, String[] parms)
        {
            mapDataExport.Export();

            return "OK!";
        }

        public string CleanRegionCache(Session session, String[] parms)
        {
            bool help = false;
            uint x = uint.MaxValue;
            uint y = uint.MaxValue;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"x=", v => x = uint.Parse(v.TrimMatchingQuotes())},
                        {"y=", v => y = uint.Parse(v.TrimMatchingQuotes())}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || x == uint.MaxValue || y == uint.MaxValue)
            {
                return "cleanregioncache --x=x --y=y";
            }

            var region = world.Regions.GetRegion(x, y);
            if (region == null)
            {
                return "Invalid x/y";
            }

            region.MarkAsDirty();
            
            return "OK";
        }

        public string DumpRegionCache(Session session, String[] parms)
        {
            bool help = false;
            uint x = uint.MaxValue;
            uint y = uint.MaxValue;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"x=", v => x = uint.Parse(v.TrimMatchingQuotes())},
                        {"y=", v => y = uint.Parse(v.TrimMatchingQuotes())}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || x == uint.MaxValue || y == uint.MaxValue)
            {
                return "dumpregioncache --x=x --y=y";
            }

            var region = world.Regions.GetRegion(x, y);
            if (region == null)
            {
                return "Invalid x/y";
            }

            var bytes = region.GetObjectBytes();
            
            return HexDump.GetString(bytes, 0);
        }

        public string DumpRegion(Session session, String[] parms)
        {
            bool help = false;
            uint x = uint.MaxValue;
            uint y = uint.MaxValue;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"x=", v => x = uint.Parse(v.TrimMatchingQuotes())},
                        {"y=", v => y = uint.Parse(v.TrimMatchingQuotes())}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || x == uint.MaxValue || y == uint.MaxValue)
            {
                return "dumpregion --x=x --y=y";
            }

            var region = world.Regions.GetRegion(x, y);
            if (region == null)
            {
                return "Invalid x/y";
            }

            StringWriter outString = new StringWriter();

            foreach (var obj in region.GetPrimaryObjects())
            {
                outString.WriteLine("groupid[{0}] objectid[{1}] type[{2}] in_world[{3}] x[{4}] y[{5}]",
                                    obj.GroupId,
                                    obj.ObjectId,
                                    obj.Type,
                                    obj.InWorld,
                                    obj.PrimaryPosition.X,
                                    obj.PrimaryPosition.Y);
            }

            return outString.ToString();
        }
    }
}