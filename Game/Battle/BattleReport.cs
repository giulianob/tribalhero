#region

using System.Collections.Generic;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Battle
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
            get
            {
                return battle;
            }
            set
            {
                battle = value;
                ReportedObjects = new ReportedObjects(battle.City);
                ReportedTroops = new ReportedTroops(battle.City);
            }
        }

        /// <summary>
        /// Indicates whether we've already begun a snapshot or not.
        /// </summary>
        public bool ReportStarted { get; set; }

        /// <summary>
        /// Not sure what this report flag is actually doing at the moment. I think it may be removed and only ReportStarted may be used.
        /// </summary>
        public bool ReportFlag { get; set; }

        /// <summary>
        /// Id of the current report. This will change as each new snapshot is taken.
        /// </summary>
        public uint ReportId { get; set; }

        /// <summary>
        /// Persistable list of the objects that have been reported for this snapshot.
        /// </summary>
        public ReportedObjects ReportedObjects { get; private set; }

        /// <summary>
        /// Persistable list of troops that have been reported for this snapshot.
        /// </summary>
        public ReportedTroops ReportedTroops { get; private set; }

        public BattleReport(IBattleReportWriter battleReportWriter)
        {
            this.battleReportWriter = battleReportWriter;
        }

        /// <summary>
        /// Starts the battle report if it hasn't yet.
        /// This will snap all of the current troops.
        /// </summary>
        public void WriteBeginReport()
        {
            if (ReportStarted)
                return;

            uint newReportId;
            battleReportWriter.SnapReport(out newReportId, Battle.BattleId);

            ReportId = newReportId;
            ReportStarted = true;
        }

        /// <summary>
        /// Starts the battle report. This should only be called once per battle when it starts.
        /// </summary>
        public void CreateBattleReport()
        {
            uint battleId;
            battleReportWriter.SnapBattle(out battleId, Battle.City.Id);
            Battle.BattleId = battleId;
            WriteBeginReport();
            ReportFlag = true;
            CompleteReport(ReportState.Entering);
        }

        /// <summary>
        /// Completes the report. This should only be called once per battle when it ends.
        /// </summary>
        public void CompleteBattle()
        {
            WriteBeginReport();
            ReportFlag = true;
            CompleteReport(ReportState.Exiting);
            battleReportWriter.SnapBattleEnd(Battle.BattleId);
        }

        /// <summary>
        /// Writes the specified combat object (if it hasn't yet) to the report
        /// </summary>
        /// <param name="combatObject"></param>
        /// <param name="isAttacker"></param>
        /// <param name="state"></param>
        public void WriteReportObject(CombatObject combatObject, bool isAttacker, ReportState state)
        {
            uint combatTroopId;

            // Start the report incase it hasn't yet
            WriteBeginReport();

            // Check if we've already snapped this troop
            if (!ReportedTroops.TryGetValue(combatObject.TroopStub, out combatTroopId))
            {
                // Snap the troop
                combatTroopId = battleReportWriter.SnapTroop(ReportId,
                                             state,
                                             combatObject.City.Id,
                                             combatObject.TroopStub.TroopId,
                                             combatObject.GroupId,
                                             isAttacker,
                                             combatObject.Loot);

                // Log any troops that are entering the battle to the view table so they are able to see this report
                // Notice that we don't log the local troop. This is because they can automatically see all of the battles that take place in their cities by using the battles table
                if (Battle.City != combatObject.City && (state == ReportState.Entering || state == ReportState.Reinforced))
                {
                    battleReportWriter.SnapBattleReportView(combatObject.City.Id, combatObject.TroopStub.TroopId, Battle.BattleId, combatObject.GroupId, isAttacker);
                }

                ReportedTroops[combatObject.TroopStub] = combatTroopId;
            }
            // If object has already been snapped, then we just update the state if it's not Staying (Staying is the default state basically.. anything else overrides it)
            else if (state != ReportState.Staying)
            {
                battleReportWriter.SnapTroopState(combatTroopId, combatObject.TroopStub, state);
            }

            // Check if we've already snapped the combat object
            if (!ReportedObjects.Contains(combatObject))
            {
                // Snap the combat objects
                battleReportWriter.SnapCombatObject(combatTroopId, combatObject);
                ReportedObjects.Add(combatObject);
            }
        }

        /// <summary>
        /// Writes a list of combat objects
        /// </summary>
        /// <param name="list"></param>
        /// <param name="isAttacker"></param>
        /// <param name="state"></param>
        public void WriteReportObjects(List<CombatObject> list, bool isAttacker, ReportState state)
        {
            WriteBeginReport();

            ReportFlag = true;

            foreach (var co in list)
            {
                WriteReportObject(co, isAttacker, state);
            }
        }

        /// <summary>
        /// Completes the current snapshot if it has been started.
        /// </summary>
        /// <param name="state"></param>
        public void CompleteReport(ReportState state)
        {
            if (!ReportStarted || !ReportFlag)
            {
                return;
            }

            WriteReportObjects(Battle.Attacker, true, state);
            WriteReportObjects(Battle.Defender, false, state);
            battleReportWriter.SnapEndReport(ReportId, Battle.BattleId, Battle.Round, Battle.Turn);
            ReportedObjects.Clear();
            ReportedTroops.Clear();
            ReportStarted = false;
            ReportFlag = false;
        }

        public void SetLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource)
        {
            battleReportWriter.SnapLootedResources(cityId, troopId, battleId, lootResource, bonusResource);
        }
    }
}