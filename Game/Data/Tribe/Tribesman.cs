using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Game.Database;
using Game.Util;

namespace Game.Data.Tribe {

    public class Tribesman: IPersistableObject  {
        public const string DB_TABLE = "tribesmen";

        public Tribe Tribe { get; private set; }
        public Player Player { get; private set; }
	    public DateTime JoinDate { get; private set; }
        public Resource Contribution { get; private set; }
        public byte Rank { get; set; }

        public Tribesman(Tribe tribe, Player player, byte rank)
        {
            Tribe = tribe;
            Player = player;
            JoinDate = DateTime.UtcNow;
            Contribution = new Resource();
        }

        public Tribesman(Tribe tribe, Player player, DateTime joinDate, Resource contribution, byte rank) {
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

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get
            {
                return new[] { new DbColumn("player_id", Player.PlayerId, DbType.UInt32), };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[]{}; }
        }

        public DbColumn[] DbColumns {
            get {
                return new[]
                       {
                            new DbColumn("tribe_id", Tribe.Id, DbType.UInt32),
                            new DbColumn("join_date", JoinDate, DbType.DateTime), 
                            new DbColumn("rank",Rank, DbType.Byte),
                            new DbColumn("crop",Contribution.Crop,DbType.Int32),
                            new DbColumn("gold",Contribution.Gold,DbType.Int32),
                            new DbColumn("iron",Contribution.Iron,DbType.Int32),
                            new DbColumn("wood",Contribution.Wood,DbType.Int32),
                       };
            }
        }

        #endregion
    }
}
