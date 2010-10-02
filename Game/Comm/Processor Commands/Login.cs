#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Fighting;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        readonly object loginLock = new object();

        public void CmdQueryXml(Session session, Packet packet)
        {
            Packet reply = new Packet(packet);
            reply.AddString(File.ReadAllText("C:\\source\\GameServer\\Game\\Setup\\CSV\\data.xml"));
            session.Write(reply);
        }

        public void CmdLogin(Session session, Packet packet)
        {
            Player player;
            Packet reply = new Packet(packet);
            reply.Option |= (ushort)Packet.Options.COMPRESSED;

            short clientVersion;
            short clientRevision;
            byte loginMode;
            string loginKey = string.Empty;
            string playerName = string.Empty;
            DateTime playerCreated = DateTime.MinValue;
            string playerPassword = string.Empty;
            uint playerId;
            bool admin;

            try
            {
                clientVersion = packet.GetInt16();
                clientRevision = packet.GetInt16();
                loginMode = packet.GetByte();
                if (loginMode == 0)
                    loginKey = packet.GetString();
                else
                {
                    playerName = packet.GetString();
                    playerPassword = packet.GetString();
                }
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.UNEXPECTED);
                session.CloseSession();
                return;
            }

            if (clientVersion < Config.client_min_version || clientRevision < Config.client_min_revision)
            {
                ReplyError(session, packet, Error.CLIENT_OLD_VERSION);
                session.CloseSession();
            }

            if (Config.database_load_players)
            {
                DbDataReader reader;
                try
                {
                    if (loginMode == 0)
                    {
                        reader =
                            Global.DbManager.ReaderQuery(
                                string.Format(
                                    "SELECT * FROM `{0}` WHERE login_key IS NOT NULL AND login_key = @login_key AND TIMEDIFF(NOW(), login_key_date) < '00:10:00.000000' LIMIT 1",
                                    Player.DB_TABLE), new[] { new DbColumn("login_key", loginKey, System.Data.DbType.String) });
                    }
                    else
                    {
                        reader =
                            Global.DbManager.ReaderQuery(
                                string.Format(
                                    "SELECT * FROM `{0}` WHERE name = @name AND password = SHA1(@password) LIMIT 1",
                                    Player.DB_TABLE), new[] { new DbColumn("name", playerName, System.Data.DbType.String), new DbColumn("password", Config.database_salt + playerPassword, System.Data.DbType.String) });
                    }
                }
                catch (Exception e)
                {
                    Global.Logger.Error("Error loading player", e);
                    ReplyError(session, packet, Error.UNEXPECTED);
                    session.CloseSession();
                    return;
                }

                if (!reader.HasRows)
                {
                    ReplyError(session, packet, Error.INVALID_LOGIN);
                    session.CloseSession();
                    return;
                }

                reader.Read();

                playerId = (uint)reader["id"];
                playerName = (string)reader["name"];
                playerCreated = DateTime.SpecifyKind((DateTime)reader["created"], DateTimeKind.Utc);
                // ReSharper disable RedundantAssignment
                admin = (bool)reader["admin"];
                // ReSharper restore RedundantAssignment

                reader.Close();

                Global.DbManager.Query(string.Format("UPDATE `{0}` SET login_key = null WHERE id = @id LIMIT 1",
                                                     Player.DB_TABLE), new[] { new DbColumn("id", playerId, System.Data.DbType.UInt32) });
            }
            else
            {
                if (!uint.TryParse(playerName, out playerId))
                {
                    ReplyError(session, packet, Error.PLAYER_NOT_FOUND);
                    session.CloseSession();
                    return;
                }

                playerName = "Player " + playerId;
            }

            //Create the session id that will be used for the calls to the web server                        
#if DEBUG
            string sessionId = playerId.ToString();
            admin = true;
#else
            SHA1 sha = new SHA1CryptoServiceProvider();

            string sessionId = BitConverter.ToString(
                sha.ComputeHash(Encoding.UTF8.GetBytes(playerId + Config.database_salt + DateTime.UtcNow.Ticks))).
                Replace("-", String.Empty);
