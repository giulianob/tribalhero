#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Game.Data.Stats;
using Game.Database;
using Game.Map;
using Game.Util;
using Persistance;

#endregion

namespace Game.Data.Troop
{
    public class TroopObject : GameObject, ITroopObject
    {
        public const string DB_TABLE = "troops";

        private TroopStats stats = new TroopStats(0, 0);

        private uint targetX;

        private uint targetY;

        public ITroopStub Stub { get; set; }

        public uint TargetX
        {
            get
            {
                return targetX;
            }
            set
            {
                CheckUpdateMode();
                targetX = value;
            }
        }

        public uint TargetY
        {
            get
            {
                return targetY;
            }
            set
            {
                CheckUpdateMode();
                targetY = value;
            }
        }

        public TroopStats Stats
        {
            get
            {
                return stats;
            }
            set
            {
                if (stats != null)
                {
                    Stats.StatsUpdate -= StatsStatsUpdate;
                }

                stats = value;
                stats.StatsUpdate += StatsStatsUpdate;
            }
        }

        public override uint ObjectId
        {
            get
            {
                return objectId;
            }
            set
            {
                CheckUpdateMode();
                objectId = value;
            }
        }

        public override ushort Type
        {
            get
            {
                return (ushort)Types.Troop;
            }
        }

        #region Constructors

        public TroopObject(ITroopStub stub)
        {
            Stub = stub;
        }

        #endregion

        #region Updates

        public override void EndUpdate()
        {
            if (!updating)
            {
                throw new Exception("Called an endupdate without first calling a beginupdate");
            }

            updating = false;

            Update();
        }

        private void StatsStatsUpdate()
        {
            CheckUpdateMode();
        }

        protected new void Update()
        {
            base.Update();

            if (!Global.FireEvents)
            {
                return;
            }

            if (updating)
            {
                return;
            }

            if (objectId > 0)
            {
                DbPersistance.Current.Save(this);
            }
        }

        #endregion

        #region Implementation of ICityRegionObject

        public Position CityRegionLocation
        {
            get
            {
                return new Position(X, Y);
            }
        }

        public uint CityRegionGroupId
        {
            get
            {
                return GroupId;
            }
        }

        public uint CityRegionObjectId
        {
            get
            {
                return objectId;
            }
        }

        public byte[] GetCityRegionObjectBytes()
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(Stub.TroopId);
                bw.Write(Stub.City.Owner.Tribesman == null ? 0 : Stub.City.Owner.Tribesman.Tribe.Id);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public CityRegion.ObjectType CityRegionType
        {
            get
            {
                return CityRegion.ObjectType.Troop;
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
                        new DbColumn("is_blocked", IsBlocked, DbType.Boolean),
                        new DbColumn("troop_stub_id", Stub != null ? Stub.TroopId : 0, DbType.Byte),
                        new DbColumn("gold", Stats.Loot.Gold, DbType.Int32),
                        new DbColumn("crop", Stats.Loot.Crop, DbType.Int32),
                        new DbColumn("wood", Stats.Loot.Wood, DbType.Int32),
                        new DbColumn("iron", Stats.Loot.Iron, DbType.Int32),
                        new DbColumn("attack_point", Stats.AttackPoint, DbType.Int32),
                        new DbColumn("attack_radius", Stats.AttackRadius, DbType.Byte),
                        new DbColumn("speed", Stats.Speed, DbType.Decimal), new DbColumn("x", X, DbType.UInt32),
                        new DbColumn("y", Y, DbType.UInt32), new DbColumn("target_x", TargetX, DbType.UInt32),
                        new DbColumn("target_y", TargetY, DbType.UInt32),
                        new DbColumn("in_world", InWorld, DbType.Boolean),
                        new DbColumn("state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters",
                                     XmlSerializer.SerializeList(State.Parameters.ToArray()),
                                     DbType.String)
                };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[]
                {new DbColumn("id", ObjectId, DbType.UInt32), new DbColumn("city_id", City.Id, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}