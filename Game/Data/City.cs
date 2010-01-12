#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Battle;
using Game.Comm;
using Game.Database;
using Game.Fighting;
using Game.Logic;
using Game.Util;

#endregion

namespace Game.Data {
    public class City : IEnumerable<Structure>, ICanDo, ILockable, IPersistableObject {
        private uint cityId;
        private string name = "Washington";
        private uint nextObjectId;
        private byte radius = 4;

        private object objLock = new object();

        private Dictionary<uint, Structure> structures = new Dictionary<uint, Structure>();
        private Dictionary<uint, TroopObject> troopobjects = new Dictionary<uint, TroopObject>();
        private Player owner;
        private ActionWorker worker;
        private LazyResource resource;

        private TroopManager troopManager;
        private UnitTemplate unitTemplate;
        private BattleManager battleManager;

        private TechnologyManager effectManager;

        #region Properties

        public Dictionary<uint, Structure>.Enumerator Structures {
            get { return structures.GetEnumerator(); }
        }

        public byte Radius {
            get { return radius; }
            set {
                if (value != radius) {
                    radius = value;
                    RadiusUpdateEvent();
                }
            }
        }

        public Structure MainBuilding {
            get { return structures[1]; }
        }

        public BattleManager Battle {
            get { return battleManager; }
            set { battleManager = value; }
        }

        public TroopManager Troops {
            get { return troopManager; }
        }

        public TechnologyManager Technologies {
            get { return effectManager; }
        }

        public TroopStub DefaultTroop {
            get { return troopManager[1]; }
            set { troopManager[1] = value; }
        }

        public UnitTemplate Template {
            get { return unitTemplate; }
        }

        public LazyResource Resource {
            get { return resource; }
        }

