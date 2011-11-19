#region

using System;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Ninject;

#endregion

namespace Game.Comm
{
    public class TroopsCommandLineModule : CommandLineModule
    {
        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("sendunits", SendUnits, true);
        }

        private string SendUnits(Session session, string[] parms)
        {
            string cityName = string.Empty;
            bool help = false;
            ushort type = 0;
            ushort count = 1;

            try
            {
                var p = new OptionSet
                        {
                                {"?|help|h", v => help = true},
                                {"city=", v => cityName = v.TrimMatchingQuotes()},
                                {"type=", v => type = ushort.Parse(v)},
                                {"count=", v => count = ushort.Parse(v)},
                        };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type,1)==null)
                return "sendunits --city=city --type=### [--count=###]";

            uint cityId;
            if (!Global.World.FindCityId(cityName, out cityId))
                return "City not found";

            City city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                    return "City not found";

                city.BeginUpdate();
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(FormationType.Normal, type, count);
                city.DefaultTroop.EndUpdate();

                city.EndUpdate();
            }

            return "OK!";
        }
    }
}