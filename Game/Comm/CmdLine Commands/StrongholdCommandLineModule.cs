#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    class StrongholdCommandLineModule : CommandLineModule
    {
        private readonly ILocker locker;

        private readonly IStrongholdManager strongholdManager;

        private readonly MapFactory mapFactory;

        private readonly Formula formula;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        public StrongholdCommandLineModule(ITribeManager tribeManager,
                                           IWorld world,
                                           ILocker locker,
                                           IStrongholdManager strongholdManager,
                                           MapFactory mapFactory,
                                           Formula formula)
        {
            this.tribeManager = tribeManager;
            this.world = world;
            this.locker = locker;
            this.strongholdManager = strongholdManager;
            this.mapFactory = mapFactory;
            this.formula = formula;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("StrongholdTransfer", CmdStrongholdTransfer, PlayerRights.Admin);
            processor.RegisterCommand("StrongholdAddTroop", CmdStrongholdAddTroop, PlayerRights.Admin);
            processor.RegisterCommand("StrongholdFindNearbyCities", CmdStrongholdFindNearbyCities, PlayerRights.Admin);
            processor.RegisterCommand("StrongholdChangeLevel", CmdStrongholdChangeLevel, PlayerRights.Admin);
        }

        private string CmdStrongholdFindNearbyCities(Session session, string[] parms)
        {
            SystemVariable mapStartIndex;
            int index = 0;
            if (Global.SystemVariables.TryGetValue("Map.start_index", out mapStartIndex))
            {
                index = (int)mapStartIndex.Value;
            }

            var list = new List<Position>(mapFactory.Locations().Take(index));
            foreach(var stronghold in strongholdManager.Where(s=>s.StrongholdState == StrongholdState.Inactive))
            {
                using(locker.Lock(stronghold))
                {
                    stronghold.BeginUpdate();
                    int count = list.Count(pt => stronghold.TileDistance(pt.X, pt.Y) <= Config.stronghold_radius_base + Config.stronghold_radius_per_level * stronghold.Lvl);
                    stronghold.NearbyCitiesCount =
                            (ushort)
                            count;
                    stronghold.EndUpdate();
                }
            }
            return "OK";
        }

        private string CmdStrongholdAddTroop(Session session, string[] parms)
        {
            bool help = false;
            string strongholdName = string.Empty;
            string cityName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"stronghold=", v => strongholdName = v.TrimMatchingQuotes()},
                        {"city=", v => cityName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(strongholdName) || string.IsNullOrEmpty(cityName))
            {
                return "StrongholdAddTroop --stronghold=tribe_name --city=city_name";
            }

            uint cityId;
            if (!world.Cities.FindCityId(cityName, out cityId))
            {
                return "City not found";
            }

            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                return "City not found";
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdName, out stronghold))
            {
                return "Stronghold not found";
            }

            using (locker.Lock(city, stronghold))
            {
                if (city.DefaultTroop.Upkeep == 0)
                {
                    return "No troops in the city!";
                }

                ITroopStub stub = city.Troops.Create();
                stub.BeginUpdate();
                stub.AddFormation(FormationType.Defense);
                foreach (var unit in city.DefaultTroop[FormationType.Normal])
                {
                    stub.AddUnit(FormationType.Defense, unit.Key, unit.Value);
                }
                stub.Template.LoadStats(TroopBattleGroup.Defense);
                stub.EndUpdate();

                if (!stronghold.Troops.AddStationed(stub))
                {
                    return "Error Adding to Station";
                }
            }
            return "OK!";
        }

        private string CmdStrongholdChangeLevel(Session session, string[] parms)
        {
            bool help = false;
            string strongholdName = string.Empty;
            byte level = 0;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"stronghold=", v => strongholdName = v.TrimMatchingQuotes()},
                        {"level=", v => level = byte.Parse(v.TrimMatchingQuotes()) },
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(strongholdName) || level == 0 || level > 20)
            {
                return "StrongholdChangeLevel --stronghold=stronghold_name --level=level";
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdName, out stronghold))
            {
                return "Stronghold not found";
            }

            using (locker.Lock(stronghold))
            {
                if (stronghold.StrongholdState == StrongholdState.Occupied)
                {
                    return "Cannot change level of an occupied stronghold.";
                }

                stronghold.BeginUpdate();
                stronghold.Lvl = level;
                stronghold.Gate = formula.StrongholdGateLimit(level);
                stronghold.EndUpdate();
            }

            return "OK!";
        }

        private string CmdStrongholdTransfer(Session session, string[] parms)
        {
            bool help = false;
            string strongholdName = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"stronghold=", v => strongholdName = v.TrimMatchingQuotes()},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(strongholdName) || string.IsNullOrEmpty(tribeName))
            {
                return "StrongholdTransfer --stronghold=stronghold_name --tribe=tribe_name";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }

            ITribe tribe;
            if (!world.TryGetObjects(tribeId, out tribe))
            {
                return "Tribe not found";
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdName, out stronghold))
            {
                return "Stronghold not found";
            }

            using (locker.Lock(custom => stronghold.LockList.ToArray(), null, tribe, stronghold))
            {
                strongholdManager.TransferTo(stronghold, tribe);
            }

            return "OK!";
        }
    }
}