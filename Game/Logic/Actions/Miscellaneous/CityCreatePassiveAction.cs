using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

namespace Game.Logic.Actions {
    public class CityCreatePassiveAction : ScheduledPassiveAction
    {
        uint newCityId;  // new city id
        uint newStructureId; // new mainbuilding

        uint x;
        uint y;
        readonly uint cityId;
        readonly string cityName;

        public CityCreatePassiveAction(uint cityId, uint x, uint y, string cityName)
        {
            this.cityId = cityId;
            this.x = x;
            this.y = y;
            this.cityName = cityName;
        }

        public CityCreatePassiveAction(uint id,
                                    DateTime beginTime,
                                    DateTime nextTime,
                                    DateTime endTime,
                                    bool isVisible, 
                                    string nlsDescription,
                                    Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
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
                return XmlSerializer.Serialize(new[] { new XmlKvPair("new_city_id", newCityId), new XmlKvPair("new_structure_id", newStructureId), });
            }
        }

        public override Error Validate(string[] parms)
        {
            ushort tileType = Global.World.GetTileType(x, y);
            if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("CityStartTile", tileType))
                return Error.TileMismatch;
            return Error.Ok;
        }

        public override Error Execute()
        {
            City city, newCity;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            if (!City.IsNameValid(cityName))
                return Error.CityNameInvalid;

            if (!Global.World.IsValidXandY(x, y))
                return Error.ActionInvalid;

            // cost requirement uses town center lvl 1 for cost
            var cost = Formula.StructureCost(city, Ioc.Kernel.Get<ObjectTypeFactory>().GetTypes("MainBuilding")[0], 1);
            if (!city.Resource.HasEnough(cost))            
                return Error.ResourceNotEnough;            

            // make sure all cities are at least level 10
            if (city.Owner.GetCityList().Any(c => c.Lvl < 10))
                return Error.EffectRequirementNotMet;

            Global.World.LockRegion(x, y);

            if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("CityStartTile", Global.World.GetTileType(x, y)))
            {
                Global.World.UnlockRegion(x, y);
                return Error.TileMismatch;
            }

            // check if tile is occupied
            if (Global.World[x, y].Exists(obj => obj is Structure)) {
                Global.World.UnlockRegion(x, y);
                return Error.StructureExists;
            }

            // create new city
            lock (Global.World.Lock) {
                // Verify city name is unique
                if (Global.World.CityNameTaken(cityName)) {
                    Global.World.UnlockRegion(x, y);
                    return Error.CityNameTaken;
                }

                // Creating Mainbuilding
                structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2000, 0);
                structure.X = x;
                structure.Y = y;

                // Creating New City
                newCity = new City(city.Owner, cityName, Formula.GetInitialCityResources(), Formula.GetInitialCityRadius(), structure);
                city.Owner.Add(newCity);

                Global.World.SetTileType(x, y, 0, true);

                Global.World.Add(newCity);
                structure.BeginUpdate();
                Global.World.Add(structure);
                Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
                structure.EndUpdate();

                var defaultTroop = new TroopStub();
                defaultTroop.BeginUpdate();
                defaultTroop.AddFormation(FormationType.Normal);
                defaultTroop.AddFormation(FormationType.Garrison);
                defaultTroop.AddFormation(FormationType.InBattle);
                newCity.Troops.Add(defaultTroop);
                defaultTroop.EndUpdate();

                // taking resource from the old city
                city.BeginUpdate();
                city.Resource.Subtract(cost);
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
            int baseBuildTime = Ioc.Kernel.Get<StructureFactory>().GetBaseStats(Ioc.Kernel.Get<ObjectTypeFactory>().GetTypes("MainBuilding")[0], 1).BuildTime;
            EndTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.BuildTime(baseBuildTime, city, city.Technologies)));
            BeginTime = DateTime.UtcNow;

            Global.World.UnlockRegion(x, y);
            
            return Error.Ok;
        }

        public override void UserCancelled()
        {            
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                CityRemover remover = new CityRemover(newCityId);
                remover.Start();

                StateChange(ActionState.Failed);
            }
        }

        #endregion

        #region Overrides of ScheduledActiveAction

        public override void Callback(object custom)
        {
            City newCity;
            Structure structure;

            using (Concurrency.Current.Lock(newCityId, newStructureId, out newCity, out structure))
            {
                if (!IsValid())
                    return;

                if (newCity==null || structure==null) {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                Ioc.Kernel.Get<StructureFactory>().GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));
                Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                newCity.Worker.DoPassive(newCity, new CityPassiveAction(newCity.Id), false);
                StateChange(ActionState.Completed);
            }

        }

        #endregion
    }
}
