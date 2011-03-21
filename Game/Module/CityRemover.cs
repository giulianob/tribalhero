using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Util;
using Game.Setup;

namespace Game.Module {
    class CityRemover:ISchedule {
        const double RETRY_INTERVAL_IN_MINS = 12 * 3600;
        private uint cityId;

        public CityRemover(uint cityId) {
            // TODO: Complete member initialization
            this.cityId = cityId;
        }

        public void Start()
        {
            Time = DateTime.UtcNow;
            Global.Scheduler.Put(this);
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        private void Reschedule(double interval = RETRY_INTERVAL_IN_MINS)
        {
            Time = DateTime.UtcNow.AddMinutes(interval);
            Global.Scheduler.Put(this);
        }

        private static ILockable[] GetForeignTroopLockList(object[] custom)
        {
            return ((City)custom[0]).Troops.StationedHere().Select(stub => stub.City).Distinct().Cast<ILockable>().ToArray();
        }

        private static ILockable[] GetLocalTroopLockList(object[] custom) {
            return ((City)custom[0]).Troops.MyStubs().Where(x=>x.StationedCity!=null).Select(stub => stub.StationedCity).Distinct().Cast<ILockable>().ToArray();
        }

        private Error removeForeignTroop(City city, TroopStub stub)
        {
            if (!Procedure.TroopObjectCreateFromStation(stub, city.MainBuilding.X, city.MainBuilding.Y)) {
                return Error.Unexpected;
            }
            var ra = new RetreatChainAction(stub.City.Id, stub.TroopId);
            return stub.City.Worker.DoPassive(stub.City, ra, true);
        }

        public void Callback(object custom) {
            City city;
            Structure mainbuilding;

            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City not found");

            using (new CallbackLock(GetForeignTroopLockList, new[] { city }, city))
            {
                if (city == null)
                    return;

                // if local city is in battle, try again later
                if (city.Battle != null)
                {
                    Reschedule();
                    return;
                }

                // send all stationed from other players back
                if (city.Troops.StationedHere().Any())
                {
                    foreach (TroopStub stub in new List<TroopStub>(city.Troops.StationedHere().ToList()))
                    {
                        if (removeForeignTroop(city, stub) != Error.Ok)
                            Global.Logger.Error(String.Format("removeForeignTroop failed! cityid[{0}] stubid[{1}]", city.Id, stub.StationedTroopId));
                    }

                }
            }

            using (new CallbackLock(GetLocalTroopLockList, new[] { city }, city))
            {
                if (city.TryGetStructure(1, out mainbuilding))
                {
                    // starve all troops)
                    city.Troops.Starve(100);

                    if (city.Troops.Upkeep > 0)
                    {
                        Reschedule();
                        return;
                    }

                    // remove all buildings except mainbuilding
                    if (city.Any(structure => structure != city.MainBuilding))
                    {
                        city.BeginUpdate();
                        foreach (Structure structure in new List<Structure>(city).Where(structure => !structure.IsBlocked && structure != city.MainBuilding))
                        {
                            structure.BeginUpdate();
                            Global.World.Remove(structure);
                            city.ScheduleRemove(structure, false);
                            structure.EndUpdate();
                        }
                        city.EndUpdate();
                        Reschedule(0.01);
                        return;
                    }
                }
            }

            // remove all remaining passive action
            if (city.TryGetStructure(1,out mainbuilding))
            {
                foreach (var passiveAction in new List<PassiveAction>(city.Worker.PassiveActions.Values))
                {
                    passiveAction.WorkerRemoved(false);
                }
            }

            using (new MultiObjectLock(cityId, out city))
            {
                // remove mainbuilding
                if (city.TryGetStructure(1, out mainbuilding))
                {
                    // remove city from the region
                    CityRegion region = Global.World.GetCityRegion(city.MainBuilding.X, city.MainBuilding.Y);
                    if (region != null)
                        region.Remove(city);

                    city.Troops.Remove(1);
                    city.MainBuilding.BeginUpdate();
                    Global.World.Remove(city.MainBuilding);
                    city.ScheduleRemove(city.MainBuilding, false);
                    city.MainBuilding.EndUpdate();
                    Reschedule(0.01);
                    return;
                }
                else
                {
                    // in the case of the OjectRemoveAction for mainbuilding is still there
                    if(city.Worker.PassiveActions.Count>0)
                    {
                        Reschedule(0.01);
                        return;
                    }
                }
                Global.World.Remove(city);
            }

        }

        #endregion
    }
}
