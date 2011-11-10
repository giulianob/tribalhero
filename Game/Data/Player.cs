#region

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
using Persistance;

#endregion

namespace Game.Data
{
    public class Player : ILockable, IPersistableObject
    {
        public const string DB_TABLE = "players";
        public const int MAX_DESCRIPTION_LENGTH = 3000;
        private readonly List<City> list = new List<City>();

        public Player(uint playerid, DateTime created, DateTime lastLogin, string name, string description, bool admin)
                : this(playerid, created, lastLogin, name, description, admin, string.Empty)
        {
        }

        public Player(uint playerid, DateTime created, DateTime lastLogin, string name, string description,bool admin, string sessionId)
        {
            PlayerId = playerid;
            LastLogin = lastLogin;
            Created = created;
            Name = name;
            SessionId = sessionId;
            Admin = admin;
            ChatFloodTime = DateTime.MinValue;
            this.description = description;
        }

        public Session Session { get; set; }

        public string Name { get; private set; }

        public uint PlayerId { get; private set; }

        public DateTime Created { get; private set; }

        public DateTime LastLogin { get; set; }

        public string SessionId { get; set; }

        public bool Admin { get; set; }

        public DateTime ChatFloodTime { get; set; }

        public int ChatFloodCount { get; set; }

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
                    Ioc.Kernel.Get<IDbManager>().Query(string.Format("UPDATE `{0}` SET `description` = @description WHERE `id` = @id LIMIT 1", DB_TABLE),
                                           new[] { new DbColumn("description", description, DbType.String), new DbColumn("id", PlayerId, DbType.UInt32) });
                }
            }
        }

        private Tribe.Tribesman tribesman;
        public Tribe.Tribesman Tribesman {
            get
            {
                return tribesman;
            }
            set 
            {
                tribesman = value;
                TribeUpdate();
            }
        }

        private uint tribeRequest;
        public uint TribeRequest
        {
            get
            {
                return tribeRequest;
            }  
            set
            {
                tribeRequest = value;
                TribeUpdate();
            }
        }

        public int AttackPoint
        {
            get
            {
                return list.Sum(x => x.AttackPoint);
            }
        }

        public int DefensePoint
        {
            get {
                return list.Sum(x => x.DefensePoint);
            }
        }

        #region ILockable Members

        public int Hash
        {
            get
            {
                return unchecked((int)PlayerId);
            }
        }

        public object Lock
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region IPersistableObject Members

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("name", Name, DbType.String, 32), new DbColumn("created", Created, DbType.DateTime),
                               new DbColumn("last_login", LastLogin, DbType.DateTime), new DbColumn("session_id", SessionId, DbType.String, 128),
                               new DbColumn("online", Session != null, DbType.Boolean), new DbColumn("admin", Admin, DbType.Boolean),
                               new DbColumn("invitation_tribe_id", TribeRequest, DbType.UInt32), 
                       };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", PlayerId, DbType.UInt32)};
            }
        }

        public DbDependency[] DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public void Add(City city)
        {
            list.Add(city);
        }

        internal int GetCityCount(bool includeDeleted = false)
        {
            return list.Count(city => includeDeleted || city.Deleted == City.DeletedState.NotDeleted);
        }

        internal IEnumerable<City> GetCityList(bool includeDeleted = false)
        {
            return list.Where(city => includeDeleted || city.Deleted == City.DeletedState.NotDeleted);
        }

        internal City GetCity(uint id)
        {
            return list.Find(city => city.Id == id && city.Deleted == City.DeletedState.NotDeleted);
        }

        public override string ToString() {
            return Name;
        }

        public void SendSystemMessage(Player from, String subject, String message)
        {
            subject = String.Format("(System) {0}", subject);
            Ioc.Kernel.Get<IDbManager>().Query(
                                   "INSERT INTO `messages` (`sender_player_id`, `recipient_player_id`, `subject`, `message`, `sender_state`, `recipient_state`, `created`) VALUES (@sender_player_id, @recipient_player_id, @subject, @message, @sender_state, @recipient_state, UTC_TIMESTAMP())",
                                   new[]
                                   {
                                           new DbColumn("sender_player_id", from == null ? 0 : from.PlayerId, DbType.UInt32),
                                           new DbColumn("recipient_player_id", PlayerId, DbType.UInt32), new DbColumn("subject", subject, DbType.String),
                                           new DbColumn("message", message, DbType.String), new DbColumn("sender_state", 2, DbType.Int16),
                                           new DbColumn("recipient_state", 0, DbType.Int16),
                                   });
        }

        public void TribeUpdate()
        {
            if (!Global.FireEvents )
                return;

            var packet = new Packet(Command.TribeChannelUpdate);
            packet.AddUInt32(Tribesman == null ? 0 : Tribesman.Tribe.Id);
            packet.AddUInt32(TribeRequest);
            packet.AddByte((byte)(Tribesman == null ? 0 : tribesman.Rank));
            Global.Channel.Post("/PLAYER/" + PlayerId, packet);
        }
    }
}