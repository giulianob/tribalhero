using Game.Data;
using Game.Data.Troop;

namespace Game.Battle
{
    public interface IBattleReportWriter
    {
        void SnapBattle(out uint battleId, uint cityId);

        void SnapBattleEnd(uint battleId);

        void SnapReport(out uint reportId, uint battleId);

        void SnapEndReport(uint reportId, uint battleId, uint round, uint turn);

        void SnapTroopState(uint reportTroopId, ITroopStub stub, ReportState state);

        uint SnapTroop(uint reportId, ReportState state, uint cityId, byte troopId, uint objectId, bool isAttacker, Resource loot);

        void SnapBattleReportView(uint cityId, byte troopId, uint battleId, uint groupId, bool isAttacker, uint enterBattleReportId);

        void SnapCombatObject(uint troopId, CombatObject co);

        void SnapLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource);

        void SnapBattleReportViewExit(uint battleId, uint groupId, uint reportId);
    }
}