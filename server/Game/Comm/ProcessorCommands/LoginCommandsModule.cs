#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common;
using Game.Comm.Api;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Store;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Map.LocationStrategies;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Newtonsoft.Json;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class LoginCommandsModule : CommandModule
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<LoginCommandsModule>();

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

        private readonly CallbackProcedure callbackProcedure;

        private readonly IChannel channel;

        private readonly IThemeManager themeManager;

        private readonly IPlayerFactory playerFactory;

        public LoginCommandsModule(IActionFactory actionFactory,
                                   ITribeManager tribeManager,
                                   IDbManager dbManager,
                                   ILocker locker,
                                   IWorld world,
                                   Procedure procedure,
                                   ICityFactory cityFactory,
                                   ILocationStrategyFactory locationStrategyFactory,
                                   IBarbarianTribeManager barbarianTribeManager,
                                   CallbackProcedure callbackProcedure,
                                   IChannel channel,
                                   IThemeManager themeManager,
                                   IPlayerFactory playerFactory)
        {
            this.actionFactory = actionFactory;
            this.tribeManager = tribeManager;
            this.dbManager = dbManager;
            this.locker = locker;
            this.world = world;
            this.procedure = procedure;
            this.callbackProcedure = callbackProcedure;
            this.channel = channel;
            this.themeManager = themeManager;
            this.playerFactory = playerFactory;
            this.cityFactory = cityFactory;
            this.locationStrategyFactory = locationStrategyFactory;
            this.barbarianTribeManager = barbarianTribeManager;
        }

        public override void RegisterCommands(IProcessor processor)
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

            LoginResponseData loginResponseData;
            if (Config.database_load_players)
            {
                ApiResponse<LoginResponseData> response;
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

                loginResponseData = response.Data;

                // If we are under admin only mode then kick out non admin
                if (Config.server_admin_only && loginResponseData.Player.Rights == PlayerRights.Basic)
                {
                    ReplyError(session, packet, Error.UnderMaintenance);
                    session.CloseSession();
                    return;
                }
            }
            else
            {
                // This is a fake login for testing only
                uint playerId;
                if (!uint.TryParse(playerName, out playerId))
                {
                    ReplyError(session, packet, Error.PlayerNotFound);
                    session.CloseSession();
                    return;
                }

                loginResponseData = new LoginResponseData
                {
                    Player = new LoginResponseData.PlayerData
                    {
                        Banned = false,
                        Id = playerId,
                        Name = "Player " + playerId,
                        Rights = PlayerRights.Basic
                    },
                    Achievements = new List<Achievement>(),
                    ThemePurchases = new List<ThemePurchase>(),
                };
            }

            //Create the session id that will be used for the calls to the web server
            string sessionId;
            if (Config.server_admin_always && !Config.server_production)
            {
                sessionId = loginResponseData.Player.Id.ToString(CultureInfo.InvariantCulture);
                loginResponseData.Player.Rights = PlayerRights.Bureaucrat;
            }
            else
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(loginResponseData.Player.Id + Config.database_salt + DateTime.UtcNow.Ticks + Config.Random.Next()));
                sessionId = BitConverter.ToString(hash).Replace("-", String.Empty);
            }

            lock (loginLock)
            {
                bool newPlayer = !world.Players.TryGetValue(loginResponseData.Player.Id, out player);

                //If it's a new player then add him to our session
                if (newPlayer)
                {
                    logger.Info(string.Format("Creating new player {0}({1}) IP: {2}", playerName, loginResponseData.Player.Id, session.RemoteIP));

                    player = playerFactory.CreatePlayer(loginResponseData.Player.Id, SystemClock.Now, SystemClock.Now, playerName, string.Empty, loginResponseData.Player.Rights, sessionId);

                    if (!world.Players.TryAdd(player.PlayerId, player))
                    {
                        session.CloseSession();
                        return;
                    }
                }
                else
                {

                    player.Name = playerName;
                }

                logger.Info(string.Format("Player login in {0}({1}) IP: {2}", player.Name, player.PlayerId, session.RemoteIP));
            }

            locker.Lock(args =>
            {
                var lockedPlayer = (IPlayer)args[0];
                return lockedPlayer.IsInTribe ? new ILockable[] {lockedPlayer.Tribesman.Tribe} : new ILockable[0];
            }, new object[] { player }, player).Do(() =>
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
                player.HasTwoFactorAuthenticated = null;
                player.TwoFactorSecretKey = loginResponseData.Player.TwoFactorSecretKey;
                player.Session = session;
                player.SessionId = sessionId;
                player.Rights = loginResponseData.Player.Rights;
                player.LastLogin = SystemClock.Now;
                player.Banned = loginResponseData.Player.Banned;
                player.Achievements.Clear();
                player.Achievements.AddRange(loginResponseData.Achievements);
                player.ThemePurchases.Clear();
                player.ThemePurchases.AddRange(loginResponseData.ThemePurchases);

                dbManager.Save(player);

                // If player was banned then kick his ass out
                if (loginResponseData.Player.Banned)
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
                reply.AddBoolean(player.SoundMuted);
                reply.AddBoolean(player.Rights >= PlayerRights.Admin);
                reply.AddString(sessionId);
                reply.AddString(player.Name);
                reply.AddInt32(Config.newbie_protection);
                reply.AddInt32(loginResponseData.Player.Balance);
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
                    PacketHelper.AddLoginToPacket(session, themeManager, reply);
                    SubscribeDefaultChannels(session, session.Player);
                }

                session.Write(reply);

                // Restart any city actions that may have been stopped due to inactivity
                foreach (var city in player.GetCityList()
                                           .Where(city => city.Worker.PassiveActions.Values.All(x => x.Type != ActionType.CityPassive)))
                {
                    city.Worker.DoPassive(city, actionFactory.CreateCityPassiveAction(city.Id), false);
                }
            });
        }

        private void CreateInitialCity(Session session, Packet packet)
        {
            locker.Lock(session.Player).Do(() => 
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

                    procedure.CreateCity(cityFactory, session.Player, cityName, 1, cityPosition, barbarianTribeManager, out city);
                }

                procedure.InitCity(city, callbackProcedure, actionFactory);

                var reply = new Packet(packet);
                reply.Option |= (ushort)Packet.Options.Compressed;
                PacketHelper.AddLoginToPacket(session, themeManager, reply);
                SubscribeDefaultChannels(session, session.Player);
                session.Write(reply);
            });
        }

        private void SubscribeDefaultChannels(Session session, IPlayer player)
        {
            // Subscribe him to the player channel
            channel.Subscribe(session, player.PlayerChannel);

            // Subscribe him to the tribe channel if available
            if (player.Tribesman != null)
            {
                channel.Subscribe(session, "/TRIBE/" + player.Tribesman.Tribe.Id);
            }

            // Subscribe to global channel
            channel.Subscribe(session, "/GLOBAL");
        }
    }
}