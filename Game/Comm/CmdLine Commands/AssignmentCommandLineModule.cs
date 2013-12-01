using System;
using System.Linq;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;

namespace Game.Comm
{
    class AssignmentCommandLineModule : CommandLineModule
    {
        private readonly ILocker locker;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        public AssignmentCommandLineModule(IWorld world, ITribeManager tribeManager, ILocker locker)
        {
            this.world = world;
            this.tribeManager = tribeManager;
            this.locker = locker;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("assignmentlist", AssignmentList, PlayerRights.Bureaucrat);
            processor.RegisterCommand("assignmentcreate", AssignmentCreate, PlayerRights.Bureaucrat);
            processor.RegisterCommand("assignmentjoin", AssignmentJoin, PlayerRights.Bureaucrat);
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
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) && string.IsNullOrEmpty(tribeName))
            {
                return "AssignmentList --player=player_name|--tribe=tribe_name";
            }

            uint playerId;
            if (!string.IsNullOrEmpty(playerName))
            {
                if (!world.FindPlayerId(playerName, out playerId))
                {
                    return "Player not found";
                }
            }
            else
            {
                if (!tribeManager.FindTribeId(tribeName, out playerId))
                {
                    return "Tribe not found";
                }
            }

            IPlayer player;
            ITribe tribe;

            return locker.Lock(playerId, out player, out tribe).Do(() =>
            {
                if (player == null)
                {
                    return "Player not found";
                }
                if (tribe == null)
                {
                    return "Player does not own a tribe";
                }

                string result = string.Format("Now[{0}] Assignments:\n", DateTime.UtcNow);
                return tribe.Assignments.Aggregate(result,
                                                   (current, assignment) => current + assignment.ToNiceString());
            });
        }

        private string AssignmentCreate(Session session, string[] parms)
        {
            bool help = false;
            string cityName = string.Empty;
            uint x = 0;
            uint y = 0;
            TimeSpan time = TimeSpan.MinValue;
            AttackMode mode = AttackMode.Normal;
            bool? isAttack = null;
            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"city=", v => cityName = v.TrimMatchingQuotes()},
                        {"x=", v => x = uint.Parse(v)},
                        {"y=", v => y = uint.Parse(v)},
                        {"timespan=", v => time = TimeSpan.Parse(v.TrimMatchingQuotes())},
                        {"mode=", v => mode = (AttackMode)Enum.Parse(typeof(AttackMode), v, true)},
                        {"isattack=", v => isAttack = Boolean.Parse(v)}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || x == 0 || y == 0 || time == TimeSpan.MinValue ||
                !isAttack.HasValue)
            {
                return
                        "AssignmentCreate --city=city_name --x=x --y=y --timespan=00:00:00 --isattack=true/false [--mode=attack_mode]";
            }

            uint cityId;
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                return "City not found!";
            }

            if (city.Owner.Tribesman == null)
            {
                return "Not in tribe";
            }

            ITribe tribe = city.Owner.Tribesman.Tribe;
            IStructure targetStructure = world.Regions.GetObjectsInTile(x, y).OfType<IStructure>().FirstOrDefault();
            if (targetStructure == null)
            {
                return "Could not find a structure for the given coordinates";
            }

            return locker.Lock(city, tribe, targetStructure.City).Do(() =>
            {
                if (city.DefaultTroop.Upkeep == 0)
                {
                    return "No troops in the city!";
                }

                targetStructure = world.Regions.GetObjectsInTile(x, y).OfType<IStructure>().First();

                if (targetStructure == null)
                {
                    return "Could not find a structure for the given coordinates";
                }

                // TODO: Clean this up.. shouldnt really need to do this here
                var stub = city.CreateTroopStub();
                FormationType formation = isAttack.GetValueOrDefault() ? FormationType.Attack : FormationType.Defense;
                stub.AddFormation(formation);
                foreach (var unit in city.DefaultTroop[FormationType.Normal])
                {
                    stub.AddUnit(formation, unit.Key, unit.Value);
                }

                int id;
                Error error = tribe.CreateAssignment(city,
                                                     stub,
                                                     x,
                                                     y,
                                                     targetStructure.City,
                                                     DateTime.UtcNow.Add(time),
                                                     mode,
                                                     "",
                                                     isAttack.GetValueOrDefault(),
                                                     out id);
                if (error != Error.Ok)
                {
                    city.Troops.Remove(stub.TroopId);
                    return Enum.GetName(typeof(Error), error);
                }

                return string.Format("OK ID[{0}]", id);
            });
        }

        private string AssignmentJoin(Session session, string[] parms)
        {
            bool help = false;
            string cityName = string.Empty;
            int id = int.MaxValue;
            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"city=", v => cityName = v.TrimMatchingQuotes()},
                        {"id=", v => id = int.Parse(v)},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName) || id == int.MaxValue)
            {
                return "AssignementCreate --city=city_name --id=id";
            }

            uint cityId;
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                return "City not found!";
            }

            if (city.Owner.Tribesman == null)
            {
                return "Not in tribe";
            }

            ITribe tribe = city.Owner.Tribesman.Tribe;
            return locker.Lock(city, tribe).Do(() =>
            {
                if (city.DefaultTroop.Upkeep == 0)
                {
                    return "No troops in the city!";
                }

                Assignment assignment = tribe.Assignments.FirstOrDefault(x => x.Id == id);

                if (assignment == null)
                {
                    return "Assignment not found";
                }

                // TODO: Clean this up.. shouldnt really need to do this here
                ITroopStub stub = city.CreateTroopStub();
                FormationType formation = assignment.IsAttack ? FormationType.Attack : FormationType.Defense;
                stub.AddFormation(formation);
                foreach (var unit in city.DefaultTroop[FormationType.Normal])
                {
                    stub.AddUnit(formation, unit.Key, unit.Value);
                }

                Error error = tribe.JoinAssignment(id, city, stub);
                if (error != Error.Ok)
                {
                    return Enum.GetName(typeof(Error), error);
                }

                return string.Format("OK");
            });
        }
    }
}