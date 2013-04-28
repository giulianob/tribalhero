#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Logic.Procedures;
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
        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly IStrongholdManager strongholdManager;
        private readonly Procedure procedure;

        private readonly ITribeFactory tribeFactory;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        public TribeCommandLineModule(ITribeFactory tribeFactory,
                                      IDbManager dbManager,
                                      ILocker locker,
                                      ITribeManager tribeManager,
                                      IWorld world,
                                      IStrongholdManager strongholdManager,
                                      Procedure procedure)
        {
            this.tribeFactory = tribeFactory;
            this.dbManager = dbManager;
            this.locker = locker;
            this.tribeManager = tribeManager;
            this.world = world;
            this.strongholdManager = strongholdManager;
            this.procedure = procedure;
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
            processor.RegisterCommand("TribeRankUpdate", RankUpdate, PlayerRights.Admin);
        }

        private string RankUpdate(Session session, string[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;
            string rank = string.Empty;
            string name = string.Empty;
            string permission = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        {"rank=", v => rank = v.TrimMatchingQuotes()},
                        {"name=", v => name = v.TrimMatchingQuotes()},
                        {"permission=", v => permission = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName) || string.IsNullOrEmpty(rank))
            {
                var str = Enum.GetNames(typeof(TribePermission)).Aggregate((s, s1) => s + "," + s1);
                return "TribeRankUpdate --rank=rank_id --tribe=tribe_name [--name=rank_name] [--Permission=Permission("+str+")]";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }
            ITribe tribe;
            using (locker.Lock(tribeId, out tribe))
            {
                ITribeRank tribeRank = tribe.Ranks.First(x=>x.Id==byte.Parse(rank));
                if(tribeRank==null) return "Rank not found";
                tribe.UpdateRank(tribeRank.Id,
                                 name == string.Empty ? tribeRank.Name : name,
                                 permission == string.Empty ? tribeRank.Permission : (TribePermission)Enum.Parse(typeof(TribePermission), permission, true));

            }
            return "OK";
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
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) && string.IsNullOrEmpty(tribeName))
            {
                return "TribeInfo --player=player_name|--tribe=tribe_name";
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
            string result;
            using (locker.Lock(playerId, out player))
            {
                if (player == null)
                {
                    return "Player not found";
                }
                if (player.Tribesman == null)
                {
                    return "Player does not own a tribe";
                }

                ITribe tribe = player.Tribesman.Tribe;
                result = string.Format("Id[{0}] Owner[{1}] Lvl[{2}] Name[{3}] Desc[{4}] PublicDesc[{5}]\n",
                                       tribe.Id,
                                       tribe.Owner.Name,
                                       tribe.Level,
                                       tribe.Name,
                                       tribe.Description,
                                       tribe.PublicDescription);
                result += tribe.Resource.ToNiceString();
                result += string.Format("Member Count[{0}]\n", tribe.Count);
                result = tribe.Tribesmen.Aggregate(result,
                                                   (current, tribesman) =>
                                                   current +
                                                   string.Format("Tribesman[{0}] CityCount[{1}] Rank[{2}] \n",
                                                                 tribesman.Player.Name,
                                                                 tribesman.Player.GetCityCount(),
                                                                 tribesman.Rank));
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
                        {"?|help|h", v => help = true},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                        {"name=", v => tribeName = v.TrimMatchingQuotes()},
                        {"desc=", v => v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
            {
                return "TribeCreate --player=player [--name=tribe_name] [--desc=tribe_description]";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                ITribe tribe;
                Error error = procedure.CreateTribe(player, tribeName, out tribe);
                if(error!=Error.Ok) return error.ToString();
                tribe.SendRanksUpdate();
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
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
            {
                return "TribeDelete --name=tribe_name";
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

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var locks =
                            strongholdManager.StrongholdsForTribe(tribe)
                                             .SelectMany(stronghold => stronghold.LockList)
                                             .ToList();

                    locks.AddRange(tribe.Tribesmen);

                    return locks.ToArray();
                };

            using (locker.Lock(lockHandler, new object[] {}, tribe))
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
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        {"newowner=", v => newOwner = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
            {
                return "TribeTransfer --tribe=tribe_name --newowner=player_name";
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

            uint newOwnerPlayerId;
            IPlayer player;
            if (!world.FindPlayerId(newOwner, out newOwnerPlayerId) ||
                !world.TryGetObjects(newOwnerPlayerId, out player))
            {
                return "New owner not found";
            }

            using (locker.Lock(custom => tribe.Tribesmen.ToArray<ILockable>(), new object[] {}, tribe, player))
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
            string publicDesc = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        {"desc=", v => desc = v.TrimMatchingQuotes()},
                        {"publicdesc=", v => publicDesc = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName) || string.IsNullOrEmpty(desc))
            {
                return "TribeUpdate --tribe=tribe_name --desc=desc --publicdesc=publicdesc";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }

            ITribe tribe;
            using (locker.Lock(tribeId, out tribe))
            {
                tribe.Description = desc;
                tribe.PublicDescription = publicDesc;
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
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(tribeName))
            {
                return "TribesmanAdd --tribe=tribe_name --player=player_name";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }

            Dictionary<uint, IPlayer> players;
            using (locker.Lock(out players, playerId, tribeId))
            {
                ITribe tribe = players[tribeId].Tribesman.Tribe;
                var tribesman = new Tribesman(tribe, players[playerId], tribe.DefaultRank);
                tribe.AddTribesman(tribesman, true);
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
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(tribeName))
            {
                return "TribesmanRemove --tribe=tribe_name --player=player_name";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }

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
                        {"?|help|h", v => help = true},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                        {"rank=", v => rank = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(rank))
            {
                return "TribesmanUpdate --rank=rank --player=player_name";
            }

            uint playerId;
            IPlayer player;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }
            if (!world.Players.TryGetValue(playerId, out player))
            {
                return "Player not found2";
            }

            if (player.Tribesman == null)
            {
                return "Player not in tribe";
            }
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
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
            {
                return "TribesmanRemove --tribe=tribe_name";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }

            ITribe tribe;
            StringBuilder result = new StringBuilder("Incomings:\n");
            using (locker.Lock(tribeId, out tribe))
            {
                foreach (var incoming in tribeManager.GetIncomingList(tribe))
                {
                    result.Append(string.Format("To [{0}-{1}] From[{2}] Arrival Time[{3}]\n",
                                                incoming.Target.LocationType.ToString(),
                                                incoming.Target.LocationId,
                                                incoming.Source.LocationId,
                                                incoming.EndTime));
                }
            }

            return result.ToString();
        }
    }
}