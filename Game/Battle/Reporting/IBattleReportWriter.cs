using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;

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

        void SnapCombatObject(uint troopId, ICombatObject co);

        void SnapLootedResources(BattleOwner owner,
                                 uint groupId,
                                 uint battleId,
                                 Resource lootResource,
                                 Resource bonusResource);

        void SnapBattleAccess(uint battleId, BattleOwner owner, byte troopId, uint groupId, bool isAttacker);

        void SnapTribeToBattle(uint battleId, uint tribeId, bool isAttacker);
    }
}