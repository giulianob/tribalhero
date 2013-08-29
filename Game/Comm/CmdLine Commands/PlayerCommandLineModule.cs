#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Tribe;
using Game.Map;
using Game.Module;
using Game.Module.Remover;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Persistance;

#endregion

namespace Game.Comm
{
    class PlayerCommandLineModule : CommandLineModule
    {
        private readonly Chat chat;

        private readonly ICityRemoverFactory cityRemoverFactory;

        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly IStructureCsvFactory structureFactory;

        private readonly TechnologyFactory technologyFactory;

        private readonly IPlayersRemoverFactory playerRemoverFactory;

        private readonly IPlayerSelectorFactory playerSelectorFactory;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        public PlayerCommandLineModule(IPlayersRemoverFactory playerRemoverFactory,
                                       IPlayerSelectorFactory playerSelectorFactory,
                                       ICityRemoverFactory cityRemoverFactory,
                                       Chat chat,
                                       IDbManager dbManager,
                                       ITribeManager tribeManager,
                                       IWorld world,
                                       ILocker locker,
                                       IStructureCsvFactory structureFactory,
                                       TechnologyFactory technologyFactory)
        {
            this.playerRemoverFactory = playerRemoverFactory;
            this.playerSelectorFactory = playerSelectorFactory;
            this.cityRemoverFactory = cityRemoverFactory;
            this.chat = chat;
            this.dbManager = dbManager;
            this.tribeManager = tribeManager;
            this.world = world;
            this.locker = locker;
            this.structureFactory = structureFactory;
            this.technologyFactory = technologyFactory;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("playerinfo", Info, PlayerRights.Moderator);
            processor.RegisterCommand("playersearch", Search, PlayerRights.Moderator);
            processor.RegisterCommand("ban", BanPlayer, PlayerRights.Moderator);
            processor.RegisterCommand("unban", UnbanPlayer, PlayerRights.Moderator);
            processor.RegisterCommand("deleteplayer", DeletePlayer, PlayerRights.Bureaucrat);
            processor.RegisterCommand("clearplayerdescription", PlayerClearDescription, PlayerRights.Moderator);
            processor.RegisterCommand("deletenewbies", DeleteNewbies, PlayerRights.Bureaucrat);
            processor.RegisterCommand("broadcast", SystemBroadcast, PlayerRights.Bureaucrat);
            processor.RegisterCommand("systemchat", SystemChat, PlayerRights.Bureaucrat);
            processor.RegisterCommand("broadcastmail", SystemBroadcastMail, PlayerRights.Bureaucrat);
            processor.RegisterCommand("setpassword", SetPassword, PlayerRights.Admin);
            processor.RegisterCommand("renameplayer", RenamePlayer, PlayerRights.Admin);
            processor.RegisterCommand("renametribe", RenameTribe, PlayerRights.Admin);
            processor.RegisterCommand("setrights", SetRights, PlayerRights.Bureaucrat);
            processor.RegisterCommand("mute", Mute, PlayerRights.Moderator);
            processor.RegisterCommand("unmute", Unmute, PlayerRights.Moderator);
            processor.RegisterCommand("togglechatmod", ToggleChatMod, PlayerRights.Moderator);
            processor.RegisterCommand("warn", Warn, PlayerRights.Moderator);
            processor.RegisterCommand("setchatlevel", SetChatLevel, PlayerRights.Admin);
            processor.RegisterCommand("giveachievement", GiveAchievement, PlayerRights.Admin);
            processor.RegisterCommand("calculateexpensivecities", CalculateExpensiveCities, PlayerRights.Bureaucrat);
        }

