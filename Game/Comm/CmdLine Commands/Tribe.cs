#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    partial class CmdLineProcessor
    {
        public string CmdTribeInfo(Session session, string[] parms)
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
                if (!Global.World.FindPlayerId(playerName, out playerId))
                    return "Player not found";
            }
            else
            {
                if (!Global.World.FindTribeId(tribeName, out playerId))
                    return "Tribe not found";               
            }

            Player player;
            string result;
            using (new MultiObjectLock(playerId, out player)) {
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

        public string CmdTribeCreate(Session session, string[] parms) {
            bool help = false;
            string playerName = string.Empty;
            string tribeName = string.Empty;
            string tribeDesc = string.Empty;

            try {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "player=", v => playerName = v.TrimMatchingQuotes() },
                        { "name=", v => tribeName = v.TrimMatchingQuotes()},
                        { "desc=", v => tribeDesc = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "TribeCreate --player=player [--name=tribe_name] [--desc=tribe_description]";

            
            uint playerId;
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
            using (new MultiObjectLock(playerId, out player))
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
                Global.DbManager.Save(tribe);

                Tribesman tribesman = new Tribesman(tribe, player, 0);
                tribe.AddTribesman(tribesman);
            }
            return "OK!";
        }

        public string CmdTribeDelete(Session session, string[] parms) {
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
            if (!Global.World.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Tribe tribe;
            if(!Global.Tribes.TryGetValue(tribeId, out tribe))
                return "Tribe not found seriously";

            using (new CallbackLock(custom => ((IEnumerable<Tribesman>)tribe).ToArray(), new object[] { }, tribe)) {
                foreach (var tribesman in new List<Tribesman>(tribe))
                {
                    tribe.RemoveTribesman(tribesman.Player.PlayerId);
                }
                Global.Tribes.Remove(tribe.Id);
                Global.DbManager.Delete(tribe);
            }
            return "OK!";
        }

        public string CmdTribeUpdate(Session session, string[] parms)
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
            if (!Global.World.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Tribe tribe;
            using (new MultiObjectLock(tribeId, out tribe)) {
                tribe.Description = desc;
                Global.DbManager.Save(tribe);
            }
            return "OK";
        }

        public string CmdTribesmanAdd(Session session, string[] parms) {
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
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!Global.World.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, Player> players;
            using (new MultiObjectLock( out players,playerId,tribeId))
            {
                Tribe tribe = players[tribeId].Tribesman.Tribe;
                Tribesman tribesman = new Tribesman(tribe, players[playerId],2);
                tribe.AddTribesman(tribesman);
            }
            return "OK";
        }

        public string CmdTribesmanRemove(Session session, string[] parms) {
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
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            uint tribeId;
            if (!Global.World.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Dictionary<uint, Player> players;
            using (new MultiObjectLock(out players, playerId, tribeId)) {
                Tribe tribe = players[tribeId].Tribesman.Tribe;
                Error ret;
                if((ret=tribe.RemoveTribesman(playerId))!=Error.Ok)
                {
                    return Enum.GetName(typeof(Error), ret);
                }
            } 
            return "OK";
        }

        public string CmdTribesmanUpdate(Session session, string[] parms) {
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
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";
            if (!Global.World.Players.TryGetValue(playerId, out player))
                return "Player not found2";

            if(player.Tribesman==null) return "Player not in tribe";
            using (new MultiObjectLock(player,player.Tribesman.Tribe)) {
                player.Tribesman.Tribe.SetRank(playerId, byte.Parse(rank));
            }
            return "OK";
        }

        public string CmdTribeIncomingList(Session session, string[] parms) {
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
            if (!Global.World.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            Tribe tribe;
            StringBuilder result = new StringBuilder("Incomings:\n");
            using (new MultiObjectLock(tribeId, out tribe)) {
                List<NotificationManager.Notification> notifications;
                //t.Where(x => x.Player.GetCityList().Where(y => y.Worker.Notifications.Where(z => z.Action is AttackChainAction && z.Subscriptions.Any(city => city == y))));
                foreach (var city in ((IEnumerable<Tribesman>)tribe).SelectMany(tribesman => tribesman.Player.GetCityList())) {
                    notifications = new List<NotificationManager.Notification>(city.Worker.Notifications.Where(x => x.Action is AttackChainAction && x.Subscriptions.Any(y => y == city)));
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