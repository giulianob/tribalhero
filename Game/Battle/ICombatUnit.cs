#region

using Game.Data;
using Game.Fighting;

#endregion

namespace Game.Battle {
    public interface ICombatUnit {
        Resource Loot { get; }

        TroopStub TroopStub { get; }

        FormationType Formation { get; }
    }
}