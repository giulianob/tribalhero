#region

using System;
using Game.Data;
using Game.Data.Forest;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using System.Linq;

#endregion

namespace Game.Comm
{
    public class ResourcesCommandLineModule : ICommandLineModule
    {
        private readonly IWorld world;
        private readonly UnitFactory unitFactory;
        private readonly IForestManager forestManager;
        private readonly ICityManager cityManager;
        private readonly ILocker locker;
        private readonly IObjectTypeFactory objectTypeFactory;
        private readonly Formula formula;

        public ResourcesCommandLineModule(
            IWorld world, 
            IForestManager forestManager, 
            ICityManager cityManager, 
            ILocker locker, 
            IObjectTypeFactory objectTypeFactory, 
            Formula formula, 
            UnitFactory unitFactory)
        {
            this.forestManager = forestManager;
            this.cityManager = cityManager;
            this.locker = locker;
            this.objectTypeFactory = objectTypeFactory;
            this.formula = formula;
            this.unitFactory = unitFactory;
            this.world = world;
        }

        public void RegisterCommands(CommandLineProcessor processor)
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
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            return locker.Lock(cityId, out city).Do(() =>
            {
                if (city == null)
                {
                    return "City not found";
                }

                city.BeginUpdate();
                city.Resource.Add(resource);
                city.EndUpdate();
                
                return "OK!";
            });            
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
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            return locker.Lock(cityId, out city).Do(() =>
            {
                if (city == null)
                {
                    return "City not found";
                }

                if (unitFactory.GetName(type, 1) == null)
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

                return "OK!";
            });
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

            foreach (ICity city in cityManager.AllCities())
            {
                locker.Lock(city).Do(() =>
                {
                    var lumbermill = city.FirstOrDefault(structure => objectTypeFactory.IsStructureType("Lumbermill", structure));
                    if (lumbermill == null)
                    {
                        return;
                    }

                    lumbermill.BeginUpdate();
                    lumbermill["Labor"] = formula.GetForestCampLaborerString(lumbermill);
                    lumbermill.EndUpdate();
                });
            }
            return string.Format("OK!  All forests' capacities set to [{0}]", capacity);
        }
    }
}