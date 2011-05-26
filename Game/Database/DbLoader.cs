#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Game.Battle;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Actions.ResourceActions;
using Game.Module;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Database
{
    public class DbLoader
    {
        public static bool LoadFromDatabase(IDbManager dbManager)
        {
            SystemVariablesUpdater.Pause();
            Global.Scheduler.Pause();
            Global.FireEvents = false;

            Global.Logger.Info("Loading database...");

            DateTime now = DateTime.UtcNow;

            // Set all players to offline
            Global.DbManager.Query("UPDATE `players` SET online = @online", new[] {new DbColumn("online", false, DbType.Boolean)});

            using (DbTransaction transaction = Global.DbManager.GetThreadTransaction())
            {
                try
                {
                    LoadSystemVariables(dbManager);

                    // Calculate how long server was down
                    TimeSpan downTime = now.Subtract((DateTime)Global.SystemVariables["System.time"].Value);
                    if (downTime.TotalMilliseconds < 0) downTime = new TimeSpan(0);
                    
                    Global.Logger.Info(string.Format("Server was down for {0}", downTime));

                    LoadMarket(dbManager);
                    LoadPlayers(dbManager);
                    LoadCities(dbManager, downTime);
                    LoadTribes(dbManager);
                    LoadTribesmen(dbManager);
                    LoadUnitTemplates(dbManager);
                    LoadStructures(dbManager);
                    LoadStructureProperties(dbManager);
                    LoadTechnologies(dbManager);
                    LoadForests(dbManager, downTime);
                    LoadTroopStubs(dbManager);
                    LoadTroopStubTemplates(dbManager);
                    LoadTroops(dbManager);
                    LoadBattleManagers(dbManager);
                    LoadActions(dbManager, downTime);
                    LoadActionReferences(dbManager);
                    LoadActionNotifications(dbManager);

                    Global.World.AfterDbLoaded();

                    //Ok data all loaded. We can get the system going now.
                    Global.SystemVariables["System.time"].Value = now;
                    Global.DbManager.Save(Global.SystemVariables["System.time"]);
                }
                catch(Exception e)
                {
                    Global.Logger.Error("Database loader error", e);
                    transaction.Rollback();
                    return false;
                }
            }

            Global.Logger.Info("Database loading finished");

            SystemVariablesUpdater.Resume();
            Global.FireEvents = true;
            Global.Scheduler.Resume();
            return true;
        }

        private static void LoadTribes(IDbManager dbManager) {
            #region Tribes

            Global.Logger.Info("Loading tribes...");
            using (var reader = dbManager.Select(Tribe.DB_TABLE))
            {
                while (reader.Read())
                {
                   // var resource = new Resource((int)reader["crop"],(int)reader["gold"],(int)reader["iron"],(int)reader["wood"],0);
                    var tribe = new Tribe(Global.World.Players[(uint)reader["player_id"]], (string)reader["name"], (string)reader["desc"], (byte)reader["level"], new Resource()) { DbPersisted = true };
                    Global.Tribes.Add(tribe.Id, tribe);
                }
            }
            #endregion
        }

        private static void LoadTribesmen(IDbManager dbManager) {
            #region Tribes

            Global.Logger.Info("Loading tribesmen...");
            using (var reader = dbManager.Select(Tribesman.DB_TABLE)) {
                while (reader.Read()) {
                    Tribe tribe = Global.Tribes[(uint)reader["tribe_id"]];
                    var contribution = new Resource((int)reader["crop"], (int)reader["gold"], (int)reader["iron"], (int)reader["wood"], 0);
                    var tribesman = new Tribesman(tribe, Global.World.Players[(uint)reader["player_id"]], (DateTime)reader["join_date"], contribution, (byte)reader["rank"])
                                    {DbPersisted = true};
                    tribe.AddTribesman(tribesman,false);
                }
            }
            #endregion
        }

        private static void LoadSystemVariables(IDbManager dbManager)
        {
            #region System variables

            Global.Logger.Info("Loading system variables...");
            using (var reader = dbManager.Select(SystemVariable.DB_TABLE))
            {
                while (reader.Read())
                {
                    var systemVariable = new SystemVariable((string)reader["name"],
                                                            DataTypeSerializer.Deserialize((string)reader["value"], (byte)reader["datatype"]))
                                         {DbPersisted = true};
                    Global.SystemVariables.Add(systemVariable.Key, systemVariable);
                }
            }

            // Set system variable defaults
            if (!Global.SystemVariables.ContainsKey("System.time"))
                Global.SystemVariables.Add("System.time", new SystemVariable("System.time", DateTime.UtcNow));

            if (!Global.SystemVariables.ContainsKey("Map.start_index"))
                Global.SystemVariables.Add("Map.start_index", new SystemVariable("Map.start_index", 0));

            #endregion
        }

        private static void LoadMarket(IDbManager dbManager)
        {
            #region Market

            Global.Logger.Info("Loading market...");
            using (var reader = dbManager.Select(Market.DB_TABLE))
            {
                while (reader.Read())
                {
                    var type = (ResourceType)((byte)reader["resource_type"]);
                    var market = new Market(type, (int)reader["price"]);
                    market.DbLoad((int)reader["outgoing"], (int)reader["incoming"]);
                    market.DbPersisted = true;
                    switch(type)
                    {
                        case ResourceType.Crop:
                            Market.Crop = market;
                            break;
                        case ResourceType.Wood:
                            Market.Wood = market;
                            break;
                        case ResourceType.Iron:
                            Market.Iron = market;
                            break;
                        default:
                            continue;
                    }
                }
            }

            #endregion
        }

        private static void LoadPlayers(IDbManager dbManager)
        {
            #region Players

            Global.Logger.Info("Loading players...");
            using (var reader = dbManager.Select(Player.DB_TABLE))
            {
                while (reader.Read())
                {
                    var player = new Player((uint)reader["id"],
                                            DateTime.SpecifyKind((DateTime)reader["created"], DateTimeKind.Utc),
                                            DateTime.SpecifyKind((DateTime)reader["last_login"], DateTimeKind.Utc),
                                            (string)reader["name"],
                                            (string)reader["description"],
                                            false) { DbPersisted = true, TribeRequest = (uint)reader["invitation_tribe_id"] };
                    Global.World.Players.Add(player.PlayerId, player);
                }
            }

            #endregion
        }

        private static void LoadCities(IDbManager dbManager, TimeSpan downTime)
        {
            #region Cities

            Global.Logger.Info("Loading cities...");
            using (var reader = dbManager.Select(City.DB_TABLE))
            {
                while (reader.Read())
                {
                    if ((City.DeletedState)reader["deleted"] == City.DeletedState.Deleted)
                        continue;

                    DateTime cropRealizeTime = DateTime.SpecifyKind((DateTime)reader["crop_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime woodRealizeTime = DateTime.SpecifyKind((DateTime)reader["wood_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime ironRealizeTime = DateTime.SpecifyKind((DateTime)reader["iron_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime laborRealizeTime = DateTime.SpecifyKind((DateTime)reader["labor_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime goldRealizeTime = DateTime.SpecifyKind((DateTime)reader["gold_realize_time"], DateTimeKind.Utc).Add(downTime);

                    var resource = new LazyResource((int)reader["crop"],
                                                    cropRealizeTime,
                                                    (int)reader["crop_production_rate"],
                                                    (int)reader["crop_upkeep"],
                                                    (int)reader["gold"],
                                                    goldRealizeTime,
                                                    (int)reader["gold_production_rate"],
                                                    (int)reader["iron"],
                                                    ironRealizeTime,
                                                    (int)reader["iron_production_rate"],
                                                    (int)reader["wood"],
                                                    woodRealizeTime,
                                                    (int)reader["wood_production_rate"],
                                                    (int)reader["labor"],
                                                    laborRealizeTime,
                                                    (int)reader["labor_production_rate"]);
                    var city = new City(Global.World.Players[(uint)reader["player_id"]], (string)reader["name"], resource, (byte)reader["radius"], null)
                               {
                                       DbPersisted = true,
                                       LootStolen = (uint)reader["loot_stolen"],
                                       AttackPoint = (int)reader["attack_point"],
                                       DefensePoint = (int)reader["defense_point"],
                                       HideNewUnits = (bool)reader["hide_new_units"],
                                       Value = (ushort)reader["value"],
                                       Deleted = (City.DeletedState)reader["deleted"]
                               };

                    Global.World.DbLoaderAdd((uint)reader["id"], city);
                    
                    city.Owner.Add(city);

                    switch(city.Deleted)
                    {
                        case City.DeletedState.Deleting:                            
                            CityRemover cr = new CityRemover(city.Id);
                            cr.Start(true);                        
                            break;
                    }
                }
            }

            #endregion
        }

        private static void LoadUnitTemplates(IDbManager dbManager)
        {
            #region Unit Template

            Global.Logger.Info("Loading unit template...");
            using (var reader = dbManager.Select(UnitTemplate.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);
                    city.Template.DbPersisted = true;

                    using (DbDataReader listReader = dbManager.SelectList(city.Template))
                    {
                        while (listReader.Read())
                            city.Template.DbLoaderAdd((ushort)listReader["type"],
                                                      UnitFactory.GetUnitStats((ushort)listReader["type"], (byte)listReader["level"]));
                    }
                }
            }

            #endregion
        }

        private static void LoadForests(IDbManager dbManager, TimeSpan downTime)
        {
            Global.Logger.Info("Loading forests...");
            using (var reader = dbManager.Select(Forest.DB_TABLE))
            {
                while (reader.Read())
                {
                    var forest = new Forest((byte)reader["level"], (int)reader["capacity"], (float)reader["rate"])
                                 {
                                         DbPersisted = true,
                                         X = (uint)reader["x"],
                                         Y = (uint)reader["y"],
                                         Labor = (ushort)reader["labor"],
                                         ObjectId = (uint)reader["id"],
                                         State = {Type = (ObjectState)((byte)reader["state"])},
                                         Wood =
                                                 new AggressiveLazyValue((int)reader["lumber"],
                                                                         DateTime.SpecifyKind((DateTime)reader["last_realize_time"], DateTimeKind.Utc).Add(downTime),
                                                                         0,
                                                                         (int)reader["upkeep"]) {Limit = (int)reader["capacity"]},
                                         DepleteTime = DateTime.SpecifyKind((DateTime)reader["deplete_time"], DateTimeKind.Utc).Add(downTime),
                                         InWorld = (bool)reader["in_world"]
                                 };

                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                        forest.State.Parameters.Add(variable);

                    // Add lumberjacks
                    foreach (var vars in XmlSerializer.DeserializeComplexList((string)reader["structures"]))
                    {
                        City city;
                        if (!Global.World.TryGetObjects((uint)vars[0], out city))
                            throw new Exception("City not found");

                        Structure structure;
                        if (!city.TryGetStructure((uint)vars[1], out structure))
                            throw new Exception("Structure not found");

                        forest.AddLumberjack(structure);
                    }

                    if (forest.InWorld)
                    {
                        // Create deplete time
                        Global.Scheduler.Put(new ForestDepleteAction(forest, forest.DepleteTime));
                        Global.World.DbLoaderAdd(forest);
                        Global.World.Forests.DbLoaderAdd(forest);
                    }

                    // Resave to include new time
                    Global.DbManager.Save(forest);
                }
            }
        }

        private static void LoadStructures(IDbManager dbManager)
        {
            #region Structures

            Global.Logger.Info("Loading structures...");
            using (var reader = dbManager.Select(Structure.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);
                    Structure structure = StructureFactory.GetNewStructure((ushort)reader["type"], (byte)reader["level"]);
                    structure.InWorld = (bool)reader["in_world"];
                    structure.Technologies.Parent = city.Technologies;
                    structure.X = (uint)reader["x"];
                    structure.Y = (uint)reader["y"];
                    structure.Stats.Hp = (ushort)reader["hp"];
                    structure.ObjectId = (uint)reader["id"];
                    structure.Stats.Labor = (ushort)reader["labor"];
                    structure.DbPersisted = true;
                    structure.State.Type = (ObjectState)((byte)reader["state"]);
                    structure.IsBlocked = (bool)reader["is_blocked"];

                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                        structure.State.Parameters.Add(variable);

                    city.Add(structure.ObjectId, structure, false);

                    if (structure.InWorld)
                        Global.World.DbLoaderAdd(structure);
                }
            }
            
            #endregion
        }

        private static void LoadStructureProperties(IDbManager dbManager)
        {
            #region Structure Properties

            Global.Logger.Info("Loading structure properties...");
            using (var reader = dbManager.Select(StructureProperties.DB_TABLE))
            {
                City city = null;
                while (reader.Read())
                {
                    // Simple optimization                        
                    if (city == null || city.Id != (uint)reader["city_id"])
                        Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    var structure = (Structure)city[(uint)reader["structure_id"]];

                    structure.Properties.DbPersisted = true;

                    using (DbDataReader listReader = dbManager.SelectList(structure.Properties))
                    {
                        while (listReader.Read())
                            structure.Properties.Add(listReader["name"],
                                                     DataTypeSerializer.Deserialize((string)listReader["value"], (byte)listReader["datatype"]));
                    }
                }
            }

            #endregion
        }

        private static void LoadTechnologies(IDbManager dbManager)
        {
            #region Technologies

            Global.Logger.Info("Loading technologies...");
            using (var reader = dbManager.Select(TechnologyManager.DB_TABLE))
            {
                while (reader.Read())
                {
                    var ownerLocation = (EffectLocation)((byte)reader["owner_location"]);

                    TechnologyManager manager;

                    switch(ownerLocation)
                    {
                        case EffectLocation.Object:
                        {
                            City city;
                            Global.World.TryGetObjects((uint)reader["city_id"], out city);
                            var structure = (Structure)city[(uint)reader["owner_id"]];
                            manager = structure.Technologies;
                        }
                            break;
                        case EffectLocation.City:
                        {
                            City city;
                            Global.World.TryGetObjects((uint)reader["city_id"], out city);
                            manager = city.Technologies;
                        }
                            break;
                        default:
                            throw new Exception("Unknown effect location?");
                    }

                    manager.DbPersisted = true;

                    using (DbDataReader listReader = dbManager.SelectList(manager))
                    {
                        while (listReader.Read())
                            manager.Add(TechnologyFactory.GetTechnology((uint)listReader["type"], (byte)listReader["level"]), false);
                    }
                }
            }

            #endregion
        }

        private static void LoadTroopStubs(IDbManager dbManager)
        {
            #region Troop Stubs

            Global.Logger.Info("Loading troop stubs...");
            using (var reader = dbManager.Select(TroopStub.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);
                    City stationedCity = null;

                    if ((uint)reader["stationed_city_id"] != 0)
                        Global.World.TryGetObjects((uint)reader["stationed_city_id"], out stationedCity);

                    var stub = new TroopStub
                               {
                                       TroopManager = city.Troops,
                                       TroopId = (byte)reader["id"],
                                       State = (TroopState)Enum.Parse(typeof(TroopState), reader["state"].ToString(), true),
                                       DbPersisted = true
                               };

                    var formationMask = (ushort)reader["formations"];
                    var formations = (FormationType[])Enum.GetValues(typeof(FormationType));
                    foreach (var type in formations)
                    {
                        if ((formationMask & (ushort)Math.Pow(2, (ushort)type)) != 0)
                            stub.AddFormation(type);
                    }

                    using (DbDataReader listReader = dbManager.SelectList(stub))
                    {
                        while (listReader.Read())
                            stub.AddUnit((FormationType)((byte)listReader["formation_type"]), (ushort)listReader["type"], (ushort)listReader["count"]);
                    }

                    city.Troops.DbLoaderAdd((byte)reader["id"], stub);
                    if (stationedCity != null)
                        stationedCity.Troops.AddStationed(stub);
                }
            }

            #endregion
        }

        private static void LoadTroopStubTemplates(IDbManager dbManager)
        {
            #region Troop Stub's Templates

            Global.Logger.Info("Loading troop stub templates...");
            using (var reader = dbManager.Select(TroopTemplate.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);
                    TroopStub stub = city.Troops[(byte)reader["troop_stub_id"]];
                    stub.Template.DbPersisted = true;

                    using (DbDataReader listReader = dbManager.SelectList(stub.Template))
                    {
                        while (listReader.Read())
                        {
                            //First we load the BaseBattleStats and pass it into the BattleStats
                            //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                            var battleStats = new BattleStats(UnitFactory.GetBattleStats((ushort)listReader["type"], (byte)listReader["level"]))
                                              {
                                                      MaxHp = (ushort)listReader["max_hp"],
                                                      Atk = (ushort)listReader["attack"],
                                                      Splash = (byte)listReader["splash"],
                                                      Def = (ushort)listReader["defense"],
                                                      Rng = (byte)listReader["range"],
                                                      Stl = (byte)listReader["stealth"],
                                                      Spd = (byte)listReader["speed"],
                                                      Carry = (ushort)listReader["carry"]
                                              };

                            stub.Template.DbLoaderAdd(battleStats);
                        }
                    }
                }
            }

            #endregion
        }

        private static void LoadTroops(IDbManager dbManager)
        {
            #region Troops

            Global.Logger.Info("Loading troops...");
            using (var reader = dbManager.Select(TroopObject.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);
                    TroopStub stub = (byte)reader["troop_stub_id"] != 0 ? city.Troops[(byte)reader["troop_stub_id"]] : null;
                    var obj = new TroopObject(stub)
                              {
                                      X = (uint)reader["x"],
                                      Y = (uint)reader["y"],
                                      TargetX = (uint)reader["target_x"],
                                      TargetY = (uint)reader["target_y"],
                                      ObjectId = (uint)reader["id"],
                                      DbPersisted = true,
                                      State = {Type = (ObjectState)((byte)reader["state"])},
                                      Stats =
                                              new TroopStats((int)reader["attack_point"],
                                                             (byte)reader["attack_radius"],
                                                             (byte)reader["speed"],
                                                             (short)reader["stamina"],
                                                             new Resource((int)reader["crop"], (int)reader["gold"], (int)reader["iron"], (int)reader["wood"], 0)),
                                      IsBlocked = (bool)reader["is_blocked"],
                                      InWorld = (bool)reader["in_world"],
                              };

                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                        obj.State.Parameters.Add(variable);

                    city.Add(obj.ObjectId, obj, false);

                    if (obj.InWorld)
                        Global.World.DbLoaderAdd(obj);
                }
            }

            #endregion
        }

        private static void LoadBattleManagers(IDbManager dbManager)
        {
            #region Battle Managers

            Global.Logger.Info("Loading battles...");
            using (var reader = dbManager.Select(BattleManager.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    var bm = new BattleManager(city);
                    city.Battle = bm;
                    bm.DbPersisted = true;
                    bm.BattleId = (uint)reader["battle_id"];
                    bm.BattleStarted = (bool)reader["battle_started"];
                    bm.Round = (uint)reader["round"];
                    bm.Turn = (uint)reader["round"];

                    bm.BattleReport.ReportFlag = (bool)reader["report_flag"];
                    bm.BattleReport.ReportStarted = (bool)reader["report_started"];
                    bm.BattleReport.ReportId = (uint)reader["report_id"];

                    using (DbDataReader listReader = dbManager.SelectList(CombatStructure.DB_TABLE, new DbColumn("city_id", city.Id, DbType.UInt32)))
                    {
                        while (listReader.Read())
                        {
                            City structureCity;
                            Global.World.TryGetObjects((uint)listReader["structure_city_id"], out structureCity);

                            var structure = (Structure)structureCity[(uint)listReader["structure_id"]];

                            //First we load the BaseBattleStats and pass it into the BattleStats
                            //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                            var battleStats = new BattleStats(structure.Stats.Base.Battle)
                                              {
                                                      MaxHp = (ushort)listReader["max_hp"],
                                                      Atk = (ushort)listReader["attack"],
                                                      Splash = (byte)listReader["splash"],
                                                      Def = (ushort)listReader["defense"],
                                                      Rng = (byte)listReader["range"],
                                                      Stl = (byte)listReader["stealth"],
                                                      Spd = (byte)listReader["speed"],                                                      
                                              };

                            var combatStructure = new CombatStructure(bm,
                                                                      structure,
                                                                      battleStats,
                                                                      (uint)listReader["hp"],
                                                                      (ushort)listReader["type"],
                                                                      (byte)listReader["level"])
                                                  {
                                                          GroupId = (uint)listReader["group_id"],
                                                          DmgDealt = (int)listReader["damage_dealt"],
                                                          DmgRecv = (int)listReader["damage_received"],
                                                          LastRound = (uint)listReader["last_round"],
                                                          RoundsParticipated = (int)listReader["rounds_participated"],
                                                          DbPersisted = true
                                                  };

                            bm.DbLoaderAddToLocal(combatStructure, (uint)listReader["id"]);
                        }
                    }

                    //this will load both defense/attack units (they are saved to same table)
                    using (DbDataReader listReader = dbManager.SelectList(DefenseCombatUnit.DB_TABLE, new DbColumn("city_id", city.Id, DbType.UInt32)))
                    {
                        while (listReader.Read())
                        {
                            City troopStubCity;
                            Global.World.TryGetObjects((uint)listReader["troop_stub_city_id"], out troopStubCity);
                            TroopStub troopStub = troopStubCity.Troops[(byte)listReader["troop_stub_id"]];

                            CombatObject combatObj;
                            if ((bool)listReader["is_local"])
                            {
                                combatObj = new DefenseCombatUnit(bm,
                                                                  troopStub,
                                                                  (FormationType)((byte)listReader["formation_type"]),
                                                                  (ushort)listReader["type"],
                                                                  (byte)listReader["level"],
                                                                  (ushort)listReader["count"],
                                                                  (ushort)listReader["left_over_hp"]);
                            }
                            else
                            {
                                combatObj = new AttackCombatUnit(bm,
                                                                 troopStub,
                                                                 (FormationType)((byte)listReader["formation_type"]),
                                                                 (ushort)listReader["type"],
                                                                 (byte)listReader["level"],
                                                                 (ushort)listReader["count"],
                                                                 (ushort)listReader["left_over_hp"],
                                                                 new Resource((int)listReader["loot_crop"],
                                                                              (int)listReader["loot_gold"],
                                                                              (int)listReader["loot_iron"],
                                                                              (int)listReader["loot_wood"],
                                                                              (int)listReader["loot_labor"]));
                            }

                            combatObj.MinDmgDealt = (ushort)listReader["damage_min_dealt"];
                            combatObj.MaxDmgDealt = (ushort)listReader["damage_max_dealt"];
                            combatObj.MinDmgRecv = (ushort)listReader["damage_min_received"];
                            combatObj.MinDmgDealt = (ushort)listReader["damage_max_received"];
                            combatObj.HitDealt = (ushort)listReader["hits_dealt"];
                            combatObj.HitDealtByUnit = (uint)listReader["hits_dealt_by_unit"];
                            combatObj.HitRecv = (ushort)listReader["hits_received"];
                            combatObj.GroupId = (uint)listReader["group_id"];
                            combatObj.DmgDealt = (int)listReader["damage_dealt"];
                            combatObj.DmgRecv = (int)listReader["damage_received"];
                            combatObj.LastRound = (uint)listReader["last_round"];
                            combatObj.RoundsParticipated = (int)listReader["rounds_participated"];
                            combatObj.DbPersisted = true;

                            bm.DbLoaderAddToCombatList(combatObj, (uint)listReader["id"], (bool)listReader["is_local"]);
                        }
                    }

                    bm.ReportedTroops.DbPersisted = true;
                    using (DbDataReader listReader = dbManager.SelectList(bm.ReportedTroops))
                    {
                        while (listReader.Read())
                        {
                            City troopStubCity;
                            Global.World.TryGetObjects((uint)listReader["troop_stub_city_id"], out troopStubCity);
                            TroopStub troopStub = troopStubCity.Troops[(byte)listReader["troop_stub_id"]];

                            if (troopStub == null)
                                continue;

                            bm.ReportedTroops[troopStub] = (uint)listReader["combat_troop_id"];
                        }
                    }

                    bm.ReportedObjects.DbPersisted = true;
                    using (DbDataReader listReader = dbManager.SelectList(bm.ReportedObjects))
                    {
                        while (listReader.Read())
                        {
                            CombatObject co = bm.GetCombatObject((uint)listReader["combat_object_id"]);

                            if (co == null)
                                continue;

                            bm.ReportedObjects.Add(co);
                        }
                    }

                    bm.RefreshBattleOrder();
                }
            }

            #endregion
        }

        private static void LoadActions(IDbManager dbManager, TimeSpan downTime)
        {
            #region Active Actions

            Global.Logger.Info("Loading active actions...");
            var types = new[]
                        {
                                typeof(uint), typeof(DateTime), typeof(DateTime), typeof(DateTime), typeof(int), typeof(byte), typeof(ushort),
                                typeof(Dictionary<string, string>)
                        };

            using (var reader = dbManager.Select(ActiveAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "Action", true, true);
                    ConstructorInfo cInfo = type.GetConstructor(types);

                    DateTime beginTime = DateTime.SpecifyKind((DateTime)reader["begin_time"], DateTimeKind.Utc).Add(downTime);

                    DateTime nextTime = DateTime.SpecifyKind((DateTime)reader["next_time"], DateTimeKind.Utc);
                    if (nextTime != DateTime.MinValue)
                        nextTime = nextTime.Add(downTime);

                    DateTime endTime = DateTime.SpecifyKind((DateTime)reader["end_time"], DateTimeKind.Utc).Add(downTime);

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);
                    var parms = new object[]
                                {
                                        (uint)reader["id"], beginTime, nextTime, endTime, (int)reader["worker_type"], (byte)reader["worker_index"],
                                        (ushort)reader["count"], properties
                                };

                    var action = (ScheduledActiveAction)cInfo.Invoke(parms);
                    action.DbPersisted = true;

                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    var workerId = (uint)reader["object_id"];
                    if (workerId == 0)
                        action.WorkerObject = city;
                    else
                        action.WorkerObject = city[(uint)reader["object_id"]];

                    city.Worker.DbLoaderDoActive(action);

                    Global.DbManager.Save(action);
                }
            }

            #endregion

            #region Passive Actions

            Global.Logger.Info("Loading passive actions...");
            var chainActions = new Dictionary<uint, List<PassiveAction>>();
            //this will hold chain actions that we encounter for the next phase

            types = new[] {typeof(uint), typeof(bool), typeof(Dictionary<string, string>)};

            var scheduledTypes = new[] {typeof(ushort), typeof(DateTime), typeof(DateTime), typeof(DateTime), typeof(bool), typeof(string), typeof(Dictionary<string, string>)};

            using (var reader = dbManager.Select(PassiveAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "Action", true, true);

                    ConstructorInfo cInfo;

                    if ((bool)reader["is_scheduled"])
                        cInfo = type.GetConstructor(scheduledTypes);
                    else
                        cInfo = type.GetConstructor(types);

                    if (cInfo == null)
                        throw new Exception(type.Name + " is missing the db loader constructor");

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);

                    object[] parms;

                    if ((bool)reader["is_scheduled"])
                    {
                        DateTime beginTime = DateTime.SpecifyKind((DateTime)reader["begin_time"], DateTimeKind.Utc);
                        beginTime = beginTime.Add(downTime);

                        DateTime nextTime = DateTime.SpecifyKind((DateTime)reader["next_time"], DateTimeKind.Utc);
                        if (nextTime != DateTime.MinValue)
                            nextTime = nextTime.Add(downTime);

                        DateTime endTime = DateTime.SpecifyKind((DateTime)reader["end_time"], DateTimeKind.Utc);
                        endTime = endTime.Add(downTime);

                        string nlsDescription = DBNull.Value.Equals(reader["nls_description"]) ? string.Empty : (string)reader["nls_description"];

                        parms = new object[] {(uint)reader["id"], beginTime, nextTime, endTime, (bool)reader["is_visible"], nlsDescription, properties};
                    }
                    else
                        parms = new object[] {(uint)reader["id"], (bool)reader["is_visible"], properties};

                    var action = (PassiveAction)cInfo.Invoke(parms);
                    action.DbPersisted = true;

                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    var workerId = (uint)reader["object_id"];
                    if (workerId == 0)
                        action.WorkerObject = city;
                    else
                        action.WorkerObject = city[workerId];

                    if ((bool)reader["is_chain"] == false)
                        city.Worker.DbLoaderDoPassive(action);
                    else
                    {
                        List<PassiveAction> chainList;
                        if (!chainActions.TryGetValue(city.Id, out chainList))
                        {
                            chainList = new List<PassiveAction>();
                            chainActions[city.Id] = chainList;
                        }

                        action.IsChain = true;

                        city.Worker.DbLoaderDoPassive(action);

                        chainList.Add(action);
                    }

                    // Resave city to update times
                    Global.DbManager.Save(action);
                }
            }

            #endregion

            #region Chain Actions

            Global.Logger.Info("Loading chain actions...");
            types = new[] {typeof(uint), typeof(string), typeof(PassiveAction), typeof(ActionState), typeof(bool), typeof(Dictionary<string, string>)};

            using (var reader = dbManager.Select(ChainAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType + "Action", true, true);
                    ConstructorInfo cInfo = type.GetConstructor(types);

                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    var currentActionId = DBNull.Value.Equals(reader["current_action_id"]) ? 0 : (uint)reader["current_action_id"];

                    List<PassiveAction> chainList;
                    PassiveAction currentAction = null;
                    //current action might be null if it has already completed and we are in the call chain part of the cycle
                    if (chainActions.TryGetValue(city.Id, out chainList))
                        currentAction = chainList.Find(lookupAction => lookupAction.ActionId == currentActionId);

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);
                    var parms = new[]
                                {
                                        (uint)reader["id"], reader["chain_callback"], currentAction, (ActionState)((byte)reader["chain_state"]),
                                        (bool)reader["is_visible"], properties
                                };

                    var action = (ChainAction)cInfo.Invoke(parms);
                    action.DbPersisted = true;

                    var workerId = (uint)reader["object_id"];
                    if (workerId == 0)
                        action.WorkerObject = city;
                    else
                        action.WorkerObject = city[(uint)reader["object_id"]];

                    city.Worker.DbLoaderDoPassive(action);

                    Global.DbManager.Save(action);
                }
            }

            #endregion
        }

        private static void LoadActionReferences(IDbManager dbManager)
        {
            #region Action References

            Global.Logger.Info("Loading action references...");
            using (var reader = dbManager.Select(ReferenceStub.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    GameAction action;
                    if ((bool)reader["is_active"])
                        action = city.Worker.ActiveActions[(uint)reader["action_id"]];
                    else
                        action = city.Worker.PassiveActions[(uint)reader["action_id"]];

                    ICanDo obj;
                    var workerId = (uint)reader["object_id"];
                    if (workerId == 0)
                        obj = city;
                    else
                        obj = city[(uint)reader["object_id"]];

                    var referenceStub = new ReferenceStub((ushort)reader["id"], obj, action) {DbPersisted = true};

                    city.Worker.References.DbLoaderAdd(referenceStub);
                }
            }

            #endregion
        }

        private static void LoadActionNotifications(IDbManager dbManager)
        {
            #region Action Notifications

            Global.Logger.Info("Loading action notifications...");
            using (var reader = dbManager.Select(NotificationManager.Notification.DB_TABLE))
            {
                while (reader.Read())
                {
                    City city;
                    Global.World.TryGetObjects((uint)reader["city_id"], out city);

                    GameObject obj = city[(uint)reader["object_id"]];
                    PassiveAction action = city.Worker.PassiveActions[(uint)reader["action_id"]];

                    var notification = new NotificationManager.Notification(obj, action);

                    using (DbDataReader listReader = dbManager.SelectList(notification))
                    {
                        while (listReader.Read())
                        {
                            City subscriptionCity;
                            Global.World.TryGetObjects((uint)listReader["subscription_city_id"], out subscriptionCity);
                            notification.Subscriptions.Add(subscriptionCity);
                        }
                    }

                    city.Worker.Notifications.DbLoaderAdd(false, notification);

                    notification.DbPersisted = true;
                }
            }

            #endregion
        }
    }
}