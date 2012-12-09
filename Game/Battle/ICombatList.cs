using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Persistance;

namespace Game.Battle
{
    public interface ICombatList : IPersistableObjectList<ICombatGroup>
    {
        int Upkeep { get; }

        bool HasInRange(ICombatObject attacker);

        CombatList.BestTargetResult GetBestTargets(ICombatObject attacker,
                                                   out IList<CombatList.Target> result,
                                                   int maxCount);

        IEnumerable<ICombatObject> AllCombatObjects();

        IEnumerable<ICombatObject> AllAliveCombatObjects();
    }
}