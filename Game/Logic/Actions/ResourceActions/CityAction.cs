#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class CityAction : ScheduledPassiveAction {
        private const int INTERVAL = 1800;
        private readonly uint cityId;
        private int laborRoundBeforeIncrements;
        private int laborTimeRemains;

        public CityAction(uint cityId) {
            this.cityId = cityId;
        }

        public CityAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                          Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            laborRoundBeforeIncrements = int.Parse(properties["labor_round_before_increments"]);
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            beginTime = DateTime.Now;
            endTime = DateTime.Now.AddSeconds(INTERVAL*Config.seconds_per_unit);
            return Error.OK;
        }

        public override void Interrupt(ActionInterrupt state) {
            switch (state) {
                case ActionInterrupt.ABORT:
                    Global.Scheduler.Del(this);
                    break;
                case ActionInterrupt.KILLED:
                    Global.Scheduler.Del(this);
                    break;
            }
        }

        public override ActionType Type {
            get { return ActionType.CITY; }
        }

        #region ISchedule Members

        public override void Callback(object custom) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                city.BeginUpdate();
/********************************** Pre Loop1 ****************************************/

                #region Repair

                ushort repairPower = 0;

                #endregion

                #region Labor

                int laborTotal = 0;

                #endregion

                /*********************************** Loop1 *******************************************/
                foreach (Structure structure in city) {

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
                
                if (Config.resource_upkeep) {                    
                    if (city.Resource.Crop.Upkeep > city.Resource.Crop.Rate) {
                        int upkeepCost = (INTERVAL / 3600) * (city.Resource.Crop.Upkeep - city.Resource.Crop.Rate);

                        if (city.Resource.Crop.Value < upkeepCost) {
                            city.Troops.Starve();
                        }

                        city.Resource.Crop.Subtract(upkeepCost);
                    }
                }

                #endregion

                #region Resource: Fast Income
                
                if (Config.resource_fast_income) {
                    Resource resource = new Resource(15000, 250, 15000, 15000, 0);
                    city.Resource.Add(resource);
                }

                #endregion

                #region Labor
                laborTimeRemains += INTERVAL;
                for(int i =0; i<1500; i+=100 ) {
                    System.Console.Out.WriteLine("i=" + i + " rate=" + 86400/Formula.GetLaborRate(i));
                }
                int laborRate = Formula.GetLaborRate(laborTotal);
                int laborProduction = laborTimeRemains/Formula.GetLaborRate(laborTotal);
                if (laborProduction > 0) {
                    laborTimeRemains -= laborProduction * laborRate;
                    city.Resource.Labor.Add(laborProduction);
                }

                #endregion

/********************************** Pre Loop2 ****************************************/

                #region Repair

                Resource repairCost = null;
                bool isRepaired = false, canRepair = false;
                if (repairPower > 0) {
                    repairCost = Formula.RepairCost(city, repairPower);
                    if (city.Resource.HasEnough(repairCost))
                        canRepair = true;
                }

                #endregion

/*********************************** Loop2 *******************************************/
                foreach (Structure structure in city) {
                    #region Repair

                    if (canRepair) {
                        if (structure.Stats.Base.Battle.MaxHp > structure.Stats.Hp &&
                            !ObjectTypeFactory.IsStructureType("NonRepairable", structure) &&
                            structure.State.Type != ObjectState.BATTLE) {
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

                beginTime = DateTime.Now;
                endTime = DateTime.Now.AddSeconds(INTERVAL*Config.seconds_per_unit);
                StateChange(ActionState.FIRED);
            }
        }

        #endregion

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("labor_round_before_increments", laborRoundBeforeIncrements)
                                                            });
            }
        }

        #endregion
    }
}