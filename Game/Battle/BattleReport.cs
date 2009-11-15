using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Game.Data;
using Game.Fighting;
using Game.Util;
using Game.Database;

namespace Game.Battle {
    public enum ReportState {
        Entering = 0,
        Staying = 1,
        Exiting = 2,
        Dying = 3,
        Retreating = 4,
        Reinforced = 5
    }

    public class BattleReport {
        static readonly string BATTLE_DB = "battles";
        static readonly string BATTLE_REPORTS_DB = "battle_reports";
        static readonly string BATTLE_REPORT_TROOPS_DB = "battle_report_troops";
        static readonly string BATTLE_REPORT_OBJECTS_DB = "battle_report_objects";

        public static StreamWriter battleLog;
        public static StreamWriter reportLog;
        public static StreamWriter troopLog;
        public static StreamWriter objectLog;

        BattleManager battle;

        bool reportStarted = false;
        public bool ReportStarted {
            get { return reportStarted; }
            set { reportStarted = value; }
        }

        bool reportFlag = false;
        public bool ReportFlag {
            get { return reportFlag; }
            set { reportFlag = value; }
        }

        uint reportId = 0;
        public uint ReportId {
            get { return reportId; }
            set { reportId = value; }
        }

        ReportedObjects reportedObjects;
        public ReportedObjects ReportedObjects {
            get { return reportedObjects; }
        }

        ReportedTroops reportedTroops;
        public ReportedTroops ReportedTroops {
            get { return reportedTroops; }
        }

        uint lastReportedRound = 0;
        uint lastReportedTurn = 0;          
        
        public BattleReport(BattleManager bm) {
            reportedObjects = new ReportedObjects(bm);
            reportedTroops = new ReportedTroops(bm);
            battle = bm;
        }

        public void WriteBeginReport() {
            if (!reportStarted) {
                snapReport(out reportId, battle.BattleId);
                reportStarted = true;
            }
        }

        public void CreateBattleReport() {
            uint battleId;
            snapBattle(out battleId, battle.City.CityId);
            battle.BattleId = battleId;
            WriteBeginReport();
            reportFlag = true;
            WriteReport(ReportState.Entering);
        }

        public void WriteReportObject(CombatObject co, bool isAttacker, ReportState state) {
            uint combatTroopId;

            WriteBeginReport();
            if (co.ClassType == BattleClass.Unit) {
                ICombatUnit cu = co as ICombatUnit;
                if (!reportedTroops.TryGetValue(cu.TroopStub, out combatTroopId)) {
                    snapTroop(reportId, state, cu.TroopStub.City.CityId, cu.TroopStub.TroopId, co.GroupId, isAttacker, out combatTroopId, cu.Loot);
                    reportedTroops[cu.TroopStub] = combatTroopId;
                }
                else if (state != ReportState.Staying) {
                    snapTroopState(cu.TroopStub, state);
                }

                if (!reportedObjects.Contains(co)) {
                    BattleReport.snapCombatObject(combatTroopId, co);
                    reportedObjects.Add(co);
                }
            }
            else {
                TroopStub stub = (co as CombatStructure).Structure.City.DefaultTroop;
                if (!reportedTroops.TryGetValue(stub, out combatTroopId)) {
                    snapTroop(reportId, state, co.City.CityId, 1, co.GroupId, isAttacker, out combatTroopId, new Resource());
                    reportedTroops[stub] = combatTroopId;
                }
                else if (state != ReportState.Staying) {
                    snapTroopState(stub, state);
                }

                if (!reportedObjects.Contains(co)) {
                    BattleReport.snapCombatObject(combatTroopId, co);
                    reportedObjects.Add(co);
                }
            }
        }

        public void WriteReportObjects(List<CombatObject> list, bool isAttacker, ReportState state) {
            uint combatTroopId;
            List<TroopStub> updatedObj = new List<TroopStub>();

            WriteBeginReport();

            reportFlag = true;

            foreach (CombatObject co in list) {

                bool snapObj = reportedObjects.Contains(co);

                if (co.ClassType == BattleClass.Unit) {
                    ICombatUnit cu = co as ICombatUnit;

                    if (!reportedTroops.TryGetValue(cu.TroopStub, out combatTroopId)) {
                        snapTroop(reportId, state, cu.TroopStub.City.CityId, cu.TroopStub.TroopId, co.GroupId, isAttacker, out combatTroopId, cu.Loot);
                        reportedTroops[cu.TroopStub] = combatTroopId;
                    }
                    else if ((state == ReportState.Reinforced || state == ReportState.Exiting) && !updatedObj.Contains(cu.TroopStub)) { //Exiting state should override anything else
                        snapTroopState(cu.TroopStub, state);
                        updatedObj.Add(cu.TroopStub);
                    }                         
                }
                else {
                    TroopStub stub = (co as CombatStructure).Structure.City.DefaultTroop;
                    if (!reportedTroops.TryGetValue(stub, out combatTroopId)) {
                        snapTroop(reportId, state, co.City.CityId, 1, co.GroupId, isAttacker, out combatTroopId, new Resource());
                        reportedTroops[stub] = combatTroopId;
                    }                        
                    else if (state == ReportState.Exiting && !updatedObj.Contains(stub)) {
                        snapTroopState(stub, state);
                        updatedObj.Add(stub);
                    }                         
                }

                if (!snapObj) {
                    BattleReport.snapCombatObject(combatTroopId, co);
                    reportedObjects.Add(co);
                }
            }
        }

