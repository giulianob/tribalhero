#region

using System;
using Game.Data;
using Game.Data.Tribe;
using Game.Database;
using Game.Map;
using Game.Module;
using Game.Module.Remover;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm
{
    class PlayerCommandLineModule : CommandLineModule
    {
        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("ban", BanPlayer, PlayerRights.Admin);
            processor.RegisterCommand("unban", UnbanPlayer, PlayerRights.Admin);
            processor.RegisterCommand("deleteplayer", DeletePlayer, PlayerRights.Bureaucrat);
            processor.RegisterCommand("clearplayerdescription", PlayerClearDescription, PlayerRights.Moderator);
            processor.RegisterCommand("deletenewbies", DeleteNewbies, PlayerRights.Bureaucrat);
            processor.RegisterCommand("broadcast", SystemBroadcast, PlayerRights.Bureaucrat);
            processor.RegisterCommand("broadcastmail", SystemBroadcastMail, PlayerRights.Bureaucrat);
            processor.RegisterCommand("setpassword", SetPassword, PlayerRights.Admin);
            processor.RegisterCommand("renameplayer", RenamePlayer, PlayerRights.Admin);
            processor.RegisterCommand("renametribe", RenameTribe, PlayerRights.Admin);
            processor.RegisterCommand("setrights", SetRights, PlayerRights.Bureaucrat);
            processor.RegisterCommand("muteplayer", Mute, PlayerRights.Moderator);
            processor.RegisterCommand("unmuteplayer", Unmute, PlayerRights.Moderator);
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
                                { "?|help|h", v => help = true }, 
                                { "player=", v => playerName = v.TrimMatchingQuotes() },
                                { "rights=", v => rights = (PlayerRights?)Enum.Parse(typeof(PlayerRights), v.TrimMatchingQuotes(), true) }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || !rights.HasValue)
                return String.Format("setrights --player=player --rights={0}", String.Join("|", Enum.GetNames(typeof(PlayerRights))));

            // Kick user out if they are logged in
            uint playerId;
            if (World.Current.FindPlayerId(playerName, out playerId))
            {

                IPlayer player;
                using (Concurrency.Current.Lock(playerId, out player))
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

        public string Mute(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                        {
                                { "?|help|h", v => help = true }, 
                                { "player=", v => playerName = v.TrimMatchingQuotes() },
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return String.Format("setrights --player=player");

            // Mute player in this world instantly
            uint playerId;
            if (World.Current.FindPlayerId(playerName, out playerId))
            {

                IPlayer player;
                using (Concurrency.Current.Lock(playerId, out player))
                {
                    if (player != null)
                    {
                        player.Muted = true;
                    }
                }
            }

            // Globally mute them
            ApiResponse response = ApiCaller.PlayerMute(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string Unmute(Session session, String[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet
                        {
                                { "?|help|h", v => help = true }, 
                                { "player=", v => playerName = v.TrimMatchingQuotes() },
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return String.Format("setrights --player=player");

            // Mute player in this world instantly
            uint playerId;
            if (World.Current.FindPlayerId(playerName, out playerId))
            {

                IPlayer player;
                using (Concurrency.Current.Lock(playerId, out player))
                {
                    if (player != null)
                    {
                        player.Muted = false;
                    }
                }
            }

            // Globally mute them
            ApiResponse response = ApiCaller.PlayerUnmute(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
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
                                { "?|help|h", v => help = true }, 
                                { "tribe=", v => tribeName = v.TrimMatchingQuotes() },
                                { "newname=", v => newTribeName = v.TrimMatchingQuotes() }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(tribeName) || string.IsNullOrEmpty(newTribeName))
                return "renametribe --tribe=name --newname=name";

            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            using (Concurrency.Current.Lock(tribeId, out tribe))
            {
                if (tribe == null)
                    return "Tribe not found";

                if (!Tribe.IsNameValid(newTribeName))
                    return "New tribe name is not allowed";

                if (World.Current.TribeNameTaken(newTribeName))
                    return "New tribe name is already taken";

                tribe.Name = newTribeName;
                DbPersistance.Current.Save(tribe);
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
                                {"subject=", v => subject = v.TrimMatchingQuotes()},
                                {"message=", v => message = v.TrimMatchingQuotes()},
                        };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(message) || string.IsNullOrEmpty(subject))
                return "broadcastmail --subject=\"SUBJECT\" --message=\"MESSAGE\"";

            using (var reader = DbPersistance.Current.ReaderQuery(string.Format("SELECT * FROM `{0}`", Player.DB_TABLE), new DbColumn[] {}))
            {
                while (reader.Read())
                {
                    IPlayer player;
                    using (Concurrency.Current.Lock((uint)reader["id"], out player))
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
                var p = new OptionSet {{"?|help|h", v => help = true}, {"message=", v => message = v.TrimMatchingQuotes()},};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(message))
                return "broadcast --message=\"MESSAGE\"";

            var packet = new Packet(Command.MessageBox);
            packet.AddString(message);

            Global.Channel.Post("/GLOBAL", packet);
            return "OK!";
        }

        public string PlayerClearDescription(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"player=", v => playerName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "playercleardescription --player=player";

            uint playerId;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            IPlayer player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";

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
                                { "?|help|h", v => help = true }, 
                                { "player=", v => playerName = v.TrimMatchingQuotes() },
                                { "newname=", v => newPlayerName = v.TrimMatchingQuotes() }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(newPlayerName))
                return "renameplayer --player=player --newname=name";

            uint playerId;
            var foundLocally = World.Current.FindPlayerId(playerName, out playerId);

            ApiResponse response = ApiCaller.RenamePlayer(playerName, newPlayerName);

            if (!response.Success)
                return response.ErrorMessage;

            if (!foundLocally)
            {
                return "Player not found on this server but renamed on main site";
            }

            IPlayer player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";

                if (player.Session != null)
                {
                    try
                    {
                        player.Session.CloseSession();
                    }
                    catch (Exception)
                    {
                    }
                }

                player.Name = newPlayerName;
                DbPersistance.Current.Save(player);
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
                                { "?|help|h", v => help = true }, 
                                { "player=", v => playerName = v.TrimMatchingQuotes() },
                                { "password=", v => password = v.TrimMatchingQuotes() }
                        };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password))
                return "setpassword --player=player --password=password";

            ApiResponse response = ApiCaller.SetPassword(playerName, password);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string BanPlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"player=", v => playerName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "ban --player=player";

            uint playerId;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            IPlayer player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";

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

            ApiResponse response = ApiCaller.Ban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string UnbanPlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"player=", v => playerName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "unban --player=player";

            ApiResponse response = ApiCaller.Unban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string DeletePlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"player=", v => playerName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "deleteplayer --player=player";

            uint playerId;
            if (!World.Current.FindPlayerId(playerName, out playerId))
                return "Player not found";

            IPlayer player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";

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
                    CityRemover cr = new CityRemover(city.Id);
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
                return "deletenewbies";

            PlayersRemover playersRemover = new PlayersRemover(new CityRemoverFactory(), new NewbieIdleSelector());

            return string.Format("OK! Deleting {0} players.", playersRemover.DeletePlayers());
        }
    }
}