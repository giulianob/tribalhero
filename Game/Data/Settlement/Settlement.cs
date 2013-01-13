using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Settlement
{
    public class Settlement : SimpleGameObject, ISettlement
    {
        public const string DB_TABLE = "settlements";

        public uint Id { get; private set; }

        private readonly IDbManager dbManager;

        public Settlement(uint id, string name, byte level, uint x, uint y, int count, IDbManager dbManager)
        {
            Id = id;
            this.dbManager = dbManager;
            this.x = x;
            this.y = y;
            Lvl = level;
        }

        public override ushort Type
        {
            get
            {
                return (ushort)Types.Settlement;
            }
        }

        public override uint GroupId
        {
            get
            {
                return (uint)SystemGroupIds.Settlement;
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

         //   dbManager.Save(this);
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
                return CityRegion.ObjectType.Settlement;
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
                        new DbColumn("level", Lvl, DbType.Byte), new DbColumn("x", x, DbType.UInt32), new DbColumn("y", y, DbType.UInt32),
                        new DbColumn("object_state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters", XmlSerializer.SerializeList(State.Parameters.ToArray()), DbType.String)
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

        public bool IsBlocked { get; set; }

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
                return LocationType.Settlement;
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

        public Settlement(uint id, uint x, uint y, IDbManager dbManager)
                : base(x, y)
        {
            Id = id;
            this.dbManager = dbManager;
        }
    }
}
