using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Tribe;
using Persistance;

namespace Game.Battle.Reporting
{
    public interface IBattleReport : IPersistable
    {
        IBattleManager Battle { set; }

        bool ReportStarted { get; set; }

        bool SnappedImportantEvent { get; set; }

        uint ReportId { get; set; }

        ReportedGroups ReportedGroups { get; }

        void CreateBattleReport();

        void CompleteBattle();

        void WriteReportGroup(ICombatGroup group, bool isAttacker, ReportState state);

        void WriteExitingObject(ICombatGroup group, bool isAttacker, ICombatObject combatObject);

        void CompleteReport(ReportState state);

        void SetLootedResources(BattleOwner owner,
                                uint groupId,
                                uint battleId,
                                Resource lootResource,
                                Resource bonusResource);

        void AddAccess(BattleOwner owner, BattleManager.BattleSide battleSide);

        void AddAccess(ICombatGroup group, BattleManager.BattleSide battleSide);

        void AddTribeToBattle(ITribe tribe, BattleManager.BattleSide side);
    }
}