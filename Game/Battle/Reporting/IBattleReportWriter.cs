using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Troop;

namespace Game.Battle.Reporting
{
    public interface IBattleReportWriter
    {
        void SnapBattle(uint battleId, BattleOwner owner, BattleLocation location);

        void SnapBattleEnd(uint battleId);

        void SnapReport(out uint reportId, uint battleId);

        void SnapEndReport(uint reportId, uint battleId, uint round, uint turn);

        void SnapGroupState(uint reportTroopId, ICombatGroup group, ReportState state);

        uint SnapGroup(uint reportId, ICombatGroup group, ReportState state, bool isAttacker);

        void SnapBattleReportView(uint cityId, byte troopId, uint battleId, uint groupId, bool isAttacker, uint enterBattleReportId);

        void SnapCombatObject(uint troopId, ICombatObject co);

        void SnapLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource);

        void SnapBattleReportViewExit(uint battleId, uint groupId, uint reportId);
    }
}