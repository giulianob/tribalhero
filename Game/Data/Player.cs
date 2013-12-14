#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game.Comm;
using Game.Data.Tribe;
using Game.Database;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Data
{
    public class Player : IPlayer
    {
        public const string DB_TABLE = "players";

        public const int MAX_DESCRIPTION_LENGTH = 3000;

        private readonly List<ICity> list = new List<ICity>();

        private string description = string.Empty;

        private uint tribeRequest;

        private ITribesman tribesman;

        public PlayerChatState ChatState { get; private set; }

        public AchievementList Achievements { get; private set; }

        public DateTime LastDeletedTribe { get; set; }

        public Player(uint playerid,
                      DateTime created,
                      DateTime lastLogin,
                      string name,
                      string description,
                      PlayerRights playerRights,
                      string sessionId = "")
        {
            ChatState = new PlayerChatState();
            Achievements = new AchievementList(playerid);
            PlayerId = playerid;
            LastLogin = lastLogin;
            Created = created;
            Name = name;
            SessionId = sessionId;
            Rights = playerRights;
            ChatState.ChatFloodTime = DateTime.MinValue;
            this.description = description;
        }

        public Session Session { get; set; }

        public string Name { get; set; }

        public uint PlayerId { get; private set; }

        public uint TutorialStep { get; set; }

        public string PlayerHash
        {
            get
            {
                return BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(Config.api_id + Name)), 0, 3).Replace("-", String.Empty);
            }
        }

        public DateTime Created { get; private set; }

        public DateTime LastLogin { get; set; }

        public string SessionId { get; set; }

        public PlayerRights Rights { get; set; }

        public bool SoundMuted { get; set; }

        public bool IsIdle
        {
            get
            {
                return Session == null && SystemClock.Now.Subtract(LastLogin).TotalDays > Config.idle_days;
            }
        }

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
                    DbPersistance.Current.Query(
                        string.Format("UPDATE `{0}` SET `description` = @description WHERE `id` = @id LIMIT 1", DB_TABLE),
                        new[]
                        {
                                new DbColumn("description", description, DbType.String),
                                new DbColumn("id", PlayerId, DbType.UInt32)
                        });
                }
            }
        }

        public ITribesman Tribesman
        {
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
            get
            {
                return list.Sum(x => x.DefensePoint);
            }
        }

        public int Value
        {
            get
            {
                return list.Sum(x => x.Value);
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return Session != null;
            }
        }

        public DateTime Muted { get; set; }

        public bool Banned { get; set; }

        public bool IsInTribe
        {
            get
            {
                return tribesman != null;
            }
        }

        public void Add(ICity city)
        {
            list.Add(city);
        }

        public int GetCityCount(bool includeDeleted = false)
        {
            return list.Count(city => includeDeleted || city.Deleted == City.DeletedState.NotDeleted);
        }

        public IEnumerable<ICity> GetCityList(bool includeDeleted = false)
        {
            return list.Where(city => includeDeleted || city.Deleted == City.DeletedState.NotDeleted);
        }

        public ICity GetCity(uint id)
        {
            return list.Find(city => city.Id == id && city.Deleted == City.DeletedState.NotDeleted);
        }

        public void SendSystemMessage(IPlayer from, String subject, String message)
        {
            subject = String.Format("(System) {0}", subject);
            DbPersistance.Current.Query(
                                        "INSERT INTO `messages` (`sender_player_id`, `recipient_player_id`, `subject`, `message`, `sender_state`, `recipient_state`, `created`) VALUES (@sender_player_id, @recipient_player_id, @subject, @message, @sender_state, @recipient_state, UTC_TIMESTAMP())",
                                        new[]
                                        {
                                                new DbColumn("sender_player_id",
                                                             from == null ? 0 : from.PlayerId,
                                                             DbType.UInt32),
                                                new DbColumn("recipient_player_id", PlayerId, DbType.UInt32),
                                                new DbColumn("subject", subject, DbType.String),
                                                new DbColumn("message", message, DbType.String),
                                                new DbColumn("sender_state", 2, DbType.Int16),
                                                new DbColumn("recipient_state", 0, DbType.Int16),
                                        });

            if (Session != null)
            {
                var packet = new Packet(Command.RefreshUnread);
                Global.Current.Channel.Post("/PLAYER/" + PlayerId, packet);
            }
        }

        public void TribeUpdate()
        {
            if (!Global.Current.FireEvents)
            {
                return;
            }

            var packet = new Packet(Command.TribeChannelUpdate);
            packet.AddUInt32(Tribesman == null ? 0 : Tribesman.Tribe.Id);
            packet.AddUInt32(TribeRequest);
            packet.AddByte((byte)(Tribesman == null ? 0 : tribesman.Rank.Id));

            Global.Current.Channel.Post("/PLAYER/" + PlayerId, packet);
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
                    new DbColumn("name", Name, DbType.String, 32), 
                    new DbColumn("created", Created, DbType.DateTime),
                    new DbColumn("muted", Muted, DbType.DateTime),
                    new DbColumn("banned", Banned, DbType.Boolean),
                    new DbColumn("last_login", LastLogin, DbType.DateTime),
                    new DbColumn("session_id", SessionId, DbType.String, 128),
                    new DbColumn("rights", (int)Rights, DbType.UInt16),
                    new DbColumn("online", Session != null, DbType.Boolean),
                    new DbColumn("invitation_tribe_id", TribeRequest, DbType.UInt32),
                    new DbColumn("tutorial_step", TutorialStep, DbType.UInt32),
                    new DbColumn("last_deleted_tribe", LastDeletedTribe, DbType.DateTime),
                    new DbColumn("sound_muted", SoundMuted, DbType.Boolean),
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

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new[]
                {
                    new DbDependency("Achievements", true, true)
                };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}