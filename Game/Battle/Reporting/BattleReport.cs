#region

using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Util;
using Persistance;

#endregion

namespace Game.Battle.Reporting
{
    public class BattleReport : IBattleReport
    {
        private readonly IBattleReportWriter battleReportWriter;
        public static readonly LargeIdGenerator BattleIdGenerator = new LargeIdGenerator(uint.MaxValue);
        public static readonly LargeIdGenerator ReportIdGenerator = new LargeIdGenerator(uint.MaxValue);
        public static readonly LargeIdGenerator BattleTroopIdGenerator = new LargeIdGenerator(uint.MaxValue);        

        private IBattleManager battle;

        /// <summary>
        /// The battle manager that this report belongs to
        /// </summary>
        public IBattleManager Battle
        {
            set
            {
                battle = value;                
                ReportedGroups = new ReportedGroups(battle.BattleId);
            }
        }

        /// <summary>
        /// Indicates whether we've already begun a snapshot or not.
        /// </summary>
        public bool ReportStarted { get; set; }

        /// <summary>
        /// Id of the current report. This will change as each new snapshot is taken.
        /// </summary>
        public uint ReportId { get; set; }

        /// <summary>
        /// Holds whether the current report has snapped an event other than Staying.
        /// </summary>
        public bool SnappedImportantEvent { get; set; }

        /// <summary>
        /// Persistable list of troops that have been reported for this snapshot.
        /// </summary>
        public ReportedGroups ReportedGroups { get; private set; }

        public BattleReport(IBattleReportWriter battleReportWriter)
        {
            this.battleReportWriter = battleReportWriter;
        }

        /// <summary>
        /// Starts the battle report if it hasn't yet.
        /// This will snap all of the current troops.
        /// </summary>
        private void WriteBeginReport()
        {
            if (ReportStarted)
                return;

            uint newReportId;
            battleReportWriter.SnapReport(out newReportId, battle.BattleId);

            ReportId = newReportId;
            ReportStarted = true;
        }

        /// <summary>
        /// Starts the battle report. This should only be called once per battle when it starts.
        /// </summary>
        public void CreateBattleReport()
        {
            battleReportWriter.SnapBattle(battle.BattleId, battle.Owner, battle.Location);
            WriteBeginReport();
            CompleteReport(ReportState.Entering);
        }

        /// <summary>
        /// Completes the report. This should only be called once per battle when it ends.
        /// </summary>
        public void CompleteBattle()
        {
            WriteBeginReport();
            CompleteReport(ReportState.Exiting);
            battleReportWriter.SnapBattleEnd(battle.BattleId);
        }

        /// <summary>
        /// Writes an object that is leaving the battle without reporting on the other objects in the group
        /// </summary>
        public void WriteExitingObject(ICombatGroup group, bool isAttacker, ICombatObject combatObject)
        {
            WriteBeginReport();

            uint reportedGroupId;
            
            bool alreadySnapped = ReportedGroups.TryGetValue(group, out reportedGroupId);
            
            if (!alreadySnapped)
            {
                reportedGroupId = battleReportWriter.SnapGroup(ReportId, group, ReportState.Staying, isAttacker);
                ReportedGroups[group] = reportedGroupId;
            }

            battleReportWriter.SnapCombatObject(reportedGroupId, combatObject);
        }

        /// <summary>
        /// Writes a report group
        /// </summary>
        public void WriteReportGroup(ICombatGroup group, bool isAttacker, ReportState state)
        {
            WriteBeginReport();

            if (state != ReportState.Staying)
            {
                SnappedImportantEvent = true;
            }

            // Holds the id of the group for the current battle report
            uint reportedGroupId;
            
            bool alreadySnapped = ReportedGroups.TryGetValue(group, out reportedGroupId);
            
            if (!alreadySnapped)
            {
                // Snap the group
                reportedGroupId = battleReportWriter.SnapGroup(ReportId,
                                                                group,
                                                                state,                                                                
                                                                isAttacker);

                ReportedGroups[group] = reportedGroupId;
            }                
            else if (state != ReportState.Staying)
            {
                battleReportWriter.SnapGroupState(reportedGroupId, group, state);
            }

            // Snap each object in the group
            foreach (var combatObject in group)
            {                
                battleReportWriter.SnapCombatObject(reportedGroupId, combatObject);
            }

            // Log any troops that are entering the battle to the view table so they are able to see this report
            // TODO: Should not really depend on an interface to decide this behavior and should not expect local troop to be group with id 1. Could do it by adding a method to 
            // the group to return whether it should log a report view and use the owner type/id pattern to log the owner.
            // This approach will have to change for stronghold since each tribe member will need to be able to see the report. We'll need to use
            // the owner type/id as specified above.
            if (group.Id != 1 && group is IReportView)
            {
                switch(state)
                {
                    // When entering, we log the initial report id.
                    case ReportState.Entering:
                        if (!alreadySnapped)
                        {
                            battleReportWriter.SnapBattleReportCityView(((IReportView)group).City.Id,
                                                                    group.TroopId,
                                                                    battle.BattleId,
                                                                    group.Id,
                                                                    isAttacker,
                                                                    ReportId);
                        }
                        break;
                    // When exiting, we log the end report id
                    case ReportState.Exiting:
                    case ReportState.Dying:
                    case ReportState.OutOfStamina:
                    case ReportState.Retreating:
                        battleReportWriter.SnapBattleReportCityViewExit(battle.BattleId, group.Id, ReportId);
                        break;
                }
            }
        }

        /// <summary>
        /// Writes all of the specified groups
        /// </summary>
        public void WriteReportGroups(IEnumerable<ICombatGroup> groups, bool isAttacker, ReportState state)
        {
            foreach (var group in groups)
            {
                WriteReportGroup(group, isAttacker, state);
            }
        }

        /// <summary>
        /// Completes the current snapshot if it has been started.
        /// </summary>
        /// <param name="state"></param>
        public void CompleteReport(ReportState state)
        {
            if (!ReportStarted || (state == ReportState.Staying && !SnappedImportantEvent))
            {
                return;
            }

            WriteReportGroups(battle.Attackers, true, state);
            WriteReportGroups(battle.Defenders, false, state);
            battleReportWriter.SnapEndReport(ReportId, battle.BattleId, battle.Round, battle.Turn);
            ReportedGroups.Clear();
            ReportStarted = false;
            SnappedImportantEvent = false;
        }

        public void SetLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource)
        {
            battleReportWriter.SnapLootedResources(cityId, troopId, battleId, lootResource, bonusResource);
        }

        public void AddAccess(BattleOwner owner)
        {
            battleReportWriter.SnapBattleAccess(battle.BattleId, owner);
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new[] {new DbDependency("ReportedGroups", true, true)};
            }        
        }
    }
}