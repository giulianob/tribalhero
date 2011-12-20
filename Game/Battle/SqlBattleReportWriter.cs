﻿using System.Data;
using Game.Data;
using Game.Data.Troop;
using Persistance;

namespace Game.Battle
{
    public class SqlBattleReportWriter : IBattleReportWriter
    {
        private readonly IDbManager dbManager;

        public const string BATTLE_DB = "battles";
        public const string BATTLE_REPORTS_DB = "battle_reports";
        public const string BATTLE_REPORT_TROOPS_DB = "battle_report_troops";
        public const string BATTLE_REPORT_OBJECTS_DB = "battle_report_objects";
        public const string BATTLE_REPORT_VIEWS_DB = "battle_report_views";

        public SqlBattleReportWriter(IDbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void SnapBattle(out uint battleId, uint cityId)
        {
            battleId = (uint)BattleReport.BattleIdGenerator.GetNext();

            dbManager.Query(string.Format(@"INSERT INTO `{0}` VALUES (@id, @city_id, UTC_TIMESTAMP(), NULL, '0')", BATTLE_DB),
                            new[] { new DbColumn("id", battleId, DbType.UInt32), new DbColumn("city_id", cityId, DbType.UInt32) });
        }

        public void SnapBattleEnd(uint battleId)
        {
            dbManager.Query(string.Format(@"UPDATE `{0}` SET `ended` = UTC_TIMESTAMP() WHERE `id` = @battle_id LIMIT 1", BATTLE_DB),
                            new[] { new DbColumn("battle_id", battleId, DbType.UInt32) });
        }

        public void SnapReport(out uint reportId, uint battleId)
        {
            reportId = (uint)BattleReport.ReportIdGenerator.GetNext();

            dbManager.Query(string.Format(@"INSERT INTO `{0}` VALUES (@id, UTC_TIMESTAMP(), @battle_id, '0', '0', '0')", BATTLE_REPORTS_DB),
                            new[] { new DbColumn("id", reportId, DbType.UInt32), new DbColumn("battle_id", battleId, DbType.UInt32) });
        }

        public void SnapEndReport(uint reportId, uint battleId, uint round, uint turn)
        {
            dbManager.Query(
                            string.Format(
                                          @"UPDATE `{0}` SET `ready` = 1, `round` = @round, turn = @turn, `created` = UTC_TIMESTAMP() WHERE id = @report_id LIMIT 1",
                                          BATTLE_REPORTS_DB),
                            new[]
                            {
                                    new DbColumn("report_id", reportId, DbType.UInt32), new DbColumn("round", round, DbType.UInt32),
                                    new DbColumn("turn", turn, DbType.UInt32),
                            });
        }

        public void SnapTroopState(uint reportTroopId, TroopStub stub, ReportState state)
        {
            // If there's a troop object we also want to update its loot
            if (stub.TroopObject == null)
            {
                dbManager.Query(string.Format(@"UPDATE `{0}` SET `state` = @state WHERE `id` = @id LIMIT 1", BATTLE_REPORT_TROOPS_DB),
                                new[] { new DbColumn("state", (byte)state, DbType.Byte), new DbColumn("id", reportTroopId, DbType.UInt32), });
            }
            else
            {
                Resource loot = stub.TroopObject.Stats.Loot;
                dbManager.Query(
                                string.Format(@"UPDATE `{0}` SET `state` = @state, `gold` = @gold, `crop` = @crop, `iron` = @iron, `wood` = @wood WHERE `id` = @id LIMIT 1",
                                              BATTLE_REPORT_TROOPS_DB),
                                new[]
                                {
                                        new DbColumn("state", state, DbType.Byte), new DbColumn("gold", loot.Gold, DbType.Int32),
                                        new DbColumn("crop", loot.Crop, DbType.Int32), new DbColumn("iron", loot.Iron, DbType.Int32),
                                        new DbColumn("wood", loot.Wood, DbType.Int32), new DbColumn("id", reportTroopId, DbType.UInt32),
                                });
            }
        }

        public uint SnapTroop(uint reportId, ReportState state, uint cityId, byte troopId, uint objectId, bool isAttacker, Resource loot)
        {
            uint battleTroopId = (uint)BattleReport.BattleTroopIdGenerator.GetNext();

            dbManager.Query(
                            string.Format(
                                          @"INSERT INTO `{0}` VALUES (@id, @report_id, @city_id, @object_id, @troop_id, @state, @is_attacker, @gold, @crop, @iron, @wood)",
                                          BATTLE_REPORT_TROOPS_DB),
                            new[]
                            {
                                    new DbColumn("id", battleTroopId, DbType.UInt32), new DbColumn("report_id", reportId, DbType.UInt32),
                                    new DbColumn("city_id", cityId, DbType.UInt32), new DbColumn("object_id", objectId, DbType.UInt32),
                                    new DbColumn("troop_id", troopId, DbType.Byte), new DbColumn("state", state, DbType.Byte),
                                    new DbColumn("is_attacker", isAttacker, DbType.Boolean), new DbColumn("gold", loot.Gold, DbType.Int32),
                                    new DbColumn("crop", loot.Crop, DbType.Int32), new DbColumn("iron", loot.Iron, DbType.Int32),
                                    new DbColumn("wood", loot.Wood, DbType.Int32),
                            });

            return battleTroopId;
        }

        public void SnapBattleReportView(uint cityId, byte troopId, uint battleId, uint objectId, bool isAttacker)
        {
            dbManager.Query(
                string.Format(@"INSERT INTO `{0}` VALUES ('', @city_id, @troop_id, @battle_id, @object_id, @is_attacker, 0, 0, 0, 0, 0, 0, 0, 0, 0, UTC_TIMESTAMP())",
                              BATTLE_REPORT_VIEWS_DB),
                new[]
                                {
                                        new DbColumn("city_id", cityId, DbType.UInt32), new DbColumn("troop_id", troopId, DbType.Byte),
                                        new DbColumn("battle_id", battleId, DbType.UInt32), new DbColumn("object_id", objectId, DbType.UInt32),
                                        new DbColumn("is_attacker", isAttacker, DbType.Boolean)
                                });
        }

        public void SnapCombatObject(uint troopId, CombatObject co)
        {
            var unit = co as ICombatUnit;

            dbManager.Query(
                            string.Format(@"INSERT INTO `{0}` VALUES ('', @object_id, @troop_id, @type, @lvl, @hp, @count, @dmg_recv, @dmg_dealt, @formation, @hit_dealt, @hit_dealt_by_unit, @hit_recv)",
                                          BATTLE_REPORT_OBJECTS_DB),
                            new[]
                            {
                                    new DbColumn("object_id", co.Id, DbType.UInt32), new DbColumn("troop_id", troopId, DbType.UInt32),
                                    new DbColumn("type", co.Type, DbType.UInt16), new DbColumn("lvl", co.Lvl, DbType.Byte), new DbColumn("hp", co.Hp, DbType.UInt32),
                                    new DbColumn("count", co.Count, DbType.UInt16), new DbColumn("dmg_recv", co.DmgRecv, DbType.Int32),
                                    new DbColumn("dmg_dealt", co.DmgDealt, DbType.Int32),
                                    new DbColumn("formation", (byte)(unit == null ? FormationType.Structure : unit.Formation), DbType.Byte),
                                    new DbColumn("hit_dealt", co.HitDealt, DbType.UInt16), new DbColumn("hit_dealt_by_unit", co.HitDealtByUnit, DbType.UInt32),
                                    new DbColumn("hit_recv", co.HitRecv, DbType.UInt16),
                            });
        }

        public void SnapLootedResources(uint cityId, byte troopId, uint battleId, Resource lootResource, Resource bonusResource)
        {
            dbManager.Query(
                            string.Format(
                                          @"UPDATE `{0}` 
                                            SET 
                                            `loot_wood` = @wood, 
                                            `loot_gold` = @gold, 
                                            `loot_crop` = @crop, 
                                            `loot_iron` = @iron, 
                                            `bonus_wood` = @bonus_wood, 
                                            `bonus_gold` = @bonus_gold, 
                                            `bonus_crop` = @bonus_crop, 
                                            `bonus_iron` = @bonus_iron 
                                            WHERE `city_id` = @city_id AND `battle_id` = @battle_id AND `troop_stub_id` = @troop_stub_id 
                                            LIMIT 1",
                                          BATTLE_REPORT_VIEWS_DB),
                            new[]
                            {
                                    new DbColumn("wood", lootResource.Wood, DbType.Int32), new DbColumn("crop", lootResource.Crop, DbType.Int32),
                                    new DbColumn("iron", lootResource.Iron, DbType.Int32), new DbColumn("gold", lootResource.Gold, DbType.Int32),
                                    new DbColumn("bonus_wood", bonusResource.Wood, DbType.Int32), new DbColumn("bonus_crop", bonusResource.Crop, DbType.Int32),
                                    new DbColumn("bonus_iron", bonusResource.Iron, DbType.Int32), new DbColumn("bonus_gold", bonusResource.Gold, DbType.Int32),
                                    new DbColumn("city_id", cityId, DbType.UInt32), new DbColumn("battle_id", battleId, DbType.UInt32),
                                    new DbColumn("troop_stub_id", troopId, DbType.Byte),
                            });
        }
    }
}