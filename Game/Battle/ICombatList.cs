using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Persistance;

namespace Game.Battle
{
    public interface ICombatList : IListOfPersistableObjects<CombatGroup>
    {
        int Upkeep { get; }

        bool HasInRange(CombatObject attacker);

        CombatList.BestTargetResult GetBestTargets(CombatObject attacker, out IList<CombatList.Target> result, int maxCount);

        IEnumerable<CombatObject> AllCombatObjects();
    }
}