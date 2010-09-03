﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Database;
using Game.Logic;
using Game.Util;

namespace Game.Data {
    public class TechnologyManager : IHasEffect, IEnumerable<Technology>, IPersistableList {
        private readonly List<Technology> technologies = new List<Technology>();
        private readonly EffectLocation ownerLocation;
        private uint ownerId;
        private readonly object owner;

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
                return technologies.Count(tech => tech.ownerLocation == ownerLocation);
            }
        }

        public TechnologyManager Parent { get; set; }

        public uint Id {
            get { return ownerId; }
            set { ownerId = value; }
        }

        #region Add/Remove

        private void AddChildCopy(Technology tech) {
            //only add tech if it applies to this tech manager
            if (!tech.Effects.Exists(effect => effect.location == ownerLocation))
                return;

            technologies.Add(tech);

            if (Parent != null)
                Parent.AddChildCopy(tech);
        }

        public bool Add(Technology tech) {
            return Add(tech, true);
        }

        public bool Add(Technology tech, bool notify) {
            CheckUpdateMode();
            
            if (technologies.Exists(technology => technology.Type == tech.Type && technology.ownerLocation == ownerLocation && technology.ownerId == ownerId))
                return false;

            tech.ownerId = ownerId;
            tech.ownerLocation = ownerLocation;
            technologies.Add(tech);

            if (Parent != null)
                Parent.AddChildCopy(tech);

            if (TechnologyAdded != null && notify)
                TechnologyAdded(tech);

            return true;
        }

        public bool Upgrade(Technology tech) {
            CheckUpdateMode();

            if (!Remove(tech.Type, false))
                return false;

            if (!Add(tech, false))
                return false;

            if (TechnologyUpgraded != null)
                TechnologyUpgraded(tech);

            return true;
        }

        public void Clear() {
            CheckUpdateMode();

            for (int i = technologies.Count - 1; i >= 0; i--) {
                Technology tech = technologies[i];
                if (tech.ownerId != ownerId || tech.ownerLocation != ownerLocation)
                    continue;

                technologies.RemoveAt(i);
                if (Parent != null)
                    Parent.RemoveChildCopy(tech, false);
            }
        }

        private bool Remove(uint techType, bool notify) {
            Technology tech = technologies.Find(technology => technology.Type == techType && technology.ownerId == ownerId && technology.ownerLocation == ownerLocation);

            if (tech == null)
                return false;
            
            if (!technologies.Remove(tech))
                return false;
            
            if (Parent != null)
                Parent.RemoveChildCopy(tech, notify);

            if (TechnologyRemoved != null && notify)
                TechnologyRemoved(tech);

            return true;
        }

        private void RemoveChildCopy(Technology tech, bool notify) {
            if (!technologies.Remove(tech))
                return;

            if (TechnologyRemoved != null && notify)
                TechnologyRemoved(tech);
        }

        #endregion

        #region Utilities

        public List<Effect> GetEffects(EffectCode effectCode, EffectInheritance inherit) {
            List<Effect> list = new List<Effect>();
            foreach (Technology tech in technologies)
                list.AddRange(tech.GetEffects(effectCode, inherit, ownerLocation));
            if ((inherit & EffectInheritance.UPWARD) == EffectInheritance.UPWARD) {
                if (Parent != null)
                    list.AddRange(Parent.GetEffects(effectCode, EffectInheritance.UPWARD | EffectInheritance.SELF));
            }
            return list;
        }

        public int Max(EffectCode effectCode, EffectInheritance inherit, byte paramOrder) {
            int max = Int32.MinValue;
            foreach (Effect e in GetEffects(effectCode, inherit)) {
                if ((int) e.value[paramOrder] > max)
                    max = (int) e.value[paramOrder];
            }
            return max;
        }

        public int Min(EffectCode effectCode, EffectInheritance inherit, byte paramOrder) {
            int min = Int32.MaxValue;
            foreach (Effect e in GetEffects(effectCode, inherit)) {
                if ((int) e.value[paramOrder] < min)
                    min = (int) e.value[paramOrder];
            }
            return min;
        }

        public int Avg(int effectId) {
            return 0;
        }

        #endregion

        #region Updates

        private bool updating;

        private void CheckUpdateMode() {
            if (!Global.FireEvents)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            switch (ownerLocation) {
                case EffectLocation.CITY:
                    MultiObjectLock.ThrowExceptionIfNotLocked((City) owner);
                    break;
                case EffectLocation.OBJECT:
                    MultiObjectLock.ThrowExceptionIfNotLocked(((Structure) owner).City);
                    break;
                case EffectLocation.PLAYER:
                    MultiObjectLock.ThrowExceptionIfNotLocked((Player) owner);
                    break;
            }
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

        public void Print() {
            Global.Logger.Info("Printing TechnologyManager Location:" + ownerLocation);
            foreach (Technology tech in technologies)
                tech.Print();
        }

        public bool TryGetTechnology(uint techType, out Technology technology) {
            technology = technologies.Find(tech => tech.Type == techType && tech.ownerId == ownerId && tech.ownerLocation == ownerLocation);

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
                list.AddRange(tech.GetAllEffects(inherit, ownerLocation));
            if ((inherit & EffectInheritance.UPWARD) == EffectInheritance.UPWARD) {
                if (Parent != null)
                    list.AddRange(Parent.GetAllEffects(EffectInheritance.UPWARD | EffectInheritance.SELF));
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
                return new[] {
                                 new DbColumn("city_id", owner is Structure ? (owner as Structure).City.Id : (owner is City ? (owner as City).Id : 0), DbType.UInt32),
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
            get { return new[] {new DbColumn("type", DbType.UInt16), new DbColumn("level", DbType.Byte)}; }
        }

        public bool DbPersisted { get; set; }

        #endregion

        #region IEnumerable<DbColumn[]> Members

        IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
            foreach (Technology tech in technologies) {
                if (tech.ownerLocation != ownerLocation || tech.ownerId != ownerId)
                    continue;

                yield return new[] {new DbColumn("type", tech.Type, DbType.UInt16), new DbColumn("level", tech.Level, DbType.Byte)};
            }
        }

        #endregion
    }
}