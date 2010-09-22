#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using Game.Battle;
using Game.Comm;
using Game.Data.Troop;
using Game.Database;
using Game.Fighting;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Util;

#endregion

namespace Game.Data {
    public class City : IEnumerable<Structure>, ICanDo, ILockable, IPersistableObject {
        private uint id;
        private string name = "Washington";
        private uint nextObjectId;
        private byte radius = 4;
        private int attackPoint;
        private int defensePoint;
        private bool hideNewUnits;

        private BattleManager battle;

        private readonly object objLock = new object();

        private readonly Dictionary<uint, Structure> structures = new Dictionary<uint, Structure>();
        private readonly Dictionary<uint, TroopObject> troopobjects = new Dictionary<uint, TroopObject>();

        #region Properties

        /// <summary>
        /// Enumerates only through structures in this city
        /// </summary>
        public Dictionary<uint, Structure>.Enumerator Structures {
            get { return structures.GetEnumerator(); }
        }

        /// <summary>
        /// Radius of city. This affects city wall and where user can build.
        /// </summary>
        public byte Radius {
            get { return radius; }
            set {
                if (value == radius)
                    return;

                radius = value;
                RadiusUpdateEvent();
            }
        }
        
        /// <summary>
        /// Returns the town center
        /// </summary>
        public Structure MainBuilding {
            get { return structures[1]; }
        }

        /// <summary>
        /// Returns the city's center point which is the town centers position
        /// </summary>
        public uint X {
            get {
                return MainBuilding.X;
            }     
        }

        /// <summary>
        /// Returns the city's center point which is the town centers position
        /// </summary>
        public uint Y {
            get {
                return MainBuilding.Y;
            }
        }
        
        /// <summary>
        /// City's battle manager. Maybe null if city is not in battle.
        /// </summary>
        public BattleManager Battle {
            get {
                return battle;   
            }
            set {
                battle = value;

                if (value == null) {
                    BattleEnded();
                } else {
                    BattleStarted();
                }
            }
        }

        /// <summary>
        /// Enumerates through all troop objects in this city
        /// </summary>
        public IEnumerable<TroopObject> TroopObjects
        {
            get {
                return troopobjects.Values;
            }
        }

        /// <summary>
        /// Troop manager which manages all troop stubs in city
        /// </summary>
        public TroopManager Troops { get; private set; }

        /// <summary>
        /// Technology manager for city
        /// </summary>
        public TechnologyManager Technologies { get; private set; }

        /// <summary>
        /// Returns the local troop 
        /// </summary>
        public TroopStub DefaultTroop {
            get { return Troops[1]; }
            set { Troops[1] = value; }
        }

        /// <summary>
        /// Returns unit template. Unit template holds levels for all units in the city.
        /// </summary>
        public UnitTemplate Template { get; private set; }

        /// <summary>
        /// Resource available in the city
        /// </summary>
        public LazyResource Resource { get; private set; }

        /// <summary>
        /// Amount of loot this city has stolen from other players
        /// </summary>
        public uint LootStolen { get; set; }        

        /// <summary>
        /// Unique city id
        /// </summary>
        public uint Id {
            get { return id; }
            set {
                CheckUpdateMode();
                id = value;
            }
        }

        /// <summary>
        /// City name
        /// </summary>
        public string Name {
            get { return name; }
            set {
                CheckUpdateMode();
                name = value;
            }
        }

        /// <summary>
        /// Player that owns this city
        /// </summary>
        public Player Owner { get; private set; }

