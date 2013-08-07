using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

namespace Game.Logic.Actions
{
    public class CityCreatePassiveAction : ScheduledPassiveAction
    {
        private readonly IActionFactory actionFactory;

        private readonly uint cityId;

        private readonly string cityName;

        private readonly ICityRemoverFactory cityRemoverFactory;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly ILocker locker;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly InitFactory initFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly ICityFactory cityFactory;

        private readonly Procedure procedure;

        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly uint x;

        private readonly uint y;

        private uint newCityId;

        private uint newStructureId;

        public CityCreatePassiveAction(uint cityId,
                                       uint x,
                                       uint y,
                                       string cityName,
                                       IActionFactory actionFactory,
                                       ICityRemoverFactory cityRemoverFactory,
                                       Formula formula,
                                       IWorld world,
                                       ILocker locker,
                                       IObjectTypeFactory objectTypeFactory,
                                       InitFactory initFactory,
                                       IStructureCsvFactory structureCsvFactory,
                                       ICityFactory cityFactory,
                                       Procedure procedure,
                                       IBarbarianTribeManager barbarianTribeManager)
        {
            this.cityId = cityId;
            this.x = x;
            this.y = y;
            this.cityName = cityName;
            this.actionFactory = actionFactory;
            this.cityRemoverFactory = cityRemoverFactory;
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.objectTypeFactory = objectTypeFactory;
            this.initFactory = initFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.cityFactory = cityFactory;
            this.procedure = procedure;
            this.barbarianTribeManager = barbarianTribeManager;
        }

        public CityCreatePassiveAction(uint id,
                                       DateTime beginTime,
                                       DateTime nextTime,
                                       DateTime endTime,
                                       bool isVisible,
                                       string nlsDescription,
                                       Dictionary<string, string> properties,
                                       IActionFactory actionFactory,
                                       ICityRemoverFactory cityRemoverFactory,
                                       Formula formula,
                                       IWorld world,
                                       ILocker locker,
                                       IObjectTypeFactory objectTypeFactory,
                                       InitFactory initFactory,
                                       IStructureCsvFactory structureCsvFactory,
                                       Procedure procedure,
                                       IBarbarianTribeManager barbarianTribeManager)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.actionFactory = actionFactory;
            this.cityRemoverFactory = cityRemoverFactory;
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.objectTypeFactory = objectTypeFactory;
            this.initFactory = initFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.procedure = procedure;
            this.barbarianTribeManager = barbarianTribeManager;
            newCityId = uint.Parse(properties["new_city_id"]);
            newStructureId = uint.Parse(properties["new_structure_id"]);
        }

        #region Overrides of GameAction

        public override ActionType Type
        {
            get
            {
                return ActionType.CityCreatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("new_city_id", newCityId), new XmlKvPair("new_structure_id", newStructureId),});
            }
        }

        public override Error Validate(string[] parms)
        {
            ushort tileType = world.Regions.GetTileType(x, y);
            if (!objectTypeFactory.IsTileType("CityStartTile", tileType))
            {
                return Error.TileMismatch;
            }
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            ICity newCity;

            if (!world.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            if (!CityManager.IsNameValid(cityName))
            {
                return Error.CityNameInvalid;
            }

            if (!world.Regions.IsValidXandY(x, y))
            {
                return Error.ActionInvalid;
            }

            // cost requirement uses town center lvl 1 for cost
            int influencePoints, wagons;
            ushort wagonType = (ushort)objectTypeFactory.GetTypes("Wagon").First();
            formula.GetNewCityCost(city.Owner.GetCityCount(), out influencePoints, out wagons);
            if (city.Owner.Value < influencePoints && !Config.actions_ignore_requirements)
            {
                return Error.ResourceNotEnough;
            }

            var totalWagons = city.DefaultTroop.Sum(f =>
            {
                if (f.Type != FormationType.Normal && f.Type != FormationType.Garrison)
                {
                    return 0;
                }

                return f.ContainsKey(wagonType) ? f[wagonType] : 0;
            });

            if (totalWagons < wagons && !Config.actions_ignore_requirements)
            {
                return Error.ResourceNotEnough;
            }

            var lockedRegions = world.Regions.LockRegions(x, y, formula.GetInitialCityRadius());

            if (!objectTypeFactory.IsTileType("CityStartTile", world.Regions.GetTileType(x, y)))
            {
                world.Regions.UnlockRegions(lockedRegions);
                return Error.TileMismatch;
            }

            // check if tile is occupied
            if (world.Regions.GetObjectsInTile(x, y).Any(obj => obj is IStructure))
            {
                world.Regions.UnlockRegions(lockedRegions);
                return Error.StructureExists;
            }

            // create new city
            lock (world.Lock)
            {
                // Verify city name is unique
                if (world.CityNameTaken(cityName))
                {
                    world.Regions.UnlockRegions(lockedRegions);
                    return Error.CityNameTaken;
                }

                var cityPosition = new Position(x, y);

                // Creating New City
                procedure.CreateCity(cityFactory, city.Owner, cityName, cityPosition, barbarianTribeManager, out newCity);                

                world.Regions.SetTileType(x, y, 0, true);

                var mainBuilding = newCity.MainBuilding;
                initFactory.InitGameObject(InitCondition.OnInit, mainBuilding, mainBuilding.Type, mainBuilding.Stats.Base.Lvl);
                
                // Take resource from the old city
                city.BeginUpdate();
                city.DefaultTroop.BeginUpdate();
                wagons -= city.DefaultTroop.RemoveUnit(FormationType.Normal, wagonType, (ushort)wagons);
                if (wagons > 0)
                {
                    city.DefaultTroop.RemoveUnit(FormationType.Garrison, wagonType, (ushort)wagons);
                }
                city.DefaultTroop.EndUpdate();
                city.EndUpdate();

                newStructureId = mainBuilding.ObjectId;
            }

            world.Regions.UnlockRegions(lockedRegions);

            newCityId = newCity.Id;            

            // add to queue for completion            
            int baseBuildTime = structureCsvFactory.GetBaseStats((ushort)objectTypeFactory.GetTypes("MainBuilding")[0], 1).BuildTime;
            EndTime = DateTime.UtcNow.AddSeconds(CalculateTime(formula.BuildTime(baseBuildTime, city, city.Technologies)));
            BeginTime = DateTime.UtcNow;

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            using (locker.Lock(newCityId, out city))
            {
                CityRemover remover = cityRemoverFactory.CreateCityRemover(newCityId);
                remover.Start();

                StateChange(ActionState.Failed);
            }
        }

        #endregion

        #region Overrides of ScheduledActiveAction

        public override void Callback(object custom)
        {
            ICity newCity;
            IStructure structure;

            using (locker.Lock(newCityId, newStructureId, out newCity, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                if (newCity == null || structure == null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));
                structure.EndUpdate();

                procedure.InitCity(newCity, initFactory, actionFactory);

                newCity.Worker.DoPassive(newCity, actionFactory.CreateCityPassiveAction(newCity.Id), false);
                StateChange(ActionState.Completed);
            }
        }

        #endregion
    }
}