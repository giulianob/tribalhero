using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Util.Locking;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public abstract class CombatGroup : PersistableObjectList<CombatObject>, IPersistableObject, ILockable
    {
        protected readonly uint BattleId;

        public uint Id { get; private set; }

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
            ItemRemoved += ObjectRemoved;
            ItemAdded += ObjectAdded;
            Id = id;
            BattleId = battleId;
        }

        private void ObjectAdded(PersistableObjectList<CombatObject> persistableObjectList, CombatObject combatObject)
        {
            combatObject.GroupId = Id;
        }

        private void ObjectRemoved(PersistableObjectList<CombatObject> persistableObjectList, CombatObject combatObject)
        {
            combatObject.GroupId = 0;
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
