using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

        public uint Id { get; private set; }

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

        public BarbarianTribe(uint id, byte level, uint x, uint y, int count, Resource resources, IDbManager dbManager, IActionWorker actionWorker)
        {
            this.dbManager = dbManager;
            this.x = x;
            this.y = y;

            Id = id;
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

        public override uint ObjectId
        {
            get
            {
                return Id;
            }            
        }

        public override void CheckUpdateMode()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            if (!updating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        public override void EndUpdate()
        {
            if (!updating)
            {
                throw new Exception("Called an endupdate without first calling a beginupdate");
            }

            updating = false;

            Update();
        }

        protected override void Update()
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

            dbManager.Save(this);
        }

        public Position CityRegionLocation
        {
            get
            {
                return new Position(X, Y);
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
                return Id;
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
                return new[] {new DbColumn("id", Id, DbType.UInt32)};
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
                        new DbColumn("x", x, DbType.UInt32), 
                        new DbColumn("y", y, DbType.UInt32),
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
                return Id;
            }
        }

        public uint IsBlocked { get; set; }

        public uint LocationId
        {
            get
            {
                return Id;
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
                return (int)Id;
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
