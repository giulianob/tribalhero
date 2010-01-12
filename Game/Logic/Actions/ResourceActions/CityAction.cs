#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class CityAction : ScheduledPassiveAction {
        private int INTERVAL = 1800;
        private uint cityId;
        private int laborRoundBeforeIncrements;

        public CityAction(uint cityId) {
            this.cityId = cityId;
        }

        public CityAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                          Dictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, isVisible) {
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
                    Global.Scheduler.del(this);
                    break;
                case ActionInterrupt.KILLED:
                    Global.Scheduler.del(this);
                    break;
            }
        }

        public override ActionType Type {
            get { return ActionType.CITY; }
        }

        #region ISchedule Members

        public override void callback(object custom) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                city.BeginUpdate();
/********************************** Pre Loop1 ****************************************/

                #region ResourceGet

                //Resource resource = new Resource(5*city.MainBuilding.Lvl,0,0,5*city.MainBuilding.Lvl);
                //Resource resource = new Resource(200, 200, 200, 200);
                Resource resource = new Resource();

                #endregion

                #region Repair

                ushort repairPower = 0;

                #endregion

                #region Labor

                int laborTotal = 0;

                #endregion

                /*********************************** Loop1 *******************************************/
                foreach (Structure structure in city) {
                    #region ResourceGet

                    #endregion

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

                #region ResourceGet

                Resource Upkeep = city.Troops.Upkeep();
                if (Config.resource_upkeep) {
                    if (city.Resource.Crop.Value + resource.Crop < Upkeep.Crop) {
                        Upkeep.Crop = city.Resource.Crop.Value + resource.Crop;
                        city.Troops.Starve();
                    }
                } else
                    Upkeep.Clear();
                if (Config.resource_fast_income)
                    resource += new Resource(500, 500, 500, 500, 0);
                city.Resource.Add(resource - Upkeep);

                #endregion

                #region Labor

                if (--laborRoundBeforeIncrements <= 0) {
                    city.Resource.Labor.Add(1);
                    laborTotal += city.Resource.Labor.Value;
                    if (laborTotal < 200)
                        laborRoundBeforeIncrements = 1;
                    else
                        laborRoundBeforeIncrements = (int) (Math.Pow((laborTotal - 200/5), 2)/500 + 1);
                    byte radius = Formula.GetRadius((uint) (laborTotal + city.Resource.Labor.Value));
                    if (radius > city.Radius)
                        city.Radius = radius;
                } else
                    laborTotal += city.Resource.Labor.Value;

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
                    XMLSerializer.Serialize(new XMLKVPair[] {
                                                                new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("labor_round_before_increments", laborRoundBeforeIncrements)
                                                            });
            }
        }

        #endregion
    }
}