#region

using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;

#endregion

namespace Game.Battle {
    public enum ReportState {
        ENTERING = 0,
        STAYING = 1,
        EXITING = 2,
        DYING = 3,
        RETREATING = 4,
        REINFORCED = 5
    }

    public class BattleReport {
        private const string BATTLE_DB = "battles";
        private const string BATTLE_REPORTS_DB = "battle_reports";
        private const string BATTLE_REPORT_TROOPS_DB = "battle_report_troops";
        private const string BATTLE_REPORT_OBJECTS_DB = "battle_report_objects";
        private const string BATTLE_REPORT_VIEWS_DB = "battle_report_views";

        private readonly BattleManager battle;

        private bool reportStarted;

        public bool ReportStarted {
            get { return reportStarted; }
            set { reportStarted = value; }
        }

        private bool reportFlag;

        public bool ReportFlag {
            get { return reportFlag; }
            set { reportFlag = value; }
        }

        private uint reportId;

        public uint ReportId {
            get { return reportId; }
            set { reportId = value; }
        }

        public ReportedObjects ReportedObjects { get; private set; }

        public ReportedTroops ReportedTroops { get; private set; }

        public BattleReport(BattleManager bm) {
            ReportedObjects = new ReportedObjects(bm);
            ReportedTroops = new ReportedTroops(bm);
            battle = bm;
        }

        public void WriteBeginReport() {
            if (reportStarted)
                return;

            SnapReport(out reportId, battle.BattleId);
            reportStarted = true;
        }

        public void CreateBattleReport() {
            uint battleId;
            SnapBattle(out battleId, battle.City.Id);
            battle.BattleId = battleId;
            WriteBeginReport();
            reportFlag = true;
            WriteReport(ReportState.ENTERING);
        }

        public void CompleteBattle() {
            SnapBattleEnd(battle.BattleId);
        }

        public void WriteReportObject(CombatObject co, bool isAttacker, ReportState state) {
            uint combatTroopId;

            WriteBeginReport();
            if (co.ClassType == BattleClass.UNIT) {
                ICombatUnit cu = co as ICombatUnit;
                if (!ReportedTroops.TryGetValue(cu.TroopStub, out combatTroopId)) {
                    SnapTroop(state, cu.TroopStub.City.Id, cu.TroopStub.TroopId, co.GroupId, isAttacker, out combatTroopId, cu.Loot);
                    ReportedTroops[cu.TroopStub] = combatTroopId;
                } else if (state != ReportState.STAYING)
                    SnapTroopState(cu.TroopStub, state);

                if (!ReportedObjects.Contains(co)) {
                    SnapCombatObject(combatTroopId, co);
                    ReportedObjects.Add(co);
                }
            } else {
                TroopStub stub = ((CombatStructure) co).Structure.City.DefaultTroop;
                if (!ReportedTroops.TryGetValue(stub, out combatTroopId)) {
                    SnapTroop(state, co.City.Id, 1, co.GroupId, isAttacker, out combatTroopId, new Resource());
                    ReportedTroops[stub] = combatTroopId;
                } else if (state != ReportState.STAYING)
                    SnapTroopState(stub, state);

                if (!ReportedObjects.Contains(co)) {
                    SnapCombatObject(combatTroopId, co);
                    ReportedObjects.Add(co);
                }
            }
        }

        public void WriteReportObjects(List<CombatObject> list, bool isAttacker, ReportState state) {
            List<TroopStub> updatedObj = new List<TroopStub>();

            WriteBeginReport();

            reportFlag = true;

            foreach (CombatObject co in list) {
                bool snapObj = ReportedObjects.Contains(co);

                uint combatTroopId;
                if (co.ClassType == BattleClass.UNIT) {
                    ICombatUnit cu = co as ICombatUnit;

                    if (!ReportedTroops.TryGetValue(cu.TroopStub, out combatTroopId)) {
                        SnapTroop(state, cu.TroopStub.City.Id, cu.TroopStub.TroopId, co.GroupId, isAttacker, out combatTroopId, cu.Loot);
                        ReportedTroops[cu.TroopStub] = combatTroopId;
                    } else if ((state == ReportState.REINFORCED || state == ReportState.EXITING) && !updatedObj.Contains(cu.TroopStub)) {
                        //Exiting state should override anything else
                        SnapTroopState(cu.TroopStub, state);
                        updatedObj.Add(cu.TroopStub);
                    }
                } else {
                    TroopStub stub = ((CombatStructure) co).Structure.City.DefaultTroop;
                    if (!ReportedTroops.TryGetValue(stub, out combatTroopId)) {
                        SnapTroop(state, co.City.Id, 1, co.GroupId, isAttacker, out combatTroopId, new Resource());
                        ReportedTroops[stub] = combatTroopId;
                    } else if (state == ReportState.EXITING && !updatedObj.Contains(stub)) {
                        SnapTroopState(stub, state);
                        updatedObj.Add(stub);
                    }
                }

                if (snapObj)
                    continue;

                SnapCombatObject(combatTroopId, co);
                ReportedObjects.Add(co);
            }
        }

