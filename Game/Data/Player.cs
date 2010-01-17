#region

using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game {
    public class Player : ILockable, IPersistableObject {
        private List<City> list = new List<City>();

        private Session session = null;

        public Session Session {
            get { return session; }
            set { session = value; }
        }

        private string name;

        public string Name {
            get { return name; }
        }

        private uint playerid;

        public uint PlayerId {
            get { return playerid; }
        }

        private string sessionId;

        public string SessionId {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public Player(uint playerid, string name) : this(playerid, name, string.Empty) {}

        public Player(uint playerid, string name, string sessionId) {
            this.playerid = playerid;
            this.name = name;
            this.sessionId = sessionId;
        }

        public void add(City city) {
            list.Add(city);
        }

        internal List<City> getCityList() {
            return list;
        }

        internal City getCity(uint id) {
            return list.Find(delegate(City city) { return city.Id == id; });
        }

        #region ILockable Members

        public int Hash {
            get { return unchecked((int) playerid); }
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
                return new DbColumn[] {
                                          new DbColumn("name", Name, DbType.String, 32),
                                          new DbColumn("session_id", SessionId, DbType.String, 128)
                                      };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new DbColumn[] {new DbColumn("id", PlayerId, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        private bool dbPersisted = false;

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion
    }
}