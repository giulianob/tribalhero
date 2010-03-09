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
using Game.Map;
using Game.Util;

#endregion

namespace Game.Data {
    public class City : IEnumerable<Structure>, ICanDo, ILockable, IPersistableObject {
        private uint id;
        private string name = "Washington";
        private uint nextObjectId;
        private byte radius = 4;

        private readonly object objLock = new object();

        private readonly Dictionary<uint, Structure> structures = new Dictionary<uint, Structure>();
        private readonly Dictionary<uint, TroopObject> troopobjects = new Dictionary<uint, TroopObject>();
        private readonly Player owner;
        private readonly LazyResource resource;
        private readonly UnitTemplate unitTemplate;

        #region Properties

        public Dictionary<uint, Structure>.Enumerator Structures {
            get { return structures.GetEnumerator(); }
        }

        public byte Radius {
            get { return radius; }
            set {
                if (value == radius)
                    return;

                radius = value;
                RadiusUpdateEvent();
            }
        }

        public Structure MainBuilding {
            get { return structures[1]; }
        }

        public BattleManager Battle { get; set; }

        public TroopManager Troops { get; private set; }

        public TechnologyManager Technologies { get; private set; }

        public TroopStub DefaultTroop {
            get { return Troops[1]; }
            set { Troops[1] = value; }
        }

        public UnitTemplate Template {
            get { return unitTemplate; }
        }

        public LazyResource Resource {
            get { return resource; }
        }

        public uint Id {
            get { return id; }
            set {
                CheckUpdateMode();
                id = value;
            }
        }

        public string Name {
            get { return name; }
            set {
                CheckUpdateMode();
                name = value;
            }
        }

        public Player Owner {
            get { return owner; }
        }

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

        #endregion

        #region Constructors

        public City(Player owner, string name, Resource resource, Structure mainBuilding)
            : this(owner, name, new LazyResource(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor), mainBuilding) {}

        public City(Player owner, string name, LazyResource resource, Structure mainBuilding) {
            this.owner = owner;
            this.name = name;
            this.resource = resource;

            Worker = new ActionWorker(this);

            Technologies = new TechnologyManager(EffectLocation.CITY, this, id);

            Troops = new TroopManager(this);

            TroopStub defaultTroop = new TroopStub();
            defaultTroop.BeginUpdate();
            defaultTroop.AddFormation(FormationType.NORMAL);
            defaultTroop.AddFormation(FormationType.GARRISON);
            defaultTroop.AddFormation(FormationType.IN_BATTLE);
            Troops.Add(defaultTroop);
            defaultTroop.EndUpdate();

            Troops.TroopUpdated += TroopManagerTroopUpdated;
            Troops.TroopRemoved += TroopManagerTroopRemoved;
            Troops.TroopAdded += TroopManagerTroopAdded;

            unitTemplate = new UnitTemplate(this);
            unitTemplate.UnitUpdated += UnitTemplateUnitUpdated;

            Worker.ActionRemoved += WorkerActionRemoved;
            Worker.ActionStarted += WorkerActionAdded;
            Worker.ActionRescheduled += WorkerActionRescheduled;
            owner.Add(this);
            
            resource.Labor.Add(10);

            if (mainBuilding != null) {
                mainBuilding.ObjectId = 1;
                Add(1, mainBuilding, false);
                Formula.ResourceCap(this);
            }

            resource.ResourcesUpdate += ResourceUpdateEvent;
        }

        #endregion

        #region Object Management

