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
using JsonFx.Json;
using Ninject;
using Persistance;
using DbTransaction = Persistance.DbTransaction;

#endregion

namespace Game.Database
{
    public class DbLoader
    {
        [Inject]
        public IWorld World { get; set; }

        [Inject]
        public IDbManager DbManager { get; set; }

        [Inject]
        public ITribeManager Tribes { get; set; }

        [Inject]
        public DbLoaderActionFactory ActionFactory { get; set; }

        [Inject]
        public IBattleManagerFactory BattleManagerFactory { get; set; }

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

        public bool LoadFromDatabase()
        {
            SystemVariablesUpdater.Current.Pause();
            Scheduler.Current.Pause();
            Global.FireEvents = false;

            Global.Logger.Info("Loading database...");

            DateTime now = DateTime.UtcNow;

            using (DbTransaction transaction = DbManager.GetThreadTransaction())
            {
                try
                {
                    // Check Schema
                    CheckSchemaVersion();

                    // Set all players to offline
                    DbManager.Query("UPDATE `players` SET `online` = @online",
                                    new[] {new DbColumn("online", false, DbType.Boolean)});

                    // Load sys vars
                    LoadSystemVariables();

                    // Calculate how long server was down
                    TimeSpan downTime = now.Subtract((DateTime)Global.SystemVariables["System.time"].Value);
                    if (downTime.TotalMilliseconds < 0)
                    {
                        downTime = new TimeSpan(0);
                    }

                    Global.Logger.Info(string.Format("Server was down for {0}", downTime));

                    LoadReportIds();
                    LoadMarket();
                    LoadPlayers();
                    LoadCities(downTime);
                    LoadTribes();
                    LoadTribesmen();
                    LoadUnitTemplates();
                    LoadStructures();
                    LoadStructureProperties();
                    LoadTechnologies();
                    LoadForests(downTime);
                    LoadStrongholds(downTime);
                    LoadTroopStubs();
                    LoadTroopStubTemplates();
                    LoadTroops();
                    LoadBattleManagers();
                    LoadActions(downTime);
                    LoadActionReferences();
                    LoadActionNotifications();
                    LoadAssignments(downTime);

                    World.AfterDbLoaded(Procedure);

                    //Ok data all loaded. We can get the system going now.
                    Global.SystemVariables["System.time"].Value = now;
                    DbManager.Save(Global.SystemVariables["System.time"]);
                }
                catch(Exception e)
                {
                    Global.Logger.Error("Database loader error", e);
                    transaction.Rollback();
                    return false;
                }
            }

            Global.Logger.Info("Database loading finished");

            SystemVariablesUpdater.Current.Resume();
            Global.FireEvents = true;
            Scheduler.Current.Resume();
            return true;
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

        private void LoadReportIds()
        {
            BattleReport.BattleIdGenerator.Set(Math.Max(GetMaxId(BattleManager.DB_TABLE, "battle_id"), GetMaxId(SqlBattleReportWriter.BATTLE_DB)));
            BattleReport.ReportIdGenerator.Set(GetMaxId(SqlBattleReportWriter.BATTLE_REPORTS_DB));
            BattleReport.BattleTroopIdGenerator.Set(GetMaxId(SqlBattleReportWriter.BATTLE_REPORT_TROOPS_DB));
        }

        private void LoadTribes()
        {
            #region Tribes

            Global.Logger.Info("Loading tribes...");
            using (var reader = DbManager.Select(Tribe.DB_TABLE))
            {
                while (reader.Read())
                {
                    if ((bool)reader["deleted"])
                    {
                        continue;
                    }

                    var resource = new Resource((int)reader["crop"],
                                                (int)reader["gold"],
                                                (int)reader["iron"],
                                                (int)reader["wood"],
                                                0);
                    var tribe = TribeFactory.CreateTribe(World.Players[(uint)reader["owner_player_id"]],
                                                         (string)reader["name"],
                                                         (string)reader["desc"],
                                                         (byte)reader["level"],
                                                         (decimal)reader["victory_point"],
                                                         (int)reader["attack_point"],
                                                         (int)reader["defense_point"],
                                                         resource,
                                                         DateTime.SpecifyKind((DateTime)reader["created"],
                                                                              DateTimeKind.Utc));
                    tribe.Id = (uint)reader["id"];
                    tribe.DbPersisted = true;

                    Tribes.DbLoaderAdd(tribe);
                }
            }

            #endregion
        }

        private void LoadTribesmen()
        {
            #region Tribes

            Global.Logger.Info("Loading tribesmen...");
            using (var reader = DbManager.Select(Tribesman.DB_TABLE))
            {
                while (reader.Read())
                {
                    ITribe tribe;
                    World.TryGetObjects((uint)reader["tribe_id"], out tribe);
                    var contribution = new Resource((int)reader["crop"],
                                                    (int)reader["gold"],
                                                    (int)reader["iron"],
                                                    (int)reader["wood"],
                                                    0);
                    var tribesman = new Tribesman(tribe,
                                                  World.Players[(uint)reader["player_id"]],
                                                  DateTime.SpecifyKind((DateTime)reader["join_date"], DateTimeKind.Utc),
                                                  contribution,
                                                  (byte)reader["rank"]) {DbPersisted = true};
                    tribe.AddTribesman(tribesman, false);
                }
            }

            #endregion
        }

        private void LoadAssignments(TimeSpan downTime)
        {
            #region Assignments

            IAssignmentFactory assignmentFactory = Ioc.Kernel.Get<IAssignmentFactory>();

            Global.Logger.Info("Loading assignments...");
            using (var reader = DbManager.Select(Assignment.DB_TABLE))
            {
                while (reader.Read())
                {
                    ITribe tribe;
                    World.TryGetObjects((uint)reader["tribe_id"], out tribe);

                    Assignment assignment = assignmentFactory.CreateAssignmentFromDb((int)reader["id"],
                                                                                     tribe,
                                                                                     (uint)reader["x"],
                                                                                     (uint)reader["y"],
                                                                                     new SimpleLocation(
                                                                                             (LocationType)
                                                                                             Enum.Parse(
                                                                                                        typeof(
                                                                                                                LocationType
                                                                                                                ),
                                                                                                        (string)
                                                                                                        reader[
                                                                                                               "location_type"
                                                                                                                ],
                                                                                                        true),
                                                                                             (uint)reader["location_id"]),
                                                                                     (AttackMode)
                                                                                     Enum.Parse(typeof(AttackMode),
                                                                                                (string)reader["mode"]),
                                                                                     DateTime.SpecifyKind(
                                                                                                          (DateTime)
                                                                                                          reader[
                                                                                                                 "attack_time"
                                                                                                                  ],
                                                                                                          DateTimeKind
                                                                                                                  .Utc)
                                                                                             .Add(downTime),
                                                                                     (uint)reader["dispatch_count"],
                                                                                     (string)reader["description"],
                                                                                     ((byte)reader["is_attack"]) == 1);

                    using (DbDataReader listReader = DbManager.SelectList(assignment))
                    {
                        while (listReader.Read())
                        {
                            ICity city;
                            if (!World.TryGetObjects((uint)listReader["city_id"], out city))
                            {
                                throw new Exception("City not found");
                            }

                            ITroopStub assignmentStub;
                            if (!city.Troops.TryGetStub((byte)listReader["stub_id"], out assignmentStub))
                            {
                                throw new Exception("Stub not found");
                            }

                            assignment.DbLoaderAdd(assignmentStub, (byte)listReader["dispatched"] == 1);
                        }
                    }

                    assignment.DbPersisted = true;

                    // Add assignment to tribe
                    tribe.DbLoaderAddAssignment(assignment);

                    // Reschedule and save assignment
                    assignment.Reschedule();
                }
            }

            #endregion
        }

        private void LoadSystemVariables()
        {
            #region System variables

            Global.Logger.Info("Loading system variables...");
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
                    Global.SystemVariables.Add(systemVariable.Key, systemVariable);
                }
            }

