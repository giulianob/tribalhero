#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Map.LocationStrategies;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Module
{
    public class Ai : ISchedule
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private static readonly List<ushort> allowedBuildings =
                new List<ushort>(new ushort[] {2110, 2202, 2301, 2302, 2501, 2502, 2402});

        private readonly List<Intelligence> playerList = new List<Intelligence>();

        private readonly Random rand = new Random();

        private DateTime time;

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time
        {
            get
            {
                return time;
            }
        }

        public void Callback(object custom)
        {
            if (playerList.Count == 0)
            {
                time = DateTime.UtcNow.AddSeconds(5);
                Scheduler.Current.Put(this);
                return;
            }

            //we want there to be relatively 
            var cnt = (int)(playerList.Count * 0.25);

            DateTime now = DateTime.UtcNow;
            int successCount = 0;

            for (int z = 0; z < cnt; ++z)
            {
                Intelligence intelligence = playerList[rand.Next(0, playerList.Count - 1)];

                using (Concurrency.Current.Lock(intelligence.player))
                {
                    if (intelligence.savingUp > 0)
                    {
                        intelligence.savingUp--;
                        continue;
                    }

                    foreach (var city in intelligence.player.GetCityList())
                    {
                        bool ret = false;

                        uint x;
                        uint y;

                        TileLocator.Current.RandomPoint(city.X, city.Y, (byte)(city.Radius - 1), true, out x, out y);

                        var step = (byte)rand.Next(0, 4);

                        switch(step)
                        {
                            case 0:
                                if (rand.Next(0, 100) < 100 * intelligence.builder)
                                {
                                    ret = UpgradeStructure(city, x, y);
                                }
                                break;
                            case 1:
                                if (rand.Next(0, 100) < 100 * intelligence.builder)
                                {
                                    ret = BuildStructure(city, x, y);
                                }
                                break;
                            case 2:
                                if (rand.Next(0, 1000) < 100 * intelligence.military)
                                {
                                    ret = TrainUnit(intelligence, city);
                                }
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

                        if (ret)
                        {
                            successCount++;
                        }
                    }
                }
            }

            var timeTaken = (int)DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            logger.Info(String.Format("Took {0} ms for {1} actions. Average: {2}ms",
                                             timeTaken,
                                             successCount,
                                             (double)timeTaken / successCount));

            time = DateTime.UtcNow.AddSeconds(30 * Config.seconds_per_unit);
            Scheduler.Current.Put(this);
        }

        #endregion

        private static bool SetLabor(ICity city)
        {
            if (city.Resource.Labor.Value == 0)
            {
                return true;
            }

            Dictionary<uint, IStructure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext())
            {
                IStructure structure = enumerator.Current.Value;

                if (structure.Stats.Base.MaxLabor > 0)
                {
                    structure.BeginUpdate();
                    structure.Stats.Labor = Math.Min(structure.Stats.Base.MaxLabor, (byte)city.Resource.Labor.Value);
                    structure.EndUpdate();
                }

                if (city.Resource.Labor.Value == 0)
                {
                    break;
                }
            }

            return true;
        }

        private bool TrainUnit(Intelligence intelligence, ICity city)
        {
            Dictionary<uint, IStructure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext())
            {
                IStructure structure = enumerator.Current.Value;

                int workerType = Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(structure);
                ActionRequirementFactory.ActionRecord record =
                        Ioc.Kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(workerType);
                if (record == null)
                {
                    continue;
                }

                foreach (var req in record.List)
                {
                    if (rand.Next(100) > 60)
                    {
                        continue;
                    }

                    if (req.Type == ActionType.UnitTrainActive)
                    {
                        ushort unitType = ushort.Parse(req.Parms[0]);
                        Resource costPerUnit = city.Template[unitType].Cost;
                        ushort count = Math.Min((ushort)15,
                                                (ushort)
                                                (city.Resource.FindMaxAffordable(costPerUnit) * intelligence.military));

                        var action = Ioc.Kernel.Get<IActionFactory>().CreateUnitTrainActiveAction(city.Id, structure.ObjectId, unitType, count);
                        if (city.Worker.DoActive(workerType, structure, action, structure.Technologies) == Error.Ok)
                        {
                            //logger.Info(string.Format("{0} training {1} units of type {2} at ({3},{4})", city.Name, count, unitType, structure.X, structure.Y));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool BuildStructure(ICity city, uint x, uint y)
        {
            List<ISimpleGameObject> objects = World.Current.GetObjects(x, y);
            if (objects.Count > 0)
            {
                return false;
            }

            Dictionary<uint, IStructure>.Enumerator enumerator = city.Structures;
            while (enumerator.MoveNext())
            {
                IStructure structure = enumerator.Current.Value;
                int workerType = Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(structure);
                ActionRequirementFactory.ActionRecord record =
                        Ioc.Kernel.Get<ActionRequirementFactory>().GetActionRequirementRecord(workerType);
                if (record == null)
                {
                    continue;
                }

                foreach (var req in record.List)
                {
                    if (rand.Next(100) > 60)
                    {
                        continue;
                    }

                    if (req.Type == ActionType.StructureBuildActive)
                    {
                        ushort buildingType = ushort.Parse(req.Parms[0]);
                        if (!allowedBuildings.Contains(buildingType))
                        {
                            continue;
                        }

                        var action = Ioc.Kernel.Get<IActionFactory>()
                                        .CreateStructureBuildActiveAction(city.Id, buildingType, x, y, 1);
                        if (city.Worker.DoActive(workerType, structure, action, structure.Technologies) == Error.Ok)
                        {
                            //logger.Info(string.Format("{0} building {1} at ({2},{3})", city.Name, buildingType, structure.Stats.Base.Lvl, x, y));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool UpgradeStructure(ICity city, uint x, uint y)
        {
            List<ISimpleGameObject> objects = World.Current.GetObjects(x, y);
            IStructure structure = null;

            foreach (IGameObject obj in objects)
            {
                if (obj is IStructure && obj.City == city)
                {
                    structure = obj as Structure;
                    break;
                }
            }

            if (structure == null)
            {
                return false;
            }

            var action = new StructureUpgradeActiveAction(city.Id, structure.ObjectId);

            if (
                    city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(structure),
                                         structure,
                                         action,
                                         structure.Technologies) == Error.Ok)
            {
                //logger.Info(string.Format("{0} upgrading {1}({2}) at ({3},{4})", city.Name, structure.Type, structure.Stats.Base.Lvl, x, y));
                return true;
            }

            return false;
        }

        public static void Init()
        {
            Scheduler.Current.Pause();

            var rand = new Random();

            var logger = LoggerFactory.Current.GetCurrentClassLogger();

            logger.Info("Loading AI...");

            for (uint i = 1; i <= Config.ai_count; ++i)
            {
                if (i % 100 == 0)
                {
                    logger.Info(String.Format("Creating NPC {0}/{1}...", i, Config.ai_count));
                }

                uint idx = 50000 + i;

                var npc = new Player(idx,
                                     DateTime.MinValue,
                                     SystemClock.Now,
                                     "NPC " + i,
                                     string.Empty,
                                     PlayerRights.Basic);
                var intelligence = new Intelligence(npc,
                                                    Math.Max(0.5, rand.NextDouble()),
                                                    Math.Max(0.5, rand.NextDouble()));

                using (Concurrency.Current.Lock(npc))
                {
                    if (!World.Current.Players.ContainsKey(idx))
                    {
                        World.Current.Players.Add(idx, npc);
                        DbPersistance.Current.Save(npc);
                        Global.Ai.playerList.Add(intelligence);
                    }
                    else
                    {
                        intelligence.player = World.Current.Players[idx];
                        Global.Ai.playerList.Add(intelligence);
                        continue;
                    }

                    IEnumerable<ICity> cities = npc.GetCityList();

                    IStructure structure;
                    Error error = Randomizer.MainBuilding(out structure,
                                                          new CityTileNextAvailableLocationStrategy(Ioc.Kernel.Get<MapFactory>(),
                                                              Ioc.Kernel.Get<Formula>()),
                                                          2);
                    if (error != Error.Ok)
                    {
                        logger.Info(npc.Name);
                        break;
                    }

                    var city = new City(World.Current.Cities.GetNextCityId(),
                                        npc,
                                        string.Format("{0} {1}", npc.Name, npc.GetCityCount() + 1),
                                        Formula.Current.GetInitialCityResources(),
                                        Formula.Current.GetInitialCityRadius(),
                                        structure,
                                        0);
                    npc.Add(city);

                    World.Current.Cities.Add(city);
                    structure.BeginUpdate();
                    World.Current.Regions.Add(structure);
                    structure.EndUpdate();

                    var defaultTroop = city.Troops.Create();
                    defaultTroop.BeginUpdate();
                    defaultTroop.AddFormation(FormationType.Normal);
                    defaultTroop.AddFormation(FormationType.Garrison);
                    defaultTroop.AddFormation(FormationType.InBattle);
                    defaultTroop.EndUpdate();

                    Ioc.Kernel.Get<InitFactory>()
                       .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
                    throw new Exception("NPC isnt working right now due ot line below");
                    //city.Worker.DoPassive(city, new CityPassiveAction(city.Id), false);

                    //TileLocator.Current.foreach_object(structure.X, structure.Y, (byte) (city.Radius - 1), false, BuildBasicStructures, city);
                }
            }

            Global.Ai.time = DateTime.UtcNow.AddSeconds(10);
            //Global.Scheduler.Put(Global.Ai);

            logger.Info("Loading AI finished.");

            Scheduler.Current.Resume();
        }

        private static bool BuildBasicStructures(uint origX, uint origY, uint x, uint y, object custom)
        {
            var city = (custom as ICity);

            ushort tileType = World.Current.Regions.GetTileType(x, y);
            if (Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("TileTree", tileType))
            {
                // Lumber mill
                IStructure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2107, 1);
                structure.X = x;
                structure.Y = y;
                structure.Stats.Labor = structure.Stats.Base.MaxLabor;

                city.Add(structure);
                World.Current.Regions.Add(structure);
            }
            else if (Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("TileCrop", tileType))
            {
                // Farm
                IStructure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2106, 1);
                structure.X = x;
                structure.Y = y;
                structure.Stats.Labor = structure.Stats.Base.MaxLabor;

                city.Add(structure);
                World.Current.Regions.Add(structure);
            }
            else if (x == origX - 1 && y == origY - 1)
            {
                // Barrack
                IStructure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2201, 1);
                structure.X = x;
                structure.Y = y;

                city.Add(structure);
                World.Current.Regions.Add(structure);
            }

            return true;
        }

        #region Nested type: Intelligence

        private class Intelligence
        {
            public readonly double builder;

            public readonly double military;

            public IPlayer player;

            public byte savingUp; // how many rounds it's saving up for

            public Intelligence(IPlayer player, double builder, double military)
            {
                this.player = player;
                this.builder = builder;
                this.military = military;
            }
        }

        #endregion
    }
}