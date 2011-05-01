using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Game.Database;
using Game.Setup;
using Game.Util;

namespace Game.Data.Tribe {
    public class Tribe:ILockable,IEnumerable<Tribesman>,IPersistableObject
    {
        public const string DB_TABLE = "tribes";
        const int MEMBERS_PER_LEVEL = 5;
        public uint Id { 
            get
            {
                return Owner.PlayerId;
            }
        }
        public Player Owner { get; private set; } 
        public string Name { get; set; }        
        public byte Level { get; set; }

        private string description = string.Empty;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;

                if (DbPersisted)
                {
                    Global.DbManager.Query(string.Format("UPDATE `{0}` SET `desc` = @desc WHERE `id` = @id LIMIT 1", DB_TABLE),
                                           new[] { new DbColumn("desc", description, DbType.String), new DbColumn("id", Id, DbType.UInt32) });
                }
            }
        }
        
        readonly Dictionary<uint, Tribesman> tribesmen = new Dictionary<uint, Tribesman>();
        
        public Resource Resource { get; private set; }

        public Tribe(Player owner, string name) :
            this(owner, name, string.Empty, 1, new Resource())
        {

        }

        public Tribe(Player owner, string name, string desc, byte level, Resource resource ) {
            Owner = owner;
            Level = level;
            Resource = resource;
            Description = desc;
            Name = name;
        }
        public bool IsOwner(Player player)
        {
            return player.PlayerId == Id;
        }

        public Error AddTribesman(Tribesman tribesman, bool save=true)
        {
            if (tribesmen.ContainsKey(tribesman.Player.PlayerId)) return Error.TribesmanAlreadyExists;
            if (tribesmen.Count >= Level * MEMBERS_PER_LEVEL) return Error.TribeFull;
            tribesman.Player.Tribesman = tribesman;
            tribesmen.Add(tribesman.Player.PlayerId, tribesman);
            if(save)
            {
                MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
                Global.DbManager.Save(tribesman);
            }
            return Error.Ok;
        }

        public Error RemoveTribesman(uint playerId)
        {
            Tribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman)) return Error.TribesmanNotFound;
            MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            tribesman.Player.Tribesman = null;
            Global.DbManager.Delete(tribesman);
            return !tribesmen.Remove(playerId) ? Error.TribesmanNotFound : Error.Ok;
        }

        public bool TryGetTribesman(uint playerId, out Tribesman tribesman)
        {
            return tribesmen.TryGetValue(playerId, out tribesman);
        }

        public Error SetRank(uint playerId, byte rank)
        {
            Tribesman tribesman;
            if (rank == 0) return Error.TribesmanNotAuthorized;
            if (!tribesmen.TryGetValue(playerId, out tribesman)) return Error.TribesmanNotFound;
            MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            if (IsOwner(tribesman.Player)) return Error.TribesmanIsOwner;

            tribesman.Rank = rank;
            Global.DbManager.Save(tribesman);
            return Error.Ok;
        }

        public bool HasRight(uint playerId, string action) {
            Tribesman tribesman;
            if (!TryGetTribesman(playerId, out tribesman)) return false;

            switch (action) {
                case "Request":
                    switch (tribesman.Rank) {
                        case 0:
                        case 1:
                            return true;
                        case 2:
                            return false;
                        default:
                            return false;
                    }
                case "Kick":
                    switch (tribesman.Rank) {
                        case 0:
                        case 1:
                            return true;
                        case 2:
                            return false;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        #region Properties
        public int Count {
            get {
                return tribesmen.Count;
            }
        } 
        #endregion


        #region ILockable Members

        public int Hash {
            get { return unchecked((int)Owner.PlayerId); }
        }

        public object Lock {
            get { return Owner; }
        }

        #endregion


        #region IEnumerable<Tribesman> Members

        public IEnumerator<Tribesman> GetEnumerator() {
            return tribesmen.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return tribesmen.Values.GetEnumerator();
        }

        #endregion

        public static bool IsNameValid(string tribeName)
        {
            return tribeName != string.Empty && tribeName.Length >= 3 && tribeName.Length <= 20 &&
                   Regex.IsMatch(tribeName, "^([a-z][a-z0-9\\s].*)$", RegexOptions.IgnoreCase);
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get
            {
                return new DbColumn[]{ new DbColumn("id",Id, DbType.UInt32)} ;
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        public DbColumn[] DbColumns {
            get
            {
                return new DbColumn[]
                       {
                               new DbColumn("name",Name,DbType.String, 20),                               
                               new DbColumn("level",Level,DbType.Byte), 
                               new DbColumn("owner_id",Owner.PlayerId,DbType.UInt32),
                       };
            }
        }

        #endregion
    }
}
