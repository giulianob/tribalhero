﻿#region

using System;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Ninject;

#endregion

namespace Game.Comm
{
    public class ResourcesCommandLineModule : CommandLineModule
    {
        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("sendresources", SendResources, true);
            processor.RegisterCommand("trainunits", TrainUnits, true);
        }

        public string SendResources(Session session, string[] parms)
        {
            var resource = new Resource();
            string cityName = string.Empty;
            bool help = false;

            try
            {
                var p = new OptionSet
                        {
                                {"?|help|h", v => help = true},
                                {"city=", v => cityName = v.TrimMatchingQuotes()},
                                {"crop=", v => resource.Crop = int.Parse(v)},
                                {"wood=", v => resource.Wood = int.Parse(v)},
                                {"labor=", v => resource.Labor = int.Parse(v)},
                                {"iron=", v => resource.Iron = int.Parse(v)},
                                {"gold=", v => resource.Gold = int.Parse(v)},
                        };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName))
                return "sendresources --city=city [--crop=###] [--wood=###] [--iron=###] [--labor=###] [--gold=###]";

            uint cityId;
            if (!Global.World.FindCityId(cityName, out cityId))
                return "City not found";

            City city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                    return "City not found";

                city.BeginUpdate();
                city.Resource.Add(resource);
                city.EndUpdate();
            }

            return "OK!";
        }
    
        public string TrainUnits(Session session, string[] parms)
        {
            ushort type = 0;
            ushort count = 0;
            string cityName = string.Empty;
            bool help = false;

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
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName))
                return "trainunits --city=city --type=type --count=count";

            uint cityId;
            if (!Global.World.FindCityId(cityName, out cityId))
                return "City not found";

            City city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                    return "City not found";

                if (Ioc.Kernel.Get<UnitFactory>().GetName(type, 1) == null)                
                    return "Unit type does not exist";

                if (count <= 0)
                    return "Invalid count";

                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(FormationType.Normal, type, count);
                city.DefaultTroop.EndUpdate();
            }

            return "OK!";
        }
    }
}