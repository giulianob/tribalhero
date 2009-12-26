using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Fighting;
using Game.Logic.Actions;
using System.IO;
using Game.Logic;
using Game.Database;
using Game.Logic.Procedures;
using System.Data.Common;
using System.Security.Cryptography;

namespace Game.Comm {
    public partial class Processor {

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

            string sessionId = string.Empty;

            try {
                loginMode = packet.getByte();
                if (loginMode == 0) {
                    loginKey = packet.getString();
                } else {
                    playerName = packet.getString();
                    playerPassword = packet.getString();
                }
            } catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            if (Config.database_load_players) {
                DbDataReader reader;
                try {
                    if (loginMode == 0)
                        reader = Global.dbManager.ReaderQuery(string.Format("SELECT * FROM `{0}` WHERE login_key IS NOT NULL AND login_key = '{1}' AND TIMEDIFF(NOW(), login_key_date) < '00:10:00.000000' LIMIT 1", Player.DB_TABLE, loginKey));
                    else
                        reader = Global.dbManager.ReaderQuery(string.Format("SELECT * FROM `{0}` WHERE name = '{1}' AND password = SHA1('{2}{3}') LIMIT 1", Player.DB_TABLE, playerName, Config.database_salt, playerPassword));
                } catch (Exception e) {
                    Global.Logger.Error("Error loading player", e);
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (!reader.HasRows) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                reader.Read();

                playerId = (uint)reader["id"];
                playerName = (string)reader["name"];

                reader.Close();

                Global.dbManager.Query(string.Format("UPDATE `{0}` SET login_key = null WHERE id = '{1}' LIMIT 1", Player.DB_TABLE, playerId, sessionId));

            } else {
                if (!uint.TryParse(playerName, out playerId)) {
                    reply_error(session, packet, Error.PLAYER_NOT_FOUND);
                    return;
                }

                playerName = "Player " + playerId;
            }

            //Create the session id that will be used for the calls to the web server
            SHA1 sha = new SHA1CryptoServiceProvider();
            sessionId = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(playerId + Config.database_salt + DateTime.Now.Ticks))).Replace("-", String.Empty);

            bool newPlayer = !Global.Players.TryGetValue(playerId, out player);

            if (newPlayer) {
                player = new Player(playerId, playerName, sessionId);

                Global.Players.Add(player.PlayerId, player);
            }
            else {
                player.SessionId = sessionId;
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
                        reply_error(session, packet, Error.MAP_FULL);
                        return;
                    }

                    Resource res;
                    if (player.PlayerId == 123)
                        res = new Resource(10000, 10000, 10000, 10000, 0);
                    else
                        res = new Resource(500, 0, 0, 500, 0);

                    City city = new City(player, "City " + player.PlayerId, res, structure);
                    
                    Global.World.add(city);
                    Global.World.add(structure);

                    InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Stats.Base.Lvl);

                    city.Worker.doPassive(city, new CityAction(city.CityId), false);
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
                reply.addByte((byte)list.Count);
                foreach (City city in list) {
                    city.Subscribe(session);
                    reply.addUInt32(city.CityId);
                    reply.addString(city.Name);
                    PacketHelper.AddToPacket(city.Resource, reply);
                    reply.addByte(city.Radius);

                    //City Actions
                    PacketHelper.AddToPacket(new List<Game.Logic.Action>(city.Worker.getVisibleActions()), reply, true);

                    //Notifications
                    reply.addUInt16(city.Worker.Notifications.Count);
                    foreach (Game.Logic.NotificationManager.Notification notification in city.Worker.Notifications)
                        PacketHelper.AddToPacket(notification, reply);

                    //Structures
                    List<Structure> structs = new List<Structure>(city);
                    reply.addUInt16((ushort)structs.Count);
                    foreach (Structure structure in structs) {
                        reply.addUInt16(Region.getRegionIndex(structure));
                        PacketHelper.AddToPacket(structure, reply, false);

                        reply.addUInt16((ushort)structure.Technologies.OwnedTechnologyCount);
                        foreach (Technology tech in structure.Technologies) {
                            if (tech.ownerLocation != EffectLocation.Object) continue;
                            reply.addUInt32(tech.Type);
                            reply.addByte(tech.Level);
                        }
                    }

                    //City Troops
                    reply.addByte(city.Troops.Size);
                    foreach (TroopStub stub in city.Troops) {
                        PacketHelper.AddToPacket(stub, reply);
                    }

                    //Unit Template
                    PacketHelper.AddToPacket(city.Template, reply);
                }

                session.write(reply);
            }
        }
    }
}

