#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Map.LocationStrategies;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject.Extensions.Logging;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class LoginCommandsModule : CommandModule
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly IActionFactory actionFactory;

        private readonly object loginLock = new object();

        private readonly ITribeManager tribeManager;

        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private readonly ILocationStrategyFactory locationStrategyFactory;

        private readonly ICityFactory cityFactory;

        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly InitFactory initFactory;

        public LoginCommandsModule(IActionFactory actionFactory,
                                   ITribeManager tribeManager,
                                   IDbManager dbManager,
                                   ILocker locker,
                                   IWorld world,
                                   Procedure procedure,
                                   ICityFactory cityFactory,
                                   ILocationStrategyFactory locationStrategyFactory,
                                   IBarbarianTribeManager barbarianTribeManager,
                                   InitFactory initFactory)
        {
            this.actionFactory = actionFactory;
            this.tribeManager = tribeManager;
            this.dbManager = dbManager;
            this.locker = locker;
            this.world = world;
            this.procedure = procedure;
            this.initFactory = initFactory;
            this.cityFactory = cityFactory;
            this.locationStrategyFactory = locationStrategyFactory;
            this.barbarianTribeManager = barbarianTribeManager;
            this.initFactory = initFactory;
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
            var achievements = new List<Achievement>();
            
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
                    logger.Error("Error loading player", e);
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

                if (((IDictionary<string, Object>)response.Data).ContainsKey("achievements"))
                {
                    foreach (var achievement in response.Data.achievements)
                    {
                        achievements.Add(new Achievement
                        {
                                Id = int.Parse(achievement.id),
                                Type = achievement.type,
                                Tier = Enum.Parse(typeof(AchievementTier), achievement.tier),
                                Description = achievement.description,
                                Title = achievement.title,
                                Icon = achievement.icon
                        });
                    }
                }

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

            lock (loginLock)
            {
                bool newPlayer = !world.Players.TryGetValue(playerId, out player);

                //If it's a new player then add him to our session
                if (newPlayer)
                {
                    logger.Info(string.Format("Creating new player {0}({1})", playerName, playerId));

                    player = new Player(playerId, SystemClock.Now, SystemClock.Now, playerName, string.Empty, playerRights, sessionId);

                    if (!world.Players.TryAdd(player.PlayerId, player))
                    {
                        session.CloseSession();
                        return;
                    }
                }
                else
                {
                    logger.Info(string.Format("Player login in {0}({1})", player.Name, player.PlayerId));

                    player.Name = playerName;
                }
            }

            using (locker.Lock(player))
            {
                // If someone is already connected as this player, kick them off potentially
                if (player.Session != null)
                {
                    player.Session.CloseSession();
                    player.Session = null;

                    // Kick people off who are spamming logins
                    if (SystemClock.Now.Subtract(player.LastLogin).TotalMilliseconds < 1500)
                    {
                        session.CloseSession();
                        return;
                    }
                }

                // Setup session references
                session.Player = player;
                player.Session = session;                
                player.SessionId = sessionId;                
                player.Rights = playerRights;
                player.LastLogin = SystemClock.Now;
                player.Banned = banned;
                player.Achievements.Clear();
                achievements.ForEach(player.Achievements.Add);

                dbManager.Save(player);

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
                reply.AddString(player.PlayerHash);
                reply.AddUInt32(player.TutorialStep);
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

                // Restart any city actions that may have been stopped due to inactivity
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
            using (locker.Lock(session.Player))
            {
                string cityName, playerName = null, playerHash = null;
                byte method;
                try
                {
                    cityName = packet.GetString().Trim();
                    method = packet.GetByte();
                    if (method == 1)
                    {
                        playerName = packet.GetString();
                        playerHash = packet.GetString();
                        if (playerName.Length == 0 || playerHash.Length == 0)
                        {
                            ReplyError(session, packet, Error.PlayerNotFound);
                            return;
                        }
                    }
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Verify city name is valid
                if (!CityManager.IsNameValid(cityName))
                {
                    ReplyError(session, packet, Error.CityNameInvalid);
                    return;
                }

                ICity city;

                lock (world.Lock)
                {
                    ILocationStrategy strategy;
                    if (method == 1)
                    {
                        uint playerId;
                        if (!world.FindPlayerId(playerName, out playerId))
                        {
                            ReplyError(session, packet, Error.PlayerNotFound);
                            return;
                        }

                        var player = world.Players[playerId];
                        if (String.Compare(player.PlayerHash, playerHash, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            ReplyError(session, packet, Error.PlayerHashNotFound);
                            return;
                        }
                        strategy = locationStrategyFactory.CreateCityTileNextToFriendLocationStrategy(Config.friend_invite_radius, player);
                    }
                    else
                    {
                        strategy = locationStrategyFactory.CreateCityTileNextAvailableLocationStrategy();
                    }

                    // Verify city name is unique
                    if (world.CityNameTaken(cityName))
                    {
                        ReplyError(session, packet, Error.CityNameTaken);
                        return;
                    }

                    Position cityPosition;
                    var locationStrategyResult = strategy.NextLocation(out cityPosition);

                    if (locationStrategyResult != Error.Ok)
                    {
                        ReplyError(session, packet, locationStrategyResult);
                        return;
                    }

                    procedure.CreateCity(cityFactory, session.Player, cityName, cityPosition, barbarianTribeManager, out city);
                }

                procedure.InitCity(city, initFactory, actionFactory);

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
            Global.Current.Channel.Subscribe(session, "/PLAYER/" + player.PlayerId);

            // Subscribe him to the tribe channel if available
            if (player.Tribesman != null)
            {
                Global.Current.Channel.Subscribe(session, "/TRIBE/" + player.Tribesman.Tribe.Id);
            }

            // Subscribe to global channel
            Global.Current.Channel.Subscribe(session, "/GLOBAL");
        }
    }
}