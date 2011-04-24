#region

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        private readonly object loginLock = new object();

        public void CmdQueryXml(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            reply.AddString(File.ReadAllText(Path.Combine(Config.data_folder, "data.xml")));
            session.Write(reply);
        }

        public void CmdLogin(Session session, Packet packet)
        {
            Player player;
            var reply = new Packet(packet);
            reply.Option |= (ushort)Packet.Options.Compressed;

            short clientVersion;
            short clientRevision;
            byte loginMode;
            string loginKey = string.Empty;
            string playerName = string.Empty;
            string playerPassword = string.Empty;
            uint playerId;
            bool admin = false;
            bool banned = false;

            try
            {
                clientVersion = packet.GetInt16();
                clientRevision = packet.GetInt16();
                loginMode = packet.GetByte();
                playerName = packet.GetString();
                if (loginMode == 0)                
                    loginKey = packet.GetString();                
                else
                    playerPassword = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                session.CloseSession();
                return;
            }

            if (clientVersion < Config.client_min_version || clientRevision < Config.client_min_revision)
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
                    if (loginMode == 0)                    
                        response = ApiCaller.CheckLoginKey(playerName, loginKey);                    
                    else                    
                        response = ApiCaller.CheckLogin(playerName, playerPassword);                    
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
                admin = int.Parse(response.Data.player.admin) == 1;

                // If we are under admin only mode then kick out non admin
                if (Config.server_admin_only && !admin)
                {
                    ReplyError(session, packet, Error.UnderMaintenance);
                    session.CloseSession();
                    return;
                }

                // If player was banned then kick his ass out
                if (banned)
                {
                    ReplyError(session, packet, Error.Banned);
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
                newPlayer = !Global.World.Players.TryGetValue(playerId, out player);

                //If it's a new player then add him to our session
                if (newPlayer)
                {
                    Global.Logger.Info(string.Format("Creating new player {0}({1})", playerName, playerId));

                    player = new Player(playerId, SystemClock.Now, SystemClock.Now, playerName, string.Empty, admin, sessionId);

                    Global.World.Players.Add(player.PlayerId, player);
                }
                else
                {
                    Global.Logger.Info(string.Format("Player login in {0}({1})", player.Name, player.PlayerId));
                    
                    player.Admin = admin;
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
                    player.SessionId = sessionId;
                    Global.DbManager.Save(player);
                }
                else
                {
                    player.SessionId = sessionId;
                    player.Session = session;
                    Global.DbManager.Save(player);
                }

                //User session backreference
                session.Player = player;                

                // Subscribe him to the player channel
                Global.Channel.Subscribe(session, "/PLAYER/" + player.PlayerId);

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
                if (player.GetCityCount() == 0)
                    reply.AddByte(1);
                else
                {
                    reply.AddByte(0);
                    PacketHelper.AddLoginToPacket(session, reply);
                }

                session.Write(reply);

                //Restart any city actions that may have been stopped due to inactivity
                foreach (var city in
                        player.GetCityList().Where(city => !city.Worker.PassiveActions.Exists(x => x.Type == ActionType.CityPassive)))
                    city.Worker.DoPassive(city, new CityPassiveAction(city.Id), false);
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

                City city;
                Structure mainBuilding;

                lock (Global.World.Lock)
                {
                    // Verify city name is unique
                    if (Global.World.CityNameTaken(cityName))
                    {
                        ReplyError(session, packet, Error.CityNameTaken);
                        return;
                    }

                    if (!Randomizer.MainBuilding(out mainBuilding, Formula.GetInitialCityRadius(), 1))
                    {
                        Global.World.Players.Remove(session.Player.PlayerId);
                        Global.DbManager.Rollback();
                        // If this happens I'll be a very happy game developer
                        ReplyError(session, packet, Error.MapFull);
                        return;
                    }

                    city = new City(session.Player, cityName, Formula.GetInitialCityResources(), Formula.GetInitialCityRadius(), mainBuilding);
                    session.Player.Add(city);

                    Global.World.Add(city);
                    mainBuilding.BeginUpdate();
                    Global.World.Add(mainBuilding);
                    mainBuilding.EndUpdate();

                    var defaultTroop = new TroopStub();
                    defaultTroop.BeginUpdate();
                    defaultTroop.AddFormation(FormationType.Normal);
                    defaultTroop.AddFormation(FormationType.Garrison);
                    defaultTroop.AddFormation(FormationType.InBattle);
                    city.Troops.Add(defaultTroop);
                    defaultTroop.EndUpdate();
                }

                InitFactory.InitGameObject(InitCondition.OnInit, mainBuilding, mainBuilding.Type, mainBuilding.Stats.Base.Lvl);

                city.Worker.DoPassive(city, new CityPassiveAction(city.Id), false);

                var reply = new Packet(packet);
                reply.Option |= (ushort)Packet.Options.Compressed;
                PacketHelper.AddLoginToPacket(session, reply);
                session.Write(reply);
            }
        }
    }
}