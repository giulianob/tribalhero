using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Module;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions {
    public class CityCreateActiveAction : ScheduledActiveAction
    {
        uint newCityId;  // new city id
        uint newStructureId; // new mainbuilding

        uint x;
        uint y;
        readonly uint cityId;
        readonly string cityName;

        public CityCreateActiveAction(uint cityId, uint x, uint y, string cityName)
        {
            this.cityId = cityId;
            this.x = x;
            this.y = y;
            this.cityName = cityName;

            if (!MapFactory.NextLocation(out this.x, out this.y, Formula.GetInitialCityRadius())) {
                throw new Exception();
            }
        }

        public CityCreateActiveAction(uint id,
                                    DateTime beginTime,
                                    DateTime nextTime,
                                    DateTime endTime,
                                    int workerType,
                                    byte workerIndex,
                                    ushort actionCount,
                                    Dictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            newCityId = uint.Parse(properties["new_city_id"]);
            newStructureId = uint.Parse(properties["new_structure_id"]);     
        }

        #region Overrides of GameAction

        public override ActionType Type
        {
            get
            {
                return ActionType.CityCreateActive;
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
            if (!ObjectTypeFactory.IsTileType("CityStartTile", tileType))
                return Error.TileMismatch;
            return Error.Ok;
        }

        public override Error Execute()
        {
            City city, newCity;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            Global.World.LockRegion(x, y);

            // cost requirement
            //var cost = new Resource(5000,2000,1000,5000,200);
            var cost = new Resource(500, 200, 100, 50, 20);
            if (!city.Resource.HasEnough(cost)) {
                Global.World.UnlockRegion(x, y);
                return Error.ResourceNotEnough;
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
                structure = StructureFactory.GetNewStructure(2000, 0);
                structure.X = x;
                structure.Y = y;

                // Creating New City
                newCity = new City(city.Owner, cityName, Formula.GetInitialCityResources(), Formula.GetInitialCityRadius(), structure);
                city.Owner.Add(newCity);

                Global.World.Add(newCity);
                structure.BeginUpdate();
                Global.World.Add(structure);
                InitFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
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
            }

            newCityId = newCity.Id;
            newStructureId = structure.ObjectId;

            // add to queue for completion
            //EndTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.BuildTime(2 * 24 * 3600, city, city.Technologies)));
            EndTime = DateTime.UtcNow.AddSeconds(120);
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
            using (new MultiObjectLock(cityId, out city))
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

            using (new MultiObjectLock(newCityId, newStructureId, out newCity, out structure))
            {
                if (!IsValid())
                    return;

                if (newCity==null || structure==null) {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                StructureFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));
                InitFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                newCity.Worker.DoPassive(newCity, new CityPassiveAction(newCity.Id), false);
                StateChange(ActionState.Completed);
            }

        }

        #endregion
    }
}
