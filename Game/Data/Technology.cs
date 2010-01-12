#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Database;
using Game.Logic;

#endregion

namespace Game.Data {
    public class TechnologyBase {
        public uint techtype;
        public byte level;
        public ushort time;
        public Resource resources;
        public List<Effect> effects;
    }

    public class Technology {
        private TechnologyBase techBase;
        public EffectLocation ownerLocation;
        public uint ownerId;

        public Technology(TechnologyBase techBase) {
            this.techBase = techBase;
        }

        public uint Type {
            get { return techBase.techtype; }
            set { techBase.techtype = value; }
        }

        public byte Level {
            get { return techBase.level; }
            set { techBase.level = value; }
        }

        public List<Effect> Effects {
            get { return techBase.effects; }
        }

        public static bool checkLocation(EffectLocation technology_location, EffectLocation effect_path,
                                         EffectLocation target_location) {
            return effect_path == target_location;
        }

        internal List<Effect> getEffects(EffectCode effect_code, EffectInheritance inherit,
                                         EffectLocation target_location) {
            List<Effect> list = new List<Effect>();
            bool isSelf = (inherit & EffectInheritance.Self) == EffectInheritance.Self;
            bool isInvisible = (inherit & EffectInheritance.Invisible) == EffectInheritance.Invisible;

            foreach (Effect effect in techBase.effects) {
                if (effect.id != effect_code)
                    continue;
                if (checkLocation(ownerLocation, effect.location, target_location)) {
                    if (isSelf) {
                        if (!effect.isPrivate)
                            list.Add(effect);
                    }
                    if (isInvisible) {
                        if (effect.isPrivate)
                            list.Add(effect);
                    }
                }
            }
            return list;
        }

        public List<Effect> getAllEffects(EffectInheritance inherit, EffectLocation target_location) {
            List<Effect> list = new List<Effect>();
            bool isSelf = (inherit & EffectInheritance.Self) == EffectInheritance.Self;
            bool isInvisible = (inherit & EffectInheritance.Invisible) == EffectInheritance.Invisible;

            foreach (Effect effect in techBase.effects) {
                if (checkLocation(ownerLocation, effect.location, target_location)) {
                    if (isSelf) {
                        if (!effect.isPrivate)
                            list.Add(effect);
                    }
                    if (isInvisible) {
                        if (effect.isPrivate)
                            list.Add(effect);
                    }
                }
            }
            return list;
        }

        public void print() {
            Global.Logger.Info(string.Format("Technology type[{0}] lvl[{1}] Location[{2}]", techBase.techtype,
                                             techBase.level, ownerLocation));
            foreach (Effect effect in techBase.effects)
                effect.print();
        }
    }

    public class TechnologyManager : IHasEffect, IEnumerable<Technology>, IPersistableList {
        private List<Technology> technologies = new List<Technology>();
        private TechnologyManager parent;
        private EffectLocation ownerLocation;
        private uint ownerId;
        private object owner;

        #region Events

        public delegate void TechnologyUpdatedCallback(Technology tech);

        public event TechnologyUpdatedCallback TechnologyAdded;
        public event TechnologyUpdatedCallback TechnologyRemoved;
        public event TechnologyUpdatedCallback TechnologyUpgraded;

        #endregion

        public TechnologyManager(EffectLocation location, object owner, uint ownerId) {
            ownerLocation = location;
            this.owner = owner;
            this.ownerId = ownerId;
        }

        public int TechnologyCount {
            get { return technologies.Count; }
        }

        public int OwnedTechnologyCount {
            get {
                int count = 0;
                foreach (Technology tech in technologies) {
                    if (tech.ownerLocation == ownerLocation)
                        count++;
                }

                return count;
            }
        }

        public TechnologyManager Parent {
            get { return parent; }
            set {
                if (parent != null) {}
                parent = value;
            }
        }

        public uint ID {
            get { return ownerId; }
            set { ownerId = value; }
        }

        #region Add/Remove

        private void addChildCopy(Technology tech, bool notify) {
            //only add tech if it applies to this tech manager
            if (tech.Effects.Exists(delegate(Effect effect) { return effect.location == ownerLocation; })) {
                technologies.Add(tech);
                if (parent != null)
                    parent.addChildCopy(tech, notify);
            }
        }

        public bool add(Technology tech) {
            return add(tech, true);
        }

        public bool add(Technology tech, bool notify) {
            if (
                technologies.Exists(
                    delegate(Technology technology) {
                        return technology.Type == tech.Type && technology.ownerLocation == ownerLocation &&
                               technology.ownerId == ownerId;
                    }))
                return false;
            tech.ownerId = ownerId;
            tech.ownerLocation = ownerLocation;
            technologies.Add(tech);
            if (parent != null)
                parent.addChildCopy(tech, notify);
            if (TechnologyAdded != null && notify)
                TechnologyAdded(tech);
            return true;
        }

        public bool upgrade(Technology tech) {
            if (!remove(tech.Type, false))
                return false;
            if (!add(tech, false))
                return false;

            if (TechnologyUpgraded != null)
                TechnologyUpgraded(tech);
            //  if (parent != null) parent.notifyUpgrade(tech);

            return true;
        }

