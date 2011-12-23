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
        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("TribeInfo", CmdTribeInfo, true);
            processor.RegisterCommand("TribeCreate", CmdTribeCreate, true);
            processor.RegisterCommand("TribeUpdate", CmdTribeUpdate, true);
            processor.RegisterCommand("TribeDelete", CmdTribeDelete, true);
            processor.RegisterCommand("TribesmanAdd", CmdTribesmanAdd, true);
            processor.RegisterCommand("TribesmanRemove", CmdTribesmanRemove, true);
            processor.RegisterCommand("TribesmanUpdate", CmdTribesmanUpdate, true);
            processor.RegisterCommand("TribeIncomingList", CmdTribeIncomingList, true);
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
            catch(Exception)
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

            Player player;
            string result;
            using (Concurrency.Current.Lock(playerId, out player)) {
                if (player == null)
                    return "Player not found";
                if (player.Tribesman == null)
                    return "Player does not own a tribe";

                Tribe tribe = player.Tribesman.Tribe;
                result = string.Format("Id[{0}] Owner[{1}] Lvl[{2}] Name[{3}] Desc[{4}] \n",tribe.Id,tribe.Owner.Name,tribe.Level,tribe.Name,tribe.Description);
                result += tribe.Resource.ToNiceString();
                result += string.Format("Member Count[{0}]\n", tribe.Count);
                foreach (var tribesman in tribe) {
                    result += string.Format("Tribesman[{0}] CityCount[{1}] Rank[{2}] \n", tribesman.Player.Name, tribesman.Player.GetCityCount(),tribesman.Rank);
                }
                
            }

            return result;
        }

        private string CmdTribeCreate(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                        { "name=", v => tribeName = v.TrimMatchingQuotes()},
                        { "desc=", v => v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "TribeCreate --player=player [--name=tribe_name] [--desc=tribe_description]";

            
            uint playerId;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player.Tribesman != null) {
                    return Enum.GetName(typeof(Error), Error.TribesmanAlreadyInTribe);
                }

                if (Global.Tribes.Any(x => x.Value.Name.Equals(tribeName))) {
                    return Enum.GetName(typeof(Error), Error.TribesmanAlreadyExists);
                }

                if (Global.Tribes.ContainsKey(player.PlayerId)) return "Tribe already exists!";

                Tribe tribe = new Tribe(player, tribeName);
                
                Global.Tribes.Add(tribe.Id, tribe);
                DbPersistance.Current.Save(tribe);

                Tribesman tribesman = new Tribesman(tribe, player, 0);
                tribe.AddTribesman(tribesman);
            }
            return "OK!";
        }

        private string CmdTribeDelete(Session session, string[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
                return "TribeDelete --name=tribe_name";


            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Tribe tribe;
            if(!Global.Tribes.TryGetValue(tribeId, out tribe))
                return "Tribe not found seriously";

            using (Concurrency.Current.Lock(custom => ((IEnumerable<Tribesman>)tribe).ToArray(), new object[] { }, tribe)) {
                foreach (var tribesman in new List<Tribesman>(tribe))
                {
                    tribe.RemoveTribesman(tribesman.Player.PlayerId);
                }
                Global.Tribes.Remove(tribe.Id);
                DbPersistance.Current.Delete(tribe);
            }
            return "OK!";
        }

        private string CmdTribeUpdate(Session session, string[] parms)
        {
            bool help = false;
            string desc = string.Empty;
            string tribeName = string.Empty;

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        { "desc=", v => desc = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName) || string.IsNullOrEmpty(desc))
                return "TribesmanAdd --tribe=tribe_name --desc=desc";

            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Tribe tribe;
            using (Concurrency.Current.Lock(tribeId, out tribe)) {
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

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName)|| string.IsNullOrEmpty(tribeName))
                return "TribesmanAdd --tribe=tribe_name --player=player_name";


            uint playerId;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, Player> players;
            using (Concurrency.Current.Lock( out players,playerId,tribeId))
            {
                Tribe tribe = players[tribeId].Tribesman.Tribe;
                Tribesman tribesman = new Tribesman(tribe, players[playerId],2);
                tribe.AddTribesman(tribesman);
            }
            return "OK";
        }

        private string CmdTribesmanRemove(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            } catch (Exception) {
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

            Dictionary<uint, Player> players;
            using (Concurrency.Current.Lock(out players, playerId, tribeId)) {
                Tribe tribe = players[tribeId].Tribesman.Tribe;
                Error ret;
                if((ret=tribe.RemoveTribesman(playerId))!=Error.Ok)
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

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                        { "rank=", v => rank = v.TrimMatchingQuotes() },
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(rank))
                return "TribesmanUpdate --rank=rank --player=player_name";

            uint playerId;
            Player player;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";
            if (!World.Current.Players.TryGetValue(playerId, out player))
                return "Player not found2";

            if(player.Tribesman==null) return "Player not in tribe";
            using (Concurrency.Current.Lock(player,player.Tribesman.Tribe)) {
                player.Tribesman.Tribe.SetRank(playerId, byte.Parse(rank));
            }
            return "OK";
        }

        private string CmdTribeIncomingList(Session session, string[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName))
                return "TribesmanRemove --tribe=tribe_name";

            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Tribe tribe;
            StringBuilder result = new StringBuilder("Incomings:\n");
            using (Concurrency.Current.Lock(tribeId, out tribe)) {
                //t.Where(x => x.Player.GetCityList().Where(y => y.Worker.Notifications.Where(z => z.Action is AttackChainAction && z.Subscriptions.Any(city => city == y))));
                foreach (var city in ((IEnumerable<Tribesman>)tribe).SelectMany(tribesman => tribesman.Player.GetCityList()))
                {
                    List<NotificationManager.Notification> notifications = new List<NotificationManager.Notification>(city.Worker.Notifications.Where(x => x.Action is AttackChainAction && x.Subscriptions.Any(y => y == city)));
                    foreach(var notification in notifications)
                    {
                        AttackChainAction action = notification.Action as AttackChainAction;
                        if (action == null) continue;
                        result.Append(string.Format("To [{0}-{1}] From[{2}] Arrival Time[{3}]\n", city.Owner.Name, city.Name, action.From, action.NextTime));
                    }
                }
            }

            return result.ToString();
        }
    }
}