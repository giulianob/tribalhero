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
        private readonly List<City> list = new List<City>();

        public Session Session { get; set; }

        public string Name { get; private set; }

        public uint PlayerId { get; private set; }

        public string SessionId { get; set; }

        public Player(uint playerid, string name) : this(playerid, name, string.Empty) {}

        public Player(uint playerid, string name, string sessionId) {
            PlayerId = playerid;
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