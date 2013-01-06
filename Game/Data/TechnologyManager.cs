#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Database;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Data
{
    public class TechnologyManager : ITechnologyManager
    {
        public const string DB_TABLE = "technologies";

        private readonly List<Technology> technologies = new List<Technology>();

        public TechnologyManager(EffectLocation location, object owner, uint ownerId)
        {
            OwnerLocation = location;
            Owner = owner;
            OwnerId = ownerId;
        }

        public EffectLocation OwnerLocation { get; private set; }

        public uint OwnerId { get; private set; }

        public object Owner { get; private set; }

        public int TechnologyCount
        {
            get
            {
                return technologies.Count;
            }
        }

        public int OwnedTechnologyCount
        {
            get
            {
                return technologies.Count(tech => tech.OwnerLocation == OwnerLocation);
            }
        }

        public ITechnologyManager Parent { get; set; }

        public uint Id
        {
            get
            {
                return OwnerId;
            }
            set
            {
                OwnerId = value;
            }
        }

        public void Print()
        {
            Global.Logger.Info("Printing TechnologyManager Location:" + OwnerLocation);
            foreach (var tech in technologies)
            {
                tech.Print();
            }
        }

        public bool TryGetTechnology(uint techType, out Technology technology)
        {
            technology =
                    technologies.Find(
                                      tech =>
                                      tech.Type == techType && tech.OwnerId == OwnerId &&
                                      tech.OwnerLocation == OwnerLocation);

            return technology != null;
        }

        #region Add/Remove

        public void AddChildCopy(Technology tech)
        {
            //only add tech if it applies to this tech manager
            if (!tech.Effects.Exists(effect => effect.Location == OwnerLocation))
            {
                return;
            }

            technologies.Add(tech);

            if (Parent != null)
            {
                Parent.AddChildCopy(tech);
            }
        }

        public bool Add(Technology tech)
        {
            return Add(tech, true);
        }

        public bool Add(Technology tech, bool notify)
        {
            CheckUpdateMode();

            if (
                    technologies.Exists(
                                        technology =>
                                        technology.Type == tech.Type && technology.OwnerLocation == OwnerLocation &&
                                        technology.OwnerId == OwnerId))
            {
                return false;
            }

            tech.OwnerId = OwnerId;
            tech.OwnerLocation = OwnerLocation;
            technologies.Add(tech);

            if (Parent != null)
            {
                Parent.AddChildCopy(tech);
            }

            if (TechnologyAdded != null && notify)
            {
                TechnologyAdded(tech);
            }

            return true;
        }

        public bool Upgrade(Technology tech)
        {
            CheckUpdateMode();

            if (!Remove(tech.Type, false))
            {
                return false;
            }

            if (!Add(tech, false))
            {
                return false;
            }

            if (TechnologyUpgraded != null)
            {
                TechnologyUpgraded(tech);
            }

            return true;
        }

        public void Clear()
        {
            CheckUpdateMode();

            for (int i = technologies.Count - 1; i >= 0; i--)
            {
                Technology tech = technologies[i];
                if (tech.OwnerId != OwnerId || tech.OwnerLocation != OwnerLocation)
                {
                    continue;
                }

                technologies.RemoveAt(i);
                if (Parent != null)
                {
                    Parent.RemoveChildCopy(tech, false);
                }
            }

            if (TechnologyCleared != null)
            {
                TechnologyCleared(this);
            }
        }

        public void RemoveChildCopy(Technology tech, bool notify)
        {
            if (!technologies.Remove(tech))
            {
                return;
            }

            if (TechnologyRemoved != null && notify)
            {
                TechnologyRemoved(tech);
            }
        }

        private bool Remove(uint techType, bool notify)
        {
            Technology tech =
                    technologies.Find(
                                      technology =>
                                      technology.Type == techType && technology.OwnerId == OwnerId &&
                                      technology.OwnerLocation == OwnerLocation);

            if (tech == null)
            {
                return false;
            }

            if (!technologies.Remove(tech))
            {
                return false;
            }

            if (Parent != null)
            {
                Parent.RemoveChildCopy(tech, notify);
            }

            if (TechnologyRemoved != null && notify)
            {
                TechnologyRemoved(tech);
            }

            return true;
        }

        #endregion

        #region Utilities

        public List<Effect> GetEffects(EffectCode effectCode, EffectInheritance inherit = EffectInheritance.All)
        {
            var list = new List<Effect>();
            foreach (var tech in technologies)
            {
                list.AddRange(tech.GetEffects(effectCode, inherit, OwnerLocation));
            }
            if ((inherit & EffectInheritance.Upward) == EffectInheritance.Upward)
            {
                if (Parent != null)
                {
                    list.AddRange(Parent.GetEffects(effectCode, EffectInheritance.Upward | EffectInheritance.Self));
                }
            }
            return list;
        }

        public int Max(EffectCode effectCode, EffectInheritance inherit, byte paramOrder)
        {
            int max = Int32.MinValue;
            foreach (var e in GetEffects(effectCode, inherit))
            {
                if ((int)e.Value[paramOrder] > max)
                {
                    max = (int)e.Value[paramOrder];
                }
            }
            return max;
        }

        public int Min(EffectCode effectCode, EffectInheritance inherit, byte paramOrder)
        {
            int min = Int32.MaxValue;
            foreach (var e in GetEffects(effectCode, inherit))
            {
                if ((int)e.Value[paramOrder] < min)
                {
                    min = (int)e.Value[paramOrder];
                }
            }
            return min;
        }

        public int Avg(int effectId)
        {
            return 0;
        }

        #endregion

        #region Updates

        private bool updating;

        public void BeginUpdate()
        {
            if (updating)
            {
                throw new Exception("Nesting beginupdate");
            }

            updating = true;
        }

        public void EndUpdate()
        {
            if (!updating)
            {
                throw new Exception("Called EndUpdate without first calling BeginUpdate");
            }

            DbPersistance.Current.Save(this);
            updating = false;
        }

        private void CheckUpdateMode()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            if (!updating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            switch(OwnerLocation)
            {
                case EffectLocation.City:
                    DefaultMultiObjectLock.ThrowExceptionIfNotLocked((ICity)Owner);
                    break;
                case EffectLocation.Object:
                    DefaultMultiObjectLock.ThrowExceptionIfNotLocked(((IStructure)Owner).City);
                    break;
                case EffectLocation.Player:
                    DefaultMultiObjectLock.ThrowExceptionIfNotLocked((IPlayer)Owner);
                    break;
            }
        }

        #endregion

        #region Events

        #region Delegates

        public delegate void TechnologyClearedCallback(ITechnologyManager manager);

        public delegate void TechnologyUpdatedCallback(Technology tech);

        #endregion

        public event TechnologyUpdatedCallback TechnologyAdded;

        public event TechnologyUpdatedCallback TechnologyRemoved;

        public event TechnologyUpdatedCallback TechnologyUpgraded;

        public event TechnologyClearedCallback TechnologyCleared;

        #endregion

        #region IEnumerable<Technology> Members

        public IEnumerator<Technology> GetEnumerator()
        {
            return technologies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return technologies.GetEnumerator();
        }

        #endregion

        #region IHasEffect Members

        public IEnumerable<Effect> GetAllEffects(EffectInheritance inherit = EffectInheritance.All)
        {
            foreach (var effect in technologies.SelectMany(tech => tech.GetAllEffects(inherit, OwnerLocation)))
            {
                yield return effect;
            }

            if ((inherit & EffectInheritance.Upward) == EffectInheritance.Upward)
            {
                if (Parent != null)
                {
                    foreach (var effect in Parent.GetAllEffects(EffectInheritance.Upward | EffectInheritance.Self))
                    {
                        yield return effect;
                    }
                }
            }
        }

        #endregion

        #region IPersistableList Members

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
                return new[]
                {
                        new DbColumn("city_id",
                                     Owner is IStructure
                                             ? (Owner as IStructure).City.Id
                                             : (Owner is ICity ? (Owner as ICity).Id : 0),
                                     DbType.UInt32),
                        new DbColumn("owner_id", OwnerId, DbType.UInt32),
                        new DbColumn("owner_location", (byte)OwnerLocation, DbType.Byte)
                };
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
                return new DbColumn[] {};
            }
        }

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[] {new DbColumn("type", DbType.UInt16), new DbColumn("level", DbType.Byte)};
            }
        }

        public bool DbPersisted { get; set; }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            return from tech in technologies
                   where tech.OwnerLocation == OwnerLocation && tech.OwnerId == OwnerId
                   select
                           new[]
                           {
                                   new DbColumn("type", tech.Type, DbType.UInt16),
                                   new DbColumn("level", tech.Level, DbType.Byte)
                           };
        }

        #endregion
    }
}