            // Set system variable defaults
            if (!Global.SystemVariables.ContainsKey("System.time"))
            {
                Global.SystemVariables.Add("System.time", new SystemVariable("System.time", DateTime.UtcNow));
            }

            if (!Global.SystemVariables.ContainsKey("Map.start_index"))
            {
                Global.SystemVariables.Add("Map.start_index", new SystemVariable("Map.start_index", 0));
            }

            #endregion
        }

        private void LoadMarket()
        {
            #region Market

            Global.Logger.Info("Loading market...");
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

            Global.Logger.Info("Loading players...");
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
                            TribeRequest = (uint)reader["invitation_tribe_id"]
                    };
                    World.Players.Add(player.PlayerId, player);
                }
            }

            #endregion
        }

        private void LoadCities(TimeSpan downTime)
        {
            #region Cities

            Global.Logger.Info("Loading cities...");
            using (var reader = DbManager.Select(City.DB_TABLE))
            {
                while (reader.Read())
                {
                    DateTime cropRealizeTime =
                            DateTime.SpecifyKind((DateTime)reader["crop_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime woodRealizeTime =
                            DateTime.SpecifyKind((DateTime)reader["wood_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime ironRealizeTime =
                            DateTime.SpecifyKind((DateTime)reader["iron_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime laborRealizeTime =
                            DateTime.SpecifyKind((DateTime)reader["labor_realize_time"], DateTimeKind.Utc).Add(downTime);
                    DateTime goldRealizeTime =
                            DateTime.SpecifyKind((DateTime)reader["gold_realize_time"], DateTimeKind.Utc).Add(downTime);

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
                    var city = new City((uint)reader["id"],
                                        World.Players[(uint)reader["player_id"]],
                                        (string)reader["name"],
                                        resource,
                                        (byte)reader["radius"],
                                        null,
                                        (decimal)reader["alignment_point"])
                    {
                            DbPersisted = true,
                            LootStolen = (uint)reader["loot_stolen"],
                            AttackPoint = (int)reader["attack_point"],
                            DefensePoint = (int)reader["defense_point"],
                            HideNewUnits = (bool)reader["hide_new_units"],
                            Value = (ushort)reader["value"],
                            Deleted = (City.DeletedState)reader["deleted"]
                    };

                    // Add to world
                    World.Cities.DbLoaderAdd(city);

                    // Restart city remover if needed
                    switch(city.Deleted)
                    {
                        case City.DeletedState.Deleting:
                            city.Owner.Add(city);
                            CityRemover cr = Ioc.Kernel.Get<ICityRemoverFactory>().CreateCityRemover(city.Id);
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

        private void LoadStrongholds(TimeSpan downTime)
        {
            #region Strongholds

            Global.Logger.Info("Loading strongholds...");
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
                    stronghold.DbPersisted = true;
                    stronghold.State.Type = (ObjectState)((byte)reader["object_state"]);
                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                    {
                        stronghold.State.Parameters.Add(variable);
                    }

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

                    // Add stronghold to main manager
                    StrongholdManager.DbLoaderAdd(stronghold);
                }
            }

            #endregion
        }

        private void LoadUnitTemplates()
        {
            #region Unit Template

            Global.Logger.Info("Loading unit template...");
            using (var reader = DbManager.Select(UnitTemplate.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }

                    city.Template.DbPersisted = true;

                    using (DbDataReader listReader = DbManager.SelectList(city.Template))
                    {
                        while (listReader.Read())
                        {
                            city.Template.DbLoaderAdd((ushort)listReader["type"],
                                                      Ioc.Kernel.Get<UnitFactory>()
                                                         .GetUnitStats((ushort)listReader["type"],
                                                                       (byte)listReader["level"]));
                        }
                    }
                }
            }

            #endregion
        }

        private void LoadForests(TimeSpan downTime)
        {
            Global.Logger.Info("Loading forests...");
            using (var reader = DbManager.Select(Forest.DB_TABLE))
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
                                                            DateTime.SpecifyKind((DateTime)reader["last_realize_time"],
                                                                                 DateTimeKind.Utc).Add(downTime),
                                                            0,
                                                            (int)reader["upkeep"]) {Limit = (int)reader["capacity"]},
                            DepleteTime =
                                    DateTime.SpecifyKind((DateTime)reader["deplete_time"], DateTimeKind.Utc)
                                            .Add(downTime),
                            InWorld = (bool)reader["in_world"]
                    };

                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                    {
                        forest.State.Parameters.Add(variable);
                    }

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
                        forest.DepleteAction = new ForestDepleteAction(forest, forest.DepleteTime);
                        Scheduler.Current.Put(forest.DepleteAction);
                        World.Regions.DbLoaderAdd(forest);
                        World.Forests.DbLoaderAdd(forest);
                    }

                    // Resave to include new time
                    DbManager.Save(forest);
                }
            }
        }

        private void LoadStructures()
        {
            #region Structures

            Global.Logger.Info("Loading structures...");
            using (var reader = DbManager.Select(Structure.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }
                    IStructure structure = Ioc.Kernel.Get<StructureFactory>()
                                              .GetNewStructure((ushort)reader["type"], (byte)reader["level"]);
                    structure.InWorld = (bool)reader["in_world"];
                    structure.Technologies.Parent = city.Technologies;
                    structure.X = (uint)reader["x"];
                    structure.Y = (uint)reader["y"];
                    structure.Stats.Hp = (decimal)reader["hp"];
                    structure.ObjectId = (uint)reader["id"];
                    structure.Stats.Labor = (ushort)reader["labor"];
                    structure.DbPersisted = true;
                    structure.State.Type = (ObjectState)((byte)reader["state"]);
                    structure.IsBlocked = (bool)reader["is_blocked"];

                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                    {
                        structure.State.Parameters.Add(variable);
                    }

                    city.Add(structure.ObjectId, structure, false);

                    if (structure.InWorld)
                    {
                        World.Regions.DbLoaderAdd(structure);
                    }
                }
            }

            #endregion
        }

        private void LoadStructureProperties()
        {
            #region Structure Properties

            Global.Logger.Info("Loading structure properties...");
            using (var reader = DbManager.Select(StructureProperties.DB_TABLE))
            {
                ICity city = null;
                while (reader.Read())
                {
                    // Simple optimization                        
                    if (city == null || city.Id != (uint)reader["city_id"])
                    {
                        if (!World.TryGetObjects((uint)reader["city_id"], out city))
                        {
                            throw new Exception("City not found");
                        }
                    }

                    var structure = (IStructure)city[(uint)reader["structure_id"]];

                    structure.Properties.DbPersisted = true;

                    using (DbDataReader listReader = DbManager.SelectList(structure.Properties))
                    {
                        while (listReader.Read())
                        {
                            structure.Properties.Add(listReader["name"],
                                                     DataTypeSerializer.Deserialize((string)listReader["value"],
                                                                                    (byte)listReader["datatype"]));
                        }
                    }
                }
            }

            #endregion
        }

        private void LoadTechnologies()
        {
            #region Technologies

            Global.Logger.Info("Loading technologies...");
            using (var reader = DbManager.Select(TechnologyManager.DB_TABLE))
            {
                while (reader.Read())
                {
                    var ownerLocation = (EffectLocation)((byte)reader["owner_location"]);

                    ITechnologyManager manager;

                    switch(ownerLocation)
                    {
                        case EffectLocation.Object:
                        {
                            ICity city;
                            if (!World.TryGetObjects((uint)reader["city_id"], out city))
                            {
                                throw new Exception("City not found");
                            }

                            var structure = (IStructure)city[(uint)reader["owner_id"]];
                            manager = structure.Technologies;
                        }
                            break;
                        case EffectLocation.City:
                        {
                            ICity city;
                            if (!World.TryGetObjects((uint)reader["city_id"], out city))
                            {
                                throw new Exception("City not found");
                            }
                            manager = city.Technologies;
                        }
                            break;
                        default:
                            throw new Exception("Unknown effect location?");
                    }

                    manager.DbPersisted = true;

                    using (DbDataReader listReader = DbManager.SelectList(manager))
                    {
                        while (listReader.Read())
                        {
                            manager.Add(
                                        Ioc.Kernel.Get<TechnologyFactory>()
                                           .GetTechnology((uint)listReader["type"], (byte)listReader["level"]),
                                        false);
                        }
                    }
                }
            }

            #endregion
        }

        private void LoadTroopStubs()
        {
            #region Troop Stubs

            List<dynamic> stationedTroops = new List<dynamic>();

            Global.Logger.Info("Loading troop stubs...");
            using (var reader = DbManager.Select(TroopStub.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }

                    var stub = new TroopStub((byte)reader["id"], city)
                    {
                            State = (TroopState)Enum.Parse(typeof(TroopState), reader["state"].ToString(), true),
                            DbPersisted = true,
                            RetreatCount = (ushort)reader["retreat_count"]
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

                    using (DbDataReader listReader = DbManager.SelectList(stub))
                    {
                        while (listReader.Read())
                        {
                            stub.AddUnit((FormationType)((byte)listReader["formation_type"]),
                                         (ushort)listReader["type"],
                                         (ushort)listReader["count"]);
                        }
                    }

                    city.Troops.DbLoaderAdd((byte)reader["id"], stub);

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

            Global.Logger.Info("Loading troop stub templates...");
            using (var reader = DbManager.Select(TroopTemplate.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }
                    ITroopStub stub = city.Troops[(byte)reader["troop_stub_id"]];
                    stub.Template.DbPersisted = true;

                    using (DbDataReader listReader = DbManager.SelectList(stub.Template))
                    {
                        while (listReader.Read())
                        {
                            //First we load the BaseBattleStats and pass it into the BattleStats
                            //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                            var battleStats =
                                    new BattleStats(
                                            Ioc.Kernel.Get<UnitFactory>()
                                               .GetBattleStats((ushort)listReader["type"], (byte)listReader["level"]))
                                    {
                                            MaxHp = (decimal)listReader["max_hp"],
                                            Atk = (decimal)listReader["attack"],
                                            Splash = (byte)listReader["splash"],
                                            Rng = (byte)listReader["range"],
                                            Stl = (byte)listReader["stealth"],
                                            Spd = (byte)listReader["speed"],
                                            Carry = (ushort)listReader["carry"],
                                            NormalizedCost = (decimal)listReader["normalized_cost"]
                                    };

                            stub.Template.DbLoaderAdd(battleStats);
                        }
                    }
                }
            }

            #endregion
        }

        private void LoadTroops()
        {
            #region Troops

            Global.Logger.Info("Loading troops...");
            using (var reader = DbManager.Select(TroopObject.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }
                    ITroopStub stub = (byte)reader["troop_stub_id"] != 0
                                              ? city.Troops[(byte)reader["troop_stub_id"]]
                                              : null;
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
                                                   new Resource((int)reader["crop"],
                                                                (int)reader["gold"],
                                                                (int)reader["iron"],
                                                                (int)reader["wood"],
                                                                0)),
                            IsBlocked = (bool)reader["is_blocked"],
                            InWorld = (bool)reader["in_world"],
                    };

                    foreach (var variable in XmlSerializer.DeserializeList((string)reader["state_parameters"]))
                    {
                        obj.State.Parameters.Add(variable);
                    }

                    city.Add(obj.ObjectId, obj, false);

                    if (obj.InWorld)
                    {
                        World.Regions.DbLoaderAdd(obj);
                    }
                }
            }

            #endregion
        }

        private void LoadBattleManagers()
        {
            #region Battle Managers

            Global.Logger.Info("Loading battles...");
            using (var reader = DbManager.Select(BattleManager.DB_TABLE))
            {
                while (reader.Read())
                {
                    // Load battle manager
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

                            battleManager = BattleManagerFactory.CreateBattleManager((uint)reader["battle_id"],
                                                                                     battleLocation,
                                                                                     battleOwner,
                                                                                     city);
                            city.Battle = battleManager;
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
                                battleManager =
                                        BattleManagerFactory.CreateStrongholdMainBattleManager(
                                                                                               (uint)reader["battle_id"],
                                                                                               battleLocation,
                                                                                               battleOwner,
                                                                                               stronghold);
                                stronghold.MainBattle = battleManager;
                            }
                            else
                            {
                                battleManager =
                                        BattleManagerFactory.CreateStrongholdGateBattleManager(
                                                                                               (uint)reader["battle_id"],
                                                                                               battleLocation,
                                                                                               battleOwner,
                                                                                               stronghold);
                                stronghold.GateBattle = battleManager;
                            }
                            break;
                        default:
                            throw new Exception(string.Format("Unknown location type {0} when loading battle manager",
                                                              battleLocation.Type));
                    }
                    battleManager.DbLoadProperties(
                                                   new JsonReader().Read<Dictionary<string, object>>(
                                                                                                     (string)
                                                                                                     reader["properties"
                                                                                                             ]));
                    battleManager.DbPersisted = true;
                    battleManager.BattleStarted = (bool)reader["battle_started"];
                    battleManager.Round = (uint)reader["round"];
                    battleManager.Turn = (uint)reader["round"];
                    battleManager.NextToAttack = (BattleManager.BattleSide)((byte)reader["next_to_attack"]);

                    battleManager.BattleReport.ReportStarted = (bool)reader["report_started"];
                    battleManager.BattleReport.ReportId = (uint)reader["report_id"];
                    battleManager.BattleReport.SnappedImportantEvent = (bool)reader["snapped_important_event"];

                    // Load combat groups
                    using (
                            DbDataReader listReader = DbManager.SelectList(CityOffensiveCombatGroup.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
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
                            var cityOffensiveCombatGroup =
                                    CombatGroupFactory.CreateCityOffensiveCombatGroup((uint)listReader["battle_id"],
                                                                                      (uint)listReader["id"],
                                                                                      troopObject);
                            cityOffensiveCombatGroup.DbPersisted = true;
                            battleManager.DbLoaderAddToCombatList(cityOffensiveCombatGroup,
                                                                  BattleManager.BattleSide.Attack);
                        }
                    }

                    using (
                            DbDataReader listReader = DbManager.SelectList(CityDefensiveCombatGroup.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
                    {
                        while (listReader.Read())
                        {
                            ICity combatGroupCity;
                            if (!World.TryGetObjects((uint)listReader["city_id"], out combatGroupCity))
                            {
                                throw new Exception("City not found");
                            }

                            ITroopStub troopStub;
                            if (!combatGroupCity.Troops.TryGetStub((byte)listReader["troop_stub_id"], out troopStub))
                            {
                                throw new Exception("Troop stub not found");
                            }

                            var cityDefensiveCombatGroup =
                                    CombatGroupFactory.CreateCityDefensiveCombatGroup((uint)listReader["battle_id"],
                                                                                      (uint)listReader["id"],
                                                                                      troopStub);
                            cityDefensiveCombatGroup.DbPersisted = true;
                            battleManager.DbLoaderAddToCombatList(cityDefensiveCombatGroup,
                                                                  BattleManager.BattleSide.Defense);
                        }
                    }

                    using (
                            DbDataReader listReader = DbManager.SelectList(StrongholdCombatGroup.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
                    {
                        while (listReader.Read())
                        {
                            IStronghold combatGroupStronghold;
                            if (!World.TryGetObjects((uint)listReader["stronghold_id"], out combatGroupStronghold))
                            {
                                throw new Exception("Stronghold not found");
                            }

                            var strongholdCombatGroup =
                                    CombatGroupFactory.CreateStrongholdCombatGroup((uint)listReader["battle_id"],
                                                                                   (uint)listReader["id"],
                                                                                   combatGroupStronghold);
                            strongholdCombatGroup.DbPersisted = true;
                            battleManager.DbLoaderAddToCombatList(strongholdCombatGroup,
                                                                  BattleManager.BattleSide.Defense);
                        }
                    }

                    // Load combat structures
                    using (
                            DbDataReader listReader = DbManager.SelectList(CombatStructure.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
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
                                                                      Ioc.Kernel.Get<Formula>(),
                                                                      Ioc.Kernel.Get<IActionFactory>(),
                                                                      Ioc.Kernel.Get<BattleFormulas>())
                            {
                                    GroupId = (uint)listReader["group_id"],
                                    DmgDealt =
                                            (decimal)
                                            listReader["damage_dealt"],
                                    DmgRecv =
                                            (decimal)
                                            listReader["damage_received"],
                                    LastRound = (uint)listReader["last_round"],
                                    RoundsParticipated =
                                            (int)
                                            listReader["rounds_participated"],
                                    DbPersisted = true
                            };

                            battleManager.GetCombatGroup((uint)listReader["group_id"]).Add(combatStructure, false);
                        }
                    }

                    // Load attack combat units
                    using (
                            DbDataReader listReader = DbManager.SelectList(AttackCombatUnit.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
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
                                                                           (FormationType)
                                                                           ((byte)listReader["formation_type"]),
                                                                           (ushort)listReader["type"],
                                                                           (byte)listReader["level"],
                                                                           (ushort)listReader["count"],
                                                                           (decimal)listReader["left_over_hp"],
                                                                           new Resource((int)listReader["loot_crop"],
                                                                                        (int)listReader["loot_gold"],
                                                                                        (int)listReader["loot_iron"],
                                                                                        (int)listReader["loot_wood"],
                                                                                        (int)listReader["loot_labor"]),
                                                                           Ioc.Kernel.Get<UnitFactory>(),
                                                                           Ioc.Kernel.Get<BattleFormulas>());

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
                            DbDataReader listReader = DbManager.SelectList(DefenseCombatUnit.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
                    {
                        while (listReader.Read())
                        {
                            ICity troopStubCity;
                            if (!World.TryGetObjects((uint)listReader["troop_stub_city_id"], out troopStubCity))
                            {
                                throw new Exception("City not found");
                            }

                            ITroopStub troopStub = troopStubCity.Troops[(byte)listReader["troop_stub_id"]];

                            ICombatObject combatObj = new DefenseCombatUnit((uint)listReader["id"],
                                                                            battleManager.BattleId,
                                                                            troopStub,
                                                                            (FormationType)
                                                                            ((byte)listReader["formation_type"]),
                                                                            (ushort)listReader["type"],
                                                                            (byte)listReader["level"],
                                                                            (ushort)listReader["count"],
                                                                            (decimal)listReader["left_over_hp"],
                                                                            Ioc.Kernel.Get<BattleFormulas>());
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
                            DbDataReader listReader = DbManager.SelectList(StrongholdCombatUnit.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
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
                                                                               Ioc.Kernel.Get<UnitFactory>(),
                                                                               Ioc.Kernel.Get<BattleFormulas>(),
                                                                               Ioc.Kernel.Get<Formula>());

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
                            DbDataReader listReader = DbManager.SelectList(StrongholdCombatStructure.DB_TABLE,
                                                                           new DbColumn("battle_id",
                                                                                        battleManager.BattleId,
                                                                                        DbType.UInt32)))
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
                                                                               Ioc.Kernel.Get<StructureFactory>(),
                                                                               Ioc.Kernel.Get<BattleFormulas>());

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
                    using (DbDataReader listReader = DbManager.SelectList(battleManager.BattleReport.ReportedGroups))
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
            }

            #endregion
        }

        private void LoadActions(TimeSpan downTime)
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

                                default:
                                    throw new Exception(string.Format("Unknown location type {0} when loading actions",
                                                                      locationType));
                            }
                        };

            #region Active Actions

            Global.Logger.Info("Loading active actions...");

            using (var reader = DbManager.Select(ActiveAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "Action",
                                             true,
                                             true);

                    DateTime beginTime =
                            DateTime.SpecifyKind((DateTime)reader["begin_time"], DateTimeKind.Utc).Add(downTime);

                    DateTime nextTime = DateTime.SpecifyKind((DateTime)reader["next_time"], DateTimeKind.Utc);
                    if (nextTime != DateTime.MinValue)
                    {
                        nextTime = nextTime.Add(downTime);
                    }

                    DateTime endTime = DateTime.SpecifyKind((DateTime)reader["end_time"], DateTimeKind.Utc)
                                               .Add(downTime);

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);

                    var action = ActionFactory.CreateScheduledActiveAction(type,
                                                                           (uint)reader["id"],
                                                                           beginTime,
                                                                           nextTime,
                                                                           endTime,
                                                                           (int)reader["worker_type"],
                                                                           (byte)reader["worker_index"],
                                                                           (ushort)reader["count"],
                                                                           properties);
                    action.DbPersisted = true;

                    var locationType =
                            (LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true);
                    var locationId = (uint)reader["location_id"];

                    IActionWorker worker = resolveWorker(action, (uint)reader["object_id"], locationType, locationId);

                    worker.DbLoaderDoActive(action);

                    DbManager.Save(action);
                }
            }

            #endregion

            #region Passive Actions

            Global.Logger.Info("Loading passive actions...");

            //this will hold chain actions that we encounter for the next phase
            var chainActions = new Dictionary<IActionWorker, List<PassiveAction>>();

            using (var reader = DbManager.Select(PassiveAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "Action",
                                             true,
                                             true);

                    Dictionary<string, string> properties = XmlSerializer.Deserialize((string)reader["properties"]);

                    PassiveAction action;

                    if ((bool)reader["is_scheduled"])
                    {
                        DateTime beginTime = DateTime.SpecifyKind((DateTime)reader["begin_time"], DateTimeKind.Utc);
                        beginTime = beginTime.Add(downTime);

                        DateTime nextTime = DateTime.SpecifyKind((DateTime)reader["next_time"], DateTimeKind.Utc);
                        if (nextTime != DateTime.MinValue)
                        {
                            nextTime = nextTime.Add(downTime);
                        }

                        DateTime endTime = DateTime.SpecifyKind((DateTime)reader["end_time"], DateTimeKind.Utc);
                        endTime = endTime.Add(downTime);

                        string nlsDescription = DBNull.Value.Equals(reader["nls_description"])
                                                        ? string.Empty
                                                        : (string)reader["nls_description"];

                        action = ActionFactory.CreateScheduledPassiveAction(type,
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
                        action = ActionFactory.CreatePassiveAction(type,
                                                                   (uint)reader["id"],
                                                                   (bool)reader["is_visible"],
                                                                   properties);
                    }

                    action.DbPersisted = true;

                    var locationType =
                            (LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true);
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

                    // Resave city to update times
                    DbManager.Save(action);
                }
            }

            #endregion

            #region Chain Actions

            Global.Logger.Info("Loading chain actions...");

            using (var reader = DbManager.Select(ChainAction.DB_TABLE))
            {
                while (reader.Read())
                {
                    var actionType = (ActionType)((int)reader["type"]);
                    Type type = Type.GetType("Game.Logic.Actions." + actionType + "Action", true, true);

                    var currentActionId = DBNull.Value.Equals(reader["current_action_id"])
                                                  ? 0
                                                  : (uint)reader["current_action_id"];

                    var locationType =
                            (LocationType)Enum.Parse(typeof(LocationType), (string)reader["location_type"], true);
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
                    var action = ActionFactory.CreateChainAction(type,
                                                                 (uint)reader["id"],
                                                                 (string)reader["chain_callback"],
                                                                 currentAction,
                                                                 (ActionState)((byte)reader["chain_state"]),
                                                                 (bool)reader["is_visible"],
                                                                 properties);

                    action.DbPersisted = true;

                    worker = resolveWorker(action, (uint)reader["object_id"], locationType, locationId);

                    worker.DbLoaderDoPassive(action);

                    DbManager.Save(action);
                }
            }

            #endregion
        }

        private void LoadActionReferences()
        {
            #region Action References

            Global.Logger.Info("Loading action references...");
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

                    var referenceStub = new ReferenceStub((ushort)reader["id"], obj, action, city) {DbPersisted = true};

                    city.References.DbLoaderAdd(referenceStub);
                }
            }

            #endregion
        }

        private void LoadActionNotifications()
        {
            #region Action Notifications

            Global.Logger.Info("Loading action notifications...");
            using (var reader = DbManager.Select(Logic.Notifications.Notification.DB_TABLE))
            {
                while (reader.Read())
                {
                    ICity city;
                    if (!World.TryGetObjects((uint)reader["city_id"], out city))
                    {
                        throw new Exception("City not found");
                    }

                    IGameObject obj = city[(uint)reader["object_id"]];
                    PassiveAction action = city.Worker.PassiveActions[(uint)reader["action_id"]];

                    var notification = new Logic.Notifications.Notification(obj, action);

                    using (DbDataReader listReader = DbManager.SelectList(notification))
                    {
                        while (listReader.Read())
                        {
                            INotificationOwner notificationOwner =
                                    ResolveLocationAs<INotificationOwner>(
                                                                          (string)
                                                                          listReader["subscription_location_type"],
                                                                          (uint)listReader["subscription_location_id"]);

                            notification.Subscriptions.Add(notificationOwner);
                        }
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
                default:
                    throw new Exception("Unknown location type");
            }
        }
    }
}