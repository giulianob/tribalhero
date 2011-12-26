using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Ninject;
using Persistance;

namespace Game.Comm
{
    class AssignmentCommandLineModule : CommandLineModule
    {
        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("assignmentlist", AssignmentList, true);
            processor.RegisterCommand("assignmentcreate", AssignmentCreate, true);
            processor.RegisterCommand("assignmentjoin", AssignmentJoin, true);
        }

        private string AssignmentList(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                        {
                                {"?|help|h", v => help = true},
                                {"player=", v => playerName = v.TrimMatchingQuotes()},
                                {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) && string.IsNullOrEmpty(tribeName))
                return "AssignmentList --player=player_name|--tribe=tribe_name";

            uint playerId;
            if (!string.IsNullOrEmpty(playerName))
            {
                if (!World.Current.FindPlayerId(playerName, out playerId))
                    return "Player not found";
            }
            else
            {
                if (!World.Current.FindTribeId(tribeName, out playerId))
                    return "Tribe not found";
            }

            Player player;
            ITribe tribe;
            string result = string.Format("Now[{0}] Assignments:\n", DateTime.UtcNow);
            using (Concurrency.Current.Lock(playerId, out player, out tribe))
            {
                if (player == null)
                    return "Player not found";
                if (tribe == null)
                    return "Player does not own a tribe";

                result = ((IEnumerable<Assignment>)tribe).Aggregate(result, (current, assignment) => current + assignment.ToString());
            }

            return result;
        }

        private string AssignmentCreate(Session session, string[] parms)
        {
            bool help = false;
            string cityName = string.Empty;
            uint x = 0;
            uint y = 0;
            TimeSpan time = TimeSpan.MinValue;
            AttackMode mode = AttackMode.Normal;
            try
            {
                var p = new OptionSet
                        {
                                {"?|help|h", v => help = true},
                                {"city=", v => cityName = v.TrimMatchingQuotes()},
                                {"x=", v => x = uint.Parse(v)},
                                {"y=", v => y = uint.Parse(v)},
                                {"timespan=", v => time = TimeSpan.Parse(v.TrimMatchingQuotes())},
                                {"mode=", v => mode = (AttackMode)Enum.Parse(typeof(AttackMode), v)},
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || x == 0 || y == 0 || time == TimeSpan.MinValue)
                return "AssignmentCreate --city=city_name --x=x --y=y --timespan=0:0:0 [--mode=attack_mode]";

            uint cityId;
            if (!World.Current.FindCityId(cityName, out cityId))
                return "City not found";

            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                return "City not found!";
            }

            if (city.Owner.Tribesman == null)
            {
                return "Not in tribe";
            }

            ITribe tribe = city.Owner.Tribesman.Tribe;
            IStructure targetStructure = World.Current.GetObjects(x, y).OfType<IStructure>().First();           
            if (targetStructure == null)
            {
                return "Could not find a structure for the given coordinates";
            }

            using (Concurrency.Current.Lock(city, tribe, targetStructure.City))
            {
                if (city.DefaultTroop.Upkeep == 0)
                {
                    return "No troops in the city!";
                }

                TroopStub stub = new TroopStub { city.DefaultTroop };
                Procedure.Current.TroopStubCreate(city, stub, TroopState.WaitingInAssignment);
                DbPersistance.Current.Save(stub);

                targetStructure = World.Current.GetObjects(x, y).OfType<IStructure>().First();

                if (targetStructure == null)
                {
                    return "Could not find a structure for the given coordinates";
                }

                int id;
                Error error = tribe.CreateAssignment(stub, x, y, targetStructure.City, DateTime.UtcNow.Add(time), mode, out id);
                if (error != Error.Ok)
                {
                    city.Troops.Remove(stub.TroopId);
                    return Enum.GetName(typeof(Error), error);
                }

                return string.Format("OK ID[{0}]", id);
            }
        }

        private string AssignmentJoin(Session session, string[] parms)
        {
            bool help = false;
            string cityName = string.Empty;
            int id = int.MaxValue;
            try
            {
                var p = new OptionSet { { "?|help|h", v => help = true }, { "city=", v => cityName = v.TrimMatchingQuotes() }, { "id=", v => id = int.Parse(v) }, };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || id == int.MaxValue)
                return "AssignementCreate --city=city_name --id=id";

            uint cityId;
            if (!World.Current.FindCityId(cityName, out cityId))
                return "City not found";

            ICity city;
            TroopStub stub = new TroopStub();
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                return "City not found!";
            }

            if (city.Owner.Tribesman == null)
            {
                return "Not in tribe";
            }

            ITribe tribe = city.Owner.Tribesman.Tribe;
            using (Concurrency.Current.Lock(city, tribe))
            {
                if (city.DefaultTroop.Upkeep == 0)
                {
                    return "No troops in the city!";
                }
                stub.Add(city.DefaultTroop);
                Procedure.Current.TroopStubCreate(city, stub, TroopState.WaitingInAssignment);
                DbPersistance.Current.Save(stub);

                Error error = tribe.JoinAssignment(id, stub);
                if (error != Error.Ok)
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Enum.GetName(typeof(Error), error);
                }

                return string.Format("OK");                
            }
        }
    }
}