using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Game.Battle;
using Game.Logic;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.BarbarianTribe
{
    public class BarbarianTribe : SimpleGameObject, IBarbarianTribe
    {
        public event EventHandler<EventArgs> CampRemainsChanged = (sender, args) => { };

        public const string DB_TABLE = "barbarian_tribes";

        public IActionWorker Worker { get; private set; }

        public Resource Resource { private set; get; }
        
        public DateTime Created { get; set; }

        public DateTime LastAttacked { get; set; }
        
        private byte campRemains;

        public byte CampRemains
        {
            get
            {
                return campRemains;
            }
            set
            {
                campRemains = value;
                CampRemainsChanged(this, new EventArgs());
            }
        }

        private readonly IDbManager dbManager;

        public BarbarianTribe(uint id,
                              byte level,
                              uint x,
                              uint y,
                              int count,
                              Resource resources,
                              IDbManager dbManager,
                              IActionWorker actionWorker) 
            : base(id, x, y)
        {
            this.dbManager = dbManager;

            Lvl = level;
            Worker = actionWorker;
            Resource = resources;
            CampRemains = (byte)count;
            Created = DateTime.UtcNow;
            LastAttacked = DateTime.MinValue;

            Resource.StatsUpdate += ResourceOnStatsUpdate;

            actionWorker.LockDelegate = () => this;
            actionWorker.Location = this;
        }

        private void ResourceOnStatsUpdate()
        {
            CheckUpdateMode();
        }

        public IBattleManager Battle { get; set; }

        public override byte Size
        {
            get
            {
                return 2;
            }
        }

        public override ushort Type
        {
            get
            {
                return (ushort)Types.BarbarianTribe;
            }
        }

        public override uint GroupId
        {
            get
            {
                return (uint)SystemGroupIds.BarbarianTribe;
            }
        }

        protected override void CheckUpdateMode()
        {
            if (!DbPersisted || !Global.Current.FireEvents)
            {
                return;
            }

            if (!Updating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        protected override bool Update()
        {
            var updated = base.Update();

            if (updated)
            {
                dbManager.Save(this);
            }

            return updated;
        }

        public Position CityRegionLocation
        {
            get
            {
                return new Position(PrimaryPosition.X, PrimaryPosition.Y);
            }
        }

        public CityRegion.ObjectType CityRegionType {
            get
            {
                return CityRegion.ObjectType.BarbarianTribe;
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
                return ObjectId;
            }
        }

        public byte[] GetCityRegionObjectBytes()
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(Lvl);
                bw.Write(CampRemains);
                ms.Position = 0;
                return ms.ToArray();
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

        public string DbTable {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", ObjectId, DbType.UInt32)};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("level", Lvl, DbType.Byte), 
                        new DbColumn("camp_remains", CampRemains, DbType.Byte), 
                        new DbColumn("x", PrimaryPosition.X, DbType.UInt32), 
                        new DbColumn("y", PrimaryPosition.Y, DbType.UInt32),
                        new DbColumn("state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters", XmlSerializer.SerializeList(State.Parameters.ToArray()), DbType.String),
                        new DbColumn("resource_crop", Resource.Crop, DbType.Int32),
                        new DbColumn("resource_wood", Resource.Wood, DbType.Int32),
                        new DbColumn("resource_iron", Resource.Iron, DbType.Int32),
                        new DbColumn("resource_gold", Resource.Gold, DbType.Int32),
                        new DbColumn("in_world", InWorld, DbType.Boolean),
                        new DbColumn("last_attacked", LastAttacked, DbType.DateTime),
                        new DbColumn("created", Created, DbType.DateTime),
                };
            }
        }

        public byte Lvl { get; private set; }

        public uint WorkerId
        {
            get
            {
                return ObjectId;
            }
        }

        public uint IsBlocked { get; set; }

        public uint LocationId
        {
            get
            {
                return ObjectId;
            }
        }

        public LocationType LocationType
        {
            get
            {
                return LocationType.BarbarianTribe;
            }
        }

        #region Implementation of ILockable

        public int Hash
        {
            get
            {
                return (int)ObjectId;
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
    }
}
