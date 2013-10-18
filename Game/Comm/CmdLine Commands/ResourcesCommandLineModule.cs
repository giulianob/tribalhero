#region

using System;
using Game.Data;
using Game.Data.Forest;
using Game.Data.Troop;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm
{
    public class ResourcesCommandLineModule : CommandLineModule
    {
        private readonly IForestManager forestManager;
        private readonly IDbManager dbManager;

        public ResourcesCommandLineModule(IForestManager forestManager, IDbManager dbManager)
        {
            this.forestManager = forestManager;
            this.dbManager = dbManager;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("sendresources", SendResources, PlayerRights.Admin);
            processor.RegisterCommand("trainunits", TrainUnits, PlayerRights.Admin);
            processor.RegisterCommand("reloadforest", ReloadForest, PlayerRights.Admin);
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
            {
                return "sendresources --city=city [--crop=###] [--wood=###] [--iron=###] [--labor=###] [--gold=###]";
            }

            uint cityId;
            if (!World.Current.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                {
                    return "City not found";
                }

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
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName))
            {
                return "trainunits --city=city --type=type --count=count";
            }

            uint cityId;
            if (!World.Current.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                {
                    return "City not found";
                }

                if (Ioc.Kernel.Get<UnitFactory>().GetName(type, 1) == null)
                {
                    return "Unit type does not exist";
                }

                if (count <= 0)
                {
                    return "Invalid count";
                }

                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.AddUnit(FormationType.Normal, type, count);
                city.DefaultTroop.EndUpdate();
            }

            return "OK!";
        }

        public string ReloadForest(Session session, string[] parms)
        {
            bool help = false;
            int capacity = 400;
            try
            {
                var p = new OptionSet
                {
                        {"capacity=", v => capacity = int.Parse(v)},
                        {"?|help|h", v => help = true},

                };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }
            if (help)
            {
                return "reloadforest --capacity=count";
            }

            forestManager.ReloadForests(capacity);
            return string.Format("OK!  All forests' capacities set to [{0}]", capacity);
        }
    }
}