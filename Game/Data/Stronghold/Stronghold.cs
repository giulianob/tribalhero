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
using Game.Logic.Notifications;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    public class Stronghold : SimpleGameObject, IStronghold
    {
        public const string DB_TABLE = "strongholds";

        private readonly IDbManager dbManager;
        private readonly Formula formula;

        private ITribe gateOpenTo;

        private ITribe tribe;

        public event EventHandler<EventArgs> GateStatusChanged = (sender, args) => { };

        public string Name { get; private set; }

        public StrongholdState StrongholdState { get; set; }

        public decimal Gate { get; set; }

        public decimal VictoryPointRate
        {
            get
            {
                return formula.StrongholdVictoryPoint(this);
            }
        }

        public ushort NearbyCitiesCount { get; set; }

        public DateTime DateOccupied { get; set; }

        public decimal BonusDays { get; set; }

        public ITroopManager Troops { get; private set; }

        public IBattleManager MainBattle { get; set; }

        public IBattleManager GateBattle { get; set; }

        public IActionWorker Worker { get; private set; }

        public NotificationManager Notifications { get; private set; }

        public IEnumerable<ILockable> LockList
        {
            get
            {
                var locks = new List<ILockable> {this};

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

        public byte Lvl { get; set; }

        public uint WorkerId
        {
            get
            {
                return ObjectId;
            }
        }

        public uint IsBlocked { get; set; }

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

        public ITribe GateOpenTo
        {
            get
            {
                return gateOpenTo;
            }
            set
            {
                gateOpenTo = value;
                GateStatusChanged(this, new EventArgs());
            }
        }

        #region Constructor

        public Stronghold(uint id,
                          string name,
                          byte level,
                          uint x,
                          uint y,
                          decimal gate,
                          IDbManager dbManager,
                          NotificationManager notificationManager,
                          ITroopManager troopManager,
                          IActionWorker actionWorker,
                          Formula formula)
                : base(id, x, y)
        {
            Notifications = notificationManager;
            this.dbManager = dbManager;
            this.formula = formula;
            
            Name = name;
            Lvl = level;
            Worker = actionWorker;
            Troops = troopManager;
            Gate = gate;

            Notifications.NotificationAdded += NotificationsUpdate;
            Notifications.NotificationRemoved += NotificationsUpdate;
            Notifications.NotificationUpdated += NotificationsUpdate;
        }

        private void NotificationsUpdate(object sender, NotificationEventArgs notificationEventArgs)
        {
            if (Tribe != null)
            {
                Tribe.SendUpdate();
            }
        }

        #endregion

        #region Implementation of ICityRegionObject

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
                return ObjectId;
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

        public override int Hash
        {
            get
            {
                return (int)ObjectId;
            }
        }

        public override object Lock
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region Overrides of SimpleGameObject

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

        protected override void CheckUpdateMode()
        {
            if (!Global.Current.FireEvents)
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

        #endregion

        #region Implementation of ILocation

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
                return new[] {new DbColumn("id", ObjectId, DbType.UInt32)};
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
                return new[]
                {
                        new DbColumn("tribe_id", tribe == null ? 0 : tribe.Id, DbType.UInt32),
                        new DbColumn("name", Name, DbType.String, 20), 
                        new DbColumn("level", Lvl, DbType.Byte),
                        new DbColumn("state", (byte)StrongholdState, DbType.Byte),
                        new DbColumn("gate", Gate, DbType.Decimal), 
                        new DbColumn("x", PrimaryPosition.X, DbType.UInt32),
                        new DbColumn("y", PrimaryPosition.Y, DbType.UInt32),
                        new DbColumn("gate_open_to", GateOpenTo == null ? 0 : GateOpenTo.Id, DbType.UInt32),
                        new DbColumn("date_occupied", DateOccupied, DbType.DateTime),
                        new DbColumn("gate_battle_id", GateBattle != null ? GateBattle.BattleId : 0, DbType.UInt32),
                        new DbColumn("main_battle_id", MainBattle != null ? MainBattle.BattleId : 0, DbType.UInt32),
                        new DbColumn("object_state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters", XmlSerializer.SerializeList(State.Parameters.ToArray()), DbType.String),
                        new DbColumn("victory_point_rate", VictoryPointRate, DbType.Decimal),
                        new DbColumn("nearby_cities", NearbyCitiesCount, DbType.UInt16),
                        new DbColumn("bonus_days", BonusDays, DbType.Decimal),
                };
            }
        }

        #endregion

        #region Implementation of IPersistableObject

        public bool DbPersisted { get; set; }

        #endregion
    }
}