        public string CalculateExpensiveCities(Session session, string[] parms)
        {
            bool help = false;
            string serverTitle = string.Empty;
            try
            {
                var p = new OptionSet
                {
                    {"?|help|h", v => help = true},
                    {"servertitle=", v => serverTitle = v.TrimMatchingQuotes() }
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(serverTitle))
            {
                return "calculateexpensivecities --servertitle=servertitle";
            }

            var values = new List<dynamic>();

            Func<Resource, decimal> calculateCost = resource => (resource.Crop + resource.Wood + resource.Gold * 2 + resource.Iron * 5 + resource.Labor * 100) / 100m;
            
            foreach (var city in world.Cities.AllCities())
            {
                decimal expenses = 0m;

                foreach (var structure in city)
                {
                    for (var lvl = 0; lvl <= structure.Lvl; lvl++)
                    {
                        expenses += calculateCost(structureFactory.GetCost(structure.Type, lvl));
                    }
                    
                    foreach (var technology in structure.Technologies)
                    {
                        for (var lvl = 0; lvl <= technology.Level; lvl++)
                        {
                            expenses += calculateCost(technologyFactory.GetTechnology(technology.Type, (byte)lvl).TechBase.Resources);
                        }
                    }
                }

                values.Add(new {City = city, Expenses = expenses});
            }

            var result = new StringBuilder();
            result.AppendLine("Id,Player,Tribe,Rank,City Cost,Achievement,Tier,Title,Description");
            int rank = 1;
            foreach (var value in values.OrderByDescending(v => v.Expenses).Take(50))
            {
                IPlayer player = value.City.Owner;
                var tribeName = player.IsInTribe ? player.Tribesman.Tribe.Name : string.Empty;
                string achievement;
                int tier;
                switch(rank)
                {
                    case 1:
                        achievement = "Gold";
                        tier = 0;
                        break;
                    case 2:
                        achievement = "Silver";
                        tier = 1;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        achievement = "Bronze";
                        tier = 2;
                        break;
                    default:
                        achievement = "Honorary";
                        tier = 3;
                        break;
                }

                string title = string.Format("#{0} Most Expensive City", rank);
                string description = string.Format("{0}: {1}", serverTitle, value.City.Name);

                result.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}", player.PlayerId, player.Name, tribeName, rank, (decimal)value.Expenses * 100m, achievement, tier, title, description);
                result.AppendLine();

                rank++;
            }

            return result.ToString();
        }

        public string SetChatLevel(Session session, string[] parms)
        {
            PlayerRights? rights = null;
            var help = false;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},                        
                        {
                                "rights=",
                                v =>
                                rights = (PlayerRights?)Enum.Parse(typeof(PlayerRights), v.TrimMatchingQuotes(), true)
                        }
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || rights == null || !rights.HasValue)
            {
                return String.Format("setchatlevel --rights={0}", String.Join("|", Enum.GetNames(typeof(PlayerRights))));
            }

            Config.chat_min_level = rights.Value;

