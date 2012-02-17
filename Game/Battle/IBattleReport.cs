using System.Collections.Generic;
using Game.Data;

namespace Game.Battle
{
    public interface IBattleReport
    {
        IBattleManager Battle { get; set; }
        bool ReportStarted { get; set; }
        bool ReportFlag { get; set; }
        uint ReportId { get; set; }
        ReportedObjects ReportedObjects { get; }
        ReportedTroops ReportedTroops { get; }
        void WriteBeginReport();
        void CreateBattleReport();
        void CompleteBattle();
        void WriteReportObject(CombatObject combatObject, bool isAttacker, ReportState state);
        void WriteReportObjects(List<CombatObject> list, bool isAttacker, ReportState state);
        void CompleteReport(ReportState state);
        void SetLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource);
    }
}