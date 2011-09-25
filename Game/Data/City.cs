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
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

#endregion

namespace Game.Data
{
    public interface ICity
    {
        /// <summary>
        ///   Enumerates only through structures in this city
        /// </summary>
        Dictionary<uint, Structure>.Enumerator Structures { get; }

        /// <summary>
        ///   Radius of city. This affects city wall and where user can build.
        /// </summary>
        byte Radius { get; set; }

        byte Lvl { get; }

        /// <summary>
        ///   Returns the city's center point which is the town centers position
        /// </summary>
        uint X { get; }

        /// <summary>
        ///   Returns the city's center point which is the town centers position
        /// </summary>
        uint Y { get; }

        /// <summary>
        ///   City's battle manager. Maybe null if city is not in battle.
        /// </summary>
        IBattleManager Battle { get; set; }

        /// <summary>
        ///   Enumerates through all troop objects in this city
        /// </summary>
        IEnumerable<TroopObject> TroopObjects { get; }

        /// <summary>
        ///   Troop manager which manages all troop stubs in city
        /// </summary>
        TroopManager Troops { get; }

        /// <summary>
        ///   Technology manager for city
        /// </summary>
        TechnologyManager Technologies { get; }

        /// <summary>
        ///   Returns the local troop
        /// </summary>
        TroopStub DefaultTroop { get; set; }

        /// <summary>
        ///   Returns unit template. Unit template holds levels for all units in the city.
        /// </summary>
        UnitTemplate Template { get; }

        /// <summary>
        ///   Resource available in the city
        /// </summary>
        LazyResource Resource { get; }

        /// <summary>
        ///   Amount of loot this city has stolen from other players
        /// </summary>
        uint LootStolen { get; set; }

        /// <summary>
        ///   Unique city id
        /// </summary>
        uint Id { get; set; }

        /// <summary>
        ///   City name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///   Player that owns this city
        /// </summary>
        Player Owner { get; }

        /// <summary>
        ///   Whether to send new units to hiding or not
        /// </summary>
        bool HideNewUnits { get; set; }

        /// <summary>
        ///   Attack points earned by this city
        /// </summary>
        int AttackPoint { get; set; }

        /// <summary>
        ///   Defense points earned by this city
        /// </summary>
        int DefensePoint { get; set; }

        ushort Value { get; set; }
        bool IsUpdating { get; }
        City.DeletedState Deleted { get; set; }
        ActionWorker Worker { get; }
        uint WorkerId { get; }
        int Hash { get; }
        object Lock { get; }
        string DbTable { get; }
        DbColumn[] DbColumns { get; }
        DbColumn[] DbPrimaryKey { get; }
        DbDependency[] DbDependencies { get; }
        bool DbPersisted { get; set; }
        Location CityRegionLocation { get; }
        CityRegion.ObjectType CityRegionType { get; }
        ushort CityRegionRelX { get; }
        ushort CityRegionRelY { get; }
        uint CityRegionGroupId { get; }
        uint CityRegionObjectId { get; }

        /// <summary>
        ///   Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name = "objectId"></param>
        /// <returns></returns>
        GameObject this[uint objectId] { get; }

        TroopObject GetTroop(uint objectId);
        bool TryGetObject(uint objectId, out GameObject obj);
        bool TryGetStructure(uint objectId, out Structure structure);
        bool TryGetTroop(uint objectId, out TroopObject troop);
        bool Add(uint objId, TroopObject troop, bool save);
        bool Add(TroopObject troop);
        bool Add(uint objId, Structure structure, bool save);
        bool Add(uint objId, Structure structure);
        bool Add(Structure structure);
        bool ScheduleRemove(TroopObject obj, bool wasKilled);
        bool ScheduleRemove(Structure obj, bool wasKilled);
        bool ScheduleRemove(Structure obj, bool wasKilled, bool cancelReferences);

        /// <summary>
        ///   Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name = "obj"></param>
        void DoRemove(Structure obj);

        /// <summary>
        ///   Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name = "obj"></param>
        void DoRemove(TroopObject obj);

