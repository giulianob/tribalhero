using System;
using System.Data;
using System.Globalization;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Persistance;

namespace Game.Battle.Reporting
{
    public class SqlBattleReportWriter : IBattleReportWriter
    {
        public const string BATTLE_DB = "battles";

        public const string BATTLE_REPORTS_DB = "battle_reports";

        public const string BATTLE_REPORT_TROOPS_DB = "battle_report_troops";

        public const string BATTLE_REPORT_OBJECTS_DB = "battle_report_objects";

        public const string BATTLE_REPORT_VIEWS_DB = "battle_report_views";

        public const string BATTLE_REPORTS_ACCESS_DB = "battle_report_access";

        public const string BATTLE_TRIBES_DB = "battle_tribes";

        private readonly IDbManager dbManager;

        public SqlBattleReportWriter(IDbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void SnapBattle(uint battleId, BattleOwner owner, BattleLocation location)
        {
            dbManager.Query(
                            string.Format(
                                          @"INSERT INTO `{0}` VALUES (@id, @owner_type, @owner_id, @location_type, @location_id, UTC_TIMESTAMP(), NULL, '0')",
                                          BATTLE_DB),
                            new[]
                            {
                                    new DbColumn("id", battleId, DbType.UInt32),
                                    new DbColumn("owner_type", owner.Type.ToString(), DbType.String, 15),
                                    new DbColumn("owner_id", owner.Id, DbType.UInt32),
                                    new DbColumn("location_type", location.Type.ToString(), DbType.String, 15),
                                    new DbColumn("location_id", location.Id, DbType.UInt32)
                            });
        }

        public void SnapBattleEnd(uint battleId)
        {
            dbManager.Query(
                            string.Format(
                                          @"UPDATE `{0}` SET `ended` = UTC_TIMESTAMP() WHERE `id` = @battle_id LIMIT 1",
                                          BATTLE_DB),
                            new[] {new DbColumn("battle_id", battleId, DbType.UInt32)});
        }

        public void SnapReport(out uint reportId, uint battleId)
        {
            reportId = (uint)BattleReport.ReportIdGenerator.GetNext();

            dbManager.Query(
                            string.Format(
                                          @"INSERT INTO `{0}` VALUES (@id, UTC_TIMESTAMP(), @battle_id, '0', '0', '0')",
                                          BATTLE_REPORTS_DB),
                            new[]
                            {
                                    new DbColumn("id", reportId, DbType.UInt32),
                                    new DbColumn("battle_id", battleId, DbType.UInt32)
                            });
        }

        public void SnapEndReport(uint reportId, uint battleId, uint round, uint turn)
        {
            dbManager.Query(
                            string.Format(
                                          @"UPDATE `{0}` SET `ready` = 1, `round` = @round, turn = @turn, `created` = UTC_TIMESTAMP() WHERE id = @report_id LIMIT 1",
                                          BATTLE_REPORTS_DB),
                            new[]
                            {
                                    new DbColumn("report_id", reportId, DbType.UInt32),
                                    new DbColumn("round", round, DbType.UInt32),
                                    new DbColumn("turn", turn, DbType.UInt32),
                            });
        }

        public void SnapGroupState(uint reportTroopId, ICombatGroup group, ReportState state)
        {
            // If there's a troop object we also want to update its loot
            Resource loot = group.GroupLoot;
            dbManager.Query(
                            string.Format(
                                          @"UPDATE `{0}` SET `state` = @state, `gold` = @gold, `crop` = @crop, `iron` = @iron, `wood` = @wood WHERE `id` = @id LIMIT 1",
                                          BATTLE_REPORT_TROOPS_DB),
                            new[]
                            {
                                    new DbColumn("state", state, DbType.Byte), new DbColumn("gold", loot.Gold, DbType.Int32),
                                    new DbColumn("crop", loot.Crop, DbType.Int32),
                                    new DbColumn("iron", loot.Iron, DbType.Int32),
                                    new DbColumn("wood", loot.Wood, DbType.Int32),
                                    new DbColumn("id", reportTroopId, DbType.UInt32),
                            });
        }

        public uint SnapGroup(uint reportId, ICombatGroup group, ReportState state, bool isAttacker)
        {
            Resource loot = group.GroupLoot;

            uint battleTroopId = (uint)BattleReport.BattleTroopIdGenerator.GetNext();

            dbManager.Query(string.Format(@"INSERT INTO `{0}` 
                                            (`id`, `battle_report_id`, `owner_type`, `owner_id`, `group_id`, `name`, `state`, `is_attacker`, `gold`, `crop`, `iron`, `wood`) VALUES
                                            (@id, @report_id, @owner_type, @owner_id, @group_id, @name, @state, @is_attacker, @gold, @crop, @iron, @wood)", BATTLE_REPORT_TROOPS_DB), new[
                                                                                                                                                                                                                                                                                                                                                                                                                                                   ]
            {
                    new DbColumn("id", battleTroopId, DbType.UInt32), new DbColumn("report_id", reportId, DbType.UInt32),
                    new DbColumn("owner_type", group.Owner.Type.ToString(), DbType.String, 15),
                    new DbColumn("owner_id", group.Owner.Id, DbType.UInt32),
                    new DbColumn("group_id", group.Id, DbType.UInt32),
                    new DbColumn("name",
                                 group.TroopId == 1 ? "[LOCAL]" : group.TroopId.ToString(CultureInfo.InvariantCulture),
                                 DbType.String,
                                 32),
                    new DbColumn("state", state, DbType.Byte),
                    new DbColumn("is_attacker", isAttacker, DbType.Boolean),
                    new DbColumn("gold", loot.Gold, DbType.Int32), new DbColumn("crop", loot.Crop, DbType.Int32),
                    new DbColumn("iron", loot.Iron, DbType.Int32), new DbColumn("wood", loot.Wood, DbType.Int32)
            });

            return battleTroopId;
        }

        public void SnapBattleAccess(uint battleId, BattleOwner owner, ushort troopId, uint groupId, bool isAttacker)
        {
            dbManager.Query(string.Format(@"INSERT INTO `{0}`
                                          VALUES ('', @owner_type, @owner_id, @troop_id, @battle_id, @object_id, @is_attacker, 0, 0, 0, 0, 0, 0, 0, 0, 0, UTC_TIMESTAMP())", BATTLE_REPORT_VIEWS_DB), new[
                                                                                                                                                                                                                                                                             ]
            {
                    new DbColumn("owner_type", owner.Type, DbType.String), 
                    new DbColumn("owner_id", owner.Id, DbType.UInt32),
                    new DbColumn("troop_id", troopId, DbType.UInt16), 
                    new DbColumn("battle_id", battleId, DbType.UInt32),
                    new DbColumn("object_id", groupId, DbType.UInt32),
                    new DbColumn("is_attacker", isAttacker, DbType.Boolean)
            });
        }

        public void SnapTribeToBattle(uint battleId, uint tribeId, bool isAttacker)
        {
            dbManager.Query(
                            string.Format(
                                          @"INSERT IGNORE `{0}` SET `battle_id` = @battle_id, `tribe_id` = @tribe_id, `is_attacker` = @is_attacker",
                                          BATTLE_TRIBES_DB),
                            new[]
                            {
                                    new DbColumn("battle_id", battleId, DbType.UInt32),
                                    new DbColumn("tribe_id", tribeId, DbType.UInt32),
                                    new DbColumn("is_attacker", isAttacker, DbType.Boolean)
                            });
        }

        public void SnapCombatObject(uint troopId, ICombatObject co)
        {
            // Delete any existing snapshots of this combat object
            dbManager.Query(
                            string.Format(
                                          @"DELETE FROM `{0}` WHERE `battle_report_troop_id` = @battle_report_troop_id AND `object_id` = @object_id LIMIT 1",
                                          BATTLE_REPORT_OBJECTS_DB),
                            new[]
                            {
                                    new DbColumn("battle_report_troop_id", troopId, DbType.UInt32),
                                    new DbColumn("object_id", co.Id, DbType.UInt32)
                            });

            // Snap the combat object
            dbManager.Query(
                            string.Format(
                                          @"INSERT INTO `{0}` VALUES ('', @object_id, @battle_report_troop_id, @type, @lvl, @hp, @count, @dmg_recv, @dmg_dealt, @hit_dealt, @hit_dealt_by_unit, @hit_recv)",
                                          BATTLE_REPORT_OBJECTS_DB),
                            new[]
                            {
                                    new DbColumn("object_id", co.Id, DbType.UInt32),
                                    new DbColumn("battle_report_troop_id", troopId, DbType.UInt32),
                                    new DbColumn("type", co.Type, DbType.UInt16),
                                    new DbColumn("lvl", co.Lvl, DbType.Byte), new DbColumn("hp", co.Hp, DbType.Decimal),
                                    new DbColumn("count", co.Count, DbType.UInt16),
                                    new DbColumn("dmg_recv", co.DmgRecv, DbType.Decimal),
                                    new DbColumn("dmg_dealt", co.DmgDealt, DbType.Decimal),
                                    new DbColumn("hit_dealt", co.HitDealt, DbType.UInt16),
                                    new DbColumn("hit_dealt_by_unit", co.HitDealtByUnit, DbType.UInt32),
                                    new DbColumn("hit_recv", co.HitRecv, DbType.UInt16),
                            });
        }

        public void SnapLootedResources(BattleOwner owner,
                                        uint groupId,
                                        uint battleId,
                                        Resource lootResource,
                                        Resource bonusResource)
        {
            uint reportViewId;
            using (
                    var reader =
                            dbManager.ReaderQuery(
                                                  string.Format(
                                                                @"SELECT `id` as `report_view_id` FROM `{0}` WHERE `owner_type` = @owner_type AND `owner_id` = @owner_id AND `battle_id` = @battle_id AND `group_id` = @group_id",
                                                                BATTLE_REPORT_VIEWS_DB),
                                                  new[]
                                                  {
                                                          new DbColumn("owner_type", owner.Type.ToString(), DbType.String),
                                                          new DbColumn("owner_id", owner.Id.ToString(), DbType.UInt32),
                                                          new DbColumn("battle_id", battleId, DbType.UInt32),
                                                          new DbColumn("group_id", groupId, DbType.UInt32)
                                                  }))
            {
                if (!reader.HasRows)
                {
                    throw new Exception("Could not find report we're trying to update");
                }

                reader.Read();

                reportViewId = (uint)reader["report_view_id"];
            }

            dbManager.Query(string.Format(@"UPDATE `{0}` 
                                            SET 
                                            `loot_wood` = @wood, 
                                            `loot_gold` = @gold, 
                                            `loot_crop` = @crop, 
                                            `loot_iron` = @iron, 
                                            `bonus_wood` = @bonus_wood, 
                                            `bonus_gold` = @bonus_gold, 
                                            `bonus_crop` = @bonus_crop, 
                                            `bonus_iron` = @bonus_iron 
                                            WHERE 
                                            `id` = @id", BATTLE_REPORT_VIEWS_DB), new[]
            {
                    new DbColumn("wood", lootResource.Wood, DbType.Int32),
                    new DbColumn("crop", lootResource.Crop, DbType.Int32),
                    new DbColumn("iron", lootResource.Iron, DbType.Int32),
                    new DbColumn("gold", lootResource.Gold, DbType.Int32),
                    new DbColumn("bonus_wood", bonusResource.Wood, DbType.Int32),
                    new DbColumn("bonus_crop", bonusResource.Crop, DbType.Int32),
                    new DbColumn("bonus_iron", bonusResource.Iron, DbType.Int32),
                    new DbColumn("bonus_gold", bonusResource.Gold, DbType.Int32),
                    new DbColumn("id", reportViewId, DbType.UInt32)
            });
        }
    }
}