using Game.Battle.CombatObjects;
using Game.Data;
using Game.Util.Locking;
using Persistance;

namespace Game.Battle.CombatGroups
{
    public interface ICombatGroup: IListOfPersistableObjects<ICombatObject>, IPersistableObject, ILockable
    {
        uint Id { get; }

        byte TroopId { get; }

        Resource GroupLoot { get; }

        BattleOwner Owner { get; }

        bool IsDead();

        bool BelongsTo(IPlayer player);
    }
}