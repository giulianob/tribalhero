#region

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class LoginCommandsModule : CommandModule
    {
        private readonly IActionFactory actionFactory;

        private readonly object loginLock = new object();

        private readonly ITribeManager tribeManager;

        public LoginCommandsModule(IActionFactory actionFactory, ITribeManager tribeManager)
        {
            this.actionFactory = actionFactory;
            this.tribeManager = tribeManager;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.Login, Login);
            processor.RegisterCommand(Command.QueryXml, QueryXml);
            processor.RegisterCommand(Command.CityCreateInitial, CreateInitialCity);
        }

        private void QueryXml(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            reply.AddString(File.ReadAllText(Path.Combine(Config.data_folder, "data.xml")));
            session.Write(reply);
        }

        private void Login(Session session, Packet packet)
        {
            IPlayer player;

            short clientVersion;
            short clientRevision;
            byte loginMode;
            string loginKey = string.Empty;
            string playerName;
            string playerPassword = string.Empty;
            uint playerId;
            bool banned = false;
            
            PlayerRights playerRights = PlayerRights.Basic;

            try
            {
                clientVersion = packet.GetInt16();
                clientRevision = packet.GetInt16();
                loginMode = packet.GetByte();
                playerName = packet.GetString();
                if (loginMode == 0)
                {
                    loginKey = packet.GetString();
                }
                else
                {
                    playerPassword = packet.GetString();
                }
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                session.CloseSession();
                return;
            }

            if (clientVersion <= Config.client_min_version && clientRevision < Config.client_min_revision)
            {
                ReplyError(session, packet, Error.ClientOldVersion);
                session.CloseSession();
                return;
            }

            if (Config.database_load_players)
            {
                ApiResponse response;
                try
                {
                    response = loginMode == 0
                                       ? ApiCaller.CheckLoginKey(playerName, loginKey)
                                       : ApiCaller.CheckLogin(playerName, playerPassword);
                }
                catch(Exception e)
                {
                    Global.Logger.Error("Error loading player", e);
                    ReplyError(session, packet, Error.Unexpected);
                    session.CloseSession();
                    return;
                }

                if (!response.Success)
                {
                    ReplyError(session, packet, Error.InvalidLogin);
                    session.CloseSession();
                    return;
                }

                playerId = uint.Parse(response.Data.player.id);
                playerName = response.Data.player.name;
                banned = int.Parse(response.Data.player.banned) == 1;
                playerRights = (PlayerRights)Int32.Parse(response.Data.player.rights);                

                // If we are under admin only mode then kick out non admin
                if (Config.server_admin_only && playerRights == PlayerRights.Basic)
                {
                    ReplyError(session, packet, Error.UnderMaintenance);
                    session.CloseSession();
                    return;
                }
            }
            else
            {
                if (!uint.TryParse(playerName, out playerId))
                {
                    ReplyError(session, packet, Error.PlayerNotFound);
                    session.CloseSession();
                    return;
                }

                playerName = "Player " + playerId;
            }

            //Create the session id that will be used for the calls to the web server
            string sessionId;
            if (Config.server_admin_always && !Config.server_production)
            {
                sessionId = playerId.ToString(CultureInfo.InvariantCulture);
                playerRights = PlayerRights.Bureaucrat;
            }
            else
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] hash =
                        sha.ComputeHash(Encoding.UTF8.GetBytes(playerId + Config.database_salt + DateTime.UtcNow.Ticks));
                sessionId = BitConverter.ToString(hash).Replace("-", String.Empty);
            }

            bool newPlayer;
            lock (loginLock)
            {
                newPlayer = !World.Current.Players.TryGetValue(playerId, out player);

                //If it's a new player then add him to our session
                if (newPlayer)
                {
                    Global.Logger.Info(string.Format("Creating new player {0}({1})", playerName, playerId));

                    player = new Player(playerId,
                                        SystemClock.Now,
                                        SystemClock.Now,
                                        playerName,
                                        string.Empty,
                                        playerRights,
                                        sessionId);

                    World.Current.Players.Add(player.PlayerId, player);
                }
                else
                {
                    Global.Logger.Info(string.Format("Player login in {0}({1})", player.Name, player.PlayerId));

                    player.Name = playerName;
                }
            }

            using (Concurrency.Current.Lock(player))
            {
                // If someone is already connected as this player, kick them off
                if (player.Session != null)
                {
                    player.Session.CloseSession();
                    player.Session = null;
                }

                // Setup session references
                session.Player = player;
                player.Session = session;                
                player.SessionId = sessionId;                
                player.Rights = playerRights;
                player.LastLogin = SystemClock.Now;
                player.Banned = banned;

                DbPersistance.Current.Save(player);

                // If player was banned then kick his ass out
                if (banned)
                {
                    ReplyError(session, packet, Error.Banned);
                    session.CloseSession();
                    return;
                }

                var reply = new Packet(packet);
                reply.Option |= (ushort)Packet.Options.Compressed;

                reply.AddString(Config.welcome_motd);

                //Player Info
                reply.AddUInt32(player.PlayerId);
                reply.AddByte((byte)(player.Rights >= PlayerRights.Admin ? 1 : 0));
                reply.AddString(sessionId);
                reply.AddString(player.Name);
                reply.AddInt32(Config.newbie_protection);
                reply.AddUInt32(UnixDateTime.DateTimeToUnix(player.Created.ToUniversalTime()));
                reply.AddInt32(player.Tribesman == null
                                       ? 0
                                       : tribeManager.GetIncomingList(player.Tribesman.Tribe).Count());
                reply.AddInt16((short)(player.Tribesman == null ? 0 : player.Tribesman.Tribe.AssignmentCount));

                //Server time
                reply.AddUInt32(UnixDateTime.DateTimeToUnix(DateTime.UtcNow.ToUniversalTime()));

                //Server rate
                reply.AddString(Config.seconds_per_unit.ToString(CultureInfo.InvariantCulture));

                // If it's a new player we send simply a 1 which means the client will need to send back a city name
                // Otherwise, we just send the whole login info
                if (player.GetCityCount() == 0)
                {
                    reply.AddByte(1);
                }
                else
                {
                    reply.AddByte(0);
                    PacketHelper.AddLoginToPacket(session, reply);
                    SubscribeDefaultChannels(session, session.Player);
                }

                session.Write(reply);

                //Restart any city actions that may have been stopped due to inactivity
                foreach (
                        var city in
                                player.GetCityList()
                                      .Where(
                                             city =>
                                             city.Worker.PassiveActions.Values.All(x => x.Type != ActionType.CityPassive))
                        )
                {
                    city.Worker.DoPassive(city, actionFactory.CreateCityPassiveAction(city.Id), false);
                }
            }
        }

        private void CreateInitialCity(Session session, Packet packet)
        {
            using (Concurrency.Current.Lock(session.Player))
            {
                string cityName;
                try
                {
                    cityName = packet.GetString().Trim();
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Verify city name is valid
                if (!City.IsNameValid(cityName))
                {
                    ReplyError(session, packet, Error.CityNameInvalid);
                    return;
                }

                ICity city;

                lock (World.Current.Lock)
                {
                    // Verify city name is unique
                    if (World.Current.CityNameTaken(cityName))
                    {
                        ReplyError(session, packet, Error.CityNameTaken);
                        return;
                    }

                    if (!Procedure.Current.CreateCity(session.Player, cityName, out city))
                    {
                        ReplyError(session, packet, Error.MapFull);
                        return;
                    }
                }

                IStructure mainBuilding = (IStructure)city[1];

                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, mainBuilding, mainBuilding.Type, mainBuilding.Stats.Base.Lvl);

                city.Worker.DoPassive(city, actionFactory.CreateCityPassiveAction(city.Id), false);

                var reply = new Packet(packet);
                reply.Option |= (ushort)Packet.Options.Compressed;
                PacketHelper.AddLoginToPacket(session, reply);
                SubscribeDefaultChannels(session, session.Player);
                session.Write(reply);
            }
        }

        private void SubscribeDefaultChannels(Session session, IPlayer player)
        {
            // Subscribe him to the player channel
            Global.Channel.Subscribe(session, "/PLAYER/" + player.PlayerId);

            // Subscribe him to the tribe channel if available
            if (player.Tribesman != null)
            {
                Global.Channel.Subscribe(session, "/TRIBE/" + player.Tribesman.Tribe.Id);
            }

            // Subscribe to global channel
            Global.Channel.Subscribe(session, "/GLOBAL");
        }
    }
}