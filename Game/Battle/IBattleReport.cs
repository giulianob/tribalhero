using System.Collections.Generic;
using Game.Data;
using Persistance;

namespace Game.Battle
{
    public interface IBattleReport : IPersistable
    {
        IBattleManager Battle { set; }
        bool ReportStarted { get; set; }
        bool ReportFlag { get; set; }
        uint ReportId { get; set; }
        ReportedObjects ReportedObjects { get; }
        ReportedTroops ReportedTroops { get; }
        void CreateBattleReport();
        void CompleteBattle();
        void WriteReportObject(CombatObject combatObject, bool isAttacker, ReportState state);
        void WriteReportObjects(IEnumerable<CombatObject> list, bool isAttacker, ReportState state);
        void CompleteReport(ReportState state);
        void SetLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource);
    }
}