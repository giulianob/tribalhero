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
        public delegate void CombatGroupChanged(CombatGroup combatGroup, CombatObject combatObject);

        public event CombatGroupChanged CombatObjectAdded = delegate { };
        public event CombatGroupChanged CombatObjectRemoved = delegate { };

        public abstract uint Id { get; }

        public abstract byte TroopId { get; }

        public abstract Resource GroupLoot { get; }

        public abstract BattleOwner Owner { get; }

        protected CombatGroup(IDbManager manager)
                : base(manager)
        {
        }

        public new void Add(CombatObject item, bool save)
        {
            base.Add(item, save);
            CombatObjectAdded(this, item);
        }

        public new void RemoveAt(int index)
        {
            var combatObject = this[index];
            base.RemoveAt(index);            
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
