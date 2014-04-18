using System;
using System.IO;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;

namespace Game.Comm.CmdLine_Commands
{
    class CityCommandLineModule : CommandLineModule
    {
        private readonly IActionFactory actionFactory;

        private readonly Procedure procedure;

        private IWorld world;

        private ILocker locker;

        public CityCommandLineModule(Procedure procedure,
                                     IActionFactory actionFactory,
                                     ILocker locker,
                                     IWorld world)
        {
            this.procedure = procedure;
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.world = world;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("renamecity", RenameCity, PlayerRights.Admin);
            processor.RegisterCommand("removestructure", RemoveStructure, PlayerRights.Bureaucrat);
            processor.RegisterCommand("deletestucktroop", DeleteStuckTroop, PlayerRights.Bureaucrat);
            processor.RegisterCommand("createcity", CreateCity, PlayerRights.Bureaucrat);
            processor.RegisterCommand("dumpcity", DumpCity, PlayerRights.Bureaucrat);
        }

        public string CreateCity(Session session, String[] parms)
        {
            bool help = false;
            string cityName = string.Empty;
            string playerName = string.Empty;
            uint x = 0;
            uint y = 0;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"newcity=", v => cityName = v.TrimMatchingQuotes()},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                        {"x=", v => x = uint.Parse(v.TrimMatchingQuotes())},
                        {"y=", v => y = uint.Parse(v.TrimMatchingQuotes())},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || cityName == string.Empty)
            {
                return "createcity --player=### --newcity=### --x=#### --y=####";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            IPlayer player;
            return locker.Lock(playerId, out player).Do(() =>
            {
                {
                    if (player == null)
                    {
                        return "Player not found";
                    }

                    ICity city = player.GetCityList().First();
                    var cityCreateAction = actionFactory.CreateCityCreatePassiveAction(city.Id, x, y, cityName);
                    Error ret = city.Worker.DoPassive(city[1], cityCreateAction, true);
                    if (ret != Error.Ok)
                    {
                        return string.Format("Error: {0}", ret);
                    }
                }

                return "OK!";
            });
        }

        public string DeleteStuckTroop(Session session, String[] parms)
        {
            bool help = false;
            ushort stubId = 0;
            string cityName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"city=", v => cityName = v.TrimMatchingQuotes()},
                        {"stubId=", v => stubId = ushort.Parse(v.TrimMatchingQuotes())}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || cityName == string.Empty)
            {
                return "deletestucktroop --city=### --stubId=###";
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

                ITroopStub stub;
                if (!city.Troops.TryGetStub(stubId, out stub))
                {
                    return "Stub not found";
                }

                if (stub == city.DefaultTroop)
                {
                    return "Cant remove local troop";
                }

                procedure.TroopStubDelete(city, stub);

                return "OK!";
            });
        }

        public string RemoveStructure(Session session, String[] parms)
        {
            bool help = false;
            uint x = 0;
            uint y = 0;

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

            if (help || x == 0 || y == 0)
            {
                return "removestructure --x=### --y=###";
            }

            IRegion region = world.Regions.GetRegion(x, y);
            if (region == null)
            {
                return "Invalid coordinates";
            }

            var structure = region.GetObjectsInTile(x, y).OfType<IStructure>().FirstOrDefault();

            if (structure == null)
            {
                return "No structures found at specified coordinates";
            }

            return locker.Lock(structure.City).Do(() =>
            {
                var removeAction = actionFactory.CreateStructureSelfDestroyPassiveAction(structure.City.Id, structure.ObjectId);
                var result = structure.City.Worker.DoPassive(structure.City, removeAction, false);

                if (result != Error.Ok)
                {
                    return string.Format("Error: {0}", result);
                }

                return "OK!";
            });
        }

        public string RenameCity(Session session, String[] parms)
        {
            bool help = false;
            string cityName = string.Empty;
            string newCityName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"city=", v => cityName = v.TrimMatchingQuotes()},
                        {"newname=", v => newCityName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || string.IsNullOrEmpty(newCityName))
            {
                return "renamecity --city=city --newname=name";
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

                // Verify city name is valid
                if (!CityManager.IsNameValid(newCityName))
                {
                    return "City name is invalid";
                }

                lock (world.Lock)
                {
                    // Verify city name is unique
                    if (world.CityNameTaken(newCityName))
                    {
                        return "City name is already taken";
                    }

                    city.BeginUpdate();
                    city.Name = newCityName;
                    city.EndUpdate();
                }

                return "OK!";
            });
        }

        public string DumpCity(Session session, String[] parms)
        {
            bool help = false;
            string cityName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"city=", v => cityName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName))
            {
                return "dumpcity --city=city";
            }

            uint cityId;
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            StringWriter outString = new StringWriter();
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                foreach (var obj in city)
                {
                    outString.WriteLine("id[{0}] type[{1}] level[{2}] in_world[{3}] is_blocked[{4}] x[{5}] y[{6}]",
                                        obj.ObjectId,
                                        obj.Type,
                                        obj.Lvl,
                                        obj.InWorld,
                                        obj.IsBlocked,
                                        obj.PrimaryPosition.X,
                                        obj.PrimaryPosition.Y);
                }
            });

            return outString.ToString();
        }
    }
}