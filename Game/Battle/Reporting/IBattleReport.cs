using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Persistance;

namespace Game.Battle.Reporting
{
    public interface IBattleReport : IPersistable
    {
        IBattleManager Battle { set; }
        bool ReportStarted { get; set; }
        uint ReportId { get; set; }
        ReportedGroups ReportedGroups { get; }
        void CreateBattleReport();
        void CompleteBattle();
        void WriteReportGroup(ICombatGroup group, bool isAttacker, ReportState state);
        void WriteReportGroups(IEnumerable<ICombatGroup> groups, bool isAttacker, ReportState state);
        void CompleteReport(ReportState state);
        void SetLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource);
    }
}