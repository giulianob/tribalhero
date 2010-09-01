#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class CityAction : ScheduledPassiveAction
    {
        private const int INTERVAL = 1800;
        private readonly uint cityId;        
        private int laborTimeRemains;

        public CityAction(uint cityId)
        {
            this.cityId = cityId;
        }

        public CityAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                          Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible)
        {
            cityId = uint.Parse(properties["city_id"]);
            laborTimeRemains = int.Parse(properties["labor_time_remains"]);
        }

        public override Error Validate(string[] parms)
        {
            return Error.OK;
        }

        public override Error Execute()
        {
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(INTERVAL * Config.seconds_per_unit);
            return Error.OK;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                StateChange(ActionState.FAILED);
            }
        }

        public override ActionType Type
        {
            get { return ActionType.CITY; }
        }

        #region ISchedule Members

        public override void Callback(object custom)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                city.BeginUpdate();
                /********************************** Pre Loop1 ****************************************/

                #region Repair

                ushort repairPower = 0;

                #endregion

                #region Labor

                int laborTotal = city.Resource.Labor.Value;

                #endregion

                /*********************************** Loop1 *******************************************/
                foreach (Structure structure in city)
                {

                    #region Repair

                    if (ObjectTypeFactory.IsStructureType("RepairBuilding", structure))
                        repairPower += Formula.RepairRate(structure);

                    #endregion

                    #region Labor

                    if (structure.Stats.Labor > 0)
                        laborTotal += structure.Stats.Labor;

                    #endregion
                }

                /********************************* Post Loop1 ****************************************/

                #region Upkeep

                if (Config.resource_upkeep)
                {
                    if (city.Resource.Crop.Upkeep > city.Resource.Crop.Rate)
                    {
                        int upkeepCost = Math.Max(1, (int)((INTERVAL / 3600f) / Config.seconds_per_unit) * (city.Resource.Crop.Upkeep - city.Resource.Crop.Rate));

                        if (city.Resource.Crop.Value < upkeepCost)
                        {
                            city.Worker.DoPassive(city, new StarveAction(city.Id), false);
                        }

                        city.Resource.Crop.Subtract(upkeepCost);
                    }
                }

                #endregion

                #region Resource: Fast Income

                if (Config.resource_fast_income)
                {
                    Resource resource = new Resource(15000, city.Resource.Gold.Value < 99500 ? 250 : 0, 15000, 15000, 0);
                    city.Resource.Add(resource);
                }

                #endregion

                #region Labor
                if (city.Owner.Session != null || DateTime.Now.Subtract(city.Owner.LastLogin).TotalDays <= 2) {
                    laborTimeRemains += INTERVAL;
                    int laborRate = Formula.GetLaborRate(laborTotal);
                    int laborProduction = laborTimeRemains/laborRate;
                    if (laborProduction > 0) {
                        laborTimeRemains -= laborProduction*laborRate;
                        city.Resource.Labor.Add(laborProduction);
                    }
                }

                #endregion

                /********************************** Pre Loop2 ****************************************/

                #region Repair

                Resource repairCost = null;
                bool isRepaired = false, canRepair = false;
                if (repairPower > 0)
                {
                    repairCost = Formula.RepairCost(city, repairPower);
                    if (city.Resource.HasEnough(repairCost))
                        canRepair = true;
                }

                #endregion

                /*********************************** Loop2 *******************************************/
                foreach (Structure structure in city)
                {
                    #region Repair

                    if (canRepair)
                    {
                        if (structure.Stats.Base.Battle.MaxHp > structure.Stats.Hp &&
                            !ObjectTypeFactory.IsStructureType("NonRepairable", structure) &&
                            structure.State.Type != ObjectState.BATTLE)
                        {
                            structure.BeginUpdate();
                            if ((structure.Stats.Hp += repairPower) > structure.Stats.Base.Battle.MaxHp)
                                structure.Stats.Hp = structure.Stats.Base.Battle.MaxHp;
                            structure.EndUpdate();
                            isRepaired = true;
                        }
                    }

                    #endregion
                }
                /********************************* Post Loop2 ****************************************/

                #region Repair

                if (isRepaired)
                    city.Resource.Subtract(repairCost);

                #endregion

                city.EndUpdate();

                beginTime = DateTime.UtcNow;
                endTime = DateTime.UtcNow.AddSeconds(INTERVAL * Config.seconds_per_unit);
                StateChange(ActionState.FIRED);
            }
        }

        #endregion

        #region IPersistable Members

        public override string Properties
        {
            get
            {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("labor_time_remains", laborTimeRemains)
                                                            });
            }
        }

        #endregion
    }
}