﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Persistance;

#endregion

namespace Game.Comm
{
    class TribeCommandLineModule : CommandLineModule
    {
        private readonly ITribeFactory tribeFactory;

        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        private readonly IStrongholdManager strongholdManager;

        public TribeCommandLineModule(ITribeFactory tribeFactory, IDbManager dbManager, ILocker locker, ITribeManager tribeManager, IWorld world, IStrongholdManager strongholdManager)
        {
            this.tribeFactory = tribeFactory;
            this.dbManager = dbManager;
            this.locker = locker;
            this.tribeManager = tribeManager;
            this.world = world;
            this.strongholdManager = strongholdManager;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("TribeInfo", Info, PlayerRights.Admin);
            processor.RegisterCommand("TribeCreate", Create, PlayerRights.Admin);
            processor.RegisterCommand("TribeUpdate", Update, PlayerRights.Admin);
            processor.RegisterCommand("TribeDelete", Delete, PlayerRights.Admin);
            processor.RegisterCommand("TribesmanAdd", Add, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribesmanRemove", TribesmanRemove, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribesmanUpdate", TribesmanUpdate, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribeIncomingList", IncomingList, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribeTransfer", Transfer, PlayerRights.Admin);
        }

        private string Info(Session session, string[] parms)
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
                return "TribeInfo --player=player_name|--tribe=tribe_name";

            uint playerId;
            if (!string.IsNullOrEmpty(playerName))
            {
                if (!world.FindPlayerId(playerName, out playerId))
                    return "Player not found";
            }
            else
            {
                if (!tribeManager.FindTribeId(tribeName, out playerId))
                    return "Tribe not found";
            }

            IPlayer player;
            string result;
            using (locker.Lock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";
                if (player.Tribesman == null)
                    return "Player does not own a tribe";

                ITribe tribe = player.Tribesman.Tribe;
                result = string.Format("Id[{0}] Owner[{1}] Lvl[{2}] Name[{3}] Desc[{4}] \n", tribe.Id, tribe.Owner.Name, tribe.Level, tribe.Name, tribe.Description);
                result += tribe.Resource.ToNiceString();
                result += string.Format("Member Count[{0}]\n", tribe.Count);
                result = tribe.Tribesmen.Aggregate(result, (current, tribesman) => current + string.Format("Tribesman[{0}] CityCount[{1}] Rank[{2}] \n", tribesman.Player.Name, tribesman.Player.GetCityCount(), tribesman.Rank));
            }

            return result;
        }

        private string Create(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                        { "name=", v => tribeName = v.TrimMatchingQuotes()},
                        { "desc=", v => v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "TribeCreate --player=player [--name=tribe_name] [--desc=tribe_description]";


            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
                return "Player not found";

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                if (player.Tribesman != null)
                {
                    return "Player already in tribe";
                }

                if (tribeManager.TribeNameTaken(tribeName))
                {
                    return "Tribe name already taken";
                }

                if (!Tribe.IsNameValid(tribeName))
                {
                    return "Tribe name is not allowed";
                }

                ITribe tribe = tribeFactory.CreateTribe(player, tribeName);

                tribeManager.Add(tribe);

                var tribesman = new Tribesman(tribe, player, 0);
                tribe.AddTribesman(tribesman);
            }
            return "OK!";
        }

        private string Delete(Session session, string[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
                return "TribeDelete --name=tribe_name";


            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            if (!world.TryGetObjects(tribeId, out tribe))
                return "Tribe not found";

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var locks =
                            strongholdManager.StrongholdsForTribe(tribe).SelectMany(stronghold => stronghold.LockList).
                                    ToList();

                    locks.AddRange(tribe.Tribesmen);

                    return locks.ToArray();
                };

            using (locker.Lock(lockHandler, new object[] { }, tribe))
            {
                tribeManager.Remove(tribe);
            }

            return "OK!";
        }

        private string Transfer(Session session, string[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;
            string newOwner = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        { "newowner=", v => newOwner = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
                return "TribeTransfer --tribe=tribe_name --newowner=player_name";


            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            if (!world.TryGetObjects(tribeId, out tribe))
                return "Tribe not found";

            uint newOwnerPlayerId;
            IPlayer player;
            if (!world.FindPlayerId(newOwner, out newOwnerPlayerId) || !world.TryGetObjects(newOwnerPlayerId, out player))
            {
                return "New owner not found";
            }

            using (locker.Lock(custom => tribe.Tribesmen.ToArray<ILockable>(), new object[] { }, tribe, player))
            {
                var ret = tribe.Transfer(newOwnerPlayerId);

                if (ret != Error.Ok)
                {
                    return Enum.GetName(typeof(Error), ret);
                }
            }

            return "OK!";
        }

        private string Update(Session session, string[] parms)
        {
            bool help = false;
            string desc = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        { "desc=", v => desc = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName) || string.IsNullOrEmpty(desc))
                return "TribesmanAdd --tribe=tribe_name --desc=desc";

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            using (locker.Lock(tribeId, out tribe))
            {
                tribe.Description = desc;
                dbManager.Save(tribe);
            }
            return "OK";
        }

        private string Add(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(tribeName))
                return "TribesmanAdd --tribe=tribe_name --player=player_name";


            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, IPlayer> players;
            using (locker.Lock(out players, playerId, tribeId))
            {
                ITribe tribe = players[tribeId].Tribesman.Tribe;
                var tribesman = new Tribesman(tribe, players[playerId], 2);
                tribe.AddTribesman(tribesman);
            }
            return "OK";
        }

        private string TribesmanRemove(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {
                                "?|help|h", v => help = true
                        },
                        {
                                "tribe=", v => tribeName = v.TrimMatchingQuotes()
                        },
                        {
                                "player=", v => playerName = v.TrimMatchingQuotes()
                        },
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(tribeName))
                return "TribesmanRemove --tribe=tribe_name --player=player_name";


            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, IPlayer> players;
            using (locker.Lock(out players, playerId, tribeId))
            {
                ITribe tribe = players[tribeId].Tribesman.Tribe;
                Error ret = tribe.RemoveTribesman(playerId, true);
                if (ret != Error.Ok)
                {
                    return Enum.GetName(typeof(Error), ret);
                }
            }

            return "OK";
        }

        private string TribesmanUpdate(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string rank = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                        { "rank=", v => rank = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(rank))
                return "TribesmanUpdate --rank=rank --player=player_name";

            uint playerId;
            IPlayer player;
            if (!world.FindPlayerId(playerName, out playerId))
                return "Player not found";
            if (!world.Players.TryGetValue(playerId, out player))
                return "Player not found2";

            if (player.Tribesman == null) return "Player not in tribe";
            using (locker.Lock(player, player.Tribesman.Tribe))
            {
                player.Tribesman.Tribe.SetRank(playerId, byte.Parse(rank));
            }
            return "OK";
        }

        private string IncomingList(Session session, string[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
                return "TribesmanRemove --tribe=tribe_name";

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            StringBuilder result = new StringBuilder("Incomings:\n");
            using (locker.Lock(tribeId, out tribe))
            {
                foreach (var incoming in tribe.GetIncomingList())
                {
                    result.Append(string.Format("To [{0}-{1}] From[{2}] Arrival Time[{3}]\n", incoming.TargetCity.Name, incoming.TargetCity.Owner.Name, incoming.SourceCity.Name, incoming.EndTime));
                }
            }

            return result.ToString();
        }
    }
}