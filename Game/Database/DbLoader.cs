#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Game.Battle;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic;
using Game.Logic.Actions;
using Game.Module;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Database {
    public class DbLoader {
        public static bool LoadFromDatabase(IDbManager dbManager) {
            Global.Scheduler.pause();
            Global.pauseEvents();

            Global.Logger.Info("Loading database...");

            DateTime now = DateTime.Now;

            DbDataReader reader;
            using (DbTransaction transaction = Global.dbManager.GetThreadTransaction()) {
                try {
                    #region System variables

                    using (reader = dbManager.Select(SystemVariable.DB_TABLE)) {
                        while (reader.Read()) {
                            SystemVariable systemVariable = new SystemVariable((string) reader["name"],
                                                                               DataTypeSerializer.Deserialize(
                                                                                   (string) reader["value"],
                                                                                   (byte) reader["datatype"]));
                            systemVariable.DbPersisted = true;
                            Global.SystemVariables.Add(systemVariable.Key, systemVariable);
                        }
                    }

                    #endregion

                    #region Market

                    using (reader = dbManager.Select(Market.DB_TABLE)) {
                        while (reader.Read()) {
                            ResourceType type = (ResourceType) ((byte) reader["resource_type"]);
                            Market market = new Market(type, (int) reader["price"], (int) reader["quantity_per_change"]);
                            market.dbLoad((int) reader["outgoing"], (int) reader["incoming"]);
                            market.DbPersisted = true;
                            switch (type) {
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

                    #region Players

                    using (reader = dbManager.Select(Player.DB_TABLE)) {
                        while (reader.Read()) {
                            Player player = new Player((uint) reader["id"], (string) reader["name"]);
                            player.DbPersisted = true;
                            Global.Players.Add(player.PlayerId, player);
                        }
                    }

                    #endregion

                    #region Cities

                    using (reader = dbManager.Select(City.DB_TABLE)) {
                        while (reader.Read()) {
                            LazyResource resource = new LazyResource((int) reader["crop"],
                                                                     (DateTime) reader["crop_realize_time"],
                                                                     (int) reader["crop_production_rate"],
                                                                     (int) reader["gold"],
                                                                     (DateTime) reader["gold_realize_time"],
                                                                     (int) reader["gold_production_rate"],
                                                                     (int) reader["iron"],
                                                                     (DateTime) reader["iron_realize_time"],
                                                                     (int) reader["iron_production_rate"],
                                                                     (int) reader["wood"],
                                                                     (DateTime) reader["wood_realize_time"],
                                                                     (int) reader["wood_production_rate"],
                                                                     (int) reader["labor"],
                                                                     (DateTime) reader["labor_realize_time"],
                                                                     (int) reader["labor_production_rate"]);
                            City city = new City(Global.Players[(uint) reader["player_id"]], (string) reader["name"],
                                                 resource, null);
                            city.Radius = (byte) reader["radius"];
                            city.DbPersisted = true;
                            Global.World.DbLoaderAdd((uint) reader["id"], city);
                        }
                    }

                    #endregion

                    #region Unit Template

                    using (reader = dbManager.Select(UnitTemplate.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);
                            city.Template.DbPersisted = true;

                            using (DbDataReader listReader = dbManager.SelectList(city.Template)) {
                                while (listReader.Read()) {
                                    city.Template.dbLoaderAdd((ushort) listReader["type"],
                                                              UnitFactory.GetUnitStats((ushort) listReader["type"],
                                                                                       (byte) listReader["level"]));
                                }
                            }
                        }
                    }

                    #endregion

                    #region Structures

                    using (reader = dbManager.Select(Structure.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);
                            Structure structure = StructureFactory.getStructure(null, (ushort) reader["type"],
                                                                                (byte) reader["level"], false, false);
                            structure.Technologies.Parent = city.Technologies;
                            structure.X = (uint) reader["x"];
                            structure.Y = (uint) reader["y"];
                            structure.Stats.Hp = (ushort) reader["hp"];
                            structure.ObjectId = (uint) reader["id"];
                            structure.Stats.Labor = (byte) reader["labor"];
                            structure.DbPersisted = true;
                            structure.State.Type = (ObjectState) ((byte) reader["state"]);

                            foreach (
                                object variable in XMLSerializer.DeserializeList((string) reader["state_parameters"]))
                                structure.State.Parameters.Add(variable);

                            city.Add(structure.ObjectId, structure, false);
                            Global.World.Add(structure);
                        }
                    }

                    #endregion

                    #region Structure Properties

                    using (reader = dbManager.Select(StructureProperties.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);
                            Structure structure = (Structure) city[(uint) reader["structure_id"]];

                            structure.Properties.DbPersisted = true;

                            using (DbDataReader listReader = dbManager.SelectList(structure.Properties)) {
                                while (listReader.Read()) {
                                    structure.Properties.add(listReader["name"],
                                                             DataTypeSerializer.Deserialize(
                                                                 (string) listReader["value"],
                                                                 (byte) listReader["datatype"]));
                                }
                            }
                        }
                    }

                    #endregion

                    #region Technologies

                    using (reader = dbManager.Select(TechnologyManager.DB_TABLE)) {
                        while (reader.Read()) {
                            EffectLocation ownerLocation = (EffectLocation) ((byte) reader["owner_location"]);

                            TechnologyManager manager;

                            if (ownerLocation == EffectLocation.Object) {
                                City city;
                                Global.World.TryGetObjects((uint) reader["city_id"], out city);
                                Structure structure = (Structure) city[(uint) reader["owner_id"]];
                                manager = structure.Technologies;
                            } else if (ownerLocation == EffectLocation.City) {
                                City city;
                                Global.World.TryGetObjects((uint) reader["city_id"], out city);
                                manager = city.Technologies;
                            } else
                                throw new Exception("Unknown effect location?");

                            manager.DbPersisted = true;

                            using (DbDataReader listReader = dbManager.SelectList(manager)) {
                                while (listReader.Read())
                                    manager.add(
                                        TechnologyFactory.getTechnology((uint) listReader["type"],
                                                                        (byte) listReader["level"]), false);
                            }
                        }
                    }

                    #endregion

                    #region Troop Stubs

                    using (reader = dbManager.Select(TroopStub.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);
                            City stationedCity = null;

                            if ((uint) reader["stationed_city_id"] != 0)
                                Global.World.TryGetObjects((uint) reader["stationed_city_id"], out stationedCity);

                            TroopStub stub = new TroopStub();

                            if ((byte) reader["id"] == 1)
                                stub = city.DefaultTroop;

                            stub.TroopManager = city.Troops;
                            stub.TroopId = (byte) reader["id"];
                            stub.State =
                                (TroopStub.TroopState)
                                Enum.Parse(typeof (TroopStub.TroopState), reader["state"].ToString());
                            stub.DbPersisted = true;

                            ushort formationMask = (ushort) reader["formations"];
                            FormationType[] formations = (FormationType[]) Enum.GetValues(typeof (FormationType));
                            foreach (FormationType type in formations) {
                                if ((formationMask & (ushort) Math.Pow(2, (ushort) type)) != 0)
                                    stub.AddFormation(type);
                            }

                            using (DbDataReader listReader = dbManager.SelectList(stub)) {
                                while (listReader.Read()) {
                                    stub.AddUnit((FormationType) ((byte) listReader["formation_type"]),
                                                 (ushort) listReader["type"], (ushort) listReader["count"]);
                                }
                            }

                            city.Troops.DbLoaderAdd((byte) reader["id"], stub);
                            if (stationedCity != null)
                                stationedCity.Troops.AddStationed(stub);
                        }
                    }

                    #endregion

                    #region Troop Stub's Templates

                    using (reader = dbManager.Select(TroopTemplate.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);
                            TroopStub stub = city.Troops[(byte) reader["troop_stub_id"]];
                            stub.Template.DbPersisted = true;

                            using (DbDataReader listReader = dbManager.SelectList(stub.Template)) {
                                while (listReader.Read()) {
                                    //First we load the BaseBattleStats and pass it into the BattleStats
                                    //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                                    BattleStats battleStats =
                                        new BattleStats(UnitFactory.GetBattleStats((ushort) listReader["type"],
                                                                                   (byte) listReader["level"]));
                                    battleStats.MaxHp = (ushort) listReader["max_hp"];
                                    battleStats.Atk = (byte) listReader["attack"];
                                    battleStats.Def = (byte) listReader["defense"];
                                    battleStats.Rng = (byte) listReader["range"];
                                    battleStats.Stl = (byte) listReader["stealth"];
                                    battleStats.Spd = (byte) listReader["speed"];

                                    stub.Template.DbLoaderAdd(battleStats);
                                }
                            }
                        }
                    }

                    #endregion

                    #region Troops

                    using (reader = dbManager.Select(TroopObject.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);
                            TroopStub stub = city.Troops[(byte) reader["troop_stub_id"]];
                            TroopObject obj = new TroopObject(stub);
                            obj.X = (uint) reader["x"];
                            obj.Y = (uint) reader["y"];
                            obj.ObjectId = (uint) reader["id"];
                            obj.DbPersisted = true;
                            obj.State.Type = (ObjectState) ((byte) reader["state"]);

                            foreach (
                                object variable in XMLSerializer.DeserializeList((string) reader["state_parameters"]))
                                obj.State.Parameters.Add(variable);

                            city.Add(obj.ObjectId, obj, false);
                            Global.World.Add(obj);
                        }
                    }

                    #endregion

                    #region Battle Managers

                    using (reader = dbManager.Select(BattleManager.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);

                            BattleManager bm = new BattleManager(city);
                            city.Battle = bm;
                            bm.DbPersisted = true;
                            bm.BattleId = (uint) reader["battle_id"];
                            bm.BattleStarted = (bool) reader["battle_started"];
                            bm.Round = (uint) reader["round"];
                            bm.Turn = (uint) reader["round"];
                            bm.Stamina = (ushort) reader["stamina"];

                            bm.BattleReport.ReportFlag = (bool) reader["report_flag"];
                            bm.BattleReport.ReportStarted = (bool) reader["report_started"];
                            bm.BattleReport.ReportId = (uint) reader["report_id"];

                            using (
                                DbDataReader listReader = dbManager.SelectList(CombatStructure.DB_TABLE,
                                                                               new DbColumn("city_id", city.Id,
                                                                                            DbType.UInt32))) {
                                while (listReader.Read()) {
                                    City structureCity;
                                    Global.World.TryGetObjects((uint) listReader["structure_city_id"], out structureCity);

                                    Structure structure = (Structure) structureCity[(uint) listReader["structure_id"]];

                                    //First we load the BaseBattleStats and pass it into the BattleStats
                                    //The BattleStats constructor will copy the basic values then we have to manually apply the values from the db
                                    BattleStats battleStats = new BattleStats(structure.Stats.Base.Battle);
                                    battleStats.MaxHp = (ushort) listReader["max_hp"];
                                    battleStats.Atk = (byte) listReader["attack"];
                                    battleStats.Def = (byte) listReader["defense"];
                                    battleStats.Rng = (byte) listReader["range"];
                                    battleStats.Stl = (byte) listReader["stealth"];
                                    battleStats.Spd = (byte) listReader["speed"];

                                    CombatStructure combatStructure = new CombatStructure(bm, structure, battleStats,
                                                                                          (uint) listReader["hp"],
                                                                                          (ushort) listReader["type"],
                                                                                          (byte) listReader["level"]);
                                    combatStructure.GroupId = (uint) listReader["group_id"];
                                    combatStructure.DmgDealt = (int) listReader["damage_dealt"];
                                    combatStructure.DmgRecv = (int) listReader["damage_received"];
                                    combatStructure.LastRound = (uint) listReader["last_round"];
                                    combatStructure.RoundsParticipated = (int) listReader["rounds_participated"];
                                    combatStructure.DbPersisted = true;

                                    bm.DbLoaderAddToLocal(combatStructure, (uint) listReader["id"]);
                                }
                            }

                            //this will load both defense/attack units (they are saved to same table)
                            using (
                                DbDataReader listReader = dbManager.SelectList(DefenseCombatUnit.DB_TABLE,
                                                                               new DbColumn("city_id", city.Id,
                                                                                            DbType.UInt32))) {
                                while (listReader.Read()) {
                                    City troopStubCity;
                                    Global.World.TryGetObjects((uint) listReader["troop_stub_city_id"],
                                                               out troopStubCity);
                                    TroopStub troopStub = troopStubCity.Troops[(byte) listReader["troop_stub_id"]];

                                    CombatObject combatObj;
                                    if ((bool) listReader["is_local"]) {
                                        combatObj = new DefenseCombatUnit(bm, troopStub,
                                                                          (FormationType)
                                                                          ((byte) listReader["formation_type"]),
                                                                          (ushort) listReader["type"],
                                                                          (byte) listReader["level"],
                                                                          (ushort) listReader["count"],
                                                                          (ushort) listReader["left_over_hp"]);
                                    } else {
                                        combatObj = new AttackCombatUnit(bm, troopStub,
                                                                         (FormationType)
                                                                         ((byte) listReader["formation_type"]),
                                                                         (ushort) listReader["type"],
                                                                         (byte) listReader["level"],
                                                                         (ushort) listReader["count"],
                                                                         (ushort) listReader["left_over_hp"]);
                                    }

                                    combatObj.GroupId = (uint) listReader["group_id"];
                                    combatObj.DmgDealt = (int) listReader["damage_dealt"];
                                    combatObj.DmgRecv = (int) listReader["damage_received"];
                                    combatObj.LastRound = (uint) listReader["last_round"];
                                    combatObj.RoundsParticipated = (int) listReader["rounds_participated"];
                                    combatObj.DbPersisted = true;

                                    bm.DbLoaderAddToCombatList(combatObj, (uint) listReader["id"],
                                                               (bool) listReader["is_local"]);
                                }
                            }

                            bm.ReportedTroops.DbPersisted = true;
                            using (DbDataReader listReader = dbManager.SelectList(bm.ReportedTroops)) {
                                while (listReader.Read()) {
                                    City troopStubCity;
                                    Global.World.TryGetObjects((uint) listReader["troop_stub_city_id"],
                                                               out troopStubCity);
                                    TroopStub troopStub = troopStubCity.Troops[(byte) listReader["troop_stub_id"]];

                                    if (troopStub == null)
                                        continue;

                                    bm.ReportedTroops[troopStub] = (uint) listReader["combat_troop_id"];
                                }
                            }

                            bm.ReportedObjects.DbPersisted = true;
                            using (DbDataReader listReader = dbManager.SelectList(bm.ReportedObjects)) {
                                while (listReader.Read()) {
                                    CombatObject co = bm.GetCombatObject((uint) listReader["combat_object_id"]);

                                    if (co == null)
                                        continue;

                                    bm.ReportedObjects.Add(co);
                                }
                            }

                            bm.RefreshBattleOrder();
                        }
                    }

                    #endregion

                    #region Load action setup

                    //Setup for loading actions
                    if (!Global.SystemVariables.ContainsKey("System.time"))
                        Global.SystemVariables.Add("System.time", new SystemVariable("System.time", DateTime.Now));
                    TimeSpan downTime = now.Subtract((DateTime) Global.SystemVariables["System.time"].Value);

                    #endregion

                    #region Active Actions

                    Type[] types = new[] {
                                             typeof (ushort), typeof (DateTime), typeof (DateTime), typeof (DateTime),
                                             typeof (int), typeof (byte), typeof (ushort), typeof (Dictionary<string, string>)
                                         };

                    using (reader = dbManager.Select(ActiveAction.DB_TABLE)) {
                        while (reader.Read()) {
                            ActionType actionType = (ActionType) ((int) reader["type"]);
                            Type type =
                                Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "ACTION",
                                             true, true);
                            ConstructorInfo cInfo = type.GetConstructor(types);

                            DateTime beginTime = ((DateTime) reader["begin_time"]).Add(downTime);

                            DateTime nextTime = (DateTime) reader["next_time"];
                            if (nextTime != DateTime.MinValue)
                                nextTime.Add(downTime);

                            DateTime endTime = ((DateTime) reader["end_time"]).Add(downTime);

                            Dictionary<string, string> properties =
                                XMLSerializer.Deserialize((string) reader["properties"]);
                            object[] parms = new object[] {
                                                              (ushort) reader["id"], beginTime, nextTime, endTime,
                                                              (int) reader["worker_type"], (byte) reader["worker_index"],
                                                              (ushort) reader["count"], properties
                                                          };

                            ScheduledActiveAction action = (ScheduledActiveAction) cInfo.Invoke(parms);
                            action.DbPersisted = true;

                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);

                            uint workerId = (uint) reader["object_id"];
                            if (workerId == 0)
                                action.WorkerObject = city;
                            else
                                action.WorkerObject = city[(uint) reader["object_id"]];

                            city.Worker.DbLoaderDoActive(action);

                            Global.dbManager.Save(action);
                        }
                    }

                    #endregion

                    #region Passive Actions

                    Dictionary<uint, List<PassiveAction>> chainActions = new Dictionary<uint, List<PassiveAction>>();
                        //this will hold chain actions that we encounter for the next phase

                    types = new[] {typeof (ushort), typeof (bool), typeof (Dictionary<string, string>)};

                    Type[] scheduledTypes = new[] {
                                                      typeof (ushort), typeof (DateTime), typeof (DateTime), typeof (DateTime),
                                                      typeof (bool), typeof (Dictionary<string, string>)
                                                  };

                    using (reader = dbManager.Select(PassiveAction.DB_TABLE)) {
                        while (reader.Read()) {
                            ActionType actionType = (ActionType) ((int) reader["type"]);
                            Type type =
                                Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "ACTION",
                                             true, true);

                            ConstructorInfo cInfo;

                            if ((bool) reader["is_scheduled"])
                                cInfo = type.GetConstructor(scheduledTypes);
                            else
                                cInfo = type.GetConstructor(types);

                            if (cInfo == null)
                                throw new Exception(type.Name + " is missing the db loader constructor");

                            Dictionary<string, string> properties =
                                XMLSerializer.Deserialize((string) reader["properties"]);

                            object[] parms;
                            PassiveAction action;

                            if ((bool) reader["is_scheduled"]) {
                                DateTime beginTime = ((DateTime) reader["begin_time"]);
                                beginTime = beginTime.Add(downTime);

                                DateTime nextTime = (DateTime) reader["next_time"];
                                if (nextTime != DateTime.MinValue)
                                    nextTime = nextTime.Add(downTime);

                                DateTime endTime = ((DateTime) reader["end_time"]);
                                endTime = endTime.Add(downTime);

                                parms = new object[] {
                                                         (ushort) reader["id"], beginTime, nextTime, endTime,
                                                         (bool) reader["is_visible"], properties
                                                     };
                            } else
                                parms = new object[] {(ushort) reader["id"], (bool) reader["is_visible"], properties};

                            action = (PassiveAction) cInfo.Invoke(parms);
                            action.DbPersisted = true;

                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);

                            uint workerId = (uint) reader["object_id"];
                            if (workerId == 0)
                                action.WorkerObject = city;
                            else
                                action.WorkerObject = city[workerId];

                            if ((bool) reader["is_chain"] == false)
                                city.Worker.DbLoaderDoPassive(action);
                            else {
                                List<PassiveAction> chainList;
                                if (!chainActions.TryGetValue(city.Id, out chainList)) {
                                    chainList = new List<PassiveAction>();
                                    chainActions[city.Id] = chainList;
                                }

                                action.IsChain = true;
                                chainList.Add(action);
                            }

                            Global.dbManager.Save(action);
                        }
                    }

                    #endregion

                    #region Chain Actions

                    types = new[] {
                                      typeof (ushort), typeof (string), typeof (PassiveAction), typeof (ActionState),
                                      typeof (bool), typeof (Dictionary<string, string>)
                                  };

                    using (reader = dbManager.Select(ChainAction.DB_TABLE)) {
                        while (reader.Read()) {
                            ActionType actionType = (ActionType) ((int) reader["type"]);
                            Type type =
                                Type.GetType("Game.Logic.Actions." + actionType.ToString().Replace("_", "") + "ACTION",
                                             true, true);
                            ConstructorInfo cInfo = type.GetConstructor(types);

                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);

                            ushort currentActionId = (ushort) reader["current_action_id"];

                            List<PassiveAction> chainList = null;
                            PassiveAction currentAction = null;
                                //current action might be null if it has already completed and we are in the call chain part of the cycle
                            if (chainActions.TryGetValue(city.Id, out chainList))
                                currentAction =
                                    chainList.Find(
                                        delegate(PassiveAction lookupAction) { return lookupAction.ActionId == currentActionId; });

                            Dictionary<string, string> properties =
                                XMLSerializer.Deserialize((string) reader["properties"]);
                            object[] parms = new object[] {
                                                              (ushort) reader["id"], reader["chain_callback"], currentAction,
                                                              (ActionState) ((byte) reader["chain_state"]), (bool) reader["is_visible"],
                                                              properties
                                                          };

                            ChainAction action = (ChainAction) cInfo.Invoke(parms);
                            action.DbPersisted = true;

                            uint workerId = (uint) reader["object_id"];
                            if (workerId == 0)
                                action.WorkerObject = city;
                            else
                                action.WorkerObject = city[(uint) reader["object_id"]];

                            city.Worker.DbLoaderDoPassive(action);

                            Global.dbManager.Save(action);
                        }
                    }

                    #endregion

                    #region Action References

                    using (reader = dbManager.Select(ReferenceStub.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);

                            Action action = null;
                            if ((bool) reader["is_active"])
                                action = city.Worker.ActiveActions[(ushort) reader["action_id"]];
                            else
                                action = city.Worker.PassiveActions[(ushort) reader["action_id"]];

                            ICanDo obj;
                            uint workerId = (uint) reader["object_id"];
                            if (workerId == 0)
                                obj = city;
                            else
                                obj = city[(uint) reader["object_id"]];

                            ReferenceStub referenceStub = new ReferenceStub((ushort) reader["id"], obj, action);

                            referenceStub.DbPersisted = true;

                            city.Worker.References.DbLoaderAdd(referenceStub);
                        }
                    }

                    #endregion

                    #region Action Notifications

                    using (reader = dbManager.Select(NotificationManager.Notification.DB_TABLE)) {
                        while (reader.Read()) {
                            City city;
                            Global.World.TryGetObjects((uint) reader["city_id"], out city);

                            GameObject obj = city[(uint) reader["object_id"]];
                            PassiveAction action = city.Worker.PassiveActions[(ushort) reader["action_id"]];

                            NotificationManager.Notification notification = new NotificationManager.Notification(obj,
                                                                                                                 action);

                            using (DbDataReader listReader = dbManager.SelectList(notification)) {
                                while (listReader.Read()) {
                                    City subscriptionCity;
                                    Global.World.TryGetObjects((uint) listReader["subscription_city_id"],
                                                               out subscriptionCity);
                                    notification.Subscriptions.Add(subscriptionCity);
                                }
                            }

                            city.Worker.Notifications.dbLoaderAdd(false, notification);

                            notification.DbPersisted = true;
                        }
                    }

                    #endregion

                    Global.World.AfterDbLoaded();

                    //Ok data all loaded. We can get the system going now.
                    Global.SystemVariables["System.time"].Value = now;
                    Global.dbManager.Save(Global.SystemVariables["System.time"]);
                }
                catch (Exception e) {
                    Global.Logger.Error("Database loader error", e);
                    transaction.Rollback();
                    return false;
                }
            }

            Global.Logger.Info("Database loading finished");

            SystemTimeUpdater.Resume();
            Global.Scheduler.resume();
            Global.resumeEvents();
            return true;
        }
    }
}