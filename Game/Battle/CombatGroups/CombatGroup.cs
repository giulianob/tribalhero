using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Util.Locking;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public abstract class CombatGroup : ListOfPersistableObjects<CombatObject>, IPersistableObject, ILockable
    {
        protected readonly uint battleId;

        public delegate void CombatGroupChanged(CombatGroup combatGroup, CombatObject combatObject);

        public event CombatGroupChanged CombatObjectAdded = delegate { };
        public event CombatGroupChanged CombatObjectRemoved = delegate { };

        public uint Id { get; protected set; }

        public abstract byte TroopId { get; }

        public abstract Resource GroupLoot { get; }

        public abstract BattleOwner Owner { get; }

        [Obsolete("For testing only", true)]
        protected CombatGroup()
        {
            
        }

        protected CombatGroup(uint battleId, uint id, IDbManager manager)
                : base(manager)
        {
            Id = id;
            this.battleId = battleId;
        }

        public new void Add(CombatObject item, bool save)
        {            
            base.Add(item, save);
            item.GroupId = Id;
            CombatObjectAdded(this, item);
        }

        public new void RemoveAt(int index)
        {            
            var combatObject = this[index];            
            base.RemoveAt(index);            
            combatObject.GroupId = 0;
            CombatObjectRemoved(this, combatObject);            
        }

        public bool IsDead()
        {
            return BackingList.All(combatObject => combatObject.IsDead);
        }

        #region Persistance

        public abstract IEnumerable<DbDependency> DbDependencies { get; }

        public abstract bool DbPersisted { get; set; }

        public abstract string DbTable { get; }

        public abstract DbColumn[] DbPrimaryKey { get; }

        public abstract DbColumn[] DbColumns { get; }

        #endregion

        public abstract int Hash { get; }

        public abstract object Lock { get; }

        public abstract bool BelongsTo(IPlayer player);
    }
}
