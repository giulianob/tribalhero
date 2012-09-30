using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Tribe;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public abstract class CombatGroup : PersistableObjectList<ICombatObject>, ICombatGroup
    {
        public delegate void CombatGroupChange(ICombatGroup group, ICombatObject combatObject);

        public event CombatGroupChange CombatObjectAdded = delegate { };

        public event CombatGroupChange CombatObjectRemoved = delegate { };

        protected readonly uint BattleId;

        public virtual uint Id { get; private set; }

        public abstract byte TroopId { get; }

        public abstract Resource GroupLoot { get; }

        public abstract BattleOwner Owner { get; }

        public abstract ITribe Tribe { get; }

        [Obsolete("For testing only", true)]
        protected CombatGroup()
        {
            
        }

        protected CombatGroup(uint battleId, uint id, IDbManager manager)
                : base(manager)
        {
            ItemAdded += ObjectAdded;
            ItemRemoved += ObjectRemoved;
            Id = id;
            BattleId = battleId;
        }

        private void ObjectRemoved(PersistableObjectList<ICombatObject> list, ICombatObject combatObject)
        {
            CombatObjectRemoved(this, combatObject);
        }

        private void ObjectAdded(PersistableObjectList<ICombatObject> persistableObjectList, ICombatObject combatObject)
        {
            combatObject.GroupId = Id;
            CombatObjectAdded(this, combatObject);
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
