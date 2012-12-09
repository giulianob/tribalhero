using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;

namespace Game.Battle.Reporting
{
    class NullBattleReportWriter : IBattleReportWriter
    {
        public void SnapBattle(uint battleId, BattleOwner owner, BattleLocation location)
        {
        }

        public void SnapBattleEnd(uint battleId)
        {
        }

        public void SnapReport(out uint reportId, uint battleId)
        {
            reportId = 0;
        }

        public void SnapEndReport(uint reportId, uint battleId, uint round, uint turn)
        {
        }

        public void SnapGroupState(uint reportTroopId, ICombatGroup @group, ReportState state)
        {
        }

        public uint SnapGroup(uint reportId, ICombatGroup @group, ReportState state, bool isAttacker)
        {
            return 0;
        }

        public void SnapCombatObject(uint troopId, ICombatObject co)
        {
        }

        public void SnapLootedResources(BattleOwner owner,
                                        uint groupId,
                                        uint battleId,
                                        Resource lootResource,
                                        Resource bonusResource)
        {
        }

        public void SnapBattleAccess(uint battleId, BattleOwner owner, byte troopId, uint groupId, bool isAttacker)
        {
        }

        public void SnapTribeToBattle(uint battleId, uint tribeId, bool isAttacker)
        {
        }

        public void SnapBattleReportCityView(uint cityId,
                                             byte troopId,
                                             uint battleId,
                                             uint groupId,
                                             bool isAttacker,
                                             uint enterBattleReportId)
        {
        }
    }
}