        private void notifyUpgrade(Technology tech) {
            if (TechnologyUpgraded != null &&
                tech.Effects.Exists(delegate(Effect effect) { return effect.location == ownerLocation; }))
                TechnologyUpgraded(tech);

            if (parent != null)
                parent.notifyUpgrade(tech);
        }

        public void clear() {
            for (int i = technologies.Count - 1; i >= 0; i--) {
                Technology tech = technologies[i];
                if (tech.ownerId != ownerId || tech.ownerLocation != ownerLocation)
                    continue;

                technologies.RemoveAt(i);
                if (parent != null)
                    parent.removeChildCopy(tech, false);
            }
        }

        private bool remove(uint techType, bool notify) {
            Technology tech =
                technologies.Find(
                    delegate(Technology technology) {
                        return technology.Type == techType && technology.ownerId == ownerId &&
                               technology.ownerLocation == ownerLocation;
                    });
            if (tech == null)
                return false;
            if (!technologies.Remove(tech))
                return false;
            if (parent != null)
                parent.removeChildCopy(tech, notify);
            if (TechnologyRemoved != null && notify)
                TechnologyRemoved(tech);
            return true;
        }

        private void removeChildCopy(Technology tech, bool notify) {
            if (technologies.Remove(tech)) {
                if (TechnologyRemoved != null && notify)
                    TechnologyRemoved(tech);
            }
        }

        #endregion

        #region Utilities

        public List<Effect> GetEffects(EffectCode effect_code, EffectInheritance inherit) {
            List<Effect> list = new List<Effect>();
            foreach (Technology tech in technologies)
                list.AddRange(tech.getEffects(effect_code, inherit, ownerLocation));
            if ((inherit & EffectInheritance.Upward) == EffectInheritance.Upward) {
                if (parent != null)
                    list.AddRange(parent.GetEffects(effect_code, EffectInheritance.Upward | EffectInheritance.Self));
            }
            return list;
        }

        public int Sum(EffectCode effect_code, EffectInheritance inherit) {
            int ret = 0;
            foreach (Effect e in GetEffects(effect_code, inherit)) {
                //     ret += e.effect_base.value;
            }
            return ret;
        }

        public int Count(int effect_id) {
            return 0;
        }

        public int Max(EffectCode effect_code, EffectInheritance inherit, byte param_order) {
            int max = Int32.MinValue;
            foreach (Effect e in GetEffects(effect_code, inherit)) {
                if ((int) e.value[param_order] > max)
                    max = (int) e.value[param_order];
            }
            return max;
        }

        public int Min(EffectCode effect_code, EffectInheritance inherit, byte param_order) {
            int min = Int32.MaxValue;
            foreach (Effect e in GetEffects(effect_code, inherit)) {
                if ((int) e.value[param_order] < min)
                    min = (int) e.value[param_order];
            }
            return min;
        }

        public int Avg(int effect_id) {
            return 0;
        }

        #endregion

        public void print() {
            Global.Logger.Info("Printing TechnologyManager Location:" + ownerLocation.ToString());
            foreach (Technology tech in technologies)
                tech.print();
        }

        public bool TryGetTechnology(uint techId, out Technology technology) {
            technology =
                technologies.Find(
                    delegate(Technology tech) { return tech.Type == techId && tech.ownerId == ownerId && tech.ownerLocation == ownerLocation; });

            return technology != null;
        }

        #region IEnumerable<Technology> Members

        public IEnumerator<Technology> GetEnumerator() {
            return technologies.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return technologies.GetEnumerator();
        }

        #endregion

        #region IHasEffect Members

        public IEnumerable<Effect> GetAllEffects(EffectInheritance inherit) {
            List<Effect> list = new List<Effect>();
            foreach (Technology tech in technologies)
                list.AddRange(tech.getAllEffects(inherit, ownerLocation));
            if ((inherit & EffectInheritance.Upward) == EffectInheritance.Upward) {
                if (parent != null)
                    list.AddRange(parent.GetAllEffects(EffectInheritance.Upward | EffectInheritance.Self));
            }
            return list;
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "technologies";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] {
                                          new DbColumn("city_id",
                                                       owner is Structure
                                                           ? (owner as Structure).City.CityId
                                                           : (owner is City ? (owner as City).CityId : 0), DbType.UInt32),
                                          new DbColumn("owner_id", ownerId, DbType.UInt32),
                                          new DbColumn("owner_location", (byte) ownerLocation, DbType.Byte)
                                      };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public DbColumn[] DbColumns {
            get { return new DbColumn[] {}; }
        }

        public DbColumn[] DbListColumns {
            get { return new DbColumn[] {new DbColumn("type", DbType.UInt16), new DbColumn("level", DbType.Byte)}; }
        }

        private bool dbPersisted = false;

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion

        #region IEnumerable<DbColumn[]> Members

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            foreach (Technology tech in technologies) {
                if (tech.ownerLocation != ownerLocation)
                    continue;

                yield return
                    new DbColumn[]
                    {new DbColumn("type", tech.Type, DbType.UInt16), new DbColumn("level", tech.Level, DbType.Byte)};
            }
        }

        #endregion
    }
}