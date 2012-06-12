#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Tribe;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
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
    class TribeCommandLineModule : CommandLineModule
    {
        private readonly ITribeFactory tribeFactory;

        public TribeCommandLineModule(ITribeFactory tribeFactory)
        {
            this.tribeFactory = tribeFactory;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("TribeInfo", CmdTribeInfo, PlayerRights.Admin);
            processor.RegisterCommand("TribeCreate", CmdTribeCreate, PlayerRights.Admin);
            processor.RegisterCommand("TribeUpdate", CmdTribeUpdate, PlayerRights.Admin);
            processor.RegisterCommand("TribeDelete", CmdTribeDelete, PlayerRights.Admin);
            processor.RegisterCommand("TribesmanAdd", CmdTribesmanAdd, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribesmanRemove", CmdTribesmanRemove, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribesmanUpdate", CmdTribesmanUpdate, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribeIncomingList", CmdTribeIncomingList, PlayerRights.Bureaucrat);
            processor.RegisterCommand("TribeTransfer", CmdTribeTransfer, PlayerRights.Admin);
        }

        private string CmdTribeInfo(Session session, string[] parms)
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
                if (!World.Current.FindPlayerId(playerName, out playerId))
                    return "Player not found";
            }
            else
            {
                if (!World.Current.FindTribeId(tribeName, out playerId))
                    return "Tribe not found";
            }

            IPlayer player;
            string result;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";
                if (player.Tribesman == null)
                    return "Player does not own a tribe";

                ITribe tribe = player.Tribesman.Tribe;
                result = string.Format("Id[{0}] Owner[{1}] Lvl[{2}] Name[{3}] Desc[{4}] \n", tribe.Id, tribe.Owner.Name, tribe.Level, tribe.Name, tribe.Description);
                result += tribe.Resource.ToNiceString();
                result += string.Format("Member Count[{0}]\n", tribe.Count);
                foreach (var tribesman in tribe.Tribesmen)
                {
                    result += string.Format("Tribesman[{0}] CityCount[{1}] Rank[{2}] \n", tribesman.Player.Name, tribesman.Player.GetCityCount(), tribesman.Rank);
                }

            }

            return result;
        }

        private string CmdTribeCreate(Session session, string[] parms)
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
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            IPlayer player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player.Tribesman != null)
                {
                    return "Player already in tribe";
                }

                if (World.Current.TribeNameTaken(tribeName))
                {
                    return "Tribe name already taken";
                }

                if (!Tribe.IsNameValid(tribeName))
                {
                    return "Tribe name is not allowed";
                }

                ITribe tribe = tribeFactory.CreateTribe(player, tribeName);

                World.Current.Add(tribe);

                var tribesman = new Tribesman(tribe, player, 0);
                tribe.AddTribesman(tribesman);
            }
            return "OK!";
        }

        private string CmdTribeDelete(Session session, string[] parms)
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
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            if (!World.Current.TryGetObjects(tribeId, out tribe))
                return "Tribe not found";

            using (Concurrency.Current.Lock(custom => tribe.Tribesmen.ToArray(), new object[] { }, tribe))
            {
                foreach (var tribesman in new List<ITribesman>(tribe.Tribesmen))
                {
                    tribe.RemoveTribesman(tribesman.Player.PlayerId);
                }
                World.Current.Remove(tribe);
            }
            return "OK!";
        }

        private string CmdTribeTransfer(Session session, string[] parms)
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
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            if (!World.Current.TryGetObjects(tribeId, out tribe))
                return "Tribe not found";

            uint newOwnerPlayerId;
            IPlayer player;
            if (!World.Current.FindPlayerId(newOwner, out newOwnerPlayerId) || !World.Current.TryGetObjects(newOwnerPlayerId, out player))
            {
                return "New owner not found";
            }

            using (Concurrency.Current.Lock(custom => tribe.Tribesmen.ToArray(), new object[] { }, tribe, player))
            {
                var ret = tribe.Transfer(newOwnerPlayerId);

                if (ret != Error.Ok)
                {
                    return Enum.GetName(typeof(Error), ret);
                }
            }

            return "OK!";
        }

        private string CmdTribeUpdate(Session session, string[] parms)
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
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            using (Concurrency.Current.Lock(tribeId, out tribe))
            {
                tribe.Description = desc;
                DbPersistance.Current.Save(tribe);
            }
            return "OK";
        }

        private string CmdTribesmanAdd(Session session, string[] parms)
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
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, IPlayer> players;
            using (Concurrency.Current.Lock(out players, playerId, tribeId))
            {
                ITribe tribe = players[tribeId].Tribesman.Tribe;
                var tribesman = new Tribesman(tribe, players[playerId], 2);
                tribe.AddTribesman(tribesman);
            }
            return "OK";
        }

        private string CmdTribesmanRemove(Session session, string[] parms)
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
                return "TribesmanRemove --tribe=tribe_name --player=player_name";


            uint playerId;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, IPlayer> players;
            using (Concurrency.Current.Lock(out players, playerId, tribeId))
            {
                ITribe tribe = players[tribeId].Tribesman.Tribe;
                Error ret;
                if ((ret = tribe.RemoveTribesman(playerId)) != Error.Ok)
                {
                    return Enum.GetName(typeof(Error), ret);
                }
            }
            return "OK";
        }

        private string CmdTribesmanUpdate(Session session, string[] parms)
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
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";
            if (!World.Current.Players.TryGetValue(playerId, out player))
                return "Player not found2";

            if (player.Tribesman == null) return "Player not in tribe";
            using (Concurrency.Current.Lock(player, player.Tribesman.Tribe))
            {
                player.Tribesman.Tribe.SetRank(playerId, byte.Parse(rank));
            }
            return "OK";
        }

        private string CmdTribeIncomingList(Session session, string[] parms)
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
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            StringBuilder result = new StringBuilder("Incomings:\n");
            using (Concurrency.Current.Lock(tribeId, out tribe))
            {
                foreach (var city in tribe.Tribesmen.SelectMany(tribesman => tribesman.Player.GetCityList()))
                {
                    List<NotificationManager.Notification> notifications = new List<NotificationManager.Notification>(city.Worker.Notifications.Where(x => x.Action is AttackChainAction && x.Subscriptions.Any(y => y == city)));
                    foreach (AttackChainAction action in notifications.Select(notification => notification.Action).OfType<AttackChainAction>())
                    {
                        result.Append(string.Format("To [{0}-{1}] From[{2}] Arrival Time[{3}]\n", city.Owner.Name, city.Name, action.From, action.NextTime));
                    }
                }
            }

            return result.ToString();
        }
    }
}