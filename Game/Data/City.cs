#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Battle;
using Game.Comm;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Notifications;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
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

        public const string DB_TABLE = "cities";

        private readonly object objLock = new object();

        private readonly Dictionary<uint, IStructure> structures = new Dictionary<uint, IStructure>();

        private readonly Dictionary<uint, ITroopObject> troopobjects = new Dictionary<uint, ITroopObject>();

        private decimal alignmentPoint;

        private int attackPoint;

        private IBattleManager battle;

        private int defensePoint;

        private bool hideNewUnits;

        private string name = "Washington";

        private uint nextObjectId;

        private byte radius;

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
                RadiusUpdateEvent();
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

                if (value == null)
                {
                    BattleEnded();
                }
                else
                {
                    BattleStarted();
                }
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
                HideNewUnitsUpdate();
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
                PointUpdate();
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
                PointUpdate();
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
                PointUpdate();
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

                if (Global.FireEvents && Id > 0)
                {
                    World.Current.Regions.CityRegions.GetCityRegion(X, Y).MarkAsDirty();
                    PointUpdate();
                }
            }
        }

        #endregion

        #region Constructors

        public City(uint id, IPlayer owner, string name, Resource resource, byte radius, IStructure mainBuilding, decimal ap)
                : this(
                        id,
                        owner,
                        name,
                        new LazyResource(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor),
                        radius,
                        mainBuilding,
                        ap)
        {
        }

        public City(uint id, IPlayer owner, string name, LazyResource resource, byte radius, IStructure mainBuilding, decimal ap)
        {
            Id = id;
            Owner = owner;
            this.name = name;
            this.radius = radius;

            AlignmentPoint = ap;
            Resource = resource;

            Worker = new ActionWorker(() => this, this);
            Notifications = new CityNotificationManager(Worker, id, DbPersistance.Current);
            References = new ReferenceManager(this);

            Technologies = new TechnologyManager(EffectLocation.City, this, id);

            Troops = new TroopManager(this, new CityTroopStubFactory(this));

            Troops.TroopUnitUpdated += TroopManagerTroopUnitUpdated;
            Troops.TroopUpdated += TroopManagerTroopUpdated;
            Troops.TroopRemoved += TroopManagerTroopRemoved;
            Troops.TroopAdded += TroopManagerTroopAdded;

            Template = new UnitTemplate(this);
            Template.UnitUpdated += UnitTemplateUnitUpdated;

            Worker.ActionRemoved += WorkerActionRemoved;
            Worker.ActionStarted += WorkerActionAdded;
            Worker.ActionRescheduled += WorkerActionRescheduled;

            if (mainBuilding != null)
            {
                mainBuilding.ObjectId = 1;
                Add(1, mainBuilding, false);
                Procedure.Current.RecalculateCityResourceRates(this);
                Procedure.Current.SetResourceCap(this);
            }

            resource.ResourcesUpdate += ResourceUpdateEvent;
        }

        #endregion

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

                if (nextObjectId < objId)
                {
                    nextObjectId = objId;
                }

                if (save)
                {
                    DbPersistance.Current.Save(troop);
                }

                ObjAddEvent(troop);
            }

            return true;
        }

        public bool Add(ITroopObject troop)
        {
            lock (objLock)
            {
                ++nextObjectId;
                troop.ObjectId = nextObjectId;
                return Add(nextObjectId, troop, true);
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

                if (nextObjectId < objId)
                {
                    nextObjectId = objId;
                }

                if (save)
                {
                    DbPersistance.Current.Save(structure);
                }

                structure.Technologies.TechnologyCleared += TechnologiesTechnologyCleared;
                structure.Technologies.TechnologyAdded += TechnologiesTechnologyAdded;
                structure.Technologies.TechnologyRemoved += TechnologiesTechnologyRemoved;
                structure.Technologies.TechnologyUpgraded += TechnologiesTechnologyUpgraded;
                ObjAddEvent(structure);
            }

            return true;
        }

        public bool Add(uint objId, IStructure structure)
        {
            return Add(objId, structure, true);
        }

        public bool Add(IStructure structure)
        {
            lock (objLock)
            {
                ++nextObjectId;
                structure.ObjectId = nextObjectId;
                return Add(nextObjectId, structure);
            }
        }

        public bool ScheduleRemove(ITroopObject obj, bool wasKilled)
        {
            lock (objLock)
            {
                if (!troopobjects.ContainsKey(obj.ObjectId) || obj.IsBlocked > 0)
                {
                    return false;
                }

                var removeAction = Ioc.Kernel.Get<IActionFactory>().CreateObjectRemovePassiveAction(Id, obj.ObjectId, wasKilled, new List<uint>());

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

                var removeAction = Ioc.Kernel.Get<IActionFactory>().CreateObjectRemovePassiveAction(Id, obj.ObjectId, wasKilled, actions);
                return Worker.DoPassive(this, removeAction, false) == Error.Ok;
            }
        }

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="obj"></param>
        public void DoRemove(IStructure obj)
        {
            lock (objLock)
            {
                obj.Technologies.BeginUpdate();
                obj.Technologies.Clear();
                obj.Technologies.EndUpdate();

                structures.Remove(obj.ObjectId);

                obj.Technologies.TechnologyCleared -= TechnologiesTechnologyCleared;
                obj.Technologies.TechnologyAdded -= TechnologiesTechnologyAdded;
                obj.Technologies.TechnologyRemoved -= TechnologiesTechnologyRemoved;
                obj.Technologies.TechnologyUpgraded -= TechnologiesTechnologyUpgraded;

                DbPersistance.Current.Delete(obj);

                ObjRemoveEvent(obj);
            }
        }

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="obj"></param>
        public void DoRemove(ITroopObject obj)
        {
            lock (objLock)
            {
                troopobjects.Remove(obj.ObjectId);

                DbPersistance.Current.Delete(obj);

                obj.City = null;

                ObjRemoveEvent(obj);
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

            DbPersistance.Current.Save(this);
            IsUpdating = false;
        }

        private void CheckUpdateMode()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            if (Id == 0 || !DbPersisted)
            {
                return;
            }

            if (!IsUpdating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        #endregion

        #region Channel Events

        public void Subscribe(IChannel s)
        {
            try
            {
                Global.Channel.Subscribe(s, "/CITY/" + Id);
            }
            catch(DuplicateSubscriptionException)
            {
            }
        }

        public void Unsubscribe(IChannel s)
        {
            Global.Channel.Unsubscribe(s, "/CITY/" + Id);
        }

        public void ResourceUpdateEvent()
        {
            if (!Global.FireEvents || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            CheckUpdateMode();

            var packet = new Packet(Command.CityResourcesUpdate);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(Resource, packet);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void RadiusUpdateEvent()
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.CityRadiusUpdate);

            World.Current.Regions.ObjectUpdateEvent(MainBuilding, X, Y);

            packet.AddUInt32(Id);
            packet.AddByte(radius);

            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void PointUpdate()
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.CityPointUpdate);

            packet.AddUInt32(Id);
            packet.AddInt32(attackPoint);
            packet.AddInt32(defensePoint);
            packet.AddUInt16(value);
            packet.AddFloat((float)alignmentPoint);

            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void HideNewUnitsUpdate()
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.CityHideNewUnitsUpdate);

            packet.AddUInt32(Id);
            packet.AddByte(hideNewUnits ? (byte)1 : (byte)0);

            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void NewCityUpdate()
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.CityNewUpdate);

            PacketHelper.AddToPacket(this, packet);

            Global.Channel.Post("/PLAYER/" + Owner.PlayerId, packet);
        }

        public void ObjAddEvent(IGameObject obj)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            bool doUpdate = IsUpdating;
            if (!doUpdate)
            {
                BeginUpdate();
            }

            Value = Formula.Current.CalculateCityValue(this);

            if (!doUpdate)
            {
                EndUpdate();
            }

            var packet = new Packet(Command.CityObjectAdd);
            packet.AddUInt16(Region.GetRegionIndex(obj));
            PacketHelper.AddToPacket(obj, packet);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void ObjRemoveEvent(IGameObject obj)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            bool doUpdate = IsUpdating;
            if (!doUpdate)
            {
                BeginUpdate();
            }

            Value = Formula.Current.CalculateCityValue(this);

            if (!doUpdate)
            {
                EndUpdate();
            }

            var packet = new Packet(Command.CityObjectRemove);
            packet.AddUInt32(Id);
            packet.AddUInt32(obj.ObjectId);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void ObjUpdateEvent(IGameObject sender, uint origX, uint origY)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            bool doUpdate = IsUpdating;
            if (!doUpdate)
            {
                BeginUpdate();
            }

            Value = Formula.Current.CalculateCityValue(this);

            if (!doUpdate)
            {
                EndUpdate();
            }

            var packet = new Packet(Command.CityObjectUpdate);
            packet.AddUInt16(Region.GetRegionIndex(sender));
            PacketHelper.AddToPacket(sender, packet);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void UnitTemplateUnitUpdated(UnitTemplate sender)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            DbPersistance.Current.Save(sender);

            var packet = new Packet(Command.UnitTemplateUpgraded);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(sender, packet);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void BattleStarted()
        {
            var packet = new Packet(Command.CityBattleStarted);
            packet.AddUInt32(Id);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        public void BattleEnded()
        {
            var packet = new Packet(Command.CityBattleEnded);
            packet.AddUInt32(Id);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void WorkerActionRescheduled(GameAction stub, ActionState state)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
            {
                return;
            }

            var packet = new Packet(Command.ActionRescheduled);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void WorkerActionAdded(GameAction stub, ActionState state)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
            {
                return;
            }

            var packet = new Packet(Command.ActionStarted);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void WorkerActionRemoved(GameAction stub, ActionState state)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
            {
                return;
            }

            var packet = new Packet(Command.ActionCompleted);
            packet.AddInt32((int)state);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void TechnologiesTechnologyUpgraded(Technology tech)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.TechUpgraded);
            packet.AddUInt32(Id);
            packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void TechnologiesTechnologyRemoved(Technology tech)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.TechRemoved);
            packet.AddUInt32(Id);
            packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void TechnologiesTechnologyCleared(ITechnologyManager manager)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.TechCleared);
            packet.AddUInt32(Id);
            packet.AddUInt32(manager.OwnerLocation == EffectLocation.City ? 0 : manager.OwnerId);

            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void TechnologiesTechnologyAdded(Technology tech)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.TechAdded);
            packet.AddUInt32(Id);
            packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + Id, packet);
        }
        
        private void TroopManagerTroopUpdated(ITroopStub stub)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            var packet = new Packet(Command.TroopUpdated);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void TroopManagerTroopUnitUpdated(ITroopStub stub)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            bool doUpdate = IsUpdating;
            if (!doUpdate)
            {
                BeginUpdate();
            }
            Resource.Crop.Upkeep = Procedure.Current.UpkeepForCity(this);
            if (!doUpdate)
            {
                EndUpdate();
            }
        }

        private void TroopManagerTroopAdded(ITroopStub stub)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            bool doUpdate = IsUpdating;
            if (!doUpdate)
            {
                BeginUpdate();
            }
            Resource.Crop.Upkeep = Procedure.Current.UpkeepForCity(this);
            if (!doUpdate)
            {
                EndUpdate();
            }

            var packet = new Packet(Command.TroopAdded);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        private void TroopManagerTroopRemoved(ITroopStub stub)
        {
            if (!Global.FireEvents || Id == 0 || Deleted != DeletedState.NotDeleted)
            {
                return;
            }

            bool doUpdate = IsUpdating;
            if (!doUpdate)
            {
                BeginUpdate();
            }
            Resource.Crop.Upkeep = Procedure.Current.UpkeepForCity(this);
            if (!doUpdate)
            {
                EndUpdate();
            }

            var packet = new Packet(Command.TroopRemoved);
            packet.AddUInt32(Id);
            packet.AddUInt32(stub.City.Id);
            packet.AddByte(stub.TroopId);
            Global.Channel.Post("/CITY/" + Id, packet);
        }

        #endregion

        public IActionWorker Worker { get; private set; }

        public int GetTotalLaborers()
        {
            return this.Sum(structure => (int)structure.Stats.Labor) + Resource.Labor.Value;
        }

        #region ICanDo Members

        public uint WorkerId
        {
            get
            {
                return 0;
            }
        }

        public uint IsBlocked { get; set; }

        #endregion

        #region IEnumerable<Structure> Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)structures.Values).GetEnumerator();
        }

        IEnumerator<IStructure> IEnumerable<IStructure>.GetEnumerator()
        {
            return ((IEnumerable<IStructure>)structures.Values).GetEnumerator();
        }

        #endregion

        #region ILockable Members

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
                return new[] {new DbDependency("Technologies", false, true), new DbDependency("Template", false, true),};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public static bool IsNameValid(string cityName)
        {
            return cityName != string.Empty && cityName.Length >= 3 && cityName.Length <= 16 &&
                   Regex.IsMatch(cityName, Global.ALPHANUMERIC_NAME, RegexOptions.IgnoreCase);
        }

        #region Implementation of ICityRegionObject

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
                return LocationType.City;
            }
        }

        #endregion
    }
}