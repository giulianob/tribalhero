#region

using System;
using Game.Data;
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
            processor.RegisterCommand("ban", BanPlayer, true);
            processor.RegisterCommand("unban", UnbanPlayer, true);
            processor.RegisterCommand("delete", DeletePlayer, true);
            processor.RegisterCommand("playercleardescription", PlayerClearDescription, true);
            processor.RegisterCommand("deleteinactives", DeleteInactives, true);
            processor.RegisterCommand("broadcast", SystemBroadcast, true);
            processor.RegisterCommand("broadcastmail", SystemBroadcastMail, true);
        }

        private string SystemBroadcastMail(Session session, String[] parms)
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

            using (var reader = Ioc.Kernel.Get<IDbManager>().ReaderQuery(string.Format("SELECT * FROM `{0}`", Player.DB_TABLE), new DbColumn[] {}))
            {
                while (reader.Read())
                {
                    Player player;
                    using (Concurrency.Current.Lock((uint)reader["id"], out player))
                    {
                        player.SendSystemMessage(null, subject, message);
                    }
                }
            }
            return "OK!";
        }

        private string SystemBroadcast(Session session, String[] parms)
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

        private string PlayerClearDescription(Session session, string[] parms)
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
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
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

        private string BanPlayer(Session session, string[] parms)
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

            ApiResponse response = ApiCaller.Ban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        private string UnbanPlayer(Session session, string[] parms)
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

        private string DeletePlayer(Session session, string[] parms)
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
                return "delete --player=player";

            uint playerId;
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
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

                foreach (City city in player.GetCityList())
                {
                    CityRemover cr = new CityRemover(city.Id);
                    cr.Start();
                }
            }

            return "OK!";

        }

        private string DeleteInactives(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

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
                return "DeleteInactives";
            PlayersRemover playersRemover = new PlayersRemover(new CityRemoverFactory(), new NewbieIdleSelector());
            return string.Format("Deleted {0} Players", playersRemover.DeletePlayers());
        }
    }
}