        public void CompleteReport(ReportState state) {
            if (!reportStarted)
                return;

            reportFlag = true;
            WriteReport(state);
        }

        public void WriteReport(ReportState state) {
            if (!reportStarted || !reportFlag)
                return;

            WriteReportObjects(battle.Attacker, true, state);
            WriteReportObjects(battle.Defender, false, state);
            SnapEndReport(reportId, battle.BattleId, battle.Round, battle.Turn);
            ReportedObjects.Clear();
            ReportedTroops.Clear();
            reportStarted = false;
            reportFlag = false;
        }

        internal static void SnapBattle(out uint battleId, uint cityId) {
            Global.DbManager.Query(string.Format("INSERT INTO `{0}` VALUES ('', '{1}', NOW(), NULL)", BATTLE_DB, cityId));
            battleId = Global.DbManager.LastInsertId();
        }

        internal static void SnapBattleEnd(uint battleId) {
            Global.DbManager.Query(string.Format("UPDATE `{0}` SET `ended` = NOW() WHERE `id` = '{1}' LIMIT 1", BATTLE_DB, battleId));
        }

        internal static void SnapReport(out uint reportId, uint battleId) {
            Global.DbManager.Query(string.Format("INSERT INTO {0} VALUES ('', NOW(), '{1}', '0', '0', '0')", BATTLE_REPORTS_DB, battleId));
            reportId = Global.DbManager.LastInsertId();
        }

        internal static void SnapEndReport(uint reportId, uint battleId, uint round, uint turn) {
            Global.DbManager.Query(string.Format("UPDATE {0} SET `ready` = 1, `round` = '{2}', turn = '{3}' WHERE id = '{1}' LIMIT 1", BATTLE_REPORTS_DB, reportId, round, turn));
        }

        internal void SnapTroopState(TroopStub stub, ReportState state) {
            uint id = ReportedTroops[stub];
            Global.DbManager.Query(string.Format("UPDATE {0} SET `state` = '{1}' WHERE `id` = '{2}' LIMIT 1", BATTLE_REPORT_TROOPS_DB, (byte) state, id));
        }

        internal void SnapTroop(ReportState state, uint cityId, byte troopId, uint objectId, bool isAttacker, out uint battleTroopId, Resource loot) {
            Global.DbManager.Query(string.Format("INSERT INTO {0} VALUES ('', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}')", BATTLE_REPORT_TROOPS_DB, reportId, cityId,
                                                 objectId, troopId, (byte) state, isAttacker ? 1 : 0, loot.Gold, loot.Crop, loot.Iron, loot.Wood));

            battleTroopId = Global.DbManager.LastInsertId();

            // Log any troops that are entering the battle to the view table so they are able to see this report
            // Notice that we don't log the local troop. This is because they can automatically see all of the battles that take place in their cities by using the battles table
            if (battle.City.Id != cityId && (state == ReportState.ENTERING || state == ReportState.REINFORCED)) {
                Global.DbManager.Query(string.Format("INSERT INTO {0} VALUES ('', '{1}', '{2}', '{3}', '{4}', '{5}', 0, NOW())", BATTLE_REPORT_VIEWS_DB, cityId, troopId, battle.BattleId, battleTroopId,
                                                     isAttacker ? 1 : 0));
            }
        }

        internal void SnapCombatObject(uint troopId, CombatObject co) {
            ICombatUnit unit = co as ICombatUnit;

            Global.DbManager.Query(string.Format("INSERT INTO {0} VALUES ('', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", BATTLE_REPORT_OBJECTS_DB, troopId, co.Type, co.Lvl, co.Hp,
                                                 co.Count, co.DmgRecv, co.DmgDealt, (byte) (unit == null ? FormationType.STRUCTURE : unit.Formation)));
        }
    }
}