        /// <summary>
        /// Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public GameObject this[uint objectId] {
            get {
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
        /// Whether to send new units to hiding or not
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
        /// Attack points earned by this city
        /// </summary>        
        public int AttackPoint { 
            get {
                return attackPoint;
            } 
            set {
                CheckUpdateMode();
                attackPoint = value;
                DefenseAttackPointUpdate();
            } 
        }

        /// <summary>
        /// Defense points earned by this city
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
        #endregion

        #region Constructors

        public City(Player owner, string name, Resource resource, Structure mainBuilding)
            : this(owner, name, new LazyResource(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor), mainBuilding) {}

        public City(Player owner, string name, LazyResource resource, Structure mainBuilding) {
            Owner = owner;
            this.name = name;
            Resource = resource;

            Worker = new ActionWorker(this);

            Technologies = new TechnologyManager(EffectLocation.CITY, this, id);

            Troops = new TroopManager(this);

            Troops.TroopUpdated += TroopManagerTroopUpdated;
            Troops.TroopRemoved += TroopManagerTroopRemoved;
            Troops.TroopAdded += TroopManagerTroopAdded;

            Template = new UnitTemplate(this);
            Template.UnitUpdated += UnitTemplateUnitUpdated;

            Worker.ActionRemoved += WorkerActionRemoved;
            Worker.ActionStarted += WorkerActionAdded;
            Worker.ActionRescheduled += WorkerActionRescheduled;
            owner.Add(this);                       

            if (mainBuilding != null) {
                mainBuilding.ObjectId = 1;
                Add(1, mainBuilding, false);
                Procedure.SetResourceCap(this);
            }

            resource.ResourcesUpdate += ResourceUpdateEvent;
        }

        #endregion

        #region Object Management

        public TroopObject GetTroop(uint objectId) {
            return troopobjects[objectId];
        }

        public bool TryGetObject(uint objectId, out GameObject obj) {
            Structure structure;
            if (structures.TryGetValue(objectId, out structure)) {
                obj = structure;
                return true;
            }

            TroopObject troop;
            if (troopobjects.TryGetValue(objectId, out troop)) {
                obj = troop;
                return true;
            }

            obj = null;
            return false;
        }

        public bool TryGetStructure(uint objectId, out Structure structure) {
            return structures.TryGetValue(objectId, out structure);
        }

        public bool TryGetTroop(uint objectId, out TroopObject troop) {
            return troopobjects.TryGetValue(objectId, out troop);
        }

        public bool Add(uint objId, TroopObject troop, bool save) {
            lock (objLock) {
                if (troopobjects.ContainsKey(objId))
                    return false;

                if (troop.Stub != null) {
                    troop.Stub.BeginUpdate();
                    troop.Stub.TroopObject = troop;
                    troop.Stub.EndUpdate();
                }

                troop.City = this;

                troopobjects.Add(objId, troop);

                if (nextObjectId < objId)
                    nextObjectId = objId;

                if (save)
                    Global.DbManager.Save(troop);

                ObjAddEvent(troop);
            }

            return true;
        }

        public bool Add(TroopObject troop) {
            lock (objLock) {
                ++nextObjectId;
                troop.ObjectId = nextObjectId;
                return Add(nextObjectId, troop, true);
            }
        }

        public bool Add(uint objId, Structure structure, bool save) {
            lock (objLock) {
                if (structures.ContainsKey(objId))
                    return false;

                structure.City = this;

                structures.Add(objId, structure);

                if (nextObjectId < objId)
                    nextObjectId = objId;

                if (save)
                    Global.DbManager.Save(structure);

                structure.Technologies.TechnologyCleared += Technologies_TechnologyCleared;
                structure.Technologies.TechnologyAdded += Technologies_TechnologyAdded;
                structure.Technologies.TechnologyRemoved += Technologies_TechnologyRemoved;
                structure.Technologies.TechnologyUpgraded += Technologies_TechnologyUpgraded;
                ObjAddEvent(structure);
            }

            return true;
        }

        public bool Add(uint objId, Structure structure) {
            return Add(objId, structure, true);
        }

        public bool Add(Structure structure) {
            lock (objLock) {
                ++nextObjectId;
                structure.ObjectId = nextObjectId;
                return Add(nextObjectId, structure);
            }
        }

        public bool ScheduleRemove(TroopObject obj, bool wasKilled) {
            lock (objLock) {
                if (!troopobjects.ContainsKey(obj.ObjectId))
                    return false;

                obj.IsBlocked = true;

                ObjectRemoveAction removeAction = new ObjectRemoveAction(Id, obj.ObjectId, wasKilled);
                return Worker.DoPassive(this, removeAction, false) == Setup.Error.OK;
            }
        }

        public bool ScheduleRemove(Structure obj, bool wasKilled) {
            lock (objLock) {
                if (obj == MainBuilding)
                    throw new Exception("Trying to remove main building");

                if (!structures.ContainsKey(obj.ObjectId))
                    return false;

                obj.IsBlocked = true;

                ObjectRemoveAction removeAction = new ObjectRemoveAction(Id, obj.ObjectId, wasKilled);
                return Worker.DoPassive(this, removeAction, false) == Setup.Error.OK;
            }
        }

        /// <summary>
        /// Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="obj"></param>
        public void DoRemove(Structure obj) {
            lock (objLock) {
                if (obj == MainBuilding)
                    throw new Exception("Trying to remove main building");

                obj.Technologies.BeginUpdate();
                obj.Technologies.Clear();
                obj.Technologies.EndUpdate();

                structures.Remove(obj.ObjectId);

                obj.Technologies.TechnologyCleared -= Technologies_TechnologyCleared;
                obj.Technologies.TechnologyAdded -= Technologies_TechnologyAdded;
                obj.Technologies.TechnologyRemoved -= Technologies_TechnologyRemoved;
                obj.Technologies.TechnologyUpgraded -= Technologies_TechnologyUpgraded;

                Global.DbManager.Delete(obj);

                ObjRemoveEvent(obj);
            }
        }

        /// <summary>
        /// Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="obj"></param>
        public void DoRemove(TroopObject obj) {
            lock (objLock) {

                troopobjects.Remove(obj.ObjectId);

                Global.DbManager.Delete(obj);

                obj.City = null;

                if (obj.Stub != null) {
                    obj.Stub.BeginUpdate();
                    obj.Stub.TroopObject = null;
                    obj.Stub.EndUpdate();
                }

                ObjRemoveEvent(obj);
            }
        }

        public List<GameObject> GetInRange(uint x, uint y, uint inRadius) {
            List<GameObject> ret = new List<GameObject>();

            foreach (Structure structure in this) {
                if (structure.TileDistance(x, y) <= inRadius)
                    ret.Add(structure);
            }

            return ret;
        }

        #endregion

        #region Updates
        
        public bool IsUpdating { get; private set; }

        private void CheckUpdateMode() {
            if (!Global.FireEvents)
                return;

            if (!IsUpdating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        public void BeginUpdate() {
            if (IsUpdating)
                throw new Exception("Nesting beginupdate");
            IsUpdating = true;
        }

        public void EndUpdate() {
            if (!IsUpdating)
                throw new Exception("Called EndUpdate without first calling BeginUpdate");

            Global.DbManager.Save(this);
            IsUpdating = false;
        }

        #endregion

        #region Channel Events

        public void Subscribe(IChannel s) {
            try {
                Global.Channel.Subscribe(s, "/CITY/" + id);
            }
            catch (DuplicateSubscriptionException) { }
        }

        public void Unsubscribe(IChannel s) {
            Global.Channel.Unsubscribe(s, "/CITY/" + id);
        }

        public void ResourceUpdateEvent() {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            Packet packet = new Packet(Command.CITY_RESOURCES_UPDATE);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(Resource, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void RadiusUpdateEvent() {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_RADIUS_UPDATE);
            
            Global.World.ObjectUpdateEvent(MainBuilding, MainBuilding.X, MainBuilding.Y);

            packet.AddUInt32(Id);
            packet.AddByte(radius);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void DefenseAttackPointUpdate()
        {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_ATTACK_DEFENSE_POINT_UPDATE);

            packet.AddUInt32(Id);
            packet.AddInt32(attackPoint);
            packet.AddInt32(defensePoint);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void HideNewUnitsUpdate() {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_HIDE_NEW_UNITS_UPDATE);

            packet.AddUInt32(Id);
            packet.AddByte(hideNewUnits ? (byte)1 : (byte)0);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void ObjAddEvent(GameObject obj) {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_OBJECT_ADD);
            packet.AddUInt16(Region.GetRegionIndex(obj));
            PacketHelper.AddToPacket(obj, packet, false);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void ObjRemoveEvent(GameObject obj) {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_OBJECT_REMOVE);
            packet.AddUInt32(Id);
            packet.AddUInt32(obj.ObjectId);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void ObjUpdateEvent(GameObject sender, uint origX, uint origY) {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_OBJECT_UPDATE);
            packet.AddUInt16(Region.GetRegionIndex(sender));
            PacketHelper.AddToPacket(sender, packet, false);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionRescheduled(GameAction stub, ActionState state) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_RESCHEDULED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionAdded(GameAction stub, ActionState state) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_STARTED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionRemoved(GameAction stub, ActionState state) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_COMPLETED);
            packet.AddInt32((int)state);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void Technologies_TechnologyUpgraded(Technology tech) {
            Packet packet = new Packet(Command.TECH_UPGRADED);
            packet.AddUInt32(Id);
            if (tech.ownerLocation == EffectLocation.CITY)
                packet.AddUInt32(0);
            else
                packet.AddUInt32(tech.ownerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void Technologies_TechnologyRemoved(Technology tech) {
            Packet packet = new Packet(Command.TECH_REMOVED);
            packet.AddUInt32(Id);
            if (tech.ownerLocation == EffectLocation.CITY)
                packet.AddUInt32(0);
            else
                packet.AddUInt32(tech.ownerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void Technologies_TechnologyCleared(TechnologyManager manager) {
            Packet packet = new Packet(Command.TECH_CLEARED);
            packet.AddUInt32(Id);
            if (manager.OwnerLocation == EffectLocation.CITY)
                packet.AddUInt32(0); 
            else
                packet.AddUInt32(manager.OwnerId);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void Technologies_TechnologyAdded(Technology tech) {
            Packet packet = new Packet(Command.TECH_ADDED);
            packet.AddUInt32(Id);
            if (tech.ownerLocation == EffectLocation.CITY)
                packet.AddUInt32(0);
            else
                packet.AddUInt32(tech.ownerId);
            packet.AddUInt32(tech.Type);
            packet.AddByte(tech.Level);

            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TroopManagerTroopUpdated(TroopStub stub) {
            BeginUpdate();
            Resource.Crop.Upkeep = Troops.Upkeep;
            EndUpdate();

            Packet packet = new Packet(Command.TROOP_UPDATED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TroopManagerTroopAdded(TroopStub stub) {
            BeginUpdate();
            Resource.Crop.Upkeep = Troops.Upkeep;
            EndUpdate();

            Packet packet = new Packet(Command.TROOP_ADDED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void TroopManagerTroopRemoved(TroopStub stub) {
            bool doUpdate = IsUpdating;
            if (!doUpdate) BeginUpdate();            
            Resource.Crop.Upkeep = Troops.Upkeep;
            if (!doUpdate) EndUpdate();

            Packet packet = new Packet(Command.TROOP_REMOVED);
            packet.AddUInt32(Id);
            packet.AddUInt32(stub.City.Id);
            packet.AddByte(stub.TroopId);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void UnitTemplateUnitUpdated(UnitTemplate sender) {
            Global.DbManager.Save(sender);
            Packet packet = new Packet(Command.UNIT_TEMPLATE_UPGRADED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(sender, packet);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void BattleStarted()
        {
            Packet packet = new Packet(Command.CITY_BATTLE_STARTED);
            packet.AddUInt32(Id);            
            Global.Channel.Post("/CITY/" + id, packet);
        }

        public void BattleEnded()
        {
            Packet packet = new Packet(Command.CITY_BATTLE_ENDED);
            packet.AddUInt32(Id);
            Global.Channel.Post("/CITY/" + id, packet);
        }
        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)structures.Values).GetEnumerator();
        }

        #endregion

        #region IEnumerable<Structure> Members

        IEnumerator<Structure> IEnumerable<Structure>.GetEnumerator() {
            return ((IEnumerable<Structure>)structures.Values).GetEnumerator();
        }

        #endregion     

        #region ICanDo Members

        public ActionWorker Worker { get; private set; }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "cities";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new[] {
                                new DbColumn("player_id", Owner.PlayerId, DbType.UInt32),
                                new DbColumn("name", Name, DbType.String, 32), 
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
                                new DbColumn("x", X, DbType.UInt32),
                                new DbColumn("y", Y, DbType.UInt32)
                            };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] { new DbColumn("id", Id, DbType.UInt32) }; }
        }

        public DbDependency[] DbDependencies {
            get {
                return new[] { new DbDependency("Technologies", false, true), new DbDependency("Template", false, true), };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        #region ILockable Members

        public int Hash {
            get { return unchecked((int)Owner.PlayerId); }
        }

        public object Lock {
            get { return Owner; }
        }

        #endregion

        #region ICanDo Members

        City ICanDo.City {
            get { return this; }
        }

        public uint WorkerId {
            get { return 0; }
        }

        #endregion

        public static bool IsNameValid(string cityName) {
            return cityName != string.Empty && cityName.Length >= 3 && cityName.Length <= 16 && Regex.IsMatch(cityName, "^([a-z][a-z0-9\\s].*)$", RegexOptions.IgnoreCase);
        }
    }
}