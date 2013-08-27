using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Persistance;

namespace Game.Battle
{
    public interface ICombatList : IPersistableObjectList<ICombatGroup>
    {
        int Upkeep { get; }

        int UpkeepNotParticipated(uint round);

        bool HasInRange(ICombatObject attacker);

        CombatList.BestTargetResult GetBestTargets(uint battleId, ICombatObject attacker, out List<CombatList.Target> result, int maxCount, uint round);

        IEnumerable<ICombatObject> AllCombatObjects();

        IEnumerable<ICombatObject> AllAliveCombatObjects();
    }
}