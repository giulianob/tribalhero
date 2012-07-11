#region

using Game.Data;
using Game.Data.Troop;

#endregion

namespace Game.Battle.CombatObjects
{
    public interface ICombatUnit
    {
        Resource Loot { get; }

        ITroopStub TroopStub { get; }

        FormationType Formation { get; }

        bool IsAttacker { get; }
    }
}