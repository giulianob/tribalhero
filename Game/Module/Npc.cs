#region

using System;
using System.Collections.Generic;
using Game.Data;
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

        public DateTime Time {
            get { return time; }
        }

        private Random rand = new Random();

        public void Callback(object custom) {
            if (playerList.Count == 0) {
                time = DateTime.Now.AddSeconds(5);
                Global.Scheduler.put(this);
                return;
            }

            //we want there to be relatively 
            int cnt = (int) (playerList.Count*0.25);

            DateTime now = DateTime.Now;
            int successCount = 0;

            for (int z = 0; z < cnt; ++z) {
                Intelligence intelligence = playerList[rand.Next(0, playerList.Count - 1)];
                
                using (new MultiObjectLock(intelligence.player)) {
                    if (intelligence.savingUp > 0) {
                        intelligence.savingUp--;
                        continue;
                    }

                    foreach (City city in intelligence.player.getCityList()) {
                        Structure mainBuilding = city.MainBuilding;

                        bool ret = false;
                        int count = 0;
                        do {
                            count++;

                            uint x;
                            uint y;

                            RadiusLocator.random_point(mainBuilding.X, mainBuilding.Y, (byte) (city.Radius - 1), true,
                                                       out x, out y);

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
                                    if (rand.Next(0, 100) < 100*intelligence.military)
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

                            if (ret) successCount++;

                            break;

                            if (count > 20)
                                Global.Logger.Warn("NPC loop count at " + count);
                        } while (!ret);
                    }
                }                
            }

            int timeTaken = (int)DateTime.Now.Subtract(now).TotalMilliseconds;
            Global.Logger.Info(String.Format("Took {0} ms for {1} actions. Average: {2}ms", timeTaken, successCount, (double)timeTaken / successCount));

            time = DateTime.Now.AddSeconds(30 * Config.seconds_per_unit);
            Global.Scheduler.put(this);
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

                int workerType = StructureFactory.getActionWorkerType(structure);
                ActionRecord record = ActionFactory.getActionRequirementRecord(workerType);
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

                        UnitTrainAction action = new UnitTrainAction(city.CityId, structure.ObjectId, unitType, count);
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
            List<GameObject> objects = Global.World.GetObjects(x, y);
            if (objects.Count > 0)
                return false;

            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;
                int workerType = StructureFactory.getActionWorkerType(structure);
                ActionRecord record = ActionFactory.getActionRequirementRecord(workerType);
                if (record == null)
                    continue;

                foreach (ActionRequirement req in record.list) {
                    if (rand.Next(100) > 60)
                        continue;

                    if (req.type == ActionType.STRUCTURE_BUILD) {
                        ushort buildingType = ushort.Parse(req.parms[0]);
                        if (!ALLOWED_BUILDINGS.Contains(buildingType))
                            continue;

                        StructureBuildAction action = new StructureBuildAction(city.CityId, buildingType, x, y);
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
            List<GameObject> objects = Global.World.GetObjects(x, y);
            Structure structure = null;

            foreach (GameObject obj in objects) {
                if (obj is Structure && obj.City == city) {
                    structure = obj as Structure;
                    break;
                }
            }

            if (structure == null)
                return false;

            StructureUpgradeAction action = new StructureUpgradeAction(city.CityId, structure.ObjectId);

            if (
                city.Worker.DoActive(StructureFactory.getActionWorkerType(structure), structure, action,
                                     structure.Technologies) == Error.OK) {
                //Global.Logger.Info(string.Format("{0} upgrading {1}({2}) at ({3},{4})", city.Name, structure.Type, structure.Stats.Base.Lvl, x, y));
                return true;
            } else
                return false;
        }

        #endregion

        public static void Init() {
            Global.Scheduler.pause();

            Random rand = new Random();

            Global.Logger.Info("Loading AI...");

            for (uint i = 1; i <= Config.ai_count; ++i) {
                
                if (i%100 == 0)
                    Global.Logger.Info(String.Format("Creating NPC {0}/{1}...", i, Config.ai_count));

                uint idx = 500000 + i;

                Player npc = new Player(idx, "NPC " + i);
                Intelligence intelligence = new Intelligence(npc, Math.Max(0.5, rand.NextDouble()),
                                                             Math.Max(0.5, rand.NextDouble()));

                using (new MultiObjectLock(npc)) {
                    if (!Global.Players.ContainsKey(idx)) {
                        Global.Players.Add(idx, npc);
                        Global.dbManager.Save(npc);
                        Global.AI.playerList.Add(intelligence);
                    } else {
                        intelligence.player = Global.Players[idx];
                        Global.AI.playerList.Add(intelligence);
                        continue;
                    }

                    List<City> cities = npc.getCityList();

                    Structure structure;
                    if (!Randomizer.MainBuilding(out structure)) {
                        Global.Logger.Info(npc.Name);
                        break;
                    }

                    City city = new City(npc, string.Format("{0} {1}", npc.Name, npc.getCityList().Count + 1),
                                         new Resource(500, 500, 500, 500, 10), structure);

                    Global.World.Add(city);
                    Global.World.Add(structure);

                    InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type,
                                               structure.Stats.Base.Lvl);

                    city.Worker.DoPassive(city, new CityAction(city.CityId), false);

                    RadiusLocator.foreach_object(structure.X, structure.Y, (byte) (city.Radius - 1), false,
                                                 BuildBasicStructures, city);
                }
            }

            Global.AI.time = DateTime.Now.AddSeconds(10);
            Global.Scheduler.put(Global.AI);

            Global.Logger.Info("Loading AI finished.");

            Global.Scheduler.resume();
        }

        private static bool BuildBasicStructures(uint origX, uint origY, uint x, uint y, object custom) {
            City city = (custom as City);

            ushort tileType = Global.World.GetTileType(x, y);
            if (ObjectTypeFactory.IsTileType("TileTree", tileType)) {
                // Lumber mill
                Structure structure = StructureFactory.getStructure(2107, 1);
                structure.X = x;
                structure.Y = y;
                structure.Stats.Labor = structure.Stats.Base.MaxLabor;

                city.Add(structure);
                Global.World.Add(structure);
            } else if (ObjectTypeFactory.IsTileType("TileCrop", tileType)) {
                // Farm
                Structure structure = StructureFactory.getStructure(2106, 1);
                structure.X = x;
                structure.Y = y;
                structure.Stats.Labor = structure.Stats.Base.MaxLabor;

                city.Add(structure);
                Global.World.Add(structure);
            } else if (x == origX - 1 && y == origY - 1) {
                // Barrack
                Structure structure = StructureFactory.getStructure(2201, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                Global.World.Add(structure);
            }

            return true;
        }
    }
}