            return "OK";
        }

        private string ToggleChatMod(Session session, string[] parms)
        {
            session.Player.ChatState.Distinguish = !session.Player.ChatState.Distinguish;

            return string.Format("OK, highlighting is now {0}", session.Player.ChatState.Distinguish ? "on" : "off");
        }

        private string SystemChat(Session session, string[] parms)
        {
            bool help = false;
            string message = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"m|message=", v => message = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(message))
            {
                return "systemchat --message=\"MESSAGE\"";
            }

            chat.SendSystemChat("SYSTEM_CHAT_LITERAL", message);

            return "OK!";
        }

        public string SetRights(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            PlayerRights? rights = null;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()},
                        {
                                "rights=",
                                v =>
                                rights = (PlayerRights?)Enum.Parse(typeof(PlayerRights), v.TrimMatchingQuotes(), true)
                        }
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || !rights.HasValue)
            {
                return String.Format("setrights --player=player --rights={0}",
                                     String.Join("|", Enum.GetNames(typeof(PlayerRights))));
            }

            // Kick user out if they are logged in
            uint playerId;
            if (world.FindPlayerId(playerName, out playerId))
            {
                IPlayer player;
                using (locker.Lock(playerId, out player))
                {
                    if (player != null && player.Session != null)
                    {
                        try
                        {
                            player.Session.CloseSession();
                        }
                        catch
                        {
                        }
                    }
                }
            }

            ApiResponse response = ApiCaller.SetPlayerRights(playerName, rights.GetValueOrDefault());

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string Info(Session session, String[] parms)
        {
            bool help = false;
            string playerName = null;

            try
            {
                
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || (string.IsNullOrEmpty(playerName)))
            {
                return String.Format("playerinfo --player=name|emailaddress");
            }

            ApiResponse response = ApiCaller.PlayerInfo(playerName);

            if (!response.Success)
            {
                return response.ErrorMessage;
            }

            try
            {
                return
                        String.Format(
                                      "id[{0}] created[{1}] name[{7}] emailAddress[{2}] lastLogin[{3}] ipAddress[{4}] banned[{5}] deleted[{6}]",
                                      response.Data.id,
                                      response.Data.created,
                                      session.Player.Rights > PlayerRights.Moderator ? response.Data.emailAddress : "N/A",
                                      session.Player.Rights > PlayerRights.Moderator ? response.Data.lastLogin : "N/A",
                                      session.Player.Rights > PlayerRights.Moderator ? response.Data.ipAddress : "N/A",
                                      response.Data.banned == "1" ? "YES" : "NO",                                      
                                      response.Data.deleted == "1" ? "YES" : "NO",
                                      response.Data.name);
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        public string Search(Session session, String[] parms)
        {
            bool help = false;
            string playerName = null;
            
            try
            {
                
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"o|player=", v => playerName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || (string.IsNullOrEmpty(playerName)))
            {
                return String.Format("playersearch --player=name|emailaddress");
            }

            ApiResponse response = ApiCaller.PlayerSearch(playerName);

            if (!response.Success)
            {
                return response.ErrorMessage;
            }

            try
            {
                return String.Join("\n", response.Data.players);
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }


        public string Mute(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            int minutes = 10;
            string reason = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()},
                        {"m=|minutes=", v => minutes = int.Parse(v.TrimMatchingQuotes()) },
                        {"r=|reason=", v => reason = v.TrimMatchingQuotes() },
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || minutes <= 0 || string.IsNullOrEmpty(reason))
            {
                return String.Format("mute --player=player --reason=\"Reason for ban\" --minutes=##");
            }

            uint playerId;
            if (world.FindPlayerId(playerName, out playerId))
            {
                IPlayer player;
                using (locker.Lock(playerId, out player))
                {
                    if (player != null)
                    {
                        player.Muted = SystemClock.Now.AddMinutes(minutes);
                        dbManager.Save(player);

                        player.SendSystemMessage(null,
                                                 "You have been temporarily muted",
                                                 string.Format("You have been temporarily muted for {0} minutes. Reason: {1}\n\n", minutes, reason) + 
                                                 "Please make sure you are following all of the game rules ( http://tribalhero.com/pages/rules ). " +
                                                 "If you have reason to believe this was an unfair judgement, you may contact the game admin directly by email at giuliano@tribalhero.com. Provide as much detail as possible and give us 24 hours to investigate and respond.");

                        return string.Format("OK player notified and muted for {0} minutes (until {1})", minutes, player.Muted.ToString("R"));
                    }                    
                }
            }

            return "Player not found";
        }

        public string Warn(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string reason = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()},
                        {"r=|reason=", v => reason = v.TrimMatchingQuotes() },
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(reason))
            {
                return String.Format("warn --player=player --reason=\"Reason for warning\"");
            }

            uint playerId;
            if (world.FindPlayerId(playerName, out playerId))
            {
                IPlayer player;
                using (locker.Lock(playerId, out player))
                {
                    if (player != null)
                    {
                        var warnMessage = string.Format("You have been warned for misconduct. Reason: {0}\n", reason) +
                                          "Please make sure you are following all of the game rules ( http://tribalhero.com/pages/rules ). If your behavior continues, you may be muted or banned. " +
                                          "If you have reason to believe this was an unfair judgement, you may contact the game admin directly by email at giuliano@tribalhero.com. Provide as much detail as possible and give us 24 hours to investigate and respond.";

                        player.SendSystemMessage(null,
                                                 "You have been warned for misconduct",
                                                 warnMessage);

                        if (player.Session != null)
                        {
                            chat.SendSystemChat(player.Session, "SYSTEM_CHAT_LITERAL", warnMessage);
                        }

                        return string.Format("OK player has been warned.");
                    }                    
                }
            }

            return "Player not found";
        }

        public string Unmute(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
            {
                return String.Format("unmute --player=player");
            }

            // Mute player in this world instantly
            uint playerId;
            if (world.FindPlayerId(playerName, out playerId))
            {
                IPlayer player;
                using (locker.Lock(playerId, out player))
                {
                    if (player != null)
                    {
                        player.Muted = DateTime.MinValue;                  
                        dbManager.Save(player);

                        player.SendSystemMessage(null, "You have been unmuted", "You have now been unmuted by a moderator and may talk again in the chat.");

                        return "OK!";
                    }
                }
            }

            return "Player not found";
        }

        public string RenameTribe(Session session, String[] parms)
        {
            bool help = false;
            string tribeName = string.Empty;
            string newTribeName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"tribe=", v => tribeName = v.TrimMatchingQuotes()},
                        {"newname=", v => newTribeName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName) || string.IsNullOrEmpty(newTribeName))
            {
                return "renametribe --tribe=name --newname=name";
            }

            uint tribeId;
            if (!tribeManager.FindTribeId(tribeName, out tribeId))
            {
                return "Tribe not found";
            }

            ITribe tribe;
            using (locker.Lock(tribeId, out tribe))
            {
                if (tribe == null)
                {
                    return "Tribe not found";
                }

                if (!Tribe.IsNameValid(newTribeName))
                {
                    return "New tribe name is not allowed";
                }

                if (tribeManager.TribeNameTaken(newTribeName))
                {
                    return "New tribe name is already taken";
                }

                tribe.Name = newTribeName;
                dbManager.Save(tribe);
            }

            return "OK!";
        }

        public string SystemBroadcastMail(Session session, String[] parms)
        {
            bool help = false;
            string message = string.Empty;
            string subject = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"s|subject=", v => subject = v.TrimMatchingQuotes()},
                        {"m|message=", v => message = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(message) || string.IsNullOrEmpty(subject))
            {
                return "broadcastmail --subject=\"SUBJECT\" --message=\"MESSAGE\"";
            }

            using (
                    var reader = dbManager.ReaderQuery(string.Format("SELECT * FROM `{0}`", Player.DB_TABLE),
                                                       new DbColumn[] {}))
            {
                while (reader.Read())
                {
                    IPlayer player;
                    using (locker.Lock((uint)reader["id"], out player))
                    {
                        player.SendSystemMessage(null, subject, message);
                    }
                }
            }
            return "OK!";
        }

        public string SystemBroadcast(Session session, String[] parms)
        {
            bool help = false;
            string message = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"m|message=", v => message = v.TrimMatchingQuotes()},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(message))
            {
                return "broadcast --message=\"MESSAGE\"";
            }

            var packet = new Packet(Command.MessageBox);
            packet.AddString(message);

            Global.Current.Channel.Post("/GLOBAL", packet);
            return "OK!";
        }

        public string PlayerClearDescription(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
            {
                return "playercleardescription --player=player";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                if (player == null)
                {
                    return "Player not found";
                }

                player.Description = string.Empty;
                player.SendSystemMessage(null,
                                         "Description Clear",
                                         "An administrator has cleared your profile description. If your description was offensive then you may be banned in the future if an innapropriate description is found.");
            }
            return "OK!";
        }

        public string RenamePlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string newPlayerName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                        {"newname=", v => newPlayerName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(newPlayerName))
            {
                return "renameplayer --player=player --newname=name";
            }

            uint playerId;
            var foundLocally = world.FindPlayerId(playerName, out playerId);

            ApiResponse response = ApiCaller.RenamePlayer(playerName, newPlayerName);

            if (!response.Success)
            {
                return response.ErrorMessage;
            }

            if (!foundLocally)
            {
                return "Player not found on this server but renamed on main site";
            }

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                if (player == null)
                {
                    return "Player not found";
                }

                if (player.Session != null)
                {
                    try
                    {
                        player.Session.CloseSession();
                    }
                    catch(Exception)
                    {
                    }
                }

                player.Name = newPlayerName;
                dbManager.Save(player);
            }

            return "OK!";
        }

        public string SetPassword(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string password = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"player=", v => playerName = v.TrimMatchingQuotes()},
                        {"password=", v => password = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password))
            {
                return "setpassword --player=player --password=password";
            }

            ApiResponse response = ApiCaller.SetPassword(playerName, password);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string BanPlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
            {
                return "ban --player=player";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                if (player != null)
                {
                    player.Banned = true;
                    dbManager.Save(player);

                    if (player.Session != null)
                    {
                        try
                        {
                            player.Session.CloseSession();
                        }
                        catch(Exception)
                        {
                        }
                    }
                }
            }

            ApiResponse response = ApiCaller.Ban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string UnbanPlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
            {
                return "unban --player=player";
            }            
            
            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                if (player != null)
                {
                    player.Banned = false;
                    dbManager.Save(player);
                }
            }

            ApiResponse response = ApiCaller.Unban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string DeletePlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()}
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
            {
                return "deleteplayer --player=player";
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                return "Player not found";
            }

            IPlayer player;
            using (locker.Lock(playerId, out player))
            {
                if (player == null)
                {
                    return "Player not found";
                }

                if (player.Session != null)
                {
                    try
                    {
                        player.Session.CloseSession();
                    }
                    catch(Exception)
                    {
                    }
                }

                foreach (ICity city in player.GetCityList())
                {
                    CityRemover cr = cityRemoverFactory.CreateCityRemover(city.Id);
                    cr.Start();
                }
            }

            return "OK!";
        }

        public string DeleteNewbies(Session session, string[] parms)
        {
            bool help = false;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}};

                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help)
            {
                return "deletenewbies";
            }

            PlayersRemover playersRemover =
                    playerRemoverFactory.CreatePlayersRemover(playerSelectorFactory.CreateNewbieIdleSelector());

            return string.Format("OK! Deleting {0} players.", playersRemover.DeletePlayers());
        }

        public string GiveAchievement(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;
            string icon = string.Empty;
            string title = string.Empty;
            string type = string.Empty;
            string description = string.Empty;
            AchievementTier? tier = null;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"p=|player=", v => playerName = v.TrimMatchingQuotes()},
                        {"title=", v => title = v.TrimMatchingQuotes() },
                        {"description=", v => description = v.TrimMatchingQuotes() },
                        {"icon=", v => icon = v.TrimMatchingQuotes() },
                        {"type=", v => type = v.TrimMatchingQuotes() },
                        {"tier=", v => tier = EnumExtension.Parse<AchievementTier>(v.TrimMatchingQuotes()) },
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || 
                string.IsNullOrEmpty(playerName) || 
                string.IsNullOrEmpty(icon) || 
                string.IsNullOrEmpty(title) || 
                string.IsNullOrEmpty(description) || 
                string.IsNullOrEmpty(type) ||
                tier == null ||
                !tier.HasValue)
            {
                return String.Format("giveachievement --player=player --type=type --tier={0} --icon=icon --title=title --description=description",
                                     String.Join("|", Enum.GetNames(typeof(AchievementTier))));
            }

            ApiResponse response = ApiCaller.GiveAchievement(playerName, tier.Value, type, icon, title, description);

            return response.Success ? "OK!" : response.ErrorMessage;
        }
    }
}