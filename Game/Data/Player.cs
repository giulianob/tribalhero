#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game {
    public class Player : ILockable, IPersistableObject {
        private readonly List<City> list = new List<City>();

        public Session Session { get; set; }

        public string Name { get; private set; }

        public uint PlayerId { get; private set; }

        public DateTime Created { get; private set; }

        public DateTime LastLogin { get; set; }

        public string SessionId { get; set; }

        public Player(uint playerid, DateTime created, DateTime lastLogin, string name) : this(playerid, created, lastLogin, name, string.Empty) {}

        public Player(uint playerid, DateTime created, DateTime lastLogin, string name, string sessionId)
        {
            PlayerId = playerid;
            LastLogin = lastLogin;
            Created = created;
            Name = name;
            SessionId = sessionId;
        }

        public void Add(City city) {
            list.Add(city);
        }

        internal List<City> GetCityList() {
            return list;
        }

        internal City GetCity(uint id) {
            return list.Find(city => city.Id == id);
        }

        public void SendSystemMessage(Player from, String subject, String message) {
            subject = String.Format("(System) {0}", subject);
            Global.DbManager.Query(string.Format("INSERT INTO `messages` (`sender_player_id`, `recipient_player_id`, `subject`, `message`, `sender_state`, `recipient_state`, `created`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', UTC_TIMESTAMP())", from == null ? 0 : from.PlayerId, PlayerId, subject, message, 2, 0));
        }

        #region ILockable Members

        public int Hash {
            get { return unchecked((int) PlayerId); }
        }

        public object Lock {
            get { return this; }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "players";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new[] {
                                  new DbColumn("name", Name, DbType.String, 32),
                                  new DbColumn("created", Created, DbType.DateTime),
                                  new DbColumn("last_login", LastLogin, DbType.DateTime),
                                  new DbColumn("session_id", SessionId, DbType.String, 128)
                              };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] {new DbColumn("id", PlayerId, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}