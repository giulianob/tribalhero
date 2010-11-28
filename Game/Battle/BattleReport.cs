#region

using System.Collections.Generic;
using System.Data;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Fighting;

#endregion

namespace Game.Battle
{
    public enum ReportState
    {
        ENTERING = 0,
        STAYING = 1,
        EXITING = 2,
        DYING = 3,
        RETREATING = 4,
        REINFORCED = 5,
        OUT_OF_STAMINA = 6
    }

    public class BattleReport
    {
        private const string BATTLE_DB = "battles";
        private const string BATTLE_REPORTS_DB = "battle_reports";
        private const string BATTLE_REPORT_TROOPS_DB = "battle_report_troops";
        private const string BATTLE_REPORT_OBJECTS_DB = "battle_report_objects";
        private const string BATTLE_REPORT_VIEWS_DB = "battle_report_views";

        private readonly BattleManager battle;

        private bool reportStarted;

        public bool ReportStarted
        {
            get { return reportStarted; }
            set { reportStarted = value; }
        }

        private bool reportFlag;

        public bool ReportFlag
        {
            get { return reportFlag; }
            set { reportFlag = value; }
        }

        private uint reportId;

        public uint ReportId
        {
            get { return reportId; }
            set { reportId = value; }
        }

        public ReportedObjects ReportedObjects { get; private set; }

        public ReportedTroops ReportedTroops { get; private set; }

        public BattleReport(BattleManager bm)
        {
            ReportedObjects = new ReportedObjects(bm);
            ReportedTroops = new ReportedTroops(bm);
            battle = bm;
        }

        public void WriteBeginReport()
        {
            if (reportStarted)
                return;

            SnapReport(out reportId, battle.BattleId);
            reportStarted = true;
        }

        public void CreateBattleReport()
        {
            uint battleId;
            SnapBattle(out battleId, battle.City.Id);
            battle.BattleId = battleId;
            WriteBeginReport();
            reportFlag = true;
            WriteReport(ReportState.ENTERING);
        }

        public void CompleteBattle()
        {
            SnapBattleEnd(battle.BattleId);
        }

        public void WriteReportObject(CombatObject co, bool isAttacker, ReportState state)
        {
            uint combatTroopId;

            WriteBeginReport();
            if (co.ClassType == BattleClass.UNIT)
            {
                ICombatUnit cu = co as ICombatUnit;
                if (!ReportedTroops.TryGetValue(cu.TroopStub, out combatTroopId))
                {
                    SnapTroop(state, cu.TroopStub.City.Id, cu.TroopStub.TroopId, co.GroupId, isAttacker, out combatTroopId, cu.TroopStub.TroopObject != null ? cu.TroopStub.TroopObject.Stats.Loot : new Resource());
                    ReportedTroops[cu.TroopStub] = combatTroopId;
                }
                else if (state != ReportState.STAYING)
                    SnapTroopState(cu.TroopStub, state);

                if (!ReportedObjects.Contains(co))
                {
                    SnapCombatObject(combatTroopId, co);
                    ReportedObjects.Add(co);
                }
            }
            else
            {
                TroopStub stub = ((CombatStructure)co).Structure.City.DefaultTroop;
                if (!ReportedTroops.TryGetValue(stub, out combatTroopId))
                {
                    SnapTroop(state, co.City.Id, 1, co.GroupId, isAttacker, out combatTroopId, new Resource());
                    ReportedTroops[stub] = combatTroopId;
                }
                else if (state != ReportState.STAYING)
                {
                    SnapTroopState(stub, state);
                }

                if (!ReportedObjects.Contains(co))
                {
                    SnapCombatObject(combatTroopId, co);
                    ReportedObjects.Add(co);
                }
            }
        }