        public uint CityId {
            get { return cityId; }
            set {
                CheckUpdateMode();
                cityId = value;
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
            : this(
                owner, name,
                new LazyResource(resource.Crop, resource.Gold, resource.Iron, resource.Wood, resource.Labor),
                mainBuilding) {}

        public City(Player owner, string name, LazyResource resource, Structure mainBuilding) {
            this.owner = owner;
            this.name = name;

            worker = new ActionWorker(this);

            effectManager = new TechnologyManager(EffectLocation.City, this, cityId);

            troopManager = new TroopManager(this);

            TroopStub defaultTroop = new TroopStub();
            defaultTroop.BeginUpdate();
            defaultTroop.addFormation(FormationType.Normal);
            defaultTroop.addFormation(FormationType.Garrison);
            defaultTroop.addFormation(FormationType.InBattle);
            troopManager.Add(defaultTroop);
            defaultTroop.EndUpdate();

            troopManager.TroopUpdated += TroopManagerTroopUpdated;
            troopManager.TroopRemoved += TroopManagerTroopRemoved;
            troopManager.TroopAdded += TroopManagerTroopAdded;

            unitTemplate = new UnitTemplate(this);
            unitTemplate.UnitUpdated += UnitTemplateUnitUpdated;

            worker.ActionRemoved += WorkerActionRemoved;
            worker.ActionStarted += WorkerActionAdded;
            worker.ActionRescheduled += WorkerActionRescheduled;
            owner.add(this);

            this.resource = resource;
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

        public bool Add(uint id, TroopObject troop, bool save) {
            lock (objLock) {
                if (troopobjects.ContainsKey(id))
                    return false;

                troop.Stub.BeginUpdate();
                troop.Stub.TroopObject = troop;
                troop.Stub.EndUpdate();

                troop.City = this;

                troopobjects.Add(id, troop);

                if (nextObjectId < id)
                    nextObjectId = id;

                if (save)
                    Global.dbManager.Save(troop);

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

        public bool Add(uint id, Structure structure, bool save) {
            lock (objLock) {
                if (structures.ContainsKey(id))
                    return false;

                structure.City = this;

                structures.Add(id, structure);

                if (nextObjectId < id)
                    nextObjectId = id;

                if (save)
                    Global.dbManager.Save(structure);

                structure.Technologies.TechnologyAdded += Technologies_TechnologyAdded;
                structure.Technologies.TechnologyRemoved += Technologies_TechnologyRemoved;
                structure.Technologies.TechnologyUpgraded += Technologies_TechnologyUpgraded;
                ObjAddEvent(structure);
            }

            return true;
        }

        public bool Add(uint id, Structure structure) {
            return Add(id, structure, true);
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

                Global.dbManager.Delete(obj);

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

                worker.Remove(obj, ActionInterrupt.KILLED);
                obj.Technologies.clear();
                structures.Remove(obj.ObjectId);

                obj.Technologies.TechnologyAdded -= Technologies_TechnologyAdded;
                obj.Technologies.TechnologyRemoved -= Technologies_TechnologyRemoved;
                obj.Technologies.TechnologyUpgraded -= Technologies_TechnologyUpgraded;

                Global.dbManager.Delete(obj);

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

            if (!MultiObjectLock.IsLocked(this))
                throw new Exception("Object not locked");
        }

        public void BeginUpdate() {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;
        }

        public void EndUpdate() {
            if (!updating)
                throw new Exception("Called EndUpdate without first calling BeginUpdate");

            Global.dbManager.Save(this);
            updating = false;
        }

        #endregion

        #region Channel Events

        public void Subscribe(IChannel s) {
            Global.Channel.Subscribe(s, "/CITY/" + cityId);
        }

        public void Unsubscribe(IChannel s) {
            Global.Channel.Unsubscribe(s, "/CITY/" + cityId);
        }

        public void ResourceUpdateEvent() {
            if (!Global.FireEvents)
                return;

            CheckUpdateMode();

            Packet packet = new Packet(Command.CITY_RESOURCES_UPDATE);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(resource, packet);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        public void RadiusUpdateEvent() {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_RADIUS_UPDATE);

            Structure mainBuilding = MainBuilding;
            Global.World.ObjUpdateEvent(mainBuilding, mainBuilding.X, mainBuilding.Y);

            packet.addUInt32(CityId);
            packet.addByte(radius);

            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        public void ObjAddEvent(GameObject obj) {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_OBJECT_ADD);
            packet.addUInt16(Region.getRegionIndex(obj));
            PacketHelper.AddToPacket(obj, packet, false);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        public void ObjRemoveEvent(GameObject obj) {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_OBJECT_REMOVE);
            packet.addUInt32(CityId);
            packet.addUInt32(obj.ObjectId);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        public void ObjUpdateEvent(GameObject sender, uint origX, uint origY) {
            if (!Global.FireEvents)
                return;

            Packet packet = new Packet(Command.CITY_OBJECT_UPDATE);
            packet.addUInt16(Region.getRegionIndex(sender));
            PacketHelper.AddToPacket(sender, packet, false);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void WorkerActionRescheduled(Action stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_RESCHEDULED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void WorkerActionAdded(Action stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_STARTED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void WorkerActionRemoved(Action stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_COMPLETED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void Technologies_TechnologyUpgraded(Technology tech) {
            Packet packet = new Packet(Command.TECH_UPGRADED);
            packet.addUInt32(CityId);
            if (tech.ownerLocation == EffectLocation.City)
                packet.addUInt32(0);
            else
                packet.addUInt32(tech.ownerId);
            packet.addUInt32(tech.Type);
            packet.addByte(tech.Level);

            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void Technologies_TechnologyRemoved(Technology tech) {
            Packet packet = new Packet(Command.TECH_REMOVED);
            packet.addUInt32(CityId);
            if (tech.ownerLocation == EffectLocation.City)
                packet.addUInt32(0);
            else
                packet.addUInt32(tech.ownerId);
            packet.addUInt32(tech.Type);
            packet.addByte(tech.Level);

            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void Technologies_TechnologyAdded(Technology tech) {
            Packet packet = new Packet(Command.TECH_ADDED);
            packet.addUInt32(CityId);
            if (tech.ownerLocation == EffectLocation.City)
                packet.addUInt32(0);
            else
                packet.addUInt32(tech.ownerId);
            packet.addUInt32(tech.Type);
            packet.addByte(tech.Level);

            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void TroopManagerTroopUpdated(TroopStub stub) {
            Packet packet = new Packet(Command.TROOP_UPDATED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void TroopManagerTroopAdded(TroopStub stub) {
            Packet packet = new Packet(Command.TROOP_ADDED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        private void TroopManagerTroopRemoved(TroopStub stub) {
            Packet packet = new Packet(Command.TROOP_REMOVED);
            packet.addUInt32(CityId);
            packet.addByte(stub.TroopId);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        public void UnitTemplateUnitUpdated(UnitTemplate sender) {
            Global.dbManager.Save(sender);
            Packet packet = new Packet(Command.UNIT_TEMPLATE_UPGRADED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(sender, packet);
            Global.Channel.Post("/CITY/" + cityId, packet);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) structures.Values).GetEnumerator();
        }

        #endregion

        #region IEnumerable<Structure> Members

        IEnumerator<Structure> IEnumerable<Structure>.GetEnumerator() {
            return ((IEnumerable<Structure>) structures.Values).GetEnumerator();
        }

        #endregion

        #region ICanDo Members

        public ActionWorker Worker {
            get { return worker; }
            set { worker = value; }
        }

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
                                          new DbColumn("labor", resource.Labor.RawValue, DbType.Int32),
                                          new DbColumn("labor_realize_time", resource.Labor.LastRealizeTime, DbType.DateTime),
                                          new DbColumn("labor_production_rate", resource.Labor.Rate, DbType.Int32),
                                      };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] {new DbColumn("id", CityId, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get {
                return new[]
                       {new DbDependency("Technologies", false, true), new DbDependency("Template", false, true),};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        #region ILockable Members

        public int Hash {
            get { return unchecked((int) owner.PlayerId); }
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
    }
}