        List<GameObject> GetInRange(uint x, uint y, uint inRadius);
        void BeginUpdate();
        void EndUpdate();
        void Subscribe(IChannel s);
        void Unsubscribe(IChannel s);
        void ResourceUpdateEvent();
        void RadiusUpdateEvent();
        void DefenseAttackPointUpdate();
        void HideNewUnitsUpdate();
        void NewCityUpdate();
        void ObjAddEvent(GameObject obj);
        void ObjRemoveEvent(GameObject obj);
        void ObjUpdateEvent(GameObject sender, uint origX, uint origY);
        void UnitTemplateUnitUpdated(UnitTemplate sender);
        void BattleStarted();
        void BattleEnded();
        byte[] GetCityRegionObjectBytes();
    }

    public class City : IEnumerable<Structure>, ICanDo, ILockable, IPersistableObject, ICityRegionObject, ICity
    {
        public enum DeletedState
        {
            NotDeleted,
            Deleting,
            Deleted,
        }

        public const string DB_TABLE = "cities";
        private readonly object objLock = new object();

        private readonly Dictionary<uint, Structure> structures = new Dictionary<uint, Structure>();
        private readonly Dictionary<uint, TroopObject> troopobjects = new Dictionary<uint, TroopObject>();
        private int attackPoint;
        private IBattleManager battle;
        private int defensePoint;
        private bool hideNewUnits;
        private uint id;
        private string name = "Washington";
        private uint nextObjectId;
        private byte radius;
        private ushort value;

        #region Properties

        /// <summary>
        ///   Enumerates only through structures in this city
        /// </summary>
        public Dictionary<uint, Structure>.Enumerator Structures
        {
            get
            {
                return structures.GetEnumerator();
            }
        }

        /// <summary>
        ///   Radius of city. This affects city wall and where user can build.
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
                    return;

