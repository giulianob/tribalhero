using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Util;
using Game.Setup;

namespace Game.Module {
    class CityRemover:ISchedule {
        const double SHORT_RETRY = 5;
        const double LONG_RETRY = 90;

        private readonly uint cityId;

        public CityRemover(uint cityId) {
            this.cityId = cityId;
        }

        public void Start(bool force = false)
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City not found");

            if (!force && city.Deleted != City.DeletedState.NotDeleted)
                return;

            if (city.Deleted == City.DeletedState.NotDeleted)
            {
                city.BeginUpdate();
                city.Deleted = City.DeletedState.Deleting;
                city.EndUpdate();
            }

            Time = DateTime.UtcNow;
            Global.Scheduler.Put(this);
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        private void Reschedule(double interval)
        {
            Time = DateTime.UtcNow.AddSeconds(interval * Config.seconds_per_unit);
            Global.Scheduler.Put(this);
        }

        private static ILockable[] GetForeignTroopLockList(object[] custom)
        {
            return ((City)custom[0]).Troops.StationedHere().Select(stub => stub.City).Distinct().Cast<ILockable>().ToArray();
        }

        private static ILockable[] GetLocalTroopLockList(object[] custom) {
            return ((City)custom[0]).Troops.MyStubs().Where(x=>x.StationedCity!=null).Select(stub => stub.StationedCity).Distinct().Cast<ILockable>().ToArray();
        }

        private static Error RemoveForeignTroop(City city, TroopStub stub)
        {
            if (!Procedure.TroopObjectCreateFromStation(stub, city.X, city.Y)) {
                return Error.Unexpected;
            }
            var ra = new RetreatChainAction(stub.City.Id, stub.TroopId);
            return stub.City.Worker.DoPassive(stub.City, ra, true);
        }

        public void Callback(object custom) {
            City city;
            Structure mainBuilding;

            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City not found");

            using (new CallbackLock(GetForeignTroopLockList, new[] { city }, city))
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
                    foreach (TroopStub stub in new List<TroopStub>(city.Troops.StationedHere().ToList()))
                    {
                        if (RemoveForeignTroop(city, stub) != Error.Ok)
                            Global.Logger.Error(String.Format("removeForeignTroop failed! cityid[{0}] stubid[{1}]", city.Id, stub.StationedTroopId));
                    }

                }
            }

            using (new CallbackLock(GetLocalTroopLockList, new[] { city }, city))
            {
                if (city.TryGetStructure(1, out mainBuilding))
                {
                    // starve all troops)
                    city.Troops.Starve(100);

                    if (city.Troops.Upkeep > 0)
                    {
                        Reschedule(LONG_RETRY);
                        return;
                    }

                    // remove all buildings except mainbuilding
                    if (city.Any(structure => !structure.IsMainBuilding))
                    {
                        city.BeginUpdate();
                        foreach (Structure structure in new List<Structure>(city).Where(structure => !structure.IsBlocked && !structure.IsMainBuilding))
                        {
                            structure.BeginUpdate();
                            Global.World.Remove(structure);
                            city.ScheduleRemove(structure, false);
                            structure.EndUpdate();
                        }
                        city.EndUpdate();
                        Reschedule(SHORT_RETRY);
                        return;
                    }

                    // remove all customized tiles
                    RadiusLocator.ForeachObject(city.X, city.Y, city.Radius, true,
                                                delegate(uint origX, uint origY, uint x1, uint y1, object c) {
                                                    Global.World.RevertTileType(x1, y1, true);
                                                    return true;
                                                }, null);

                }
            }

            // remove all remaining passive action
            if (city.TryGetStructure(1,out mainBuilding))
            {
                foreach (var passiveAction in new List<PassiveAction>(city.Worker.PassiveActions.Values))
                {
                    passiveAction.WorkerRemoved(false);
                }
            }

            using (new MultiObjectLock(cityId, out city))
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
                    CityRegion region = Global.World.GetCityRegion(city.X, city.Y);
                    if (region != null)
                        region.Remove(city);                    
                    
                    mainBuilding.BeginUpdate();
                    Global.World.Remove(mainBuilding);
                    city.ScheduleRemove(mainBuilding, false);
                    mainBuilding.EndUpdate();

                    Reschedule(SHORT_RETRY);
                    return;
                }

                // in the case of the OjectRemoveAction for mainbuilding is still there
                if(city.Worker.PassiveActions.Count > 0)
                {
                    Reschedule(SHORT_RETRY);
                    return;
                }

                Global.World.Remove(city);
            }

        }

        #endregion
    }
}