        public TroopObject GetTroop(uint objectId) {
            return troopobjects[objectId];
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

                troop.Stub.BeginUpdate();
                troop.Stub.TroopObject = troop;
                troop.Stub.EndUpdate();

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

        public bool Remove(TroopObject obj) {
            lock (objLock) {
                if (!troopobjects.ContainsKey(obj.ObjectId))
                    return false;

                troopobjects.Remove(obj.ObjectId);

                Global.DbManager.Delete(obj);

                obj.City = null;
                obj.Stub.BeginUpdate();
                obj.Stub.TroopObject = null;
                obj.Stub.EndUpdate();

                ObjRemoveEvent(obj);
            }

            return true;
        }

        public bool Remove(Structure obj) {
            lock (objLock) {
                if (obj == MainBuilding)
                    throw new Exception("Trying to remove main building");

                if (!structures.ContainsKey(obj.ObjectId))
                    return false;

                Worker.Remove(obj, ActionInterrupt.KILLED);
                obj.Technologies.Clear();
                structures.Remove(obj.ObjectId);

                obj.Technologies.TechnologyAdded -= Technologies_TechnologyAdded;
                obj.Technologies.TechnologyRemoved -= Technologies_TechnologyRemoved;
                obj.Technologies.TechnologyUpgraded -= Technologies_TechnologyUpgraded;

                Global.DbManager.Delete(obj);

                obj.City = null;
                ObjRemoveEvent(obj);
            }

            return true;
        }

        public List<GameObject> GetInRange(uint x, uint y, uint inRadius) {
            List<GameObject> ret = new List<GameObject>();

            foreach (Structure structure in this) {
                if (structure.Distance(x, y) <= inRadius)
                    ret.Add(structure);
            }

            return ret;
        }

        #endregion

        #region Updates

        private bool updating;

        private void CheckUpdateMode() {
            if (!Global.FireEvents)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        public void BeginUpdate() {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;
        }

        public void EndUpdate() {
            if (!updating)
                throw new Exception("Called EndUpdate without first calling BeginUpdate");

            Global.DbManager.Save(this);
            updating = false;
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
            PacketHelper.AddToPacket(resource, packet);
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

        private void WorkerActionRescheduled(GameAction stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_RESCHEDULED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionAdded(GameAction stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_STARTED);
            packet.AddUInt32(Id);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + id, packet);
        }

        private void WorkerActionRemoved(GameAction stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_COMPLETED);
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
            bool doUpdate = updating;
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
                                new DbColumn("player_id", owner.PlayerId, DbType.UInt32),
                                new DbColumn("name", Name, DbType.String, 32), new DbColumn("radius", Radius, DbType.Byte),
                                new DbColumn("gold", resource.Gold.RawValue, DbType.Int32),
                                new DbColumn("gold_realize_time", resource.Gold.LastRealizeTime, DbType.DateTime),
                                new DbColumn("gold_production_rate", resource.Gold.Rate, DbType.Int32),
                                new DbColumn("wood", resource.Wood.RawValue, DbType.Int32),
                                new DbColumn("wood_realize_time", resource.Wood.LastRealizeTime, DbType.DateTime),
                                new DbColumn("wood_production_rate", resource.Wood.Rate, DbType.Int32),
                                new DbColumn("iron", resource.Iron.RawValue, DbType.Int32),
                                new DbColumn("iron_realize_time", resource.Iron.LastRealizeTime, DbType.DateTime),
                                new DbColumn("iron_production_rate", resource.Iron.Rate, DbType.Int32),
                                new DbColumn("crop", resource.Crop.RawValue, DbType.Int32),
                                new DbColumn("crop_realize_time", resource.Crop.LastRealizeTime, DbType.DateTime),
                                new DbColumn("crop_production_rate", resource.Crop.Rate, DbType.Int32),
                                new DbColumn("crop_upkeep", resource.Crop.Upkeep, DbType.Int32),
                                new DbColumn("labor", resource.Labor.RawValue, DbType.Int32),
                                new DbColumn("labor_realize_time", resource.Labor.LastRealizeTime, DbType.DateTime),
                                new DbColumn("labor_production_rate", resource.Labor.Rate, DbType.Int32),
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
            get { return unchecked((int)owner.PlayerId); }
        }

        public object Lock {
            get { return owner; }
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