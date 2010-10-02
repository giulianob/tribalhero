#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Module {
    public class AI : ISchedule {
        private static List<ushort> ALLOWED_BUILDINGS =
            new List<ushort>(new ushort[] {2110, 2202, 2301, 2302, 2501, 2502, 2402});

        private DateTime time;
        private List<Intelligence> playerList = new List<Intelligence>();

        private class Intelligence {
            public Player player;
            public byte savingUp; // how many rounds it's saving up for

            public double builder;
            public double military;

            public Intelligence(Player player, double builder, double military) {
                this.player = player;
                this.builder = builder;
                this.military = military;
            }
        }

        #region ISchedule Members
        
        public bool IsScheduled { get; set; }

        public DateTime Time {
            get { return time; }
        }

        private Random rand = new Random();

        public void Callback(object custom) {
            if (playerList.Count == 0) {
                time = DateTime.UtcNow.AddSeconds(5);
                Global.Scheduler.Put(this);
                return;
            }

            //we want there to be relatively 
            int cnt = (int) (playerList.Count*0.25);

            DateTime now = DateTime.UtcNow;
            int successCount = 0;

            for (int z = 0; z < cnt; ++z) {
                Intelligence intelligence = playerList[rand.Next(0, playerList.Count - 1)];
                
                using (new MultiObjectLock(intelligence.player)) {
                    if (intelligence.savingUp > 0) {
                        intelligence.savingUp--;
                        continue;
                    }

                    foreach (City city in intelligence.player.GetCityList()) {
                        Structure mainBuilding = city.MainBuilding;

                        bool ret = false;

                        uint x;
                        uint y;

                        TileLocator.random_point(mainBuilding.X, mainBuilding.Y, (byte) (city.Radius - 1), true, out x, out y);

                        byte step = (byte) rand.Next(0, 4);

                        switch (step) {
                            case 0:
                                if (rand.Next(0, 100) < 100*intelligence.builder)
                                    ret = UpgradeStructure(city, x, y);
                                break;
                            case 1:
                                if (rand.Next(0, 100) < 100*intelligence.builder)
                                    ret = BuildStructure(city, x, y);
                                break;
                            case 2:
                                if (rand.Next(0, 1000) < 100*intelligence.military)
                                    ret = TrainUnit(intelligence, city);
                                break;
                            case 3:
                                ret = SetLabor(city);
                                break;
                            case 4:
                                intelligence.savingUp = (byte) (rand.Next(5, 20)*intelligence.builder);
                                ret = true;
                                break;
                            default:
                                break;
                        }

                        if (ret)
                            successCount++;
                    }
                }                
            }

            int timeTaken = (int)DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            Global.Logger.Info(String.Format("Took {0} ms for {1} actions. Average: {2}ms", timeTaken, successCount, (double)timeTaken / successCount));

            time = DateTime.UtcNow.AddSeconds(30 * Config.seconds_per_unit);
            Global.Scheduler.Put(this);
        }

        private bool SetLabor(City city) {
            if (city.Resource.Labor.Value == 0)
                return true;

            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;

                if (structure.Stats.Base.MaxLabor > 0) {
                    structure.BeginUpdate();
                    structure.Stats.Labor = Math.Min(structure.Stats.Base.MaxLabor, (byte) city.Resource.Labor.Value);
                    structure.EndUpdate();
                }

                if (city.Resource.Labor.Value == 0)
                    break;
            }

            return true;
        }

        private bool TrainUnit(Intelligence intelligence, City city) {
            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;

                int workerType = StructureFactory.GetActionWorkerType(structure);
                ActionRecord record = ActionFactory.GetActionRequirementRecord(workerType);
                if (record == null)
                    continue;

                foreach (ActionRequirement req in record.list) {
                    if (rand.Next(100) > 60)
                        continue;

                    if (req.type == ActionType.UNIT_TRAIN) {
                        ushort unitType = ushort.Parse(req.parms[0]);
                        Resource costPerUnit = city.Template[unitType].Cost;
                        ushort count = Math.Min((ushort) 15,
                                                (ushort)
                                                (city.Resource.FindMaxAffordable(costPerUnit)*intelligence.military));

                        UnitTrainAction action = new UnitTrainAction(city.Id, structure.ObjectId, unitType, count);
                        if (city.Worker.DoActive(workerType, structure, action, structure.Technologies) == Error.OK) {
                            //Global.Logger.Info(string.Format("{0} training {1} units of type {2} at ({3},{4})", city.Name, count, unitType, structure.X, structure.Y));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool BuildStructure(City city, uint x, uint y) {
            List<SimpleGameObject> objects = Global.World.GetObjects(x, y);
            if (objects.Count > 0)
                return false;

            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;
                int workerType = StructureFactory.GetActionWorkerType(structure);
                ActionRecord record = ActionFactory.GetActionRequirementRecord(workerType);
                if (record == null)
                    continue;

                foreach (ActionRequirement req in record.list) {
                    if (rand.Next(100) > 60)
                        continue;

                    if (req.type == ActionType.STRUCTURE_BUILD) {
                        ushort buildingType = ushort.Parse(req.parms[0]);
                        if (!ALLOWED_BUILDINGS.Contains(buildingType))
                            continue;

                        StructureBuildAction action = new StructureBuildAction(city.Id, buildingType, x, y);
                        if (city.Worker.DoActive(workerType, structure, action, structure.Technologies) == Error.OK) {
                            //Global.Logger.Info(string.Format("{0} building {1} at ({2},{3})", city.Name, buildingType, structure.Stats.Base.Lvl, x, y));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool UpgradeStructure(City city, uint x, uint y) {
            List<SimpleGameObject> objects = Global.World.GetObjects(x, y);
            Structure structure = null;

            foreach (GameObject obj in objects) {
                if (obj is Structure && obj.City == city) {
                    structure = obj as Structure;
                    break;
                }
            }

            if (structure == null)
                return false;

            StructureUpgradeAction action = new StructureUpgradeAction(city.Id, structure.ObjectId);

            if (
                city.Worker.DoActive(StructureFactory.GetActionWorkerType(structure), structure, action,
                                     structure.Technologies) == Error.OK) {
                //Global.Logger.Info(string.Format("{0} upgrading {1}({2}) at ({3},{4})", city.Name, structure.Type, structure.Stats.Base.Lvl, x, y));
                return true;
            } else
                return false;
        }

        #endregion

        public static void Init() {
            Global.Scheduler.Pause();

            Random rand = new Random();

            Global.Logger.Info("Loading AI...");

            for (uint i = 1; i <= Config.ai_count; ++i) {

                if (i%100 == 0)
                    Global.Logger.Info(String.Format("Creating NPC {0}/{1}...", i, Config.ai_count));

                uint idx = 500000 + i;

                Player npc = new Player(idx, DateTime.MinValue, SystemClock.Now, "NPC " + i, false);
                Intelligence intelligence = new Intelligence(npc, Math.Max(0.5, rand.NextDouble()), Math.Max(0.5, rand.NextDouble()));

                using (new MultiObjectLock(npc)) {
                    if (!Global.Players.ContainsKey(idx)) {
                        Global.Players.Add(idx, npc);
                        Global.DbManager.Save(npc);
                        Global.Ai.playerList.Add(intelligence);
                    } else {
                        intelligence.player = Global.Players[idx];
                        Global.Ai.playerList.Add(intelligence);
                        continue;
                    }

                    List<City> cities = npc.GetCityList();

                    Structure structure;
                    if (!Randomizer.MainBuilding(out structure, 2)) {
                        Global.Logger.Info(npc.Name);
                        break;
                    }

                    City city = new City(npc, string.Format("{0} {1}", npc.Name, npc.GetCityList().Count + 1), new Resource(500, 500, 500, 500, 10), structure);

                    Global.World.Add(city);
                    structure.BeginUpdate();
                    Global.World.Add(structure);
                    structure.EndUpdate();
                    
                    
                    TroopStub defaultTroop = new TroopStub();
                    defaultTroop.AddFormation(FormationType.NORMAL);
                    defaultTroop.AddFormation(FormationType.GARRISON);
                    defaultTroop.AddFormation(FormationType.IN_BATTLE);
                    city.Troops.Add(defaultTroop);
                    

                    InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Stats.Base.Lvl);

                    city.Worker.DoPassive(city, new CityAction(city.Id), false);

                    //TileLocator.foreach_object(structure.X, structure.Y, (byte) (city.Radius - 1), false, BuildBasicStructures, city);
                }
            }

            Global.Ai.time = DateTime.UtcNow.AddSeconds(10);
            //Global.Scheduler.Put(Global.Ai);

            Global.Logger.Info("Loading AI finished.");

            Global.Scheduler.Resume();
        }

        private static bool BuildBasicStructures(uint origX, uint origY, uint x, uint y, object custom) {
            City city = (custom as City);

            ushort tileType = Global.World.GetTileType(x, y);
            if (ObjectTypeFactory.IsTileType("TileTree", tileType)) {
                // Lumber mill
                Structure structure = StructureFactory.GetNewStructure(2107, 1);
                structure.X = x;
                structure.Y = y;
                structure.Stats.Labor = structure.Stats.Base.MaxLabor;

                city.Add(structure);
                Global.World.Add(structure);
            } else if (ObjectTypeFactory.IsTileType("TileCrop", tileType)) {
                // Farm
                Structure structure = StructureFactory.GetNewStructure(2106, 1);
                structure.X = x;
                structure.Y = y;
                structure.Stats.Labor = structure.Stats.Base.MaxLabor;

                city.Add(structure);
                Global.World.Add(structure);
            } else if (x == origX - 1 && y == origY - 1) {
                // Barrack
                Structure structure = StructureFactory.GetNewStructure(2201, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                Global.World.Add(structure);
            }

            return true;
        }
    }
}