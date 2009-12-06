using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Logic;
using Game.Setup;
using Game.Data;
using Game.Util;
using Game.Database;
using Game.Logic.Procedures;
using Game.Fighting;
using Game.Logic.Actions;

namespace Game.Module {
    public class AI : ISchedule {
        static readonly List<ushort> ALLOWED_BUILDINGS = new List<ushort>(new ushort[] { 2110, 2202, 2301, 2302, 2501, 2502, 2402 });

        DateTime time;
        List<Intelligence> playerList = new List<Intelligence>();

        class Intelligence {
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

        public AI() {
        }

        #region ISchedule Members

        public DateTime Time {
            get { return time; }
        }

        Random rand = new Random();

        public void callback(object custom) {
            if (playerList.Count == 0) {
                time = DateTime.Now.AddSeconds(5);
                Global.Scheduler.put(this);
                return;
            }

            int cnt = (int)(playerList.Count * 0.10);

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

                            Map.RadiusLocator.random_point(mainBuilding.X, mainBuilding.Y, (byte)(city.Radius - 1), true, out x, out y);

                            byte step = (byte)rand.Next(0, 4);

                            switch (step) {
                                case 0:
                                    if (rand.Next(0, 100) < 100 * intelligence.builder)
                                        ret = UpgradeStructure(city, x, y);
                                    break;
                                case 1:
                                    if (rand.Next(0, 100) < 100 * intelligence.builder)
                                        ret = BuildStructure(city, x, y);
                                    break;
                                case 2:
                                    if (rand.Next(0, 100) < 100 * intelligence.military)
                                        ret = TrainUnit(intelligence, city);
                                    break;
                                case 3:
                                    ret = SetLabor(city);
                                    break;
                                case 4:
                                    intelligence.savingUp = (byte)(rand.Next(5, 20) * intelligence.builder);
                                    ret = true;
                                    break;
                                default:
                                    break;
                            }

                            if (count > 20)
                                Global.Logger.Warn("NPC loop count at " + count);
                        } while (!ret);
                    }
                }
            }

            time = DateTime.Now.AddSeconds(8);
            Global.Scheduler.put(this);
        }

        bool SetLabor(City city) {
            if (city.Resource.Labor.Value == 0) return true;

            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;

                if (structure.Stats.MaxLabor > 0) {
                    structure.BeginUpdate();
                    structure.Labor = Math.Min(structure.Stats.MaxLabor, (byte)city.Resource.Labor.Value);
                    structure.EndUpdate();
                }

                if (city.Resource.Labor.Value == 0) break;
            }

            return true;
        }

        bool TrainUnit(Intelligence intelligence, City city) {           
            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;

                int workerType = StructureFactory.getActionWorkerType(structure);
                ActionRecord record = ActionFactory.getActionRequirementRecord(workerType);
                if (record == null) continue;

                foreach (ActionRequirement req in record.list) {
                    if (rand.Next(100) > 60) continue;

                    if (req.type == ActionType.UNIT_TRAIN) {                        
                        ushort unitType = ushort.Parse(req.parms[0]);
                        Resource costPerUnit = city.Template[unitType].resource;
                        ushort count = Math.Min((ushort)15, (ushort)(city.Resource.FindMaxAffordable(costPerUnit) * intelligence.military));

                        UnitTrainAction action = new UnitTrainAction(city.CityId, structure.ObjectID, unitType, count);
                        if (city.Worker.doActive(workerType, structure, action, structure.Technologies) == Error.OK) {
                            Global.Logger.Info(string.Format("{0} training {1} units of type {2} at ({3},{4})", city.Name, count, unitType, structure.X, structure.Y));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool BuildStructure(City city, uint x, uint y) {
            List<GameObject> objects = Global.World.getObjects(x, y);
            if (objects.Count > 0) return false;

            Dictionary<uint, Structure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext()) {
                Structure structure = enumerator.Current.Value;
                int workerType = StructureFactory.getActionWorkerType(structure);
                ActionRecord record = ActionFactory.getActionRequirementRecord(workerType);
                if (record == null) continue;

                foreach (ActionRequirement req in record.list) {
                    if (rand.Next(100) > 60) continue;

                    if (req.type == ActionType.STRUCTURE_BUILD) {                        
                        ushort buildingType = ushort.Parse(req.parms[0]);
                        if (!ALLOWED_BUILDINGS.Contains(buildingType)) continue;

                        StructureBuildAction action = new StructureBuildAction(city.CityId, buildingType, x, y);
                        if (city.Worker.doActive(workerType, structure, action, structure.Technologies) == Error.OK) {
                            Global.Logger.Info(string.Format("{0} building {1} at ({2},{3})", city.Name, buildingType, structure.Lvl, x, y));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool UpgradeStructure(City city, uint x, uint y) {
            List<GameObject> objects = Global.World.getObjects(x, y);
            Structure structure = null;

            foreach (GameObject obj in objects) {
                if (obj is Structure && obj.City == city) {
                    structure = obj as Structure;
                    break;
                }
            }

            if (structure == null) return false;

            StructureUpgradeAction action = new StructureUpgradeAction(city.CityId, structure.ObjectID);

            if (city.Worker.doActive(StructureFactory.getActionWorkerType(structure), structure, action, structure.Technologies) == Error.OK) {
                Global.Logger.Info(string.Format("{0} upgrading {1}({2}) at ({3},{4})", city.Name, structure.Type, structure.Lvl, x, y));
                return true;
            }
            else {
                return false;
            }
        }
        #endregion

        public static void Init() {
            Global.Scheduler.pause();

            Random rand = new Random();

            Global.Logger.Info("Loading AI...");

            for (uint i = 1; i <= 200; ++i) {
                uint idx = 500 + i;

                Player npc = new Player(idx, "NPC " + i);
                Intelligence intelligence = new Intelligence(npc, Math.Max(0.5, rand.NextDouble()), Math.Max(0.5, rand.NextDouble()));

                using (new MultiObjectLock(npc)) {

                    if (!Global.Players.ContainsKey(idx)) {
                        Global.Players.Add(idx, npc);
                        Global.dbManager.Save(npc);
                        Global.AI.playerList.Add(intelligence);
                    }
                    else {
                        intelligence.player = Global.Players[idx];
                        Global.AI.playerList.Add(intelligence);
                        continue;
                    }

                    List<City> cities = npc.getCityList();

                    Structure structure;
                    if (!Randomizer.MainBuilding(out structure, 2)) {
                        Global.Logger.Info(npc.Name);
                        break;
                    }

                    City city = new City(npc, string.Format("{0} {1}", npc.Name, npc.getCityList().Count + 1), new Resource(500, 500, 500, 500, 10), structure);

                    Global.World.add(city);
                    Global.World.add(structure);

                    InitFactory.initGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);

                    city.Worker.doPassive(city, new CityAction(city.CityId), false);

                    Map.RadiusLocator.foreach_object(structure.X, structure.Y, (byte)(city.Radius - 1), false, BuildBasicStructures, city);
                }
            }

            Global.AI.time = DateTime.Now.AddSeconds(10);
            Global.Scheduler.put(Global.AI);

            Global.Logger.Info("Loading AI finished.");

            Global.Scheduler.resume();
        }

        static bool BuildBasicStructures(uint origX, uint origY, uint x, uint y, object custom) {
            City city = (custom as City);

            ushort tileType = Global.World.getTileType(x, y);
            if (ObjectTypeFactory.IsTileType("TileTree", tileType)) { // Lumber mill
                Structure structure = StructureFactory.getStructure(2107, 1);
                structure.X = x;
                structure.Y = y;
                structure.Labor = structure.Stats.MaxLabor;

                city.add(structure);
                Global.World.add(structure);
            }
            else if (ObjectTypeFactory.IsTileType("TileCrop", tileType)) { // Farm
                Structure structure = StructureFactory.getStructure(2106, 1);
                structure.X = x;
                structure.Y = y;
                structure.Labor = structure.Stats.MaxLabor;

                city.add(structure);
                Global.World.add(structure);
            }
            else if (x == origX - 1 && y == origY - 1) { // Barrack
                Structure structure = StructureFactory.getStructure(2201, 1);
                structure.X = x;
                structure.Y = y;

                city.add(structure);
                Global.World.add(structure);
            }

            return true;
        }

    }
}

