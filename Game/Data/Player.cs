using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Data;
using Game.Database;
using Game.Util;
using Game.Comm;

namespace Game {
    public class Player: ILockable, IPersistableObject {
        List<City> list = new List<City>();
        
        Session session = null;
        public Session Session {
            get { return session; }
            set { session = value; }
        }

        string name;
        public string Name {
            get { return name; }
            set { name = value; }
        }

        uint playerid;
        public uint PlayerId {
            get { return playerid; }
            set { playerid = value; }
        }

        public Player(uint playerid, string name) {
            this.playerid = playerid;
            this.name = name;
        }

        public void add(City city) {
            list.Add(city);
        }

        internal List<City> getCityList() {
            return list;
        }

        internal City getCity(uint id) {
            return list.Find(delegate(City city) {
                return city.CityId == id;
            });
        }

        #region ILockable Members

        public int Hash {
            get { return unchecked((int)playerid); }
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
                    new DbColumn("name", Name, System.Data.DbType.String, 32)
                };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", PlayerId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }
        #endregion
    }
}
