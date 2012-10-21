using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Game.Battle;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
        
    class Stronghold : SimpleGameObject, IStronghold
    {
        private readonly IDbManager dbManager;
        private readonly Formula formula;

        public const string DB_TABLE = "strongholds";

        #region Implementation of IStronghold

        public uint Id { get; private set; }

        public string Name { get; private set; }

        public ushort Radius { get; private set; }

        public StrongholdState StrongholdState { get; set; }
        
        public decimal Gate { get; set; }

        public decimal VictoryPointRate
        {
            get
            {
                return (decimal)(DateTime.UtcNow.Subtract(DateOccupied).TotalDays/2 + 10);
            }
        }

        public DateTime DateOccupied { get; set; }

        public ITroopManager Troops { get; private set; }

        private ITribe tribe;

        public IBattleManager MainBattle { get; set; }

        public IBattleManager GateBattle { get; set; }

        public IActionWorker Worker { get; private set; }

        public IEnumerable<ILockable> LockList
        {
            get
            {
                var locks = new List<ILockable>
                {
                        this
                };

                if (Tribe != null)
                {
                    locks.Add(Tribe);
                }

                if (GateBattle != null)
                {
                    locks.AddRange(GateBattle.LockList);
                }

                if (MainBattle != null)
                {
                    locks.AddRange(MainBattle.LockList);
                }

                locks.AddRange(Troops.StationedHere());

                return locks.Distinct();
            }
        }

        public uint WorkerId
        {
            get
            {
                return Id;
            }
        }

        public bool IsBlocked { get; set; }

        public ITribe Tribe
        {
            get
            {
                return tribe;
            }
            set
            {
                StrongholdState = value != null ? StrongholdState.Occupied : StrongholdState.Neutral;
                tribe = value;
            }
        }

        public ITribe GateOpenTo { get; set; }

        #endregion

        #region Constructor

        public Stronghold(uint id, string name, byte level, uint x, uint y, decimal gate, IDbManager dbManager, Formula formula)
        {
            this.dbManager = dbManager;
            this.formula = formula;
            Id = id;
            Name = name;
            Lvl = level;
            this.x = x;
            this.y = y;
            Worker = new ActionWorker(() => this, this);
            Troops = new TroopManager(this, null);
            Gate = gate;
        }

        #endregion

        #region Implementation of IHasLevel

        public byte Lvl { get; private set; }

        #endregion

        #region Implementation of ICityRegionObject

        public Position CityRegionLocation
        {
            get
            {
                return new Position(X, Y);
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

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        public override void EndUpdate()
        {
            if (!updating)
                throw new Exception("Called an endupdate without first calling a beginupdate");

            updating = false;

            Update();            
        }

        protected new void Update()
        {
            base.Update();

            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            dbManager.Save(this);
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

        public LocationType LocationType
        {
            get
            {
                return LocationType.Stronghold;
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

        public IEnumerable<DbDependency> DbDependencies
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
                               new DbColumn("gate", Gate, DbType.Decimal),
                               new DbColumn("x", x, DbType.UInt32),
                               new DbColumn("y", y, DbType.UInt32),
                               new DbColumn("gate_open_to", GateOpenTo == null ? 0 : GateOpenTo.Id, DbType.UInt32),
                               new DbColumn("date_occupied", DateOccupied, DbType.DateTime),
                               new DbColumn("gate_battle_id", GateBattle != null ? GateBattle.BattleId : 0, DbType.UInt32),
                               new DbColumn("main_battle_id", MainBattle != null ? MainBattle.BattleId : 0, DbType.UInt32),
                               new DbColumn("object_state", (byte)State.Type, DbType.Boolean),
                               new DbColumn("state_parameters", XmlSerializer.SerializeList(State.Parameters.ToArray()), DbType.String)

                       };
            }
        }

        #endregion

        #region Implementation of IPersistableObject

        public bool DbPersisted { get; set; }

        #endregion
    }
}
