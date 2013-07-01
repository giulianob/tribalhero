using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
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
                                       ICityFactory cityFactory)
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
                                       IStructureCsvFactory structureCsvFactory)
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
            IStructure structure;

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

            world.Regions.LockRegion(x, y);

            if (!objectTypeFactory.IsTileType("CityStartTile", world.Regions.GetTileType(x, y)))
            {
                world.Regions.UnlockRegion(x, y);
                return Error.TileMismatch;
            }

            // check if tile is occupied
            if (world.Regions.GetObjectsInTile(x, y).Any(obj => obj is IStructure))
            {
                world.Regions.UnlockRegion(x, y);
                return Error.StructureExists;
            }

            // create new city
            lock (world.Lock)
            {
                // Verify city name is unique
                if (world.CityNameTaken(cityName))
                {
                    world.Regions.UnlockRegion(x, y);
                    return Error.CityNameTaken;
                }

                // Creating New City
                newCity = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                                   city.Owner,
                                   cityName,
                                   formula.GetInitialCityResources(),
                                   formula.GetInitialCityRadius(),                                   
                                   formula.GetInitialAp());

                // Creating Mainbuilding
                structure = newCity.CreateStructure(2000, 0, x, y);                
                
                structure.BeginUpdate();
                city.Owner.Add(newCity);
                world.Regions.SetTileType(x, y, 0, true);
                world.Regions.Add(structure);
                world.Cities.Add(newCity);                                
                initFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
                structure.EndUpdate();                
                

                var defaultTroop = newCity.CreateTroopStub();
                defaultTroop.BeginUpdate();
                defaultTroop.AddFormation(FormationType.Normal);
                defaultTroop.AddFormation(FormationType.Garrison);
                defaultTroop.AddFormation(FormationType.InBattle);
                defaultTroop.EndUpdate();

                // taking resource from the old city
                city.BeginUpdate();
                city.DefaultTroop.BeginUpdate();
                wagons -= city.DefaultTroop.RemoveUnit(FormationType.Normal, wagonType, (ushort)wagons);
                if (wagons > 0)
                {
                    city.DefaultTroop.RemoveUnit(FormationType.Garrison, wagonType, (ushort)wagons);
                }
                city.DefaultTroop.EndUpdate();
                city.EndUpdate();
            }

            newCityId = newCity.Id;
            newStructureId = structure.ObjectId;

            // add to queue for completion            
            int baseBuildTime = structureCsvFactory.GetBaseStats((ushort)objectTypeFactory.GetTypes("MainBuilding")[0], 1).BuildTime;
            EndTime = DateTime.UtcNow.AddSeconds(CalculateTime(formula.BuildTime(baseBuildTime, city, city.Technologies)));
            BeginTime = DateTime.UtcNow;

            world.Regions.UnlockRegion(x, y);

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
                initFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                newCity.Worker.DoPassive(newCity, actionFactory.CreateCityPassiveAction(newCity.Id), false);
                StateChange(ActionState.Completed);
            }
        }

        #endregion
    }
}