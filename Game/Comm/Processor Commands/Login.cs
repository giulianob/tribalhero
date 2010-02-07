#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm {
    public partial class Processor {
        readonly object loginLock = new object();

        public void CmdQueryXml(Session session, Packet packet) {
            Packet reply = new Packet(packet);
            reply.addString(File.ReadAllText("C:\\source\\GameServer\\Game\\Setup\\CSV\\data.xml"));
            session.write(reply);
        }

        public void CmdLogin(Session session, Packet packet) {
            Player player;
            Packet reply = new Packet(packet);

            byte loginMode;
            string loginKey = string.Empty;
            string playerName = string.Empty;
            string playerPassword = string.Empty;
            uint playerId;

            try {
                loginMode = packet.getByte();
                if (loginMode == 0)
                    loginKey = packet.getString();
                else {
                    playerName = packet.getString();
                    playerPassword = packet.getString();
                }
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                session.CloseSession();
                return;
            }

            if (Config.database_load_players) {
                DbDataReader reader;
                try {
                    if (loginMode == 0) {
                        reader =
                            Global.dbManager.ReaderQuery(
                                string.Format(
                                    "SELECT * FROM `{0}` WHERE login_key IS NOT NULL AND login_key = '{1}' AND TIMEDIFF(NOW(), login_key_date) < '00:10:00.000000' LIMIT 1",
                                    Player.DB_TABLE, loginKey));
                    } else {
                        reader =
                            Global.dbManager.ReaderQuery(
                                string.Format(
                                    "SELECT * FROM `{0}` WHERE name = '{1}' AND password = SHA1('{2}{3}') LIMIT 1",
                                    Player.DB_TABLE, playerName, Config.database_salt, playerPassword));
                    }
                }
                catch (Exception e) {
                    Global.Logger.Error("Error loading player", e);
                    reply_error(session, packet, Error.UNEXPECTED);
                    session.CloseSession();
                    return;
                }

                if (!reader.HasRows) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    session.CloseSession();
                    return;
                }

                reader.Read();

                playerId = (uint) reader["id"];
                playerName = (string) reader["name"];

                reader.Close();

                Global.dbManager.Query(string.Format("UPDATE `{0}` SET login_key = null WHERE id = '{1}' LIMIT 1",
                                                     Player.DB_TABLE, playerId));
            } else {
                if (!uint.TryParse(playerName, out playerId)) {
                    reply_error(session, packet, Error.PLAYER_NOT_FOUND);
                    session.CloseSession();
                    return;
                }

                playerName = "Player " + playerId;
            }

            //Create the session id that will be used for the calls to the web server
            SHA1 sha = new SHA1CryptoServiceProvider();
            string sessionId = BitConverter.ToString(
                sha.ComputeHash(Encoding.UTF8.GetBytes(playerId + Config.database_salt + DateTime.Now.Ticks))).
                Replace("-", String.Empty);

            bool newPlayer;
            lock (loginLock) {
                newPlayer = !Global.Players.TryGetValue(playerId, out player);

                if (newPlayer) {
                    Global.Logger.Info(String.Format("Creating new player {0}({1})", playerName, playerId));
                    player = new Player(playerId, playerName, sessionId);
                    Global.Players.Add(player.PlayerId, player);
                }
                else {
                    Global.Logger.Info(String.Format("Player login in {0}({1})", player.Name, player.PlayerId));
                    player.SessionId = sessionId;
                }
            }

            using (new MultiObjectLock(player)) {
                if (!newPlayer) {
                    if (player.Session != null) {
                        player.Session.CloseSession();
                        player.Session = null;
                    }
                    player.Session = session;
                    Global.dbManager.Save(player);
                } else {
                    player.DbPersisted = Config.database_load_players;

                    Global.dbManager.Save(player);

                    Structure structure;
                    if (!Randomizer.MainBuilding(out structure)) {
                        Global.Players.Remove(player.PlayerId);
                        Global.dbManager.Rollback();
                        //If this happens and its not a bug, I'll be a very happy game developer
                        reply_error(session, packet, Error.MAP_FULL);
                        return;
                    }

                    Resource res = new Resource(500, 0, 0, 500, 0);

                    City city = new City(player, "City " + player.PlayerId, res, structure);

                    Global.World.Add(city);
                    Global.World.Add(structure);

                    InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Stats.Base.Lvl);

                    city.Worker.DoPassive(city, new CityAction(city.Id), false);
                }

                //User session
                session.Player = player;
                player.Session = session;

                //Player Id
                reply.addUInt32(player.PlayerId);
                reply.addString(sessionId);
                reply.addString(player.Name);

                //Server time
                reply.addUInt32(UnixDateTime.DateTimeToUnix(DateTime.Now.ToUniversalTime()));

                //Server rate
                reply.addString(Config.seconds_per_unit.ToString());

                //Cities
                List<City> list = player.getCityList();
                reply.addByte((byte) list.Count);
                foreach (City city in list) {
                    city.Subscribe(session);
                    reply.addUInt32(city.Id);
                    reply.addString(city.Name);
                    PacketHelper.AddToPacket(city.Resource, reply);
                    reply.addByte(city.Radius);

                    //City Actions
                    PacketHelper.AddToPacket(new List<GameAction>(city.Worker.GetVisibleActions()), reply, true);

                    //Notifications
                    reply.addUInt16(city.Worker.Notifications.Count);
                    foreach (NotificationManager.Notification notification in city.Worker.Notifications)
                        PacketHelper.AddToPacket(notification, reply);

                    //Structures
                    List<Structure> structs = new List<Structure>(city);
                    reply.addUInt16((ushort) structs.Count);
                    foreach (Structure structure in structs) {
                        reply.addUInt16(Region.getRegionIndex(structure));
                        PacketHelper.AddToPacket(structure, reply, false);

                        reply.addUInt16((ushort) structure.Technologies.OwnedTechnologyCount);
                        foreach (Technology tech in structure.Technologies) {
                            if (tech.ownerLocation != EffectLocation.OBJECT)
                                continue;
                            reply.addUInt32(tech.Type);
                            reply.addByte(tech.Level);
                        }
                    }

                    //City Troops
                    reply.addByte(city.Troops.Size);
                    foreach (TroopStub stub in city.Troops)
                        PacketHelper.AddToPacket(stub, reply);

                    //Unit Template
                    PacketHelper.AddToPacket(city.Template, reply);
                }

                session.write(reply);
            }
        }
    }
}