#endif

            bool newPlayer;
            lock (loginLock)
            {
                newPlayer = !Global.Players.TryGetValue(playerId, out player);

                //If it's a new player then add him to our session
                if (newPlayer)
                {
                    Global.Logger.Info(string.Format("Creating new player {0}({1})", playerName, playerId));

                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    player = new Player(playerId, playerCreated, SystemClock.Now, playerName, admin, sessionId);
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse

                    Global.Players.Add(player.PlayerId, player);
                }
                else
                {
                    Global.Logger.Info(string.Format("Player login in {0}({1})", player.Name, player.PlayerId));
                    player.SessionId = sessionId;
                    player.LastLogin = SystemClock.Now;
                }
            }

            using (new MultiObjectLock(player))
            {
                if (!newPlayer)
                {
                    // If someone is already connected as this player, kick them off
                    if (player.Session != null)
                    {
                        player.Session.CloseSession();
                        player.Session = null;
                    }

                    player.Session = session;
                    Global.DbManager.Save(player);
                }
                else
                {
                    player.DbPersisted = Config.database_load_players;

                    Global.DbManager.Save(player);
                }

                //User session
                session.Player = player;
                player.Session = session;

                //Player Id
                reply.AddUInt32(player.PlayerId);
                reply.AddByte((byte)(player.Admin ? 1 : 0));
                reply.AddString(sessionId);
                reply.AddString(player.Name);

                //Server time
                reply.AddUInt32(UnixDateTime.DateTimeToUnix(DateTime.UtcNow.ToUniversalTime()));

                //Server rate
                reply.AddString(Config.seconds_per_unit.ToString());

                // If it's a new player we send simply a 1 which means the client will need to send back a city name
                // Otherwise, we just send the whole login info
                if (player.GetCityList().Count == 0)
                {
                    reply.AddByte(1);
                }
                else
                {
                    reply.AddByte(0);
                    PacketHelper.AddLoginToPacket(session, reply);
                }

                session.Write(reply);
            }
        }

        public void CmdCreateInitialCity(Session session, Packet packet)
        {
            using (new MultiObjectLock(session.Player))
            {
                string cityName;
                try
                {
                    cityName = packet.GetString().Trim();
                }
                catch (Exception)
                {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                // Verify city name is valid
                if (!City.IsNameValid(cityName))
                {
                    ReplyError(session, packet, Error.CITY_NAME_INVALID);
                    return;
                }

                City city;
                Structure mainBuilding;

                lock (Global.World.Lock)
                {
                    // Verify city name is unique
                    if (Global.World.CityNameTaken(cityName))
                    {
                        ReplyError(session, packet, Error.CITY_NAME_TAKEN);
                        return;
                    }

                    if (!Randomizer.MainBuilding(out mainBuilding))
                    {
                        Global.Players.Remove(session.Player.PlayerId);
                        Global.DbManager.Rollback();
                        // If this happens I'll be a very happy game developer
                        ReplyError(session, packet, Error.MAP_FULL);
                        return;
                    }

                    city = new City(session.Player, cityName, Formula.GetInitialCityResources(), mainBuilding);

                    Global.World.Add(city);
                    mainBuilding.BeginUpdate();
                    Global.World.Add(mainBuilding);
                    mainBuilding.EndUpdate();

                    TroopStub defaultTroop = new TroopStub();
                    defaultTroop.BeginUpdate();
                    defaultTroop.AddFormation(FormationType.NORMAL);
                    defaultTroop.AddFormation(FormationType.GARRISON);
                    defaultTroop.AddFormation(FormationType.IN_BATTLE);
                    city.Troops.Add(defaultTroop);
                    defaultTroop.EndUpdate();
                }

                InitFactory.InitGameObject(InitCondition.ON_INIT, mainBuilding, mainBuilding.Type, mainBuilding.Stats.Base.Lvl);

                city.Worker.DoPassive(city, new CityAction(city.Id), false);

                Packet reply = new Packet(packet);
                reply.Option |= (ushort)Packet.Options.COMPRESSED;
                PacketHelper.AddLoginToPacket(session, reply);
                session.Write(reply);
            }
        }
    }
}