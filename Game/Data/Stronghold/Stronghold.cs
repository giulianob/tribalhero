using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Map;
using Persistance;

namespace Game.Data.Stronghold
{
    class Stronghold : SimpleGameObject, IStronghold, IStation
    {
        public const string DB_TABLE = "strongholds";

        #region Implementation of IStronghold

        public uint Id { get; private set; }
        public string Name { get; private set; }
        public ushort Radius { get; private set; }
        public StrongholdState StrongholdState { get; set; }
        public LazyValue Gate { get; private set; }
        public ITroopManager Troops { get; private set; }

        private ITribe tribe;
        public ITribe Tribe
        {
            get
            {
                return tribe;
            }
            set
            {
                StrongholdState = value!=null ? StrongholdState.Occupied : StrongholdState.Neutral;
                tribe = value;
            }
        }

        #endregion

        #region Constructor

        public Stronghold(uint id, string name, byte level, uint x, uint y)
        {
            Id = id;
            Name = name;
            Lvl = level;
            this.x = x;
            this.y = y;
            Troops = new TroopManager(this, null);

            StrongholdState = StrongholdState.Inactive;
            Gate = new LazyValue(0);
        }

        #endregion

        #region Implementation of IHasLevel

        public byte Lvl { get; private set; }

        #endregion

        #region Implementation of ICityRegionObject

        public Location CityRegionLocation
        {
            get
            {
                return new Location(X, Y);
            }
        }

        public CityRegion.ObjectType CityRegionType
        {
            get
            {
                return CityRegion.ObjectType.Stronghold;
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
                bw.Write(Tribe != null ? Tribe.Id : 0);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        #endregion

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

        #region Overrides of SimpleGameObject

        public override ushort Type
        {
            get
            {
                return (ushort)Types.Stronghold;
            }
        }

        public override uint GroupId
        {
            get
            {
                return (uint)SystemGroupIds.Stronghold;
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
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            //DefaultMultiObjectLock.ThrowExceptionIfNotLocked(World.Current.Forests);
        }

        public override void EndUpdate()
        {
            if (!updating)
                throw new Exception("Called an endupdate without first calling a beginupdate");

            updating = false;

            Update();
        }

        #endregion

        #region Implementation of ILocation

        public uint LocationId
        {
            get
            {
                return Id;
            }
        }

        public StationType LocationType
        {
            get
            {
                return StationType.Stronghold;
            }
        }

        public uint LocationX
        {
            get
            {
                return x;
            }
        }

        public uint LocationY
        {
            get
            {
                return y;
            }
        }

        #endregion

        #region Implementation of IStation

        public ITroopManager TroopManager
        {
            get
            {
                return Troops;
            }
        }

        #endregion

        #region Implementation of IPersistable

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
                return new[] { new DbColumn("id", Id, DbType.UInt32) };
            }
        }

        public DbDependency[] DbDependencies
        {
            get
            {
                return new DbDependency[] { };
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("tribe_id", tribe == null ? 0 : tribe.Id, DbType.UInt32), 
                               new DbColumn("name", Name, DbType.String, 20),
                               new DbColumn("level", Lvl, DbType.Byte), 
                               new DbColumn("state",(byte)StrongholdState, DbType.Byte),
                               new DbColumn("gate", Gate.Value, DbType.Int32),
                               new DbColumn("x", x, DbType.UInt32),
                               new DbColumn("y", y, DbType.UInt32),
                       };
            }
        }

        #endregion

        #region Implementation of IPersistableObject

        public bool DbPersisted { get; set; }

        #endregion
        }
}
