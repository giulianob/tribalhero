#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using Game.Battle;
using Game.Data.Events;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Notifications;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Data
{
    public class City : ICity
    {
        public enum DeletedState
        {
            NotDeleted,

            Deleting,

            Deleted,
        }

        #region Events

        public delegate void CityEventHandler<TEventArgs>(ICity city, TEventArgs e);
        
        public event CityEventHandler<PropertyChangedEventArgs> PropertyChanged = (sender, args) => { };
                     
        public event CityEventHandler<TroopStubEventArgs> TroopUnitUpdated = (sender, args) => { };
                     
        public event CityEventHandler<TroopStubEventArgs> TroopUpdated = (sender, args) => { };
                     
        public event CityEventHandler<TroopStubEventArgs> TroopRemoved = (sender, args) => { };
                     
        public event CityEventHandler<TroopStubEventArgs> TroopAdded = (sender, args) => { };
                     
        public event CityEventHandler<ActionWorkerEventArgs> ActionRemoved = (sender, args) => { };
                     
        public event CityEventHandler<ActionWorkerEventArgs> ActionStarted = (sender, args) => { };
                     
        public event CityEventHandler<ActionWorkerEventArgs> ActionRescheduled = (sender, args) => { };
                     
        public event CityEventHandler<EventArgs> ResourcesUpdated = (sender, args) => { };
                     
        public event CityEventHandler<EventArgs> UnitTemplateUpdated = (sender, args) => { };
                     
        public event CityEventHandler<TechnologyEventArgs> TechnologyCleared = (sender, args) => { };
                     
        public event CityEventHandler<TechnologyEventArgs> TechnologyAdded = (sender, args) => { };
                     
        public event CityEventHandler<TechnologyEventArgs> TechnologyRemoved = (sender, args) => { };
                     
        public event CityEventHandler<TechnologyEventArgs> TechnologyUpgraded = (sender, args) => { };
                     
        public event CityEventHandler<GameObjectArgs> ObjectAdded = (sender, args) => { };
                     
        public event CityEventHandler<GameObjectArgs> ObjectRemoved = (sender, args) => { };

        public event CityEventHandler<GameObjectArgs> ObjectUpdated = (sender, args) => { };

        public event CityEventHandler<ActionReferenceArgs> ReferenceRemoved = (sender, args) => { };

        public event CityEventHandler<ActionReferenceArgs> ReferenceAdded = (sender, args) => { };

        #endregion

        public const string DB_TABLE = "cities";

        private readonly object objLock = new object();

        private readonly Dictionary<uint, IStructure> structures = new Dictionary<uint, IStructure>();

        private readonly Dictionary<uint, ITroopObject> troopobjects = new Dictionary<uint, ITroopObject>();

        private readonly LargeIdGenerator objectIdGen = new LargeIdGenerator(uint.MaxValue);

        private decimal alignmentPoint;

        private int attackPoint;

        private IBattleManager battle;

        private int defensePoint;

        private bool hideNewUnits;

        private string name;        

        private byte radius;

        private readonly ITroopStubFactory troopStubFactory;

        private readonly IDbManager dbManager;

        private readonly IGameObjectFactory gameObjectFactory;

        private readonly IActionFactory actionFactory;

        private ushort value;

        #region Properties

        /// <summary>
        ///     Returns the town center
        /// </summary>
        public IStructure MainBuilding
        {
            get
            {
                IStructure mainBuilding;
                return !structures.TryGetValue(1, out mainBuilding) ? null : mainBuilding;
            }
        }

        /// <summary>
        ///     Enumerates only through structures in this city
        /// </summary>
        public Dictionary<uint, IStructure>.Enumerator Structures
        {
            get
            {
                return structures.GetEnumerator();
            }
        }

        /// <summary>
        ///     Radius of city. This affects city wall and where user can build.
        /// </summary>
        public byte Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value == radius)
                {
                    return;
                }

                radius = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Radius"));
            }
        }

        public NotificationManager Notifications { get; private set; }

        public ReferenceManager References { get; private set; }

        public byte Lvl
        {
            get
            {
                return (byte)(MainBuilding == null ? 1 : MainBuilding.Lvl);
            }
        }

        /// <summary>
        ///     Returns the city's center point which is the town centers position
        /// </summary>
        public uint X
        {
            get
            {
                return MainBuilding == null ? 0 : MainBuilding.X;
            }
            set
            {
                throw new NotSupportedException("Cannot set city X");
            }
        }

        /// <summary>
        ///     Returns the city's center point which is the town centers position
        /// </summary>
        public uint Y
        {
            get
            {
                return MainBuilding == null ? 0 : MainBuilding.Y;
            }
            set
            {
                throw new NotSupportedException("Cannot set city Y");
            }
        }

        /// <summary>
        ///     City's battle manager. Maybe null if city is not in battle.
        /// </summary>
        public IBattleManager Battle
        {
            get
            {
                return battle;
            }
            set
            {
                battle = value;

                PropertyChanged(this, new PropertyChangedEventArgs("Battle"));
            }
        }

        /// <summary>
        ///     Enumerates through all troop objects in this city
        /// </summary>
        public IEnumerable<ITroopObject> TroopObjects
        {
            get
            {
                return troopobjects.Values;
            }
        }

        /// <summary>
        ///     Troop manager which manages all troop stubs in city
        /// </summary>
        public ITroopManager Troops { get; private set; }

        /// <summary>
        ///     Technology manager for city
        /// </summary>
        public ITechnologyManager Technologies { get; private set; }

        /// <summary>
        ///     Returns the local troop
        /// </summary>
        public ITroopStub DefaultTroop
        {
            get
            {
                return Troops[1];
            }
            set
            {
                Troops[1] = value;
            }
        }

        /// <summary>
        ///     Returns unit template. Unit template holds levels for all units in the city.
        /// </summary>
        public IUnitTemplate Template { get; private set; }

        /// <summary>
        ///     Resource available in the city
        /// </summary>
        public ILazyResource Resource { get; private set; }

        /// <summary>
        ///     Amount of loot this city has stolen from other players
        /// </summary>
        public uint LootStolen { get; set; }

        /// <summary>
        ///     Unique city id
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        ///     City name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                CheckUpdateMode();
                name = value;
            }
        }

        /// <summary>
        ///     Player that owns this city
        /// </summary>
        public IPlayer Owner { get; private set; }

        /// <summary>
        ///     Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public IGameObject this[uint objectId]
        {
            get
            {
                IStructure structure;
                if (structures.TryGetValue(objectId, out structure))
                {
                    return structure;
                }

                ITroopObject troop;
                if (troopobjects.TryGetValue(objectId, out troop))
                {
                    return troop;
                }

                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        ///     Whether to send new units to hiding or not
        /// </summary>
        public bool HideNewUnits
        {
            get
            {
                return hideNewUnits;
            }
            set
            {
                CheckUpdateMode();
                hideNewUnits = value;

                PropertyChanged(this, new PropertyChangedEventArgs("HideNewUnits"));
            }
        }

        /// <summary>
        ///     Attack points earned by this city
        /// </summary>
        public int AttackPoint
        {
            get
            {
                return attackPoint;
            }
            set
            {
                CheckUpdateMode();
                attackPoint = value;
                PropertyChanged(this, new PropertyChangedEventArgs("AttackPoint"));
            }
        }

        /// <summary>
        ///     Defense points earned by this city
        /// </summary>
        public int DefensePoint
        {
            get
            {
                return defensePoint;
            }
            set
            {
                CheckUpdateMode();
                defensePoint = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DefensePoint"));
            }
        }

        public decimal AlignmentPoint
        {
            get
            {
                return Owner.IsIdle ? 50 : alignmentPoint;
            }
            set
            {
                if (Owner.IsIdle)
                {
                    value = 50;
                }

                CheckUpdateMode();
                alignmentPoint = Math.Min(100m, Math.Max(0m, value));
                PropertyChanged(this, new PropertyChangedEventArgs("AlignmentPoint"));
            }
        }

        public ushort Value
        {
            get
            {
                return value;
            }
            set
            {
                CheckUpdateMode();
                this.value = value;

                PropertyChanged(this, new PropertyChangedEventArgs("Value"));                
            }
        }

        #endregion

        public City(uint id,
                    IPlayer owner,
                    string name,
                    LazyResource resource,
                    byte radius,
                    decimal ap,
                    IActionWorker worker,
                    CityNotificationManager notifications,
                    ReferenceManager references,
                    ITechnologyManager technologies,
                    ITroopManager troops,
                    IUnitTemplate template,
                    ITroopStubFactory troopStubFactory,
                    IDbManager dbManager,
                    IGameObjectFactory gameObjectFactory,
                    IActionFactory actionFactory)
        {
            Id = id;
            Owner = owner;
            this.name = name;
            this.radius = radius;
            this.troopStubFactory = troopStubFactory;
            this.dbManager = dbManager;
            this.gameObjectFactory = gameObjectFactory;
            this.actionFactory = actionFactory;

            AlignmentPoint = ap;
            Resource = resource;

            Worker = worker;
            Notifications = notifications;
            References = references;
            Technologies = technologies;
            Troops = troops;
            Template = template;

            #region Event Proxies

            Template.UnitUpdated += evtTemplate =>
                {
                    if (Global.Current.FireEvents && DbPersisted)
                    {
                        dbManager.Save(evtTemplate);
                    }

                    UnitTemplateUpdated(this, new EventArgs());
                };

            Troops.TroopAdded += stub => TroopAdded(this, new TroopStubEventArgs {Stub = stub});
            Troops.TroopRemoved += stub => TroopRemoved(this, new TroopStubEventArgs {Stub = stub});
            Troops.TroopUpdated += stub => TroopUpdated(this, new TroopStubEventArgs {Stub = stub});
            Troops.TroopUnitUpdated += stub => TroopUnitUpdated(this, new TroopStubEventArgs {Stub = stub});

            Worker.ActionRemoved += (stub, state) => ActionRemoved(this, new ActionWorkerEventArgs {State = state, Stub = stub});
            Worker.ActionStarted += (stub, state) => ActionStarted(this, new ActionWorkerEventArgs {State = state, Stub = stub});
            Worker.ActionRescheduled += (stub, state) => ActionRescheduled(this, new ActionWorkerEventArgs {State = state, Stub = stub});

            Resource.ResourcesUpdate += () =>
                {
                    CheckUpdateMode();
                    ResourcesUpdated(this, new EventArgs());
                };

            Technologies.TechnologyCleared += OnTechnologyCleared;
            Technologies.TechnologyAdded += OnTechnologyAdded;
            Technologies.TechnologyRemoved += OnTechnologyRemoved;
            Technologies.TechnologyUpgraded += OnTechnologyUpgraded;

            References.ReferenceAdded += (sender, args) => ReferenceAdded(this, args);
            References.ReferenceRemoved += (sender, args) => ReferenceRemoved(this, args);

            #endregion
        }

        #region Object Management

        public ITroopObject GetTroop(uint objectId)
        {
            return troopobjects[objectId];
        }

        public bool TryGetObject(uint objectId, out IGameObject obj)
        {
            IStructure structure;
            if (structures.TryGetValue(objectId, out structure))
            {
                obj = structure;
                return true;
            }

            ITroopObject troop;
            if (troopobjects.TryGetValue(objectId, out troop))
            {
                obj = troop;
                return true;
            }

            obj = null;
            return false;
        }

        public bool TryGetStructure(uint objectId, out IStructure structure)
        {
            return structures.TryGetValue(objectId, out structure);
        }

        public bool TryGetTroop(uint objectId, out ITroopObject troop)
        {
            return troopobjects.TryGetValue(objectId, out troop);
        }

        public bool Add(uint objId, ITroopObject troop, bool save)
        {
            lock (objLock)
            {
                if (troopobjects.ContainsKey(objId))
                {
                    return false;
                }

                troop.City = this;

                troopobjects.Add(objId, troop);

                objectIdGen.Set(objId);

                if (save)
                {
                    dbManager.Save(troop);
                }

                troop.ObjectUpdated += OnObjectUpdated;

                ObjectAdded(this, new GameObjectArgs { Object = troop });
            }

            return true;
        }

        private void OnObjectUpdated(object sender, SimpleGameObjectArgs e)
        {
            ObjectUpdated(this, new GameObjectArgs {Object = (IGameObject)e.SimpleGameObject, OriginalX = e.OriginalX, OriginalY = e.OriginalY});
        }

        public bool Add(ITroopObject troop)
        {
            lock (objLock)
            {
                troop.ObjectId = objectIdGen.GetNext();
                return Add(troop.ObjectId, troop, true);
            }
        }

        public bool Add(uint objId, IStructure structure, bool save)
        {
            lock (objLock)
            {
                if (structures.ContainsKey(objId))
                {
                    return false;
                }

                structure.City = this;

                structures.Add(objId, structure);

                objectIdGen.Set(objId);

                if (save)
                {
                    dbManager.Save(structure);
                }
                
                structure.ObjectUpdated += OnObjectUpdated;
                structure.Technologies.TechnologyCleared += OnTechnologyCleared;
                structure.Technologies.TechnologyAdded += OnTechnologyAdded;
                structure.Technologies.TechnologyRemoved += OnTechnologyRemoved;
                structure.Technologies.TechnologyUpgraded += OnTechnologyUpgraded;

                ObjectAdded(this, new GameObjectArgs { Object = structure });
            }

            return true;
        }

        private void OnTechnologyUpgraded(Technology tech)
        {
            TechnologyUpgraded(this, new TechnologyEventArgs {TechnologyManager = Technologies, Technology = tech});
        }

        private void OnTechnologyRemoved(Technology tech)
        {
            TechnologyRemoved(this, new TechnologyEventArgs {TechnologyManager = Technologies, Technology = tech});
        }

        private void OnTechnologyAdded(Technology tech)
        {
            TechnologyAdded(this, new TechnologyEventArgs {TechnologyManager = Technologies, Technology = tech});
        }

        private void OnTechnologyCleared(ITechnologyManager manager)
        {
            TechnologyCleared(this, new TechnologyEventArgs {TechnologyManager = manager});
        }

        public bool ScheduleRemove(ITroopObject obj, bool wasKilled)
        {
            lock (objLock)
            {
                if (!troopobjects.ContainsKey(obj.ObjectId) || obj.IsBlocked > 0)
                {
                    return false;
                }

                var removeAction = actionFactory.CreateObjectRemovePassiveAction(Id, obj.ObjectId, wasKilled, new List<uint>());

                return Worker.DoPassive(this, removeAction, false) == Error.Ok;
            }
        }

        public bool ScheduleRemove(IStructure obj, bool wasKilled, bool cancelReferences = false)
        {
            lock (objLock)
            {
                if (!structures.ContainsKey(obj.ObjectId) || obj.IsBlocked > 0)
                {
                    return false;
                }

                var actions = new List<uint>();
                if (cancelReferences)
                {
                    actions =
                            (from reference in References
                             where reference.WorkerObject == obj
                             select reference.Action.ActionId).ToList();
                }

                References.Remove(obj);

                var removeAction = actionFactory.CreateObjectRemovePassiveAction(Id, obj.ObjectId, wasKilled, actions);
                return Worker.DoPassive(this, removeAction, false) == Error.Ok;
            }
        }

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="structure"></param>
        public void DoRemove(IStructure structure)
        {
            lock (objLock)
            {
                structure.Technologies.BeginUpdate();
                structure.Technologies.Clear();
                structure.Technologies.EndUpdate();

                structures.Remove(structure.ObjectId);

                dbManager.Delete(structure);

                structure.ObjectUpdated -= OnObjectUpdated;
                structure.Technologies.TechnologyCleared -= OnTechnologyCleared;
                structure.Technologies.TechnologyAdded -= OnTechnologyAdded;
                structure.Technologies.TechnologyRemoved -= OnTechnologyRemoved;
                structure.Technologies.TechnologyUpgraded -= OnTechnologyUpgraded;

                ObjectRemoved(this, new GameObjectArgs { Object = structure });
            }
        }

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="troop"></param>
        public void DoRemove(ITroopObject troop)
        {
            lock (objLock)
            {
                troopobjects.Remove(troop.ObjectId);

                dbManager.Delete(troop);

                troop.City = null;

                troop.ObjectUpdated -= OnObjectUpdated;

                ObjectRemoved(this, new GameObjectArgs { Object = troop });
            }
        }

        public List<IGameObject> GetInRange(uint x, uint y, uint inRadius)
        {
            return this.Where(structure => structure.TileDistance(x, y) <= inRadius).Cast<IGameObject>().ToList();
        }

        #endregion

        #region Updates

        public bool IsUpdating { get; private set; }

        public DeletedState Deleted { get; set; }

        public void BeginUpdate()
        {
            if (IsUpdating)
            {
                throw new Exception("Nesting beginupdate");
            }
            IsUpdating = true;
        }

        public void EndUpdate()
        {
            if (!IsUpdating)
            {
                throw new Exception("Called EndUpdate without first calling BeginUpdate");
            }

            dbManager.Save(this);
            IsUpdating = false;
        }

        private void CheckUpdateMode()
        {
            if (!Global.Current.FireEvents || Id == 0 || !DbPersisted)
            {
                return;
            }

            if (!IsUpdating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        public ITroopStub CreateTroopStub()
        {
            var stub = troopStubFactory.CreateTroopStub((byte)Troops.IdGen.GetNext());
            Troops.Add(stub);
            return stub;
        }

        public IStructure CreateStructure(ushort type, byte level)
        {
            var structure = gameObjectFactory.CreateStructure(Id, objectIdGen.GetNext(), type, level);
            Add(structure.ObjectId, structure, true);
            return structure;
        }

        #endregion

        public IActionWorker Worker { get; private set; }

        public uint WorkerId
        {
            get
            {
                return 0;
            }
        }

        public uint IsBlocked { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)structures.Values).GetEnumerator();
        }

        IEnumerator<IStructure> IEnumerable<IStructure>.GetEnumerator()
        {
            return ((IEnumerable<IStructure>)structures.Values).GetEnumerator();
        }

        public int Hash
        {
            get
            {
                return unchecked((int)Owner.PlayerId);
            }
        }

        public object Lock
        {
            get
            {
                return Owner;
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
                        new DbColumn("player_id", Owner.PlayerId, DbType.UInt32), new DbColumn("name", Name, DbType.String, 32)
                        , new DbColumn("value", Value, DbType.UInt16),
                        new DbColumn("alignment_point", AlignmentPoint, DbType.Decimal),
                        new DbColumn("radius", Radius, DbType.Byte),
                        new DbColumn("hide_new_units", HideNewUnits, DbType.Boolean),
                        new DbColumn("loot_stolen", LootStolen, DbType.UInt32),
                        new DbColumn("attack_point", AttackPoint, DbType.Int32),
                        new DbColumn("defense_point", DefensePoint, DbType.Int32),
                        new DbColumn("gold", Resource.Gold.RawValue, DbType.Int32),
                        new DbColumn("gold_realize_time", Resource.Gold.LastRealizeTime, DbType.DateTime),
                        new DbColumn("gold_production_rate", Resource.Gold.Rate, DbType.Int32),
                        new DbColumn("wood", Resource.Wood.RawValue, DbType.Int32),
                        new DbColumn("wood_realize_time", Resource.Wood.LastRealizeTime, DbType.DateTime),
                        new DbColumn("wood_production_rate", Resource.Wood.Rate, DbType.Int32),
                        new DbColumn("iron", Resource.Iron.RawValue, DbType.Int32),
                        new DbColumn("iron_realize_time", Resource.Iron.LastRealizeTime, DbType.DateTime),
                        new DbColumn("iron_production_rate", Resource.Iron.Rate, DbType.Int32),
                        new DbColumn("crop", Resource.Crop.RawValue, DbType.Int32),
                        new DbColumn("crop_realize_time", Resource.Crop.LastRealizeTime, DbType.DateTime),
                        new DbColumn("crop_production_rate", Resource.Crop.Rate, DbType.Int32),
                        new DbColumn("crop_upkeep", Resource.Crop.Upkeep, DbType.Int32),
                        new DbColumn("labor", Resource.Labor.RawValue, DbType.Int32),
                        new DbColumn("labor_realize_time", Resource.Labor.LastRealizeTime, DbType.DateTime),
                        new DbColumn("labor_production_rate", Resource.Labor.Rate, DbType.Int32),
                        new DbColumn("x", X, DbType.UInt32), new DbColumn("y", Y, DbType.UInt32),
                        new DbColumn("deleted", Deleted, DbType.Int32)
                };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", Id, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new[] {new DbDependency("Technologies", false, true), new DbDependency("Template", false, true)};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public Position CityRegionLocation
        {
            get
            {
                return new Position(X, Y);
            }
        }

        public byte[] GetCityRegionObjectBytes()
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(Lvl);
                bw.Write(Owner.PlayerId);
                bw.Write(value);
                bw.Write((float)alignmentPoint);
                bw.Write(Owner.Tribesman == null ? 0 : Owner.Tribesman.Tribe.Id);
                bw.Write((byte)(BattleProcedure.IsNewbieProtected(Owner) ? 1 : 0));
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public CityRegion.ObjectType CityRegionType
        {
            get
            {
                return CityRegion.ObjectType.City;
            }
        }

        public ushort CityRegionRelX
        {
            get
            {
                return (ushort)(X % Config.city_region_width);
            }
        }

        public ushort CityRegionRelY
        {
            get
            {
                return (ushort)(Y % Config.city_region_height);
            }
        }

        public uint CityRegionGroupId
        {
            get
            {
                return Id;
            }
        }

        public uint CityRegionObjectId
        {
            get
            {
                return 1;
            }
        }

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
                return LocationType.City;
            }
        }
    }
}