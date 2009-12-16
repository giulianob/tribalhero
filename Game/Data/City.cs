using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using System.Collections;
using Game.Util;
using Game.Comm;
using Game.Logic;
using Game.Fighting;
using Game.Battle;
using Game.Database;
using Game.Logic.Actions;

namespace Game {
    public class City : IEnumerable<Structure>, ICanDo, ILockable, IPersistableObject {
        uint cityId = 0;
        string name = "NEW MODULE";
        uint nextObjectId = 0;
        byte radius = 4;

        object objLock = new object();
                
        Dictionary<uint, Structure> structures = new Dictionary<uint, Structure>();
        Dictionary<uint, TroopObject> troopobjects = new Dictionary<uint, TroopObject>();
        Player owner;
        ActionWorker worker;
        LazyResource resource;
        
        TroopManager troopManager;
        UnitTemplate unitTemplate;
        BattleManager battleManager;

        TechnologyManager effectManager;        

        #region Properties
      
      	public Dictionary<uint, Structure>.Enumerator Structures {
            get { return structures.GetEnumerator(); }
        }

        public byte Radius {
            get { return radius; }
            set {
                if (value != radius) {
                    radius = value;
                    radius_UpdateEvent();
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
            set { cityId = value; }
        }
        
        public string Name {
            get { return name; }
            set { name = value; }
        }
        
        public uint Population {
            get {
                return 0;
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
        public City(Player owner, string name, Resource resource, Structure mainBuilding) :
            this(owner, name, new LazyResource(null,resource.Crop,resource.Gold,resource.Iron,resource.Wood,resource.Labor), mainBuilding)
        {
            
        }

        public City(Player owner, string name, LazyResource resource, Structure mainBuilding) {
            this.owner = owner;
            this.name = name;
            resource.City = this;            
            this.resource = resource;

            worker = new ActionWorker(this);

            effectManager = new TechnologyManager(EffectLocation.City, this, cityId);

            troopManager = new TroopManager(this);
            TroopStub default_troop = new TroopStub(troopManager);
            default_troop.addFormation(FormationType.Normal);
            default_troop.addFormation(FormationType.Garrison);
            default_troop.addFormation(FormationType.InBattle);
            troopManager.Add(default_troop);

            troopManager.TroopUpdated += new TroopManager.UpdateCallback(troop_manager_TroopUpdated);
            troopManager.TroopRemoved += new TroopManager.UpdateCallback(troop_manager_TroopRemoved);
            troopManager.TroopAdded += new TroopManager.UpdateCallback(troop_manager_TroopAdded);

            unitTemplate = new UnitTemplate(this);
            unitTemplate.UnitUpdated += new UnitTemplate.UpdateCallback(unitTemplate_UnitUpdated);

            worker.ActionRemoved += new ActionWorker.UpdateCallback(worker_ActionRemoved);
            worker.ActionStarted += new ActionWorker.UpdateCallback(worker_ActionAdded);
            worker.ActionRescheduled += new ActionWorker.UpdateCallback(worker_ActionRescheduled);
            owner.add(this);

            if (mainBuilding != null) {
                mainBuilding.ObjectID = 1;
                add(1, mainBuilding, false);
            }            
        }
        #endregion

        #region Object Management
        public TroopObject getTroop(uint objectId) {
            return troopobjects[objectId];
        }

        public bool tryGetStructure(uint objectId, out Structure structure) {
            return structures.TryGetValue(objectId, out structure);
        }

        public bool tryGetTroop(uint objectId, out TroopObject troop) {
            return troopobjects.TryGetValue(objectId, out troop);
        }

        public bool add(uint id, TroopObject troop, bool save) {
            lock (objLock) {
                troop.City = this;
                troop.Stub.TroopObject = troop;
                try {
                    troopobjects.Add(id, troop);
                }
                catch (Exception) {
                    troop.City = null;
                    return false;
                }

                if (nextObjectId < id)
                    nextObjectId = id;

                if (save)
                    Global.dbManager.Save(troop);

                obj_AddEvent(troop);
            }

            return true;
        }

        public bool add(TroopObject troop) {
            lock (objLock) {               
                ++nextObjectId;
                troop.ObjectID = nextObjectId;
                return add(nextObjectId, troop, true);
            }
        }

        public bool add(uint id, Structure structure, bool save) {
            lock (objLock) {
                structure.City = this;
                try {
                    structures.Add(id, structure);
                }
                catch (Exception e) {
                    Global.Logger.Error(e);
                    structure.City = null;
                    return false;
                }

                if (nextObjectId < id)
                    nextObjectId = id;

                if (save)
                    Global.dbManager.Save(structure);

                structure.Technologies.TechnologyAdded += new TechnologyManager.TechnologyUpdatedCallback(Technologies_TechnologyAdded);
                structure.Technologies.TechnologyRemoved += new TechnologyManager.TechnologyUpdatedCallback(Technologies_TechnologyRemoved);
                structure.Technologies.TechnologyUpgraded += new TechnologyManager.TechnologyUpdatedCallback(Technologies_TechnologyUpgraded);
                obj_AddEvent(structure);
            }

            return true;
        }

        public bool add(uint id, Structure structure) {
            return add(id, structure, true);
        }

        public bool add(Structure structure) {
            lock (objLock) {
                ++nextObjectId;
                structure.ObjectID = nextObjectId;
                return add(nextObjectId, structure);
            }
        }

        public bool remove(TroopObject obj) {
            lock (objLock) {
                try {
                    troopobjects.Remove(obj.ObjectID);
                }
                catch (Exception) {
                    return false;
                }

                Global.dbManager.Delete(obj);

                obj.City = null;
                obj.Stub.TroopObject = null;

                obj_RemoveEvent(obj);
            }

            return true;
        }

        public bool remove(Structure obj) {
            lock (objLock) {
                try {
                    worker.Remove(obj, ActionInterrupt.KILLED);
                    obj.Technologies.clear();
                    structures.Remove(obj.ObjectID);

                    obj.Technologies.TechnologyAdded -= new TechnologyManager.TechnologyUpdatedCallback(Technologies_TechnologyAdded);
                    obj.Technologies.TechnologyRemoved -= new TechnologyManager.TechnologyUpdatedCallback(Technologies_TechnologyRemoved);
                    obj.Technologies.TechnologyUpgraded -= new TechnologyManager.TechnologyUpdatedCallback(Technologies_TechnologyUpgraded);
                }
                catch (Exception) {
                    return false;
                }

                Global.dbManager.Delete(obj, obj.Technologies);

                obj.City = null;
                obj_RemoveEvent(obj);
            }

            return true;
        }

        public List<GameObject> getInRange(uint x, uint y, uint radius) {
            List<GameObject> ret = new List<GameObject>();

            foreach (Structure structure in this) {
                if (structure.distance(x, y) <= radius)
                    ret.Add(structure);
            }

            return ret;
        }
        #endregion

        #region Channel Events
        public void Subscribe(IChannel s) {
            Global.Channel.Subscribe(s,"/CITY/"+this.cityId);
        }

        public void Unsubscribe(IChannel s) {
            Global.Channel.Unsubscribe(s, "/CITY/" + this.cityId);
        }

        public void resource_UpdateEvent() {
            if (!Global.FireEvents) return; 

            Packet packet = new Packet(Command.CITY_RESOURCES_UPDATE);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(resource, packet);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        public void radius_UpdateEvent() {
            if (!Global.FireEvents) return; 

            Packet packet = new Packet(Command.CITY_RADIUS_UPDATE);

            Structure mainBuilding = MainBuilding;
            Global.World.obj_UpdateEvent(mainBuilding, mainBuilding.X, mainBuilding.Y);
            
            packet.addUInt32(CityId);
            packet.addByte(radius);

            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        public void obj_AddEvent(GameObject obj) {
            if (!Global.FireEvents) return; 

            Packet packet = new Packet(Command.CITY_OBJECT_ADD);
            packet.addUInt16(Region.getRegionIndex(obj));
            PacketHelper.AddToPacket(obj, packet, false);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        public void obj_RemoveEvent(GameObject obj) {
            if (!Global.FireEvents) return; 

            Packet packet = new Packet(Command.CITY_OBJECT_REMOVE);
            packet.addUInt32(CityId);
            packet.addUInt32(obj.ObjectID);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        public void obj_UpdateEvent(GameObject sender, uint origX, uint origY) {
            if (!Global.FireEvents) return; 

            Packet packet = new Packet(Command.CITY_OBJECT_UPDATE);
            packet.addUInt16(Region.getRegionIndex(sender));
            PacketHelper.AddToPacket(sender, packet, false);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void worker_ActionRescheduled(Game.Logic.Action stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_RESCHEDULED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void worker_ActionAdded(Game.Logic.Action stub) {
           if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_STARTED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void worker_ActionRemoved(Game.Logic.Action stub) {
            if (stub is PassiveAction && !(stub as PassiveAction).IsVisible)
                return;

            Packet packet = new Packet(Command.ACTION_COMPLETED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet, true);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void Technologies_TechnologyUpgraded(Technology tech) {
            Packet packet = new Packet(Command.TECH_UPGRADED);
            packet.addUInt32(CityId);
            if (tech.ownerLocation == EffectLocation.City) {
                packet.addUInt32(0);
            }
            else {
                packet.addUInt32(tech.ownerId);
            }
            packet.addUInt32(tech.Type);
            packet.addByte(tech.Level);

            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void Technologies_TechnologyRemoved(Technology tech) {
            Packet packet = new Packet(Command.TECH_REMOVED);
            packet.addUInt32(CityId);
            if (tech.ownerLocation == EffectLocation.City) {
                packet.addUInt32(0);
            }
            else {
                packet.addUInt32(tech.ownerId);
            }
            packet.addUInt32(tech.Type);
            packet.addByte(tech.Level);

            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void Technologies_TechnologyAdded(Technology tech) {
            Packet packet = new Packet(Command.TECH_ADDED);
            packet.addUInt32(CityId);
            if (tech.ownerLocation == EffectLocation.City) {
                packet.addUInt32(0);
            }
            else {
                packet.addUInt32(tech.ownerId);
            }
            packet.addUInt32(tech.Type);
            packet.addByte(tech.Level);

            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void troop_manager_TroopUpdated(TroopStub stub) {
            Packet packet = new Packet(Command.TROOP_UPDATED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void troop_manager_TroopAdded(TroopStub stub) {
            Packet packet = new Packet(Command.TROOP_ADDED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(stub, packet);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        void troop_manager_TroopRemoved(TroopStub stub) {
            Packet packet = new Packet(Command.TROOP_REMOVED);
            packet.addUInt32(CityId);
            packet.addByte(stub.TroopId);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }

        public void unitTemplate_UnitUpdated(UnitTemplate sender) {
            Global.dbManager.Save(sender);
            Packet packet = new Packet(Command.UNIT_TEMPLATE_UPGRADED);
            packet.addUInt32(CityId);
            PacketHelper.AddToPacket(sender, packet);
            Global.Channel.Post("/CITY/" + this.cityId, packet);
        }
        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((IEnumerable)structures.Values).GetEnumerator();
        }

        #endregion

        #region IEnumerable<Structure> Members

        IEnumerator<Structure> IEnumerable<Structure>.GetEnumerator() {
            return ((IEnumerable<Structure>)structures.Values).GetEnumerator();

        }

        #endregion

        #region ICanDo Members

        public ActionWorker Worker {
            get {
                return this.worker;
            }
            set {
                worker = value;
            }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "cities";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {                    
                    new DbColumn("player_id", owner.PlayerId, System.Data.DbType.UInt32),
                    new DbColumn("name", Name, System.Data.DbType.String, 32),
                    new DbColumn("gold", resource.Gold.RawValue, System.Data.DbType.Int32),
                    new DbColumn("gold_realize_time", resource.Gold.LastRealizeTime, System.Data.DbType.DateTime),
                    new DbColumn("gold_production_rate", resource.Gold.Rate, System.Data.DbType.Int32),
                    new DbColumn("wood", resource.Wood.RawValue, System.Data.DbType.Int32),
                    new DbColumn("wood_realize_time", resource.Wood.LastRealizeTime, System.Data.DbType.DateTime),
                    new DbColumn("wood_production_rate", resource.Wood.Rate, System.Data.DbType.Int32),
                    new DbColumn("iron", resource.Iron.RawValue, System.Data.DbType.Int32),
                    new DbColumn("iron_realize_time", resource.Iron.LastRealizeTime, System.Data.DbType.DateTime),
                    new DbColumn("iron_production_rate", resource.Iron.Rate, System.Data.DbType.Int32),
                    new DbColumn("crop", resource.Crop.RawValue, System.Data.DbType.Int32),
                    new DbColumn("crop_realize_time", resource.Crop.LastRealizeTime, System.Data.DbType.DateTime),
                    new DbColumn("crop_production_rate", resource.Crop.Rate, System.Data.DbType.Int32),
                    new DbColumn("labor", resource.Labor.RawValue, System.Data.DbType.Int32),
                    new DbColumn("labor_realize_time", resource.Labor.LastRealizeTime, System.Data.DbType.DateTime),
                    new DbColumn("labor_production_rate", resource.Labor.Rate, System.Data.DbType.Int32),
                };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                    new DbColumn("id", CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        bool dbPersisted = false;
        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }
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
    }
}