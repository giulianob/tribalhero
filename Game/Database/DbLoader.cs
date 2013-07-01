#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Forest;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Actions.ResourceActions;
using Game.Logic.Formulas;
using Game.Logic.Notifications;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Logging;
using Persistance;
using System.Linq;
using DbTransaction = Persistance.DbTransaction;
using JsonReader = JsonFx.Json.JsonReader;

#endregion

namespace Game.Database
{
    public class DbLoader
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        [Inject]
        public IKernel Kernel { get; set; }

        [Inject]
        public IWorld World { get; set; }

        [Inject]
        public IDbManager DbManager { get; set; }

        [Inject]
        public ITribeManager Tribes { get; set; }

        [Inject]
        public DbLoaderActionFactory DbLoaderActionFactory { get; set; }

        [Inject]
        public IActionFactory ActionFactory { get; set; }

        [Inject]
        public IBarbarianTribeFactory BarbarianTribeFactory { get; set; }

        [Inject]
        public IBattleManagerFactory BattleManagerFactory { get; set; }

        [Inject]
        public IBarbarianTribeManager BarbarianTribeManager { get; set; }

        [Inject]
        public ITribeFactory TribeFactory { get; set; }

        [Inject]
        public ICombatUnitFactory CombatUnitFactory { get; set; }

        [Inject]
        public IStrongholdManager StrongholdManager { get; set; }

        [Inject]
        public IStrongholdFactory StrongholdFactory { get; set; }

        [Inject]
        public Procedure Procedure { get; set; }

        [Inject]
        public ICombatGroupFactory CombatGroupFactory { get; set; }

        [Inject]
        public IForestFactory ForestFactory { get; set; }

        public void LoadFromDatabase()
        {
            SystemVariablesUpdater.Current.Pause();
            Scheduler.Current.Pause();
            Global.Current.FireEvents = false;

            logger.Info("Loading database...");

            using (DbTransaction transaction = DbManager.GetThreadTransaction())
            {
                try
                {
                    // Check Schema
                    CheckSchemaVersion();

                    // Set all players to offline
                    DbManager.Query("UPDATE `players` SET `online` = 0");

                    LoadSystemVariables();
                    UpdateTimestampsFromDowntime((DateTime)Global.Current.SystemVariables["System.time"].Value);

                    LoadReportIds();
                    LoadMarket();
                    LoadPlayers();
                    LoadAchievements();
                    LoadCities();
                    LoadTribes();
                    LoadTribesmen();
                    LoadUnitTemplates();
                    LoadStructures();
                    LoadStructureProperties();
                    LoadTechnologies();
                    LoadForests();
                    LoadStrongholds();
                    LoadBarbarianTribes();
                    LoadTroopStubs();
                    LoadTroopStubTemplates();
                    LoadTroops();
                    LoadBattleManagers();
                    LoadActions();
                    LoadActionReferences();
                    LoadActionNotifications();
                    LoadAssignments();

                    World.AfterDbLoaded(Procedure, Ioc.Kernel.Get<IForestManager>());

                    //Ok data all loaded. We can get the system going now.
                    Global.Current.SystemVariables["System.time"].Value = DateTime.UtcNow;
                    DbManager.Save(Global.Current.SystemVariables["System.time"]);
                }
                catch(Exception)
                {                    
                    transaction.Rollback();
                    throw;
                }
            }

            logger.Info("Database loading finished");

            SystemVariablesUpdater.Current.Resume();
            Global.Current.FireEvents = true;
            Scheduler.Current.Resume();            
        }

        private void CheckSchemaVersion()
        {
            using (var reader = DbManager.ReaderQuery(@"SELECT max(`version`) as max_version FROM `schema_migrations`"))
            {
                reader.Read();
                string currentDbVersion = (string)reader["max_version"];

                if (currentDbVersion != Config.database_schema_version)
                {
                    throw new Exception(
                            string.Format(
                                          "Expected schema to be version {0} but found version {1}. Execute 'SELECT max(version) FROM `schema_migrations`' to get the latest version and update the config.",
                                          Config.database_schema_version,
                                          currentDbVersion));
                }
            }
        }

        private uint GetMaxId(string table, string column = "id")
        {
            using (var reader = DbManager.ReaderQuery(string.Format("SELECT max(`{1}`) FROM `{0}`", table, column)))
            {
                reader.Read();
                if (DBNull.Value.Equals(reader[0]))
                {
                    return 0;
                }

                return (uint)reader[0];
            }
        }

        private void UpdateTimestampsFromDowntime(DateTime serverTime)
        {
            // Calculate how long server was down
            TimeSpan downTime = SystemClock.Now.Subtract(serverTime);
            if (downTime.TotalMilliseconds < 0)
            {
                return;
            }

            logger.Info(string.Format("Server was down for {0}", downTime));
            
            Action<string, string[]> pushTime = (table, columns) =>
                {
                    foreach (var column in columns)
                    {
                        var query = string.Format("UPDATE `{0}` SET `{1}` = DATE_ADD(`{1}`, INTERVAL {2} SECOND) WHERE `{1}` > '0001-01-01 00:00:00'",
                                                  table,
                                                  column,
                                                  downTime.TotalSeconds.ToString("0"));

                        DbManager.Query(query, new DbColumn[] {});
                    }
                };

            // Update all the timestamps
            pushTime(City.DB_TABLE, new[] {"crop_realize_time", "wood_realize_time", "iron_realize_time", "labor_realize_time", "gold_realize_time"});
            pushTime(Forest.DB_TABLE, new[] {"deplete_time", "last_realize_time"});
            pushTime(ActiveAction.DB_TABLE, new[] {"begin_time", "next_time", "end_time"});
            pushTime(PassiveAction.DB_TABLE, new[] {"begin_time", "next_time", "end_time"});
            pushTime(Assignment.DB_TABLE, new[] {"attack_time"});
        }

        private void LoadReportIds()
        {
            BattleReport.BattleIdGenerator.Set(Math.Max(GetMaxId(BattleManager.DB_TABLE, "battle_id"), GetMaxId(SqlBattleReportWriter.BATTLE_DB)));
            BattleReport.ReportIdGenerator.Set(GetMaxId(SqlBattleReportWriter.BATTLE_REPORTS_DB));
            BattleReport.BattleTroopIdGenerator.Set(GetMaxId(SqlBattleReportWriter.BATTLE_REPORT_TROOPS_DB));
        }

        private void LoadTribes()
        {
            #region Tribes

            logger.Info("Loading tribes...");
            using (var reader = DbManager.Select(Tribe.DB_TABLE))
            {
                while (reader.Read())
                {
                    if ((bool)reader["deleted"])
                    {
                        Tribes.DbLoaderSetIdUsed((uint)reader["id"]);
                        continue;
                    }

                    var resource = new Resource((int)reader["crop"], (int)reader["gold"], (int)reader["iron"], (int)reader["wood"]);

                    var tribe = TribeFactory.CreateTribe(World.Players[(uint)reader["owner_player_id"]],
                                                         (string)reader["name"],
                                                         (string)reader["desc"],
                                                         (byte)reader["level"],
                                                         (decimal)reader["victory_point"],
                                                         (int)reader["attack_point"],
                                                         (int)reader["defense_point"],
                                                         resource,
                                                         DateTime.SpecifyKind((DateTime)reader["created"], DateTimeKind.Utc));
                    
                    foreach (var obj in JsonConvert.DeserializeObject<TribeRank[]>((string)reader["ranks"]))
                    {
                        tribe.CreateRank(obj.Id, obj.Name, obj.Permission);
                    }

                    tribe.LeavingTribesmates.AddRange(JsonConvert.DeserializeObject<LeavingTribesmate[]>((string)reader["leaving_tribesmates"]));

                    foreach (var obj in JsonConvert.DeserializeObject<TribeRank[]>((string)reader["ranks"]))
                    {
                        tribe.CreateRank(obj.Id, obj.Name, obj.Permission);
                    }

                    tribe.Id = (uint)reader["id"];                    
                    tribe.PublicDescription = (string)reader["public_desc"];

                    tribe.DbPersisted = true;
                    Tribes.DbLoaderAdd(tribe);
                }
            }

            #endregion
        }

