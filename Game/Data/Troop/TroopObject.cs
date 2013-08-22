#region

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Game.Data.Stats;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Data.Troop
{
    public class TroopObject : GameObject, ITroopObject
    {
        private readonly IDbManager dbManager;

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

        public override byte Size
        {
            get
            {
                return 1;
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

        public TroopObject(uint id, ITroopStub stub, uint x, uint y, IDbManager dbManager) 
            : base(id, x, y)
        {
            this.dbManager = dbManager;
            Stub = stub;
        }

        #endregion

        #region Updates

        private void StatsStatsUpdate()
        {
            CheckUpdateMode();
        }

        protected override bool Update()
        {
            var update = base.Update();

            if (update && ObjectId > 0)
            {
                dbManager.Save(this);
            }

            return update;
        }

        #endregion

        public uint MiniMapGroupId
        {
            get
            {
                return GroupId;
            }
        }

        public uint MiniMapObjectId
        {
            get
            {
                return ObjectId;
            }
        }

        public byte[] GetMiniMapObjectBytes()
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

        public MiniMapRegion.ObjectType MiniMapObjectType
        {
            get
            {
                return MiniMapRegion.ObjectType.Troop;
            }
        }

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
                        new DbColumn("is_blocked", IsBlocked, DbType.UInt32),
                        new DbColumn("troop_stub_id", Stub != null ? Stub.TroopId : 0, DbType.UInt16),
                        new DbColumn("gold", Stats.Loot.Gold, DbType.Int32),
                        new DbColumn("crop", Stats.Loot.Crop, DbType.Int32),
                        new DbColumn("wood", Stats.Loot.Wood, DbType.Int32),
                        new DbColumn("iron", Stats.Loot.Iron, DbType.Int32),
                        new DbColumn("attack_point", Stats.AttackPoint, DbType.Int32),
                        new DbColumn("attack_radius", Stats.AttackRadius, DbType.Byte),
                        new DbColumn("speed", Stats.Speed, DbType.Decimal), 
                        new DbColumn("x", PrimaryPosition.X, DbType.UInt32),
                        new DbColumn("y", PrimaryPosition.Y, DbType.UInt32), 
                        new DbColumn("target_x", TargetX, DbType.UInt32),
                        new DbColumn("target_y", TargetY, DbType.UInt32),
                        new DbColumn("in_world", InWorld, DbType.Boolean),
                        new DbColumn("state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters", XmlSerializer.SerializeList(State.Parameters.ToArray()), DbType.String)
                };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[]
                {
                        new DbColumn("id", ObjectId, DbType.UInt32),
                        new DbColumn("city_id", City.Id, DbType.UInt32)
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

        public bool DbPersisted { get; set; }

        #endregion

        public override int Hash
        {
            get
            {
                return City != null ? City.Hash : int.MaxValue;
            }
        }

        public override object Lock
        {
            get
            {
                return City != null ? City.Lock : null;
            }
        }
    }
}