        public void WriteReportObjects(List<CombatObject> list, bool isAttacker, ReportState state)
        {
            List<TroopStub> updatedObj = new List<TroopStub>();

            WriteBeginReport();

            reportFlag = true;

            foreach (CombatObject co in list)
            {
                bool snapObj = ReportedObjects.Contains(co);

                uint combatTroopId;
                if (co.ClassType == BattleClass.UNIT)
                {
                    ICombatUnit cu = co as ICombatUnit;

                    if (!ReportedTroops.TryGetValue(cu.TroopStub, out combatTroopId))
                    {
                        SnapTroop(state, cu.TroopStub.City.Id, cu.TroopStub.TroopId, co.GroupId, isAttacker, out combatTroopId, cu.TroopStub.TroopObject != null ? cu.TroopStub.TroopObject.Stats.Loot : new Resource());
                        ReportedTroops[cu.TroopStub] = combatTroopId;
                    }
                    else if (state != ReportState.STAYING && !updatedObj.Contains(cu.TroopStub))
                    {
                        //Exiting state should override anything else
                        SnapTroopState(cu.TroopStub, state);
                        updatedObj.Add(cu.TroopStub);
                    }
                }
                else
                {
                    TroopStub stub = ((CombatStructure)co).Structure.City.DefaultTroop;
                    if (!ReportedTroops.TryGetValue(stub, out combatTroopId))
                    {
                        SnapTroop(state, co.City.Id, 1, co.GroupId, isAttacker, out combatTroopId, new Resource());
                        ReportedTroops[stub] = combatTroopId;
                    }
                    else if (state != ReportState.STAYING && !updatedObj.Contains(stub))
                    {
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

        public void CompleteReport(ReportState state)
        {
            if (!reportStarted)
                return;

            reportFlag = true;
            WriteReport(state);
        }

        public void WriteReport(ReportState state)
        {
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

        static void SnapBattle(out uint battleId, uint cityId)
        {
            Global.DbManager.Query(string.Format("INSERT INTO `{0}` VALUES ('', @city_id, NOW(), NULL, '0')", BATTLE_DB), new[] { new DbColumn("city_id", cityId, DbType.UInt32) });
            battleId = Global.DbManager.LastInsertId();
        }

        static void SnapBattleEnd(uint battleId)
        {
            Global.DbManager.Query(string.Format("UPDATE `{0}` SET `ended` = NOW() WHERE `id` = @battle_id LIMIT 1", BATTLE_DB), new[] { new DbColumn("battle_id", battleId, DbType.UInt32) });
        }

        static void SnapReport(out uint reportId, uint battleId)
        {
            Global.DbManager.Query(string.Format("INSERT INTO `{0}` VALUES ('', NOW(), @battle_id, '0', '0', '0')", BATTLE_REPORTS_DB), new[] { new DbColumn("battle_id", battleId, DbType.UInt32) });
            reportId = Global.DbManager.LastInsertId();
        }

        internal static void SnapEndReport(uint reportId, uint battleId, uint round, uint turn)
        {
            Global.DbManager.Query(string.Format("UPDATE `{0}` SET `ready` = 1, `round` = @round, turn = @turn, `created` = NOW() WHERE id = @report_id LIMIT 1", BATTLE_REPORTS_DB),
                                   new[]
                                       {
                                           new DbColumn("report_id", reportId, DbType.UInt32), 
                                           new DbColumn("round", round, DbType.UInt32),
                                           new DbColumn("turn", turn, DbType.UInt32),
                                       });
        }

        void SnapTroopState(TroopStub stub, ReportState state)
        {
            uint id = ReportedTroops[stub];

            // If there's a troop object we also want to update its loot
            if (stub.TroopObject == null)
            {
                Global.DbManager.Query(string.Format("UPDATE `{0}` SET `state` = @state WHERE `id` = @id LIMIT 1", BATTLE_REPORT_TROOPS_DB),
                                       new[] { new DbColumn("state", (byte)state, DbType.Byte), new DbColumn("id", id, DbType.UInt32), });
            }
            else
            {
                Resource loot = stub.TroopObject.Stats.Loot;
                Global.DbManager.Query(
                    string.Format("UPDATE `{0}` SET `state` = @state, `gold` = @gold, `crop` = @crop, `iron` = @iron, `wood` = @wood WHERE `id` = @id LIMIT 1", BATTLE_REPORT_TROOPS_DB),
                    new[]
                        {
                            new DbColumn("state", state, DbType.Byte), new DbColumn("gold", loot.Gold, DbType.Int32), new DbColumn("crop", loot.Crop, DbType.Int32),
                            new DbColumn("iron", loot.Iron, DbType.Int32), new DbColumn("wood", loot.Wood, DbType.Int32), new DbColumn("id", loot.Gold, DbType.UInt32),
                        });
            }
        }

        void SnapTroop(ReportState state, uint cityId, byte troopId, uint objectId, bool isAttacker, out uint battleTroopId, Resource loot)
        {
            Global.DbManager.Query(
                string.Format("INSERT INTO `{0}` VALUES ('', @report_id, @city_id, @object_id, @troop_id, @state, @is_attacker, @gold, @crop, @iron, @wood)", BATTLE_REPORT_TROOPS_DB),
                new[]
                    {
                        new DbColumn("report_id", reportId, DbType.UInt32), new DbColumn("city_id", cityId, DbType.UInt32),
                        new DbColumn("object_id", objectId, DbType.UInt32), new DbColumn("troop_id", troopId, DbType.Byte), new DbColumn("state", state, DbType.Byte),
                        new DbColumn("is_attacker", isAttacker, DbType.Boolean), new DbColumn("gold", loot.Gold, DbType.Int32),
                        new DbColumn("crop", loot.Crop, DbType.Int32), new DbColumn("iron", loot.Iron, DbType.Int32), new DbColumn("wood", loot.Wood, DbType.Int32),
                    });


            battleTroopId = Global.DbManager.LastInsertId();

            // Log any troops that are entering the battle to the view table so they are able to see this report
            // Notice that we don't log the local troop. This is because they can automatically see all of the battles that take place in their cities by using the battles table
            if (battle.City.Id != cityId && (state == ReportState.ENTERING || state == ReportState.REINFORCED))
            {
                Global.DbManager.Query(
                    string.Format("INSERT INTO `{0}` VALUES ('', @city_id, @troop_id, @battle_id, @object_id, @is_attacker, 0, 0, 0, 0, 0, 0, 0, 0, 0, NOW())", BATTLE_REPORT_VIEWS_DB),
                    new[]
                        {
                            new DbColumn("city_id", cityId, DbType.UInt32), new DbColumn("troop_id", troopId, DbType.Byte), new DbColumn("battle_id", battle.BattleId, DbType.UInt32),
                            new DbColumn("object_id", objectId, DbType.UInt32), new DbColumn("is_attacker", isAttacker, DbType.Boolean)
                        });
            }
        }

        void SnapCombatObject(uint troopId, CombatObject co)
        {
            ICombatUnit unit = co as ICombatUnit;

            Global.DbManager.Query(
                string.Format("INSERT INTO `{0}` VALUES ('', @object_id, @troop_id, @type, @lvl, @hp, @count, @dmg_recv, @dmg_dealt, @formation, @hit_dealt, @hit_dealt_by_unit, @hit_recv)",
                              BATTLE_REPORT_OBJECTS_DB),
                new[]
                    {
                        new DbColumn("object_id", co.Id, DbType.UInt32), new DbColumn("troop_id", troopId, DbType.UInt32), new DbColumn("type", co.Type, DbType.UInt16), new DbColumn("lvl", co.Lvl, DbType.Byte), new DbColumn("hp", co.Hp, DbType.UInt32),
                        new DbColumn("count", co.Count, DbType.UInt16), new DbColumn("dmg_recv", co.DmgRecv, DbType.Int32), new DbColumn("dmg_dealt", co.DmgDealt, DbType.Int32),
                        new DbColumn("formation", (byte) (unit == null ? FormationType.STRUCTURE : unit.Formation), DbType.Byte), new DbColumn("hit_dealt", co.HitDealt, DbType.UInt16),
                        new DbColumn("hit_dealt_by_unit", co.HitDealtByUnit, DbType.UInt32), new DbColumn("hit_recv", co.HitRecv, DbType.UInt16),
                    });
        }

        public void SetLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource)
        {
            Global.DbManager.Query(
                string.Format(
                    @"UPDATE `{0}` SET `loot_wood` = @wood, `loot_gold` = @gold, `loot_crop` = @crop, `loot_iron` = @iron, `bonus_wood` = @wood, `bonus_gold` = @bonus_gold, `bonus_crop` = @bonus_crop, `bonus_iron` = @bonus_iron 
                      WHERE `city_id` = @city_id AND `battle_id` = @battle_id AND `troop_stub_id` = @troop_stub_id LIMIT 1",
                    BATTLE_REPORT_VIEWS_DB),
                new[]
                    {
                        new DbColumn("wood", lootResource.Wood, DbType.Int32), new DbColumn("crop", lootResource.Crop, DbType.Int32), new DbColumn("iron", lootResource.Iron, DbType.Int32),
                        new DbColumn("gold", lootResource.Gold, DbType.Int32), new DbColumn("bonus_wood", bonusResource.Wood, DbType.Int32), new DbColumn("bonus_crop", bonusResource.Crop, DbType.Int32),
                        new DbColumn("bonus_iron", bonusResource.Iron, DbType.Int32), new DbColumn("bonus_gold", bonusResource.Gold, DbType.Int32), new DbColumn("city_id", cityId, DbType.UInt32),
                        new DbColumn("battle_id", battleId, DbType.UInt32), new DbColumn("troop_stub_id", troopId, DbType.Byte),
                    });
        }
    }
}