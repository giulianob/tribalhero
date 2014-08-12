using System.Collections.Generic;
using System.Data;
using System.Linq;
using Persistance;

namespace Game.Data
{
    public class AchievementList : List<Achievement>, IPersistableList
    {
        private readonly uint playerId;

        public const string DB_TABLE = "achievements";

        public AchievementList(uint playerId)
        {
            this.playerId = playerId;
        }

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[]
                {
                        new DbColumn("player_id", playerId, DbType.UInt32)
                };
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new DbColumn[] {};
            }
        }

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("id", DbType.Int32),
                        new DbColumn("title", DbType.String),
                        new DbColumn("description", DbType.String),
                        new DbColumn("type", DbType.String),
                        new DbColumn("icon", DbType.String),
                        new DbColumn("tier", DbType.Byte),
                };
            }
        }
        
        public IDictionary<AchievementTier, byte> GetAchievementCountByTier()
        {
            return this.GroupBy(p => p.Tier).ToDictionary(p => p.Key, p =>
                {
                    var cnt = p.Count();
                    return cnt > byte.MaxValue ? byte.MaxValue : (byte)cnt;
                });
        }

        public bool DbPersisted { get; set; }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            return this.Select(achievement => new[] 
            {
                    new DbColumn("id", achievement.Id, DbType.Int32),
                    new DbColumn("title", achievement.Title, DbType.String),
                    new DbColumn("description", achievement.Description, DbType.String),
                    new DbColumn("type", achievement.Type, DbType.String),
                    new DbColumn("icon", achievement.Icon, DbType.String),
                    new DbColumn("tier", achievement.Tier, DbType.Byte),
            });
        }
    }
}