        private void LoadTribesmen()
        {
            #region Tribes

            logger.Info("Loading tribesmen...");
            using (var reader = DbManager.Select(Tribesman.DB_TABLE))
            {
                while (reader.Read())
                {
                    ITribe tribe;
                    World.TryGetObjects((uint)reader["tribe_id"], out tribe);
                    var contribution = new Resource((int)reader["crop"], (int)reader["gold"], (int)reader["iron"], (int)reader["wood"]);

                    var tribesman = new Tribesman(tribe,
                                                  World.Players[(uint)reader["player_id"]],
                                                  DateTime.SpecifyKind((DateTime)reader["join_date"], DateTimeKind.Utc),
                                                  contribution,
                                                  tribe.Ranks.First(x=>x.Id==(byte)reader["rank"])) {DbPersisted = true};
                    tribe.DbLoaderAdd(tribesman);
                }
            }

            #endregion
        }

        private void LoadAssignments()
        {
            #region Assignments

            IAssignmentFactory assignmentFactory = Kernel.Get<IAssignmentFactory>();

            logger.Info("Loading assignments...");

            ILookup<int, dynamic> stubLookup;
            using (var listReader = DbManager.SelectList(Assignment.DB_TABLE))
            {
                stubLookup = ReaderToLookUp(listReader,
                                            reader => new {
                                                Id = (int)reader["id"],
                                                CityId = (uint)reader["city_id"],
                                                StubId = (ushort)reader["stub_id"],
                                                Dispatched = (byte)reader["dispatched"] == 1
                                            },
                                            key => (int)key.Id);
            }

            foreach (var reader in DbManager.Select(Assignment.DB_TABLE).ReadAll())
            {
                ITribe tribe;
                World.TryGetObjects((uint)reader["tribe_id"], out tribe);

                var location = new SimpleLocation((LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true),
                                                  (uint)reader["location_id"]);

                var attackMode = (AttackMode)Enum.Parse(typeof(AttackMode), (string)reader["mode"]);

                DateTime targetTime = DateTime.SpecifyKind((DateTime)reader["attack_time"], DateTimeKind.Utc);

                var id = (int)reader["id"];
                Assignment assignment = assignmentFactory.CreateAssignmentFromDb(id,
                                                                                 tribe,
                                                                                 (uint)reader["x"],
                                                                                 (uint)reader["y"],
                                                                                 location,
                                                                                 attackMode,
                                                                                 targetTime,
                                                                                 (uint)reader["dispatch_count"],
                                                                                 (string)reader["description"],
                                                                                 ((byte)reader["is_attack"]) == 1);

                foreach (var stub in stubLookup[id])
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)stub.CityId, out city))
                    {
                        throw new Exception("City not found");
                    }

                    ITroopStub assignmentStub;
                    if (!city.Troops.TryGetStub((ushort)stub.StubId, out assignmentStub))
                    {
                        throw new Exception("Stub not found");
                    }

                    assignment.DbLoaderAdd(assignmentStub, stub.Dispatched);
                }

                assignment.DbPersisted = true;

                // Add assignment to tribe
                tribe.DbLoaderAddAssignment(assignment);

                // Reschedule assignment
                assignment.Reschedule();
            }

            #endregion
        }

        private void LoadSystemVariables()
        {
            #region System variables

            logger.Info("Loading system variables...");
            using (var reader = DbManager.Select(SystemVariable.DB_TABLE))
            {
                while (reader.Read())
                {
                    var systemVariable = new SystemVariable((string)reader["name"],
                                                            DataTypeSerializer.Deserialize((string)reader["value"],
                                                                                           (byte)reader["datatype"]))
                    {
                            DbPersisted = true
                    };
                    Global.Current.SystemVariables.Add(systemVariable.Key, systemVariable);
                }
            }

            // Set system variable defaults
            if (!Global.Current.SystemVariables.ContainsKey("System.time"))
            {
                Global.Current.SystemVariables.Add("System.time", new SystemVariable("System.time", DateTime.UtcNow));
            }

            if (!Global.Current.SystemVariables.ContainsKey("Map.start_index"))
            {
                Global.Current.SystemVariables.Add("Map.start_index", new SystemVariable("Map.start_index", 0));
            }

            if (!Global.Current.SystemVariables.ContainsKey("Server.date"))
            {
                Global.Current.SystemVariables.Add("Server.date", new SystemVariable("Server.date", DateTime.UtcNow));
                DbManager.Save(Global.Current.SystemVariables["Server.date"]);
            }
            #endregion
        }

        private void LoadMarket()
        {
            #region Market

            logger.Info("Loading market...");
            using (var reader = DbManager.Select(Market.DB_TABLE))
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

        private void LoadPlayers()
        {
            #region Players

            logger.Info("Loading players...");
            using (var reader = DbManager.Select(Player.DB_TABLE))
            {
                while (reader.Read())
                {
                    var player = new Player((uint)reader["id"],
                                            DateTime.SpecifyKind((DateTime)reader["created"], DateTimeKind.Utc),
                                            DateTime.SpecifyKind((DateTime)reader["last_login"], DateTimeKind.Utc),
                                            (string)reader["name"],
                                            (string)reader["description"],
                                            PlayerRights.Basic)
                    {
                            DbPersisted = true,
                            TutorialStep = (uint)reader["tutorial_step"],
                            TribeRequest = (uint)reader["invitation_tribe_id"],
                            Muted = DateTime.SpecifyKind((DateTime)reader["muted"], DateTimeKind.Utc),
                            LastDeletedTribe = DateTime.SpecifyKind((DateTime)reader["last_deleted_tribe"], DateTimeKind.Utc),
                            Banned = (bool)reader["banned"]
                    };

                    if (!World.Players.TryAdd(player.PlayerId, player))
                    {
                        throw new Exception("Failed to load players");
                    }
                }
            }

            #endregion
        }

        private void LoadAchievements()
        {
            logger.Info("Loading achievements...");
            using (var reader = DbManager.Select(AchievementList.DB_TABLE))
            {
                while (reader.Read())
                {
                    IPlayer player;

                    if (!World.Players.TryGetValue((uint)reader["player_id"], out player))
                    {
                        throw new Exception("Player not found");
                    }

                    player.Achievements.DbPersisted = true;
                }
            }

            using (var reader = DbManager.SelectList(AchievementList.DB_TABLE))
            {
                while (reader.Read())
                {
                    IPlayer player;

                    if (!World.Players.TryGetValue((uint)reader["player_id"], out player))
                    {
                        throw new Exception("Player not found");
                    }

                    player.Achievements.Add(new Achievement
                    {
                            Id = (int)reader["id"],
                            Tier = (AchievementTier)((byte)reader["tier"]),
                            Title = (string)reader["title"],
                            Description = (string)reader["description"],
                            Icon = (string)reader["icon"],
                            Type = (string)reader["type"]
                    });                    
                }
            }
        }

        private void LoadCities()
        {
            #region Cities

            var cityFactory = Kernel.Get<ICityFactory>();

            logger.Info("Loading cities...");
            using (var reader = DbManager.Select(City.DB_TABLE))
            {
                while (reader.Read())
                {
                    DateTime cropRealizeTime = DateTime.SpecifyKind((DateTime)reader["crop_realize_time"], DateTimeKind.Utc);
                    DateTime woodRealizeTime = DateTime.SpecifyKind((DateTime)reader["wood_realize_time"], DateTimeKind.Utc);
                    DateTime ironRealizeTime = DateTime.SpecifyKind((DateTime)reader["iron_realize_time"], DateTimeKind.Utc);
                    DateTime laborRealizeTime = DateTime.SpecifyKind((DateTime)reader["labor_realize_time"], DateTimeKind.Utc);
                    DateTime goldRealizeTime = DateTime.SpecifyKind((DateTime)reader["gold_realize_time"], DateTimeKind.Utc);

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

                    ICity city = cityFactory.CreateCity((uint)reader["id"],
                                                        World.Players[(uint)reader["player_id"]],
                                                        (string)reader["name"],
                                                        resource,
                                                        (byte)reader["radius"],
                                                        (decimal)reader["alignment_point"]);

                    city.DbPersisted = true;
                    city.LootStolen = (uint)reader["loot_stolen"];
                    city.AttackPoint = (int)reader["attack_point"];
                    city.DefensePoint = (int)reader["defense_point"];
                    city.HideNewUnits = (bool)reader["hide_new_units"];
                    city.Value = (ushort)reader["value"];
                    city.Deleted = (City.DeletedState)reader["deleted"];

                    // Add to world
                    World.Cities.DbLoaderAdd(city);

                    // Restart city remover if needed
                    switch(city.Deleted)
                    {
                        case City.DeletedState.Deleting:
                            city.Owner.Add(city);
                            CityRemover cr = Kernel.Get<ICityRemoverFactory>().CreateCityRemover(city.Id);
                            cr.Start(true);
                            break;
                        case City.DeletedState.NotDeleted:
                            city.Owner.Add(city);
                            break;
                    }
                }
            }

            #endregion
        }

        private void LoadStrongholds()
        {
            #region Strongholds

            logger.Info("Loading strongholds...");
            using (var reader = DbManager.Select(Stronghold.DB_TABLE))
            {
                while (reader.Read())
                {
                    var stronghold = StrongholdFactory.CreateStronghold((uint)reader["id"],
                                                                        (string)reader["name"],
                                                                        (byte)reader["level"],
                                                                        (uint)reader["x"],
                                                                        (uint)reader["y"],
                                                                        (decimal)reader["gate"]);
                    stronghold.StrongholdState = (StrongholdState)((byte)reader["state"]);
                    stronghold.NearbyCitiesCount = (ushort)reader["nearby_cities"];
                    stronghold.DbPersisted = true;
                    stronghold.State.Type = (ObjectState)((byte)reader["object_state"]);
                    stronghold.State.Parameters = XmlSerializer.DeserializeList((string)reader["state_parameters"]);                    

                    // Load owner tribe
                    var tribeId = (uint)reader["tribe_id"];
                    ITribe tribe;
                    if (tribeId != 0 && World.TryGetObjects(tribeId, out tribe))
                    {
                        stronghold.Tribe = tribe;
                    }

                    // Load current tribe that knocked down gate
                    var gateOpenToTribeId = (uint)reader["gate_open_to"];
                    ITribe gateOpenToTribe;
                    if (gateOpenToTribeId != 0 && World.TryGetObjects(gateOpenToTribeId, out gateOpenToTribe))
                    {
                        stronghold.GateOpenTo = gateOpenToTribe;
                    }

                    stronghold.DateOccupied = (DateTime)reader["date_occupied"];
                    stronghold.BonusDays = (decimal)reader["bonus_days"];
                    // Add stronghold to main manager
                    StrongholdManager.DbLoaderAdd(stronghold);
                }
            }

            #endregion
        }

        private void LoadBarbarianTribes()
        {
            #region Barbarian Tribes

            logger.Info("Loading barbarian tribes...");
            using (var reader = DbManager.Select(BarbarianTribe.DB_TABLE))
            {
                while (reader.Read())
                {
                    var barbarianTribe = BarbarianTribeFactory.CreateBarbarianTribe((uint)reader["id"],                                                                        
                                                                        (byte)reader["level"],
                                                                        (uint)reader["x"],
                                                                        (uint)reader["y"],
                                                                        (byte)reader["camp_remains"]);

                    barbarianTribe.Resource.Clear();
                    barbarianTribe.Resource.Add(new Resource((int)reader["resource_crop"],
                                                             (int)reader["resource_gold"],
                                                             (int)reader["resource_iron"],
                                                             (int)reader["resource_wood"]));
                    barbarianTribe.DbPersisted = true;
                    barbarianTribe.LastAttacked = DateTime.SpecifyKind((DateTime)reader["last_attacked"], DateTimeKind.Utc);
                    barbarianTribe.Created = DateTime.SpecifyKind((DateTime)reader["created"], DateTimeKind.Utc);
                    barbarianTribe.InWorld = (bool)reader["in_world"];
                    barbarianTribe.State.Type = (ObjectState)((byte)reader["state"]);
                    barbarianTribe.State.Parameters = XmlSerializer.DeserializeList((string)reader["state_parameters"]);

                    // Add stronghold to main manager
                    BarbarianTribeManager.DbLoaderAdd(barbarianTribe);
                }
            }

            #endregion
        }

        private void LoadUnitTemplates()
        {
            #region Unit Template

            var unitFactory = Ioc.Kernel.Get<UnitFactory>();

            logger.Info("Loading unit template...");

            ILookup<uint, dynamic> unitLookup;
            using (var listReader = DbManager.SelectList(UnitTemplate.DB_TABLE))
            {
                unitLookup = ReaderToLookUp(listReader, 
                                            reader => new {
                                                CityId = (uint)reader["city_id"],
                                                Type = (ushort)reader["type"],
                                                Level = (byte)reader["level"]
                                            }, 
                                            key => (uint)key.CityId);
            }

            using (var reader = DbManager.Select(UnitTemplate.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    var cityId = (uint)reader["city_id"];
                    if (!World.TryGetObjects(cityId, out city))
                    {
                        throw new Exception("City not found");
                    }

                    city.Template.DbPersisted = true;
                    
                    foreach (var unit in unitLookup[cityId])
                    {
                        city.Template.DbLoaderAdd(unit.Type, unitFactory.GetUnitStats(unit.Type, unit.Level));
                    }                    
                }
            }

            #endregion
        }

        private void LoadForests()
        {
            var forestManager = Ioc.Kernel.Get<IForestManager>();
            logger.Info("Loading forests...");
            using (var reader = DbManager.Select(Forest.DB_TABLE))
            {
                while (reader.Read())
                {
                    var forest = ForestFactory.CreateForest((uint)reader["id"],
                                                            (byte)reader["level"],
                                                            (int)reader["capacity"],
                                                            (float)reader["rate"],
                                                            (uint)reader["x"],
                                                            (uint)reader["y"]);

                    forest.DbPersisted = true;
                    forest.Labor = (ushort)reader["labor"];
                    forest.State.Type = (ObjectState)((byte)reader["state"]);
                    forest.Wood = new AggressiveLazyValue((int)reader["lumber"],
                                                          DateTime.SpecifyKind((DateTime)reader["last_realize_time"], DateTimeKind.Utc),
                                                          0,
                                                          (int)reader["upkeep"]) {Limit = (int)reader["capacity"]};
                    forest.DepleteTime = DateTime.SpecifyKind((DateTime)reader["deplete_time"], DateTimeKind.Utc);
                    forest.InWorld = (bool)reader["in_world"];

                    forest.State.Parameters = XmlSerializer.DeserializeList((string)reader["state_parameters"]);

                    // Add lumberjacks
                    foreach (var vars in XmlSerializer.DeserializeComplexList((string)reader["structures"]))
                    {
                        ICity city;
                        if (!World.TryGetObjects((uint)vars[0], out city))
                        {
                            throw new Exception("City not found");
                        }

                        IStructure structure;
                        if (!city.TryGetStructure((uint)vars[1], out structure))
                        {
                            throw new Exception("Structure not found");
                        }

                        forest.AddLumberjack(structure);
                    }

                    if (forest.InWorld)
                    {
                        // Create deplete time
                        forest.DepleteAction = ActionFactory.CreateForestDepleteAction(forest, forest.DepleteTime);
                        Scheduler.Current.Put(forest.DepleteAction);
                        World.Regions.DbLoaderAdd(forest);
                        forestManager.DbLoaderAdd(forest);
                    }
                }
            }
        }

        private void LoadStructures()
        {
            #region Structures

            var gameObjectFactory = Kernel.Get<IGameObjectFactory>();

            logger.Info("Loading structures...");
            ICity city = null;
            foreach (var reader in DbManager.Select(Structure.DB_TABLE).ReadAll())
            {                
                if (city == null || city.Id != (uint)reader["city_id"])
                {
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }
                }

                IStructure structure = gameObjectFactory.CreateStructure((uint)reader["city_id"], 
                    (uint)reader["id"], 
                    (ushort)reader["type"],
                    (byte)reader["level"],
                    (uint)reader["x"],
                    (uint)reader["y"]);
                structure.InWorld = (bool)reader["in_world"];
                structure.Technologies.Parent = city.Technologies;
                structure.Stats.Hp = (decimal)reader["hp"];
                structure.Stats.Labor = (ushort)reader["labor"];
                structure.DbPersisted = true;
                structure.State.Type = (ObjectState)((byte)reader["state"]);
                structure.IsBlocked = (uint)reader["is_blocked"];

                structure.State.Parameters = XmlSerializer.DeserializeList((string)reader["state_parameters"]);

                city.Add(structure.ObjectId, structure, false);

                if (structure.InWorld)
                {
                    World.Regions.DbLoaderAdd(structure);
                }

                structure.Properties.DbPersisted = true;
            }

            #endregion
        }

        private void LoadStructureProperties()
        {
            #region Structure Properties

            logger.Info("Loading structure properties...");
            using (var reader = DbManager.SelectList(StructureProperties.DB_TABLE))
            {
                ICity city = null;
                while (reader.Read())
                {
                    // Simple optimization                        
                    if ((city == null || city.Id != (uint)reader["city_id"]) && !World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }

                    var structure = (IStructure)city[(uint)reader["structure_id"]];

                    structure.Properties.Add(reader["name"], DataTypeSerializer.Deserialize((string)reader["value"], (byte)reader["datatype"]));                    
                }
            }            

            #endregion
        }

        private void LoadTechnologies()
        {
            #region Technologies

            var technologyFactory = Kernel.Get<TechnologyFactory>();
            
            logger.Info("Loading technologies...");

            // Creates a lookup for speed
            ILookup<Tuple<uint, uint, byte>, dynamic> techs;
            
            using (var reader = DbManager.SelectList(TechnologyManager.DB_TABLE))
            {
                techs = ReaderToLookUp(reader, 
                    row => new {
                        cityId = (uint)row["city_id"],
                        ownerId = (uint)row["owner_id"],
                        ownerLocation = (byte)row["owner_location"],
                        type = (uint)row["type"],
                        level = (byte)row["level"]
                    },
                    row => new Tuple<uint, uint, byte>(row.cityId, row.ownerId, row.ownerLocation));
            }

            using (var reader = DbManager.Select(TechnologyManager.DB_TABLE))
            {
                while (reader.Read())
                {
                    var ownerLocation = (EffectLocation)((byte)reader["owner_location"]);
                    var ownerId = (uint)reader["owner_id"];

                    ITechnologyManager manager;

                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }

                    switch(ownerLocation)
                    {
                        case EffectLocation.Object:
                            manager = ((IStructure)city[ownerId]).Technologies;
                            break;
                        case EffectLocation.City:
                            manager = city.Technologies;                            
                            break;
                        default:
                            throw new Exception("Unknown effect location?");
                    }

                    manager.DbPersisted = true;

                    foreach (var tech in techs[new Tuple<uint, uint, byte>(city.Id, ownerId, (byte)ownerLocation)])
                    {
                        manager.Add(technologyFactory.GetTechnology((uint)tech.type, (byte)tech.level), false);
                    }
                }
            }

            #endregion
        }

        private void LoadTroopStubs()
        {
            #region Troop Stubs
            ILookup<Tuple<uint, ushort>, dynamic> troopStubUnits;

            using (var listReader = DbManager.SelectList(TroopStub.DB_TABLE))
            {
                troopStubUnits = ReaderToLookUp(listReader,
                                                reader => new
                                                {
                                                        id = (ushort)reader["id"],
                                                        cityId = (uint)reader["city_id"],
                                                        formationType = (FormationType)((byte)reader["formation_type"]),
                                                        type = (ushort)reader["type"],
                                                        count = (ushort)reader["count"]
                                                },
                                                key => new Tuple<uint, ushort>(key.cityId, key.id));
            }

            List<dynamic> stationedTroops = new List<dynamic>();

            logger.Info("Loading troop stubs...");
            using (var reader = DbManager.Select(TroopStub.DB_TABLE))
            {
                while (reader.Read())
                {
                    ushort id = (ushort)reader["id"];
                    uint cityId = (uint)reader["city_id"];

                    ICity city;
                    if (!World.TryGetObjects(cityId, out city))
                    {
                        throw new Exception("City not found");
                    }
                    
                    var stub = new TroopStub(id, city)
                    {
                            State = (TroopState)Enum.Parse(typeof(TroopState), reader["state"].ToString(), true),
                            DbPersisted = true,
                            InitialCount = (ushort)reader["initial_count"],
                            RetreatCount = (ushort)reader["retreat_count"],
                            AttackMode = (AttackMode)((byte)reader["attack_mode"]),
                    };

                    var formationMask = (ushort)reader["formations"];
                    var formations = (FormationType[])Enum.GetValues(typeof(FormationType));
                    foreach (var type in formations)
                    {
                        if ((formationMask & (ushort)Math.Pow(2, (ushort)type)) != 0)
                        {
                            stub.AddFormation(type);
                        }
                    }

                    foreach (var unit in troopStubUnits[new Tuple<uint, ushort>(cityId, id)])
                    {
                        stub.AddUnit(unit.formationType, unit.type, unit.count);
                    }

                    city.Troops.DbLoaderAdd(id, stub);

                    var stationType = (byte)reader["station_type"];
                    if (stationType != 0)
                    {
                        var stationId = (uint)reader["station_id"];
                        stationedTroops.Add(new {stub, stationType, stationId});
                    }
                }
            }

            foreach (var stubInfo in stationedTroops)
            {
                string stationType = ((LocationType)stubInfo.stationType).ToString();
                uint stationId = stubInfo.stationId;

                IStation station = ResolveLocationAs<IStation>(stationType, stationId);
                station.Troops.DbLoaderAddStation(stubInfo.stub);
            }

            #endregion
        }

        private void LoadTroopStubTemplates()
        {
            #region Troop Stub's Templates

            var unitFactory = Ioc.Kernel.Get<UnitFactory>();

            logger.Info("Loading troop stub templates...");

            ILookup<Tuple<uint, ushort>, dynamic> unitLookup;
            using (var listReader = DbManager.SelectList(TroopTemplate.DB_TABLE))
            {
                unitLookup = ReaderToLookUp(listReader,
                                            reader => new {
                                                TroopStubId = (ushort)reader["troop_stub_id"],
                                                CityId = (uint)reader["city_id"],
                                                Type = (ushort)reader["type"], 
                                                Level = (byte)reader["level"],
                                                MaxHp = (decimal)reader["max_hp"],
                                                Atk = (decimal)reader["attack"],
                                                Splash = (byte)reader["splash"],
                                                Rng = (byte)reader["range"],
                                                Stl = (byte)reader["stealth"],
                                                Spd = (byte)reader["speed"],
                                                Carry = (ushort)reader["carry"],
                                                NormalizedCost = (decimal)reader["normalized_cost"]                                                    
                                            },
                                            key => new Tuple<uint, ushort>(key.CityId, key.TroopStubId));
            }

            using (var reader = DbManager.Select(TroopTemplate.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    var cityId = (uint)reader["city_id"];
                    var troopStubId = (ushort)reader["troop_stub_id"];

                    if (!World.TryGetObjects(cityId, out city))
                    {
                        throw new Exception("City not found");
                    }                    
                    ITroopStub stub = city.Troops[troopStubId];
                    stub.Template.DbPersisted = true;

                    foreach (var unit in unitLookup[new Tuple<uint, ushort>(cityId, troopStubId)])
                    {
                        //First we load the BaseBattleStats and pass it into the BattleStats
                        //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                        var battleStats = new BattleStats(unitFactory.GetBattleStats(unit.Type, unit.Level))
                        {
                                MaxHp = unit.MaxHp,
                                Atk = unit.Atk,
                                Splash = unit.Splash,
                                Rng = unit.Rng,
                                Stl = unit.Stl,
                                Spd = unit.Spd,
                                Carry = unit.Carry,
                                NormalizedCost = unit.NormalizedCost
                        };
                        
                        stub.Template.DbLoaderAdd(battleStats);
                    }
                }
            }

            #endregion
        }

        private void LoadTroops()
        {
            #region Troops

            var gameObjectFactory = Kernel.Get<IGameObjectFactory>();

            logger.Info("Loading troops...");
            using (var reader = DbManager.Select(TroopObject.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }
                    var troopStubid = (ushort)reader["troop_stub_id"];

                    ITroopStub stub = troopStubid != 0
                                              ? city.Troops[troopStubid]
                                              : null;

                    var troopObject = gameObjectFactory.CreateTroopObject((uint)reader["id"],
                                                                          stub,
                                                                          (uint)reader["x"],
                                                                          (uint)reader["y"]);
                    troopObject.TargetX = (uint)reader["target_x"];
                    troopObject.TargetY = (uint)reader["target_y"];
                    troopObject.DbPersisted = true;
                    troopObject.State.Type = (ObjectState)((byte)reader["state"]);
                    troopObject.Stats =
                            new TroopStats((int)reader["attack_point"],
                                           (byte)reader["attack_radius"],
                                           (decimal)reader["speed"],
                                           new Resource((int)reader["crop"], (int)reader["gold"], (int)reader["iron"], (int)reader["wood"]));
                    troopObject.IsBlocked = (uint)reader["is_blocked"];
                    troopObject.InWorld = (bool)reader["in_world"];

                    troopObject.State.Parameters = XmlSerializer.DeserializeList((string)reader["state_parameters"]);

                    city.Add(troopObject.ObjectId, troopObject, false);

                    if (troopObject.InWorld)
                    {
                        World.Regions.DbLoaderAdd(troopObject);
                    }
                }
            }

            #endregion
        }

        private void LoadBattleManagers()
        {
            #region Battle Managers

            logger.Info("Loading battles...");

            List<IBattleManager> battleManagers = new List<IBattleManager>();

            using (var reader = DbManager.Select(BattleManager.DB_TABLE))
            {
                while (reader.Read())
                {
                    IBattleManager battleManager;
                    var battleOwner = new BattleOwner((string)reader["owner_type"], (uint)reader["owner_id"]);
                    var battleLocation = new BattleLocation((string)reader["location_type"], (uint)reader["location_id"]);

                    switch(battleLocation.Type)
                    {
                        case BattleLocationType.City:
                            ICity city;
                            if (!World.TryGetObjects((uint)reader["location_id"], out city))
                            {
                                throw new Exception("City not found");
                            }

                            battleManager = BattleManagerFactory.CreateBattleManager((uint)reader["battle_id"], battleLocation, battleOwner, city);
                            city.Battle = battleManager;
                            break;
                        case BattleLocationType.BarbarianTribe:
                            IBarbarianTribe barbarianTribe;
                            if (!World.TryGetObjects((uint)reader["location_id"], out barbarianTribe))
                            {
                                throw new Exception("Barbarian tribe not found");
                            }

                            battleManager = BattleManagerFactory.CreateBarbarianBattleManager((uint)reader["battle_id"],
                                                                                              battleLocation,
                                                                                              battleOwner,
                                                                                              barbarianTribe);
                            barbarianTribe.Battle = battleManager;
                            break;
                        case BattleLocationType.Stronghold:
                        case BattleLocationType.StrongholdGate:
                            IStronghold stronghold;
                            if (!World.TryGetObjects((uint)reader["location_id"], out stronghold))
                            {
                                throw new Exception("Stronghold not found");
                            }

                            if (battleLocation.Type == BattleLocationType.Stronghold)
                            {
                                battleManager = BattleManagerFactory.CreateStrongholdMainBattleManager((uint)reader["battle_id"],
                                                                                                       battleLocation,
                                                                                                       battleOwner,
                                                                                                       stronghold);
                                stronghold.MainBattle = battleManager;
                            }
                            else
                            {
                                battleManager = BattleManagerFactory.CreateStrongholdGateBattleManager((uint)reader["battle_id"],
                                                                                                       battleLocation,
                                                                                                       battleOwner,
                                                                                                       stronghold);
                                stronghold.GateBattle = battleManager;
                            }
                            break;
                        default:
                            throw new Exception(string.Format("Unknown location type {0} when loading battle manager", battleLocation.Type));
                    }

                    battleManager.DbLoadProperties(new JsonReader().Read<Dictionary<string, object>>((string)reader["properties"]));
                    battleManager.DbPersisted = true;
                    battleManager.BattleStarted = (bool)reader["battle_started"];
                    battleManager.Round = (uint)reader["round"];
                    battleManager.Turn = (uint)reader["round"];
                    battleManager.NextToAttack = (BattleManager.BattleSide)((byte)reader["next_to_attack"]);

                    battleManager.BattleReport.ReportStarted = (bool)reader["report_started"];
                    battleManager.BattleReport.ReportId = (uint)reader["report_id"];
                    battleManager.BattleReport.SnappedImportantEvent = (bool)reader["snapped_important_event"];

                    battleManagers.Add(battleManager);
                }
            }

            foreach (var battleManager in battleManagers)
            {
                // Load combat groups
                using (
                        var listReader = DbManager.SelectList(CityOffensiveCombatGroup.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        ICity combatGroupCity;
                        if (!World.TryGetObjects((uint)listReader["city_id"], out combatGroupCity))
                        {
                            throw new Exception("City not found");
                        }

                        ITroopObject troopObject;
                        if (!combatGroupCity.TryGetTroop((uint)listReader["troop_object_id"], out troopObject))
                        {
                            throw new Exception("Troop object not found");
                        }
                        var cityOffensiveCombatGroup = CombatGroupFactory.CreateCityOffensiveCombatGroup((uint)listReader["battle_id"],
                                                                                                         (uint)listReader["id"],
                                                                                                         troopObject);
                        cityOffensiveCombatGroup.DbPersisted = true;
                        battleManager.DbLoaderAddToCombatList(cityOffensiveCombatGroup, BattleManager.BattleSide.Attack);
                    }
                }

                using (
                        var listReader = DbManager.SelectList(CityDefensiveCombatGroup.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        ICity combatGroupCity;
                        if (!World.TryGetObjects((uint)listReader["city_id"], out combatGroupCity))
                        {
                            throw new Exception("City not found");
                        }

                        ITroopStub troopStub;
                            if (!combatGroupCity.Troops.TryGetStub((ushort)listReader["troop_stub_id"], out troopStub))
                        {
                            throw new Exception("Troop stub not found");
                        }

                        var cityDefensiveCombatGroup = CombatGroupFactory.CreateCityDefensiveCombatGroup((uint)listReader["battle_id"],
                                                                                                         (uint)listReader["id"],
                                                                                                         troopStub);
                        cityDefensiveCombatGroup.DbPersisted = true;
                        battleManager.DbLoaderAddToCombatList(cityDefensiveCombatGroup, BattleManager.BattleSide.Defense);
                    }
                }

                using (
                        var listReader = DbManager.SelectList(BarbarianTribeCombatGroup.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        IBarbarianTribe combatGroupBarbarianTribe;
                        if (!World.TryGetObjects((uint)listReader["barbarian_tribe_id"], out combatGroupBarbarianTribe))
                        {
                            throw new Exception("Barbarian tribe not found");
                        }

                        var barbarianTribeCombatGroup = CombatGroupFactory.CreateBarbarianTribeCombatGroup((uint)listReader["battle_id"],
                                                                                                           (uint)listReader["id"],
                                                                                                           combatGroupBarbarianTribe);
                        barbarianTribeCombatGroup.DbPersisted = true;
                        battleManager.DbLoaderAddToCombatList(barbarianTribeCombatGroup, BattleManager.BattleSide.Defense);
                    }
                }

                using (
                        var listReader = DbManager.SelectList(StrongholdCombatGroup.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        IStronghold combatGroupStronghold;
                        if (!World.TryGetObjects((uint)listReader["stronghold_id"], out combatGroupStronghold))
                        {
                            throw new Exception("Stronghold not found");
                        }

                        var strongholdCombatGroup = CombatGroupFactory.CreateStrongholdCombatGroup((uint)listReader["battle_id"],
                                                                                                   (uint)listReader["id"],
                                                                                                   combatGroupStronghold);
                        strongholdCombatGroup.DbPersisted = true;
                        battleManager.DbLoaderAddToCombatList(strongholdCombatGroup, BattleManager.BattleSide.Defense);
                    }
                }

                // Load combat structures
                using (
                        var listReader = DbManager.SelectList(CombatStructure.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        ICity structureCity;
                        if (!World.TryGetObjects((uint)listReader["structure_city_id"], out structureCity))
                        {
                            throw new Exception("City not found");
                        }

                        var structure = (IStructure)structureCity[(uint)listReader["structure_id"]];

                        //First we load the BaseBattleStats and pass it into the BattleStats
                        //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                        var battleStats = new BattleStats(structure.Stats.Base.Battle)
                        {
                                MaxHp = (decimal)listReader["max_hp"],
                                Atk = (decimal)listReader["attack"],
                                Splash = (byte)listReader["splash"],
                                Rng = (byte)listReader["range"],
                                Stl = (byte)listReader["stealth"],
                                Spd = (byte)listReader["speed"],
                        };

                        var combatStructure = new CombatStructure((uint)listReader["id"],
                                                                  battleManager.BattleId,
                                                                  structure,
                                                                  battleStats,
                                                                  (decimal)listReader["hp"],
                                                                  (ushort)listReader["type"],
                                                                  (byte)listReader["level"],
                                                                      Kernel.Get<Formula>(),
                                                                      Kernel.Get<IActionFactory>(),
                                                                      Kernel.Get<BattleFormulas>())
                        {
                                GroupId = (uint)listReader["group_id"],
                                DmgDealt = (decimal)listReader["damage_dealt"],
                                DmgRecv = (decimal)listReader["damage_received"],
                                LastRound = (uint)listReader["last_round"],
                                RoundsParticipated = (int)listReader["rounds_participated"],
                                DbPersisted = true
                        };

                        battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatStructure, false);
                    }
                }

                // Load attack combat units
                using (
                        var listReader = DbManager.SelectList(AttackCombatUnit.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        ICity troopStubCity;
                        if (!World.TryGetObjects((uint)listReader["troop_stub_city_id"], out troopStubCity))
                        {
                            throw new Exception("City not found");
                        }
                        ITroopObject troopObject = (ITroopObject)troopStubCity[(uint)listReader["troop_object_id"]];

                        ICombatObject combatObj = new AttackCombatUnit((uint)listReader["id"],
                                                                       battleManager.BattleId,
                                                                       troopObject,
                                                                       (FormationType)((byte)listReader["formation_type"]),
                                                                       (ushort)listReader["type"],
                                                                       (byte)listReader["level"],
                                                                       (ushort)listReader["count"],
                                                                       (decimal)listReader["left_over_hp"],
                                                                       new Resource((int)listReader["loot_crop"],
                                                                                    (int)listReader["loot_gold"],
                                                                                    (int)listReader["loot_iron"],
                                                                                    (int)listReader["loot_wood"],
                                                                                    (int)listReader["loot_labor"]),
                                                                           Kernel.Get<UnitFactory>(),
                                                                           Kernel.Get<BattleFormulas>());

                        combatObj.MinDmgDealt = (ushort)listReader["damage_min_dealt"];
                        combatObj.MaxDmgDealt = (ushort)listReader["damage_max_dealt"];
                        combatObj.MinDmgRecv = (ushort)listReader["damage_min_received"];
                        combatObj.MinDmgDealt = (ushort)listReader["damage_max_received"];
                        combatObj.HitDealt = (ushort)listReader["hits_dealt"];
                        combatObj.HitDealtByUnit = (uint)listReader["hits_dealt_by_unit"];
                        combatObj.HitRecv = (ushort)listReader["hits_received"];
                        combatObj.GroupId = (uint)listReader["group_id"];
                        combatObj.DmgDealt = (decimal)listReader["damage_dealt"];
                        combatObj.DmgRecv = (decimal)listReader["damage_received"];
                        combatObj.LastRound = (uint)listReader["last_round"];
                        combatObj.RoundsParticipated = (int)listReader["rounds_participated"];
                        combatObj.DbPersisted = true;

                        battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatObj, false);
                    }
                }

                // Load defense combat units
                using (
                        var listReader = DbManager.SelectList(DefenseCombatUnit.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        ICity troopStubCity;
                        if (!World.TryGetObjects((uint)listReader["troop_stub_city_id"], out troopStubCity))
                        {
                            throw new Exception("City not found");
                        }

                            ITroopStub troopStub = troopStubCity.Troops[(ushort)listReader["troop_stub_id"]];

                        ICombatObject combatObj = new DefenseCombatUnit((uint)listReader["id"],
                                                                        battleManager.BattleId,
                                                                        troopStub,
                                                                        (FormationType)((byte)listReader["formation_type"]),
                                                                        (ushort)listReader["type"],
                                                                        (byte)listReader["level"],
                                                                        (ushort)listReader["count"],
                                                                        (decimal)listReader["left_over_hp"],
                                                                            Kernel.Get<BattleFormulas>());
                        combatObj.MinDmgDealt = (ushort)listReader["damage_min_dealt"];
                        combatObj.MaxDmgDealt = (ushort)listReader["damage_max_dealt"];
                        combatObj.MinDmgRecv = (ushort)listReader["damage_min_received"];
                        combatObj.MinDmgDealt = (ushort)listReader["damage_max_received"];
                        combatObj.HitDealt = (ushort)listReader["hits_dealt"];
                        combatObj.HitDealtByUnit = (uint)listReader["hits_dealt_by_unit"];
                        combatObj.HitRecv = (ushort)listReader["hits_received"];
                        combatObj.GroupId = (uint)listReader["group_id"];
                        combatObj.DmgDealt = (decimal)listReader["damage_dealt"];
                        combatObj.DmgRecv = (decimal)listReader["damage_received"];
                        combatObj.LastRound = (uint)listReader["last_round"];
                        combatObj.RoundsParticipated = (int)listReader["rounds_participated"];
                        combatObj.DbPersisted = true;

                        battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatObj, false);
                    }
                }

                // Load 
                using (
                        var listReader = DbManager.SelectList(BarbarianTribeCombatUnit.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        IBarbarianTribe barbarianTribe;
                        if (!World.TryGetObjects((uint)listReader["barbarian_tribe_id"], out barbarianTribe))
                        {
                            throw new Exception("Stronghold not found");
                        }

                        BarbarianTribeCombatUnit combatObj = CombatUnitFactory.CreateBarbarianTribeCombatUnit((uint)listReader["id"],
                                                                                                              battleManager.BattleId,
                                                                                                              (ushort)listReader["type"],
                                                                                                              (byte)listReader["level"],
                                                                                                              (ushort)listReader["count"],
                                                                                                              barbarianTribe);

                        combatObj.LeftOverHp = (decimal)listReader["left_over_hp"];
                        combatObj.MinDmgDealt = (ushort)listReader["damage_min_dealt"];
                        combatObj.MaxDmgDealt = (ushort)listReader["damage_max_dealt"];
                        combatObj.MinDmgRecv = (ushort)listReader["damage_min_received"];
                        combatObj.MinDmgDealt = (ushort)listReader["damage_max_received"];
                        combatObj.HitDealt = (ushort)listReader["hits_dealt"];
                        combatObj.HitDealtByUnit = (uint)listReader["hits_dealt_by_unit"];
                        combatObj.HitRecv = (ushort)listReader["hits_received"];
                        combatObj.GroupId = (uint)listReader["group_id"];
                        combatObj.DmgDealt = (decimal)listReader["damage_dealt"];
                        combatObj.DmgRecv = (decimal)listReader["damage_received"];
                        combatObj.LastRound = (uint)listReader["last_round"];
                        combatObj.RoundsParticipated = (int)listReader["rounds_participated"];
                        combatObj.DbPersisted = true;

                        battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatObj, false);
                    }
                }

                // Load stronghold combat units
                using (
                        var listReader = DbManager.SelectList(StrongholdCombatUnit.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        IStronghold stronghold;
                        if (!World.TryGetObjects((uint)listReader["stronghold_id"], out stronghold))
                        {
                            throw new Exception("Stronghold not found");
                        }

                        ICombatObject combatObj = new StrongholdCombatUnit((uint)listReader["id"],
                                                                           battleManager.BattleId,
                                                                           (ushort)listReader["type"],
                                                                           (byte)listReader["level"],
                                                                           (ushort)listReader["count"],
                                                                           stronghold,
                                                                           (decimal)listReader["left_over_hp"],
                                                                               Kernel.Get<UnitFactory>(),
                                                                               Kernel.Get<BattleFormulas>(),
                                                                               Kernel.Get<Formula>());

                        combatObj.MinDmgDealt = (ushort)listReader["damage_min_dealt"];
                        combatObj.MaxDmgDealt = (ushort)listReader["damage_max_dealt"];
                        combatObj.MinDmgRecv = (ushort)listReader["damage_min_received"];
                        combatObj.MinDmgDealt = (ushort)listReader["damage_max_received"];
                        combatObj.HitDealt = (ushort)listReader["hits_dealt"];
                        combatObj.HitDealtByUnit = (uint)listReader["hits_dealt_by_unit"];
                        combatObj.HitRecv = (ushort)listReader["hits_received"];
                        combatObj.GroupId = (uint)listReader["group_id"];
                        combatObj.DmgDealt = (decimal)listReader["damage_dealt"];
                        combatObj.DmgRecv = (decimal)listReader["damage_received"];
                        combatObj.LastRound = (uint)listReader["last_round"];
                        combatObj.RoundsParticipated = (int)listReader["rounds_participated"];
                        combatObj.DbPersisted = true;

                        battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatObj, false);
                    }
                }

                using (
                        var listReader = DbManager.SelectList(StrongholdCombatStructure.DB_TABLE,
                                                              new DbColumn("battle_id", battleManager.BattleId, DbType.UInt32)))
                {
                    while (listReader.Read())
                    {
                        IStronghold stronghold;
                        if (!World.TryGetObjects((uint)listReader["stronghold_id"], out stronghold))
                        {
                            throw new Exception("Stronghold not found");
                        }

                        ICombatObject combatObj = new StrongholdCombatGate((uint)listReader["id"],
                                                                           battleManager.BattleId,
                                                                           (ushort)listReader["type"],
                                                                           (byte)listReader["level"],
                                                                           (decimal)listReader["hp"],
                                                                           stronghold,
                                                                               Kernel.Get<IStructureCsvFactory>(),
                                                                               Kernel.Get<BattleFormulas>());

                        combatObj.MinDmgDealt = (ushort)listReader["damage_min_dealt"];
                        combatObj.MaxDmgDealt = (ushort)listReader["damage_max_dealt"];
                        combatObj.MinDmgRecv = (ushort)listReader["damage_min_received"];
                        combatObj.MinDmgDealt = (ushort)listReader["damage_max_received"];
                        combatObj.HitDealt = (ushort)listReader["hits_dealt"];
                        combatObj.HitDealtByUnit = (uint)listReader["hits_dealt_by_unit"];
                        combatObj.HitRecv = (ushort)listReader["hits_received"];
                        combatObj.GroupId = (uint)listReader["group_id"];
                        combatObj.DmgDealt = (decimal)listReader["damage_dealt"];
                        combatObj.DmgRecv = (decimal)listReader["damage_received"];
                        combatObj.LastRound = (uint)listReader["last_round"];
                        combatObj.RoundsParticipated = (int)listReader["rounds_participated"];
                        combatObj.DbPersisted = true;

                        battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatObj, false);
                    }
                }

                // Reported groups
                battleManager.BattleReport.ReportedGroups.DbPersisted = true;
                using (var listReader = DbManager.SelectList(battleManager.BattleReport.ReportedGroups))
                {
                    while (listReader.Read())
                    {
                        // Group may not exist anymore if it was snapped and then exited the battle
                        var group = battleManager.GetCombatGroup((uint)listReader["group_id"]);

                        if (group == null)
                        {
                            continue;
                        }

                        battleManager.BattleReport.ReportedGroups[group] = (uint)listReader["combat_troop_id"];
                    }
                }
                battleManager.DbFinishedLoading();

                World.DbLoaderAdd(battleManager);
            }

            #endregion
        }

        private void LoadActions()
        {
            // Used to help get the proper action worker
            Func<GameAction, uint, LocationType, uint, IActionWorker> resolveWorker =
                    (action, workerId, locationType, locationId) =>
                        {
                            switch(locationType)
                            {
                                case LocationType.City:
                                    ICity city;
                                    if (!World.TryGetObjects(locationId, out city))
                                    {
                                        throw new Exception("City not found");
                                    }

                                    if (action != null)
                                    {
                                        action.WorkerObject = workerId == 0 ? (ICanDo)city : city[workerId];
                                    }

                                    return city.Worker;

                                case LocationType.Stronghold:
                                    IStronghold stronghold;
                                    if (!World.TryGetObjects(locationId, out stronghold))
                                    {
                                        throw new Exception("Stronghold not found");
                                    }

                                    if (action != null)
                                    {
                                        action.WorkerObject = stronghold;
                                    }

                                    return stronghold.Worker;

                                case LocationType.BarbarianTribe:
                                    IBarbarianTribe barbarianTribe;
                                    if (!World.TryGetObjects(locationId, out barbarianTribe))
                                    {
                                        throw new Exception("Barbarian tribe not found");
                                    }

                                    if (action != null)
                                    {
                                        action.WorkerObject = barbarianTribe;
                                    }

                                    return barbarianTribe.Worker;

                                default:
                                    throw new Exception(string.Format("Unknown location type {0} when loading actions",
                                                                      locationType));
                            }
                        };

            #region Active Actions

            logger.Info("Loading active actions...");

            using (var reader = DbManager.Select(ActiveAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    
                    Type type = Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "Action", true, true);

                    DateTime beginTime = DateTime.SpecifyKind((DateTime)reader["begin_time"], DateTimeKind.Utc);
                    DateTime nextTime = DateTime.SpecifyKind((DateTime)reader["next_time"], DateTimeKind.Utc);
                    DateTime endTime = DateTime.SpecifyKind((DateTime)reader["end_time"], DateTimeKind.Utc);

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);

                    var action = DbLoaderActionFactory.CreateScheduledActiveAction(type,
                                                                           (uint)reader["id"],
                                                                           beginTime,
                                                                           nextTime,
                                                                           endTime,
                                                                           (int)reader["worker_type"],
                                                                           (byte)reader["worker_index"],
                                                                           (ushort)reader["count"],
                                                                           properties);
                    action.DbPersisted = true;

                    var locationType = (LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true);
                    var locationId = (uint)reader["location_id"];

                    IActionWorker worker = resolveWorker(action, (uint)reader["object_id"], locationType, locationId);

                    worker.DbLoaderDoActive(action);
                }
            }

            #endregion

            #region Passive Actions

            logger.Info("Loading passive actions...");

            //this will hold chain actions that we encounter for the next phase
            var chainActions = new Dictionary<IActionWorker, List<PassiveAction>>();

            using (var reader = DbManager.Select(PassiveAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "Action", true, true);

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);

                    PassiveAction action;

                    if ((bool)reader["is_scheduled"])
                    {
                        DateTime beginTime = DateTime.SpecifyKind((DateTime)reader["begin_time"], DateTimeKind.Utc);
                        DateTime nextTime = DateTime.SpecifyKind((DateTime)reader["next_time"], DateTimeKind.Utc);
                        DateTime endTime = DateTime.SpecifyKind((DateTime)reader["end_time"], DateTimeKind.Utc);

                        string nlsDescription = DBNull.Value.Equals(reader["nls_description"]) ? string.Empty : (string)reader["nls_description"];

                        action = DbLoaderActionFactory.CreateScheduledPassiveAction(type,
                                                                            (uint)reader["id"],
                                                                            beginTime,
                                                                            nextTime,
                                                                            endTime,
                                                                            (bool)reader["is_visible"],
                                                                            nlsDescription,
                                                                            properties);
                    }
                    else
                    {
                        action = DbLoaderActionFactory.CreatePassiveAction(type,
                                                                   (uint)reader["id"],
                                                                   (bool)reader["is_visible"],
                                                                   properties);
                    }

                    action.DbPersisted = true;

                    var locationType = (LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true);
                    var locationId = (uint)reader["location_id"];

                    IActionWorker worker = resolveWorker(action, (uint)reader["object_id"], locationType, locationId);

                    if ((bool)reader["is_chain"] == false)
                    {
                        worker.DbLoaderDoPassive(action);
                    }
                    else
                    {
                        List<PassiveAction> chainList;
                        if (!chainActions.TryGetValue(worker, out chainList))
                        {
                            chainList = new List<PassiveAction>();
                            chainActions[worker] = chainList;
                        }

                        action.IsChain = true;

                        worker.DbLoaderDoPassive(action);

                        chainList.Add(action);
                    }
                }
            }

            #endregion

            #region Chain Actions

            logger.Info("Loading chain actions...");

            using (var reader = DbManager.Select(ChainAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType + "Action", true, true);

                    var currentActionId = DBNull.Value.Equals(reader["current_action_id"])
                                                  ? 0
                                                  : (uint)reader["current_action_id"];

                    var locationType = (LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true);
                    var locationId = (uint)reader["location_id"];

                    IActionWorker worker = resolveWorker(null, (uint)reader["object_id"], locationType, locationId);

                    List<PassiveAction> chainList;
                    PassiveAction currentAction = null;
                    //current action might be null if it has already completed and we are in the call chain part of the cycle
                    if (chainActions.TryGetValue(worker, out chainList))
                    {
                        currentAction = chainList.Find(lookupAction => lookupAction.ActionId == currentActionId);
                    }

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);
                    var action = DbLoaderActionFactory.CreateChainAction(type,
                                                                 (uint)reader["id"],
                                                                 (string)reader["chain_callback"],
                                                                 currentAction,
                                                                 (ActionState)((byte)reader["chain_state"]),
                                                                 (bool)reader["is_visible"],
                                                                 properties);

                    action.DbPersisted = true;

                    worker = resolveWorker(action, (uint)reader["object_id"], locationType, locationId);

                    worker.DbLoaderDoPassive(action);
                }
            }

            #endregion
        }

        private void LoadActionReferences()
        {
            #region Action References

            logger.Info("Loading action references...");
            using (var reader = DbManager.Select(ReferenceStub.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }

                    GameAction action;
                    if ((bool)reader["is_active"])
                    {
                        action = city.Worker.ActiveActions[(uint)reader["action_id"]];
                    }
                    else
                    {
                        action = city.Worker.PassiveActions[(uint)reader["action_id"]];
                    }

                    ICanDo obj;
                    var workerId = (uint)reader["object_id"];
                    if (workerId == 0)
                    {
                        obj = city;
                    }
                    else
                    {
                        obj = city[(uint)reader["object_id"]];
                    }

                    var referenceStub = new ReferenceStub((ushort)reader["id"], obj, action, city.Id) {DbPersisted = true};

                    city.References.DbLoaderAdd(referenceStub);
                }
            }

            #endregion
        }

        private void LoadActionNotifications()
        {
            #region Action Notifications

            logger.Info("Loading action notifications...");

            ILookup<Tuple<uint, uint, uint>, dynamic> notificationOwners;

            using (var listReader = DbManager.SelectList(Logic.Notifications.Notification.DB_TABLE))
            {
                notificationOwners = ReaderToLookUp(listReader,
                                                    reader => new
                                                    {
                                                            CityId = (uint)reader["city_id"],
                                                            ObjectId = (uint)reader["object_id"],
                                                            ActionId = (uint)reader["action_id"],
                                                            SubscriptionLocationType = (string)reader["subscription_location_type"],
                                                            SubscriptionLocationId = (uint)reader["subscription_location_id"],
                                                    },
                                                    key => new Tuple<uint, uint, uint>(key.CityId, key.ObjectId, key.ActionId));
            }

            using (var reader = DbManager.Select(Logic.Notifications.Notification.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    var cityId = (uint)reader["city_id"];
                    var objectId = (uint)reader["object_id"];
                    var actionId = (uint)reader["action_id"];

                    if (!World.TryGetObjects(cityId, out city))
                    {
                        throw new Exception("City not found");
                    }

                    var notification = new Logic.Notifications.Notification(city[objectId], city.Worker.PassiveActions[actionId]);

                    foreach (var notificationOwner in notificationOwners[new Tuple<uint, uint, uint>(cityId, objectId, actionId)])
                    {
                        notification.Subscriptions.Add(ResolveLocationAs<INotificationOwner>(notificationOwner.SubscriptionLocationType,
                                                                                             notificationOwner.SubscriptionLocationId));
                    }

                    city.Notifications.DbLoaderAdd(false, notification);
                    notification.DbPersisted = true;
                }
            }

            #endregion
        }

        private T ResolveLocationAs<T>(string locationType, uint locationId)
        {
            switch((LocationType)Enum.Parse(typeof(LocationType), locationType, true))
            {
                case LocationType.City:
                    ICity city;
                    if (!World.TryGetObjects(locationId, out city))
                    {
                        throw new Exception("City not found");
                    }
                    return (T)city;
                case LocationType.Stronghold:
                    IStronghold stronghold;
                    if (!StrongholdManager.TryGetStronghold(locationId, out stronghold))
                    {
                        throw new Exception("Stronghold not found");
                    }
                    return (T)stronghold;
                case LocationType.BarbarianTribe:
                    IBarbarianTribe barbarianTribe;
                    if (!World.TryGetObjects(locationId, out barbarianTribe))
                    {
                        throw new Exception("Barbarian tribe not found");
                    }
                    return (T)barbarianTribe;
                default:
                    throw new Exception("Unknown location type");
            }
        }

        private ILookup<T, dynamic> ReaderToLookUp<T>(DbDataReader reader, Func<DbDataReader, dynamic> projection, Func<dynamic, T> keySelector)
        {
            var list = new List<dynamic>();
            while (reader.Read())
            {
                list.Add(projection(reader));
            }

            return list.ToLookup(keySelector);
        }
    }
}