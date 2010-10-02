#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game
{
    public class Player : ILockable, IPersistableObject
    {
        private readonly List<City> list = new List<City>();

        public Session Session { get; set; }

        public string Name { get; private set; }

        public uint PlayerId { get; private set; }

        public DateTime Created { get; private set; }

        public DateTime LastLogin { get; set; }

        public string SessionId { get; set; }

        public bool Admin { get; set; }

        public Player(uint playerid, DateTime created, DateTime lastLogin, string name, bool admin) : this(playerid, created, lastLogin, name, admin, string.Empty) { }

        public Player(uint playerid, DateTime created, DateTime lastLogin, string name, bool admin, string sessionId)
        {
            PlayerId = playerid;
            LastLogin = lastLogin;
            Created = created;
            Name = name;
            SessionId = sessionId;
            Admin = admin;
        }

        public void Add(City city)
        {
            list.Add(city);
        }

        internal List<City> GetCityList()
        {
            return list;
        }

        internal City GetCity(uint id)
        {
            return list.Find(city => city.Id == id);
        }

        public void SendSystemMessage(Player from, String subject, String message)
        {
            subject = String.Format("(System) {0}", subject);
            Global.DbManager.Query(
                "INSERT INTO `messages` (`sender_player_id`, `recipient_player_id`, `subject`, `message`, `sender_state`, `recipient_state`, `created`) VALUES (@sender_player_id, @recipient_player_id, @subject, @message, @sender_state, @recipient_state, UTC_TIMESTAMP())",
                new[]
                    {
                        new DbColumn("sender_player_id", from == null ? 0 : from.PlayerId, DbType.UInt32), new DbColumn("recipient_player_id", PlayerId, DbType.UInt32),
                        new DbColumn("subject", subject, DbType.String), new DbColumn("message", subject, DbType.String), new DbColumn("sender_state", 2, DbType.Int16),
                        new DbColumn("recipient_state", 0, DbType.Int16),
                    });
        }

        #region ILockable Members

        public int Hash
        {
            get { return unchecked((int)PlayerId); }
        }

        public object Lock
        {
            get { return this; }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "players";

        public string DbTable
        {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[] {
                                  new DbColumn("name", Name, DbType.String, 32),
                                  new DbColumn("created", Created, DbType.DateTime),
                                  new DbColumn("last_login", LastLogin, DbType.DateTime),
                                  new DbColumn("session_id", SessionId, DbType.String, 128),
                                  new DbColumn("online", Session != null, DbType.Boolean),
                                  new DbColumn("admin", Admin, DbType.Boolean)
                              };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get { return new[] { new DbColumn("id", PlayerId, DbType.UInt32) }; }
        }

        public DbDependency[] DbDependencies
        {
            get { return new DbDependency[] { }; }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}