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
using Ninject;

namespace Game.Logic.Actions
{
    public class CityCreatePassiveAction : ScheduledPassiveAction
    {
        private readonly IActionFactory actionFactory;

        private readonly uint cityId;

        private readonly string cityName;

        private readonly ICityRemoverFactory cityRemoverFactory;

        private readonly uint x;

        private readonly uint y;

        private uint newCityId; // new city id

        private uint newStructureId; // new mainbuilding

        public CityCreatePassiveAction(uint cityId,
                                       uint x,
                                       uint y,
                                       string cityName,
                                       IActionFactory actionFactory,
                                       ICityRemoverFactory cityRemoverFactory)
        {
            this.cityId = cityId;
            this.x = x;
            this.y = y;
            this.cityName = cityName;
            this.actionFactory = actionFactory;
            this.cityRemoverFactory = cityRemoverFactory;
        }

        public CityCreatePassiveAction(uint id,
                                       DateTime beginTime,
                                       DateTime nextTime,
                                       DateTime endTime,
                                       bool isVisible,
                                       string nlsDescription,
                                       Dictionary<string, string> properties,
                                       IActionFactory actionFactory)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.actionFactory = actionFactory;
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
                return
                        XmlSerializer.Serialize(new[]
                        {new XmlKvPair("new_city_id", newCityId), new XmlKvPair("new_structure_id", newStructureId),});
            }
        }

        public override Error Validate(string[] parms)
        {
            ushort tileType = World.Current.Regions.GetTileType(x, y);
            if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("CityStartTile", tileType))
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

            if (!World.Current.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            if (!City.IsNameValid(cityName))
            {
                return Error.CityNameInvalid;
            }

            if (!World.Current.Regions.IsValidXandY(x, y))
            {
                return Error.ActionInvalid;
            }

            // cost requirement uses town center lvl 1 for cost
            int influencePoints, wagons;
            ushort wagonType = Ioc.Kernel.Get<ObjectTypeFactory>().GetTypes("Wagon").First();
            Formula.Current.GetNewCityCost(city.Owner.GetCityCount(), out influencePoints, out wagons);
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

            World.Current.Regions.LockRegion(x, y);

            if (
                    !Ioc.Kernel.Get<ObjectTypeFactory>()
                        .IsTileType("CityStartTile", World.Current.Regions.GetTileType(x, y)))
            {
                World.Current.Regions.UnlockRegion(x, y);
                return Error.TileMismatch;
            }

            // check if tile is occupied
            if (World.Current[x, y].Exists(obj => obj is IStructure))
            {
                World.Current.Regions.UnlockRegion(x, y);
                return Error.StructureExists;
            }

            // create new city
            lock (World.Current.Lock)
            {
                // Verify city name is unique
                if (World.Current.CityNameTaken(cityName))
                {
                    World.Current.Regions.UnlockRegion(x, y);
                    return Error.CityNameTaken;
                }

                // Creating Mainbuilding
                structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2000, 0);
                structure.X = x;
                structure.Y = y;

                // Creating New City
                newCity = new City(World.Current.Cities.GetNextCityId(),
                                   city.Owner,
                                   cityName,
                                   Formula.Current.GetInitialCityResources(),
                                   Formula.Current.GetInitialCityRadius(),
                                   structure,
                                   Formula.Current.GetInitialAp());
                city.Owner.Add(newCity);

                World.Current.Regions.SetTileType(x, y, 0, true);

                World.Current.Cities.Add(newCity);
                structure.BeginUpdate();
                World.Current.Regions.Add(structure);
                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
                structure.EndUpdate();

                var defaultTroop = newCity.Troops.Create();
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

                // send new city channel notification
                if (newCity.Owner.Session != null)
                {
                    newCity.Subscribe(newCity.Owner.Session);
                    newCity.NewCityUpdate();
                }
            }

            newCityId = newCity.Id;
            newStructureId = structure.ObjectId;

            // add to queue for completion            
            int baseBuildTime =
                    Ioc.Kernel.Get<StructureFactory>()
                       .GetBaseStats(Ioc.Kernel.Get<ObjectTypeFactory>().GetTypes("MainBuilding")[0], 1)
                       .BuildTime;
            EndTime =
                    DateTime.UtcNow.AddSeconds(
                                               CalculateTime(Formula.Current.BuildTime(baseBuildTime,
                                                                                       city,
                                                                                       city.Technologies)));
            BeginTime = DateTime.UtcNow;

            World.Current.Regions.UnlockRegion(x, y);

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
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

            using (Concurrency.Current.Lock(newCityId, newStructureId, out newCity, out structure))
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
                Ioc.Kernel.Get<StructureFactory>()
                   .GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));
                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                newCity.Worker.DoPassive(newCity, actionFactory.CreateCityPassiveAction(newCity.Id), false);
                StateChange(ActionState.Completed);
            }
        }

        #endregion
    }
}