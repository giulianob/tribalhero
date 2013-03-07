using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Persistance;

namespace Game.Module
{
    public class CityRemover : ICityRemover, ISchedule
    {
        private const double SHORT_RETRY = 5;

        private const double LONG_RETRY = 90;

        private readonly IActionFactory actionFactory;

        private readonly uint cityId;

        public CityRemover(uint cityId, IActionFactory actionFactory)
        {
            this.cityId = cityId;
            this.actionFactory = actionFactory;
        }

        public bool Start(bool force = false)
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                throw new Exception("City not found");
            }

            if (!force && city.Deleted != City.DeletedState.NotDeleted)
            {
                return false;
            }

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

        public void Callback(object custom)
        {
            ICity city;
            IStructure mainBuilding;

            if (!World.Current.TryGetObjects(cityId, out city))
            {
                throw new Exception("City not found");
            }

            using (Concurrency.Current.Lock(GetForeignTroopLockList, new[] {city}, city))
            {
                if (city == null)
                {
                    return;
                }

                // if local city is in battle, try again later
                if (city.Battle != null)
                {
                    Reschedule(SHORT_RETRY);
                    return;
                }

                // send all stationed from other players back
                if (city.Troops.StationedHere().Any())
                {
                    IEnumerable<ITroopStub> stationedTroops = new List<ITroopStub>(city.Troops.StationedHere());

                    foreach (var stub in stationedTroops)
                    {
                        if (RemoveForeignTroop(stub) != Error.Ok)
                        {
                            Global.Logger.Error(String.Format("removeForeignTroop failed! cityid[{0}] stubid[{1}]",
                                                              city.Id,
                                                              stub.StationTroopId));
                        }
                    }
                }

                // If city is being targetted by an assignment, try again later
                var reader =
                        DbPersistance.Current.ReaderQuery(
                                                          string.Format(
                                                                        "SELECT id FROM `{0}` WHERE `location_type` = 'City' AND `location_id` = @locationId  LIMIT 1",
                                                                        Assignment.DB_TABLE),
                                                          new[] {new DbColumn("locationId", city.Id, DbType.UInt32)});
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
                    if (city.TroopObjects.Any() || city.Troops.Any(s => s.State != TroopState.Idle && s.State != TroopState.Stationed))
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
                        foreach (IStructure structure in
                                new List<IStructure>(city).Where(structure => structure.IsBlocked == 0 && !structure.IsMainBuilding))
                        {
                            structure.BeginUpdate();
                            World.Current.Regions.Remove(structure);
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
                                                              World.Current.Regions.RevertTileType(x1, y1, true);
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
                if (city.Notifications.Count > 0)
                {
                    Reschedule(SHORT_RETRY);
                    return;
                }

                // remove mainbuilding
                if (city.TryGetStructure(1, out mainBuilding))
                {
                    // remove default troop
                    city.Troops.Remove(1);

                    // remove city from the region
                    World.Current.Regions.CityRegions.Remove(city);

                    mainBuilding.BeginUpdate();
                    World.Current.Regions.Remove(mainBuilding);
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
                Global.Logger.Info(string.Format("Player {0}:{1} City {2}:{3} Lvl {4} is deleted.",
                                                 city.Owner.Name,
                                                 city.Owner.PlayerId,
                                                 city.Name,
                                                 city.Id,
                                                 city.Lvl));
                World.Current.Cities.Remove(city);
            }
        }

        private void Reschedule(double interval)
        {
            Time = DateTime.UtcNow.AddMinutes(interval * Config.seconds_per_unit);
            Scheduler.Current.Put(this);
        }

        private static ILockable[] GetForeignTroopLockList(object[] custom)
        {
            return
                    ((ICity)custom[0]).Troops.StationedHere()
                                      .Select(stub => stub.City)
                                      .Distinct()
                                      .Cast<ILockable>()
                                      .ToArray();
        }

        private static ILockable[] GetLocalTroopLockList(object[] custom)
        {
            return
                    ((ICity)custom[0]).Troops.MyStubs()
                                      .Where(x => x.Station != null)
                                      .Select(stub => stub.Station)
                                      .Distinct()
                                      .Cast<ILockable>()
                                      .ToArray();
        }

        private Error RemoveForeignTroop(ITroopStub stub)
        {
            var ra = actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId);
            return stub.City.Worker.DoPassive(stub.City, ra, true);
        }

        #endregion
    }
}