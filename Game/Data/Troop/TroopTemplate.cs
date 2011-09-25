﻿#region

using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Battle;
using Game.Data.Stats;
using Persistance;

#endregion

namespace Game.Data.Troop
{
    public class TroopTemplate : IPersistableList, IEnumerable<KeyValuePair<ushort, BattleStats>>
    {
        public const string DB_TABLE = "troop_templates";
        private readonly TroopStub stub;
        private Dictionary<ushort, BattleStats> stats = new Dictionary<ushort, BattleStats>();

        public TroopTemplate(TroopStub stub)
        {
            this.stub = stub;
        }

        public byte Count
        {
            get
            {
                return (byte)stats.Count;
            }
        }

        public BattleStats this[ushort type]
        {
            get
            {
                return stats[type];
            }
        }

        #region IEnumerable<KeyValuePair<ushort,BattleStats>> Members

        IEnumerator<KeyValuePair<ushort, BattleStats>> IEnumerable<KeyValuePair<ushort, BattleStats>>.GetEnumerator()
        {
            return stats.GetEnumerator();
        }

        #endregion

        #region IPersistableList Members

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
                return new[] {new DbColumn("city_id", stub.TroopManager.City.Id, DbType.UInt32), new DbColumn("troop_stub_id", stub.TroopId, DbType.UInt32),};
            }
        }

        public DbDependency[] DbDependencies
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

        public DbColumn[] DbListColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("type", DbType.UInt16), new DbColumn("level", DbType.Byte), new DbColumn("max_hp", DbType.UInt16),
                               new DbColumn("attack", DbType.UInt16), new DbColumn("splash", DbType.Byte),
                               new DbColumn("range", DbType.Byte), new DbColumn("stealth", DbType.Byte), new DbColumn("speed", DbType.Byte),
                               new DbColumn("carry", DbType.UInt16), 
                       };
            }
        }

        public bool DbPersisted { get; set; }

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator()
        {
            Dictionary<ushort, BattleStats>.Enumerator itr = stats.GetEnumerator();
            while (itr.MoveNext())
            {
                BattleStats battleStats = itr.Current.Value;
                yield return
                        new[]
                        {
                                new DbColumn("type", battleStats.Base.Type, DbType.UInt16), new DbColumn("level", battleStats.Base.Lvl, DbType.Byte),
                                new DbColumn("max_hp", battleStats.MaxHp, DbType.UInt16), new DbColumn("attack", battleStats.Atk, DbType.UInt16),
                                new DbColumn("splash", battleStats.Splash, DbType.Byte), new DbColumn("range", battleStats.Rng, DbType.Byte), new DbColumn("stealth", battleStats.Stl, DbType.Byte),
                                new DbColumn("speed", battleStats.Spd, DbType.Byte), new DbColumn("carry", battleStats.Carry, DbType.UInt16), 
                        };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stats.GetEnumerator();
        }

        #endregion

        public void ClearStats()
        {
            stats = new Dictionary<ushort, BattleStats>();

            stub.FireUpdated();
        }

        /// <summary>
        ///   Must call begin/end update on the troop stub that owns this template
        /// </summary>
        public void LoadStats(TroopBattleGroup group)
        {
            stats = new Dictionary<ushort, BattleStats>();

            foreach (var formation in stub)
            {
                foreach (var type in formation.Keys)
                {
                    if (stats.ContainsKey(type))
                        continue;

                    BattleStats stat = BattleFormulas.LoadStats(type, stub.City.Template[type].Lvl, stub.City, group);
                    stats.Add(type, stat);
                }
            }

            stub.FireUpdated();
        }

        public void DbLoaderAdd(BattleStats battleStats)
        {
            stats.Add(battleStats.Base.Type, battleStats);
        }
    }
}