                radius = value;
                RadiusUpdateEvent();
            }
        }

        /// <summary>
        ///   Returns the town center
        /// </summary>
        private Structure MainBuilding
        {
            get
            {
                Structure mainBuilding;
                return !structures.TryGetValue(1, out mainBuilding) ? null : mainBuilding;
            }
        }

        public byte Lvl
        {
            get
            {
                return (byte)(MainBuilding == null ? 1 : MainBuilding.Lvl);
            }
        }

        /// <summary>
        ///   Returns the city's center point which is the town centers position
        /// </summary>
        public uint X
        {
            get
            {
                return MainBuilding == null ? 0 : MainBuilding.X;
            }
        }

        /// <summary>
        ///   Returns the city's center point which is the town centers position
        /// </summary>
        public uint Y
        {
            get
            {
                return MainBuilding == null ? 0 : MainBuilding.Y;
            }
        }

        /// <summary>
        ///   City's battle manager. Maybe null if city is not in battle.
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
                    BattleEnded();
                else
                    BattleStarted();
            }
        }

        /// <summary>
        ///   Enumerates through all troop objects in this city
        /// </summary>
        public IEnumerable<TroopObject> TroopObjects
        {
            get
            {
                return troopobjects.Values;
            }
        }

        /// <summary>
        ///   Troop manager which manages all troop stubs in city
        /// </summary>
        public TroopManager Troops { get; private set; }

        /// <summary>
        ///   Technology manager for city
        /// </summary>
        public TechnologyManager Technologies { get; private set; }

        /// <summary>
        ///   Returns the local troop
        /// </summary>
        public TroopStub DefaultTroop
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
        ///   Returns unit template. Unit template holds levels for all units in the city.
        /// </summary>
        public UnitTemplate Template { get; private set; }

        /// <summary>
        ///   Resource available in the city
        /// </summary>
        public LazyResource Resource { get; private set; }

        /// <summary>
        ///   Amount of loot this city has stolen from other players
        /// </summary>
        public uint LootStolen { get; set; }

        /// <summary>
        ///   Unique city id
        /// </summary>
        public uint Id
        {
            get
            {
                return id;
            }
            set
            {
                CheckUpdateMode();
                id = value;
            }
        }

        /// <summary>
        ///   City name
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
        ///   Player that owns this city
        /// </summary>
        public Player Owner { get; private set; }

        /// <summary>
        ///   Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name = "objectId"></param>
        /// <returns></returns>
        public GameObject this[uint objectId]
        {
            get
            {
                Structure structure;
                if (structures.TryGetValue(objectId, out structure))
                    return structure;

                TroopObject troop;
                if (troopobjects.TryGetValue(objectId, out troop))
                    return troop;

                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        ///   Whether to send new units to hiding or not
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
        ///   Attack points earned by this city
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
                DefenseAttackPointUpdate();
            }
        }

        /// <summary>
        ///   Defense points earned by this city
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
                DefenseAttackPointUpdate();
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

                if (Global.FireEvents && id > 0)
                {                    
                    Global.World.GetCityRegion(X, Y).MarkAsDirty();
                    DefenseAttackPointUpdate();
                }                
            }
        }

        #endregion

        #region Constructors

        public City(Player owner, string name, Resource resource, byte radius, Structure mainBuilding)
                : this(owner, name, new LazyResource(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor), radius, mainBuilding)
        {
        }

        public City(Player owner, string name, LazyResource resource, byte radius, Structure mainBuilding)
        {
            Owner = owner;
            this.name = name;
            this.radius = radius;
            Resource = resource;            

            Worker = new ActionWorker(this);

            Technologies = new TechnologyManager(EffectLocation.City, this, id);

            Troops = new TroopManager(this);

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
                Procedure.SetResourceCap(this);
            }

            resource.ResourcesUpdate += ResourceUpdateEvent;
        }

        #endregion

        #region Object Management

        public TroopObject GetTroop(uint objectId)
        {
            return troopobjects[objectId];
        }

        public bool TryGetObject(uint objectId, out GameObject obj)
        {
            Structure structure;
            if (structures.TryGetValue(objectId, out structure))
            {
                obj = structure;
                return true;
            }

            TroopObject troop;
            if (troopobjects.TryGetValue(objectId, out troop))
            {
                obj = troop;
                return true;
            }

            obj = null;
            return false;
        }

        public bool TryGetStructure(uint objectId, out Structure structure)
        {
            return structures.TryGetValue(objectId, out structure);
        }

        public bool TryGetTroop(uint objectId, out TroopObject troop)
        {
            return troopobjects.TryGetValue(objectId, out troop);
        }

        public bool Add(uint objId, TroopObject troop, bool save)
        {
            lock (objLock)
            {
                if (troopobjects.ContainsKey(objId))
                    return false;

                if (troop.Stub != null)
                {
                    troop.Stub.BeginUpdate();
                    troop.Stub.TroopObject = troop;
                    troop.Stub.EndUpdate();
                }

                troop.City = this;

                troopobjects.Add(objId, troop);

                if (nextObjectId < objId)
                    nextObjectId = objId;

                if (save)
                    Ioc.Kernel.Get<IDbManager>().Save(troop);

                ObjAddEvent(troop);
            }

            return true;
        }

        public bool Add(TroopObject troop)
        {
            lock (objLock)
            {
                ++nextObjectId;
                troop.ObjectId = nextObjectId;
                return Add(nextObjectId, troop, true);
            }
        }

        public bool Add(uint objId, Structure structure, bool save)
        {
            lock (objLock)
            {
                if (structures.ContainsKey(objId))
                    return false;

                structure.City = this;

                structures.Add(objId, structure);

                if (nextObjectId < objId)
                    nextObjectId = objId;

                if (save)
                    Ioc.Kernel.Get<IDbManager>().Save(structure);

                structure.Technologies.TechnologyCleared += TechnologiesTechnologyCleared;
                structure.Technologies.TechnologyAdded += TechnologiesTechnologyAdded;
                structure.Technologies.TechnologyRemoved += TechnologiesTechnologyRemoved;
                structure.Technologies.TechnologyUpgraded += TechnologiesTechnologyUpgraded;
                ObjAddEvent(structure);
            }

            return true;
        }

        public bool Add(uint objId, Structure structure)
        {
            return Add(objId, structure, true);
        }

        public bool Add(Structure structure)
        {
            lock (objLock)
            {
                ++nextObjectId;
                structure.ObjectId = nextObjectId;
                return Add(nextObjectId, structure);
            }
        }

        public bool ScheduleRemove(TroopObject obj, bool wasKilled)
        {
            lock (objLock)
            {
                if (!troopobjects.ContainsKey(obj.ObjectId))
                    return false;

                obj.IsBlocked = true;

                var removeAction = new ObjectRemovePassiveAction(Id, obj.ObjectId, wasKilled, new List<uint>());
                return Worker.DoPassive(this, removeAction, false) == Error.Ok;
            }
        }

        public bool ScheduleRemove(Structure obj, bool wasKilled)
        {
            return ScheduleRemove(obj, wasKilled, false);
        }

        public bool ScheduleRemove(Structure obj, bool wasKilled, bool cancelReferences)
        {
            lock (objLock)
            {
                if (!structures.ContainsKey(obj.ObjectId) || obj.IsBlocked)
                    return false;

                obj.IsBlocked = true;

                var actions = new List<uint>();
                if (cancelReferences)
                {
                    actions = (from reference in Worker.References
                               where reference.WorkerObject == obj
                               select reference.Action.ActionId).ToList();
                }

                Worker.References.Remove(obj);

                var removeAction = new ObjectRemovePassiveAction(Id, obj.ObjectId, wasKilled, actions);
                return Worker.DoPassive(this, removeAction, false) == Error.Ok;
            }
        }

        /// <summary>
        ///   Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name = "obj"></param>
        public void DoRemove(Structure obj)
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

                Ioc.Kernel.Get<IDbManager>().Delete(obj);

                ObjRemoveEvent(obj);
            }
        }

        /// <summary>
        ///   Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name = "obj"></param>
        public void DoRemove(TroopObject obj)
        {
            lock (objLock)
            {
                troopobjects.Remove(obj.ObjectId);

                Ioc.Kernel.Get<IDbManager>().Delete(obj);

                obj.City = null;

                if (obj.Stub != null)
                {
                    obj.Stub.BeginUpdate();
                    obj.Stub.TroopObject = null;
                    obj.Stub.EndUpdate();
                }

                ObjRemoveEvent(obj);
            }
        }

        public List<GameObject> GetInRange(uint x, uint y, uint inRadius)
        {
            return this.Where(structure => structure.TileDistance(x, y) <= inRadius).Cast<GameObject>().ToList();
        }

        #endregion

        #region Updates

        public bool IsUpdating { get; private set; }
        public DeletedState Deleted { get; set; }

        private void CheckUpdateMode()
        {
            if (!Global.FireEvents)
                return;

            if (id == 0)
                return;

            if (!IsUpdating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        public void BeginUpdate()
        {
            if (IsUpdating)
                throw new Exception("Nesting beginupdate");
            IsUpdating = true;
        }

        public void EndUpdate()
        {
            if (!IsUpdating)
                throw new Exception("Called EndUpdate without first calling BeginUpdate");

            Ioc.Kernel.Get<IDbManager>().Save(this);
            IsUpdating = false;
        }

        #endregion

        #region Channel Events

        public void Subscribe(IChannel s)
        {
            try
            {
                Global.Channel.Subscribe(s, "/CITY/" + id);
            }
            catch(DuplicateSubscriptionException)
            {
            }
        }

        public void Unsubscribe(IChannel s)
        {
            Global.Channel.Unsubscribe(s, "/CITY/" + id);
        }

        public void ResourceUpdateEvent()
        {
            if (!Global.FireEvents || Deleted != DeletedState.NotDeleted)
                return;

            CheckUpdateMode();

            var packet = new Packet(Command.CityResourcesUpdate);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(Resource, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void RadiusUpdateEvent()
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.CityRadiusUpdate);

            Global.World.ObjectUpdateEvent(MainBuilding, X, Y);

            packet.AddUInt32(Id);
            packet.AddByte(radius);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void DefenseAttackPointUpdate()
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.CityAttackDefensePointUpdate);

            packet.AddUInt32(Id);
            packet.AddInt32(attackPoint);
            packet.AddInt32(defensePoint);
            packet.AddUInt16(value);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void HideNewUnitsUpdate()
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.CityHideNewUnitsUpdate);

            packet.AddUInt32(Id);
            packet.AddByte(hideNewUnits ? (byte)1 : (byte)0);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void NewCityUpdate()
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.CityNewUpdate);

            PacketHelper.AddToPacket(this, packet);

            Global.Channel.Post("/PLAYER/" + Owner.PlayerId, packet);
        }

        public void ObjAddEvent(GameObject obj)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            bool doUpdate = IsUpdating;
            if (!doUpdate)
                BeginUpdate();
            
            Value = Formula.CalculateCityValue(this);
            
            if (!doUpdate)
                EndUpdate();

            var packet = new Packet(Command.CityObjectAdd);
            packet.AddUInt16(Region.GetRegionIndex(obj));
            PacketHelper.AddToPacket(obj, packet, false);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void ObjRemoveEvent(GameObject obj)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;
            
            bool doUpdate = IsUpdating;
            if (!doUpdate)
                BeginUpdate();
            
            Value = Formula.CalculateCityValue(this);

            if (!doUpdate)
                EndUpdate();

            var packet = new Packet(Command.CityObjectRemove);
            packet.AddUInt32(Id);
            packet.AddUInt32(obj.ObjectId);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void ObjUpdateEvent(GameObject sender, uint origX, uint origY)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            bool doUpdate = IsUpdating;
            if (!doUpdate)
                BeginUpdate();
            
            Value = Formula.CalculateCityValue(this);

            if (!doUpdate)
                EndUpdate();

            var packet = new Packet(Command.CityObjectUpdate);
            packet.AddUInt16(Region.GetRegionIndex(sender));
            PacketHelper.AddToPacket(sender, packet, false);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionRescheduled(GameAction stub, ActionState state)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            var packet = new Packet(Command.ActionRescheduled);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionAdded(GameAction stub, ActionState state)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            var packet = new Packet(Command.ActionStarted);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionRemoved(GameAction stub, ActionState state)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            var packet = new Packet(Command.ActionCompleted);
            packet.AddInt32((int)state);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TechnologiesTechnologyUpgraded(Technology tech)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.TechUpgraded);
            packet.AddUInt32(Id);
            packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TechnologiesTechnologyRemoved(Technology tech)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.TechRemoved);
            packet.AddUInt32(Id);
            packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TechnologiesTechnologyCleared(TechnologyManager manager)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.TechCleared);
            packet.AddUInt32(Id);
            packet.AddUInt32(manager.OwnerLocation == EffectLocation.City ? 0 : manager.OwnerId);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TechnologiesTechnologyAdded(Technology tech)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            var packet = new Packet(Command.TechAdded);
            packet.AddUInt32(Id);
            packet.AddUInt32(tech.OwnerLocation == EffectLocation.City ? 0 : tech.OwnerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TroopManagerTroopUpdated(TroopStub stub)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            bool doUpdate = IsUpdating;
            if (!doUpdate)
                BeginUpdate();
            Resource.Crop.Upkeep = Troops.Upkeep;
            if (!doUpdate)
                EndUpdate();

            var packet = new Packet(Command.TroopUpdated);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TroopManagerTroopAdded(TroopStub stub)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            bool doUpdate = IsUpdating;
            if (!doUpdate)
                BeginUpdate();
            Resource.Crop.Upkeep = Troops.Upkeep;
            if (!doUpdate)
                EndUpdate();

            var packet = new Packet(Command.TroopAdded);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TroopManagerTroopRemoved(TroopStub stub)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            bool doUpdate = IsUpdating;
            if (!doUpdate)
                BeginUpdate();
            Resource.Crop.Upkeep = Troops.Upkeep;
            if (!doUpdate)
                EndUpdate();

            var packet = new Packet(Command.TroopRemoved);
            packet.AddUInt32(Id);
            packet.AddUInt32(stub.City.Id);
            packet.AddByte(stub.TroopId);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void UnitTemplateUnitUpdated(UnitTemplate sender)
        {
            if (!Global.FireEvents || id == 0 || Deleted != DeletedState.NotDeleted)
                return;

            Ioc.Kernel.Get<IDbManager>().Save(sender);

            var packet = new Packet(Command.UnitTemplateUpgraded);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(sender, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void BattleStarted()
        {
            var packet = new Packet(Command.CityBattleStarted);
            packet.AddUInt32(Id);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void BattleEnded()
        {
            var packet = new Packet(Command.CityBattleEnded);
            packet.AddUInt32(Id);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        #endregion

        public ActionWorker Worker { get; private set; }

        #region ICanDo Members

        City ICanDo.City
        {
            get
            {
                return this;
            }
        }

        public uint WorkerId
        {
            get
            {
                return 0;
            }
        }

        #endregion

        #region IEnumerable<Structure> Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)structures.Values).GetEnumerator();
        }

        IEnumerator<Structure> IEnumerable<Structure>.GetEnumerator()
        {
            return ((IEnumerable<Structure>)structures.Values).GetEnumerator();
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
                               new DbColumn("player_id", Owner.PlayerId, DbType.UInt32), new DbColumn("name", Name, DbType.String, 32),
                               new DbColumn("value", Value, DbType.UInt16),
                               new DbColumn("radius", Radius, DbType.Byte), new DbColumn("hide_new_units", HideNewUnits, DbType.Boolean),
                               new DbColumn("loot_stolen", LootStolen, DbType.UInt32), new DbColumn("attack_point", AttackPoint, DbType.Int32),
                               new DbColumn("defense_point", DefensePoint, DbType.Int32), new DbColumn("gold", Resource.Gold.RawValue, DbType.Int32),
                               new DbColumn("gold_realize_time", Resource.Gold.LastRealizeTime, DbType.DateTime),
                               new DbColumn("gold_production_rate", Resource.Gold.Rate, DbType.Int32), new DbColumn("wood", Resource.Wood.RawValue, DbType.Int32),
                               new DbColumn("wood_realize_time", Resource.Wood.LastRealizeTime, DbType.DateTime),
                               new DbColumn("wood_production_rate", Resource.Wood.Rate, DbType.Int32), new DbColumn("iron", Resource.Iron.RawValue, DbType.Int32),
                               new DbColumn("iron_realize_time", Resource.Iron.LastRealizeTime, DbType.DateTime),
                               new DbColumn("iron_production_rate", Resource.Iron.Rate, DbType.Int32), new DbColumn("crop", Resource.Crop.RawValue, DbType.Int32),
                               new DbColumn("crop_realize_time", Resource.Crop.LastRealizeTime, DbType.DateTime),
                               new DbColumn("crop_production_rate", Resource.Crop.Rate, DbType.Int32), new DbColumn("crop_upkeep", Resource.Crop.Upkeep, DbType.Int32),
                               new DbColumn("labor", Resource.Labor.RawValue, DbType.Int32),
                               new DbColumn("labor_realize_time", Resource.Labor.LastRealizeTime, DbType.DateTime),
                               new DbColumn("labor_production_rate", Resource.Labor.Rate, DbType.Int32), new DbColumn("x", X, DbType.UInt32),
                               new DbColumn("y", Y, DbType.UInt32), new DbColumn("deleted", Deleted, DbType.Int32),
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

        public DbDependency[] DbDependencies
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
                   Regex.IsMatch(cityName, "^([a-z][a-z0-9\\s].*)$", RegexOptions.IgnoreCase);
        }

        #region Implementation of ICityRegionObject

        public Location CityRegionLocation
        {
            get
            {
                return new Location(X, Y);
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
    }
}