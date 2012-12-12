#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Battle.CombatGroups;
using Persistance;

#endregion

namespace Game.Battle.Reporting
{
    public class ReportedGroups : Dictionary<ICombatGroup, uint>, IPersistableList
    {
        public const string DB_TABLE = "reported_groups";

        private readonly uint battleId;

        public ReportedGroups(uint battleId)
        {
            this.battleId = battleId;
        }

        #region IPersistableList Members

        public bool DbPersisted { get; set; }

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
                return new[] {new DbColumn("battle_id", battleId, DbType.UInt32)};
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
                return new[] {new DbColumn("combat_troop_id", DbType.UInt32), new DbColumn("group_id", DbType.UInt32)};
            }
        }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            return
                    this.Select(
                                reportedObject =>
                                new[]
                                {
                                        new DbColumn("combat_troop_id", reportedObject.Value, DbType.UInt32),
                                        new DbColumn("group_id", reportedObject.Key.Id, DbType.UInt32)
                                });
        }

        #endregion
    }
}