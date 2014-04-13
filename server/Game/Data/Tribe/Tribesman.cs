using System;
using System.Collections.Generic;
using System.Data;
using Persistance;

namespace Game.Data.Tribe
{
    public class Tribesman : ITribesman
    {
        public const string DB_TABLE = "tribesmen";

        public Tribesman(ITribe tribe, IPlayer player, ITribeRank rank)
        {
            Tribe = tribe;
            Player = player;
            JoinDate = DateTime.UtcNow;
            Rank = rank;
            Contribution = new Resource();
        }

        public Tribesman(ITribe tribe, IPlayer player, DateTime joinDate, Resource contribution, ITribeRank rank)
        {
            Tribe = tribe;
            Player = player;
            JoinDate = joinDate;
            Contribution = contribution;
            Rank = rank;
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

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
                return new[] {new DbColumn("player_id", Player.PlayerId, DbType.UInt32),};
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
                return new[]
                {
                        new DbColumn("tribe_id", Tribe.Id, DbType.UInt32), new DbColumn("join_date", JoinDate, DbType.DateTime)
                        , new DbColumn("rank", Rank.Id, DbType.Byte), new DbColumn("crop", Contribution.Crop, DbType.Int32)
                        , new DbColumn("gold", Contribution.Gold, DbType.Int32),
                        new DbColumn("iron", Contribution.Iron, DbType.Int32),
                        new DbColumn("wood", Contribution.Wood, DbType.Int32),
                };
            }
        }

        #endregion

        #region ILockable Members

        public int Hash
        {
            get
            {
                return unchecked((int)Player.PlayerId);
            }
        }

        public object Lock
        {
            get
            {
                return Player;
            }
        }

        #endregion

        public ITribe Tribe { get; private set; }

        public IPlayer Player { get; private set; }

        public DateTime JoinDate { get; private set; }

        public Resource Contribution { get; set; }

        public ITribeRank Rank { get; set; }
    }
}