        public void CompleteReport(ReportState state) {
            if (reportStarted) {
                reportFlag = true;
                WriteReport(state);
            }
        }

        public void WriteReport(ReportState state) {
            if (reportStarted && reportFlag) {
                WriteReportObjects(battle.Attacker, true, state);
                WriteReportObjects(battle.Defender, false, state);
                BattleReport.snapEndReport(reportId, battle.BattleId, battle.Round, battle.Turn);
                reportedObjects.Clear();                
                reportedTroops.Clear();
                reportStarted = false;
                reportFlag = false;
                lastReportedTurn = battle.Turn;
                lastReportedRound = battle.Round;
            }
        }

        public static void WriterInit() {
            File.Delete("battle.battle.log");
            File.Delete("battle.report.log");
            File.Delete("battle.troop.log");
            File.Delete("battle.object.log");
            battleLog = new StreamWriter(File.Open("battle.battle.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            reportLog = new StreamWriter(File.Open("battle.report.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            troopLog = new StreamWriter(File.Open("battle.troop.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            objectLog = new StreamWriter(File.Open("battle.object.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            battleLog.AutoFlush = true;
            reportLog.AutoFlush = true;
            troopLog.AutoFlush = true;
            objectLog.AutoFlush = true;
        }

        internal static void snapBattle(out uint battleId, uint cityId) {
            Global.dbManager.Query(string.Format("INSERT INTO {0} VALUES ('', '{1}', NOW())", BATTLE_DB, cityId));
            battleId = Global.dbManager.LastInsertId();

            battleLog.WriteLine("battle_id[{0}] city_id[{1}] time[{2}]", battleId, cityId, DateTime.Now);
        }

        internal static void snapReport(out uint reportId, uint battleId) {
            Global.dbManager.Query(string.Format("INSERT INTO {0} VALUES ('', NOW(), '{1}', '0', '0', '0')", BATTLE_REPORTS_DB, battleId));
            reportId = Global.dbManager.LastInsertId();

            reportLog.WriteLine("battle_id[{0}] report_id[{1}]", battleId, reportId);
        }

        internal static void snapEndReport(uint reportId, uint battleId, uint round, uint turn) {
            Global.dbManager.Query(string.Format("UPDATE {0} SET ready = 1, round = '{2}', turn = '{3}' WHERE id = '{1}' LIMIT 1", BATTLE_REPORTS_DB, reportId, round, turn));

            reportLog.WriteLine("battle_id[{0}] report_id[{1}] round[{2}] turn[{3}] report ready", battleId, reportId, round, turn);
        }

        internal void snapTroopState(TroopStub stub, ReportState state) {
            uint id = reportedTroops[stub];
            Global.dbManager.Query(string.Format("UPDATE {0} SET state = '{1}' WHERE id = '{2}' LIMIT 1", BATTLE_REPORT_TROOPS_DB, (byte)state, id));

            troopLog.WriteLine("id[{0}] state[{1}] update troop state", id, state);
        }

        internal static void snapTroop(uint reportId, ReportState state, uint cityId, byte troopId, uint objectId, bool isAttacker, out uint battleTroopId, Resource loot) {
            Global.dbManager.Query(string.Format("INSERT INTO {0} VALUES ('', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}')",
                BATTLE_REPORT_TROOPS_DB,
                reportId,
                cityId,
                objectId,
                troopId,
                (byte)state,
                isAttacker ? 1 : 0,
                loot.Gold,
                loot.Crop,
                loot.Iron,
                loot.Wood));

            battleTroopId = Global.dbManager.LastInsertId();

            troopLog.WriteLine("id[{0}] reportId[{1}] state[{2}] cityId[{3}] troop_id[{4}] isAttacker[{5}] crop[{6}] gold[{7}] iron[{8}] wood[{9}]",
                                    battleTroopId,
                                    reportId,
                                    state,
                                    cityId,
                                    troopId,
                                    isAttacker,
                                    loot.Crop,
                                    loot.Gold,
                                    loot.Iron,
                                    loot.Wood);
        }

        internal static void snapCombatObject(uint troopId, CombatObject co) {
            ICombatUnit unit = co as ICombatUnit;

            Global.dbManager.Query(string.Format("INSERT INTO {0} VALUES ('', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')",
                BATTLE_REPORT_OBJECTS_DB,
                troopId,
                co.Type,
                co.Lvl,
                co.HP,
                co.Count,
                co.DmgRecv,
                co.DmgDealt,
                (byte)(unit == null ? FormationType.Structure : unit.Formation)
            ));

            objectLog.WriteLine("troopId[{0}] groupid[{1}] formation[{2}] type[{3}] count[{4}] hp[{5}] dmg_dealt[{6}] dmg_recv[{7}]",
                                    troopId,
                                    co.Id,
                                    unit == null ? FormationType.Structure : unit.Formation,
                                    co.Type,
                                    co.Count,
                                    co.HP,
                                    co.DmgDealt,
                                    co.DmgRecv);
        }
    }
}