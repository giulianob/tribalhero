using System;
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
        private readonly Procedure procedure;

        public CityCommandLineModule(Procedure procedure)
        {
            this.procedure = procedure;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("renamecity", RenameCity, true);
            processor.RegisterCommand("removestructure", RemoveStructure, true);
            processor.RegisterCommand("deletestucktroop", DeleteStuckTroop, true);
        }

        public string DeleteStuckTroop(Session session, String[] parms)
        {
            bool help = false;
            byte stubId = 0;
            string cityName = string.Empty;

            try
            {
                var p = new OptionSet
                        {
                                { "?|help|h", v => help = true }, 
                                { "city=", v => cityName = v.TrimMatchingQuotes() },
                                { "stubId=", v => stubId = byte.Parse(v.TrimMatchingQuotes()) }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || cityName == string.Empty)
                return "deletestucktroop --city=### --stubId=###";

            uint cityId;
            if (!World.Current.FindCityId(cityName, out cityId))
                return "City not found";

            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                    return "City not found";

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
            }

            return "OK!";            
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
                                { "?|help|h", v => help = true }, 
                                { "x=", v => x = uint.Parse(v.TrimMatchingQuotes()) },
                                { "y=", v => y = uint.Parse(v.TrimMatchingQuotes()) }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || x == 0 || y == 0)
                return "removestructure --x=### --y=###";

            Region region = World.Current.GetRegion(x, y);
            if (region == null)
                return "Invalid coordinates";

            var structure = region.GetObjects(x, y).OfType<IStructure>().FirstOrDefault();
            
            if (structure == null)
            {                
                return "No structures found at specified coordinates";
            }

            using (Concurrency.Current.Lock(structure.City))
            {
                var removeAction = new StructureSelfDestroyPassiveAction(structure.City.Id, structure.ObjectId);
                var result = structure.City.Worker.DoPassive(structure.City, removeAction, false);
                
                if (result != Error.Ok)
                {
                    return string.Format("Error: {0}", result);
                }
            }

            return "OK!";
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
                                { "?|help|h", v => help = true }, 
                                { "city=", v => cityName = v.TrimMatchingQuotes() },
                                { "newname=", v => newCityName = v.TrimMatchingQuotes() }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || string.IsNullOrEmpty(newCityName))
                return "renamecity --cityr=city --newname=name";

            uint cityId;
            if (!World.Current.FindCityId(cityName, out cityId))
                return "City not found";

            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                    return "City not found";

                // Verify city name is valid
                if (!City.IsNameValid(newCityName))
                {
                    return "City name is invalid";
                }

                lock (World.Current.Lock)
                {
                    // Verify city name is unique
                    if (World.Current.CityNameTaken(newCityName))
                    {
                        return "City name is already taken";
                    }

                    city.BeginUpdate();
                    city.Name = newCityName;
                    city.EndUpdate();
                }
            }

            return "OK!";
        }
    }
}
