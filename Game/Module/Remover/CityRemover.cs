﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

namespace Game.Module
{
    class CityRemoverFactory : ICityRemoverFactory
    {

        #region ICityRemoverFactory Members

        public ICityRemover CreateCityRemover(ICity city) {
            return new CityRemover(city.Id);
        }

        #endregion
    }

    class CityRemover : ICityRemover, ISchedule
    {
        private const double SHORT_RETRY = 5;
        private const double LONG_RETRY = 90;

        private readonly uint cityId;

        public CityRemover(uint cityId)
        {
            this.cityId = cityId;
        }

        public bool Start(bool force = false)
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
                throw new Exception("City not found");

            if (!force && city.Deleted != City.DeletedState.NotDeleted)
                return false;

            if (city.Deleted == City.DeletedState.NotDeleted)
            {
                city.BeginUpdate();
                city.Deleted = City.DeletedState.Deleting;
                city.EndUpdate();
            }

            Time = DateTime.UtcNow;
            Scheduler.Current.Put(this);
            return true;
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        private void Reschedule(double interval)
        {
            Time = DateTime.UtcNow.AddSeconds(interval*Config.seconds_per_unit);
            Scheduler.Current.Put(this);
        }

        private static ILockable[] GetForeignTroopLockList(object[] custom)
        {
            return ((ICity)custom[0]).Troops.StationedHere().Select(stub => stub.City).Distinct().Cast<ILockable>().ToArray();
        }

        private static ILockable[] GetLocalTroopLockList(object[] custom)
        {
            return
                    ((ICity)custom[0]).Troops.MyStubs().Where(x => x.StationedCity != null).Select(stub => stub.StationedCity).Distinct().Cast<ILockable>().
                            ToArray();
        }

        private static Error RemoveForeignTroop(ICity city, ITroopStub stub)
        {
            if (!Procedure.Current.TroopObjectCreateFromStation(stub, city.X, city.Y))
            {
                return Error.Unexpected;
            }
            var ra = new RetreatChainAction(stub.City.Id, stub.TroopId);
            return stub.City.Worker.DoPassive(stub.City, ra, true);
        }

        public void Callback(object custom)
        {
            ICity city;
            IStructure mainBuilding;

            if (!World.Current.TryGetObjects(cityId, out city))
                throw new Exception("City not found");

            using (Concurrency.Current.Lock(GetForeignTroopLockList, new[] {city}, city))
            {
                if (city == null)
                    return;

                // if local city is in battle, try again later
                if (city.Battle != null)
                {
                    Reschedule(LONG_RETRY);
                    return;
                }

                // send all stationed from other players back
                if (city.Troops.StationedHere().Any())
                {
                    IEnumerable<ITroopStub> stationedTroops = new List<ITroopStub>(city.Troops.StationedHere());

                    foreach (var stub in stationedTroops)
                    {
                        if (RemoveForeignTroop(city, stub) != Error.Ok)
                            Global.Logger.Error(String.Format("removeForeignTroop failed! cityid[{0}] stubid[{1}]", city.Id, stub.StationedTroopId));
                    }
                }

                // If city is being targetted by an assignment, try again later
                var reader =
                        DbPersistance.Current.ReaderQuery(string.Format("SELECT id FROM `{0}` WHERE `city_id` = @cityId  LIMIT 1", Assignment.DB_TABLE),
                                                                 new[] {new DbColumn("cityId", city.Id, DbType.UInt32)});
                bool beingTargetted = reader.HasRows;
                reader.Close();
                if (beingTargetted)
                {
                    Reschedule(LONG_RETRY);
                    return;
                }
            }

            using (Concurrency.Current.Lock(GetLocalTroopLockList, new[] {city}, city))
            {
                if (city.TryGetStructure(1, out mainBuilding))
                {
                    // don't continue unless all troops are either idle or stationed
                    if (city.Troops.Any(s => s.State != TroopState.Idle && s.State != TroopState.Stationed))
                    {
                        Reschedule(LONG_RETRY);
                        return;
                    }

                    // starve all troops)
                    city.Troops.Starve(100, true);

                    if (city.Troops.Upkeep > 0)
                    {
                        Reschedule(LONG_RETRY);
                        return;
                    }

                    // remove all buildings except mainbuilding
                    if (city.Any(structure => !structure.IsMainBuilding))
                    {
                        city.BeginUpdate();
                        foreach (IStructure structure in new List<IStructure>(city).Where(structure => !structure.IsBlocked && !structure.IsMainBuilding))
                        {
                            structure.BeginUpdate();
                            World.Current.Remove(structure);
                            city.ScheduleRemove(structure, false);
                            structure.EndUpdate();
                        }
                        city.EndUpdate();
                        Reschedule(SHORT_RETRY);
                        return;
                    }

                    // remove all customized tiles
                    TileLocator.Current.ForeachObject(city.X,
                                                city.Y,
                                                city.Radius,
                                                true,
                                                delegate(uint origX, uint origY, uint x1, uint y1, object c)
                                                    {
                                                        World.Current.RevertTileType(x1, y1, true);
                                                        return true;
                                                    },
                                                null);

                }
            }

            // remove all remaining passive action
            if (city.TryGetStructure(1, out mainBuilding))
            {
                foreach (var passiveAction in new List<PassiveAction>(city.Worker.PassiveActions.Values))
                {
                    passiveAction.WorkerRemoved(false);
                }
            }

            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city.Worker.Notifications.Count > 0)
                {
                    Reschedule(LONG_RETRY);
                    return;
                }

                // remove mainbuilding
                if (city.TryGetStructure(1, out mainBuilding))
                {
                    // remove default troop
                    city.Troops.Remove(1);

                    // remove city from the region
                    CityRegion region = World.Current.GetCityRegion(city.X, city.Y);
                    if (region != null)
                        region.Remove(city);

                    mainBuilding.BeginUpdate();
                    World.Current.Remove(mainBuilding);
                    city.ScheduleRemove(mainBuilding, false);
                    mainBuilding.EndUpdate();

                    Reschedule(SHORT_RETRY);
                    return;
                }

                // in the case of the OjectRemoveAction for mainbuilding is still there
                if (city.Worker.PassiveActions.Count > 0)
                {
                    Reschedule(SHORT_RETRY);
                    return;
                }
                Global.Logger.Info(string.Format("Player {0}:{1} City {2}:{3} Lvl {4} is deleted.", city.Owner.Name, city.Owner.PlayerId, city.Name, city.Id, city.Lvl));
                World.Current.Remove(city);
            }

        }

        #endregion


    }
}
