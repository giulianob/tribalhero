#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using System.Linq;

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

        public CityAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible)
        {
            cityId = uint.Parse(properties["city_id"]);
            laborTimeRemains = int.Parse(properties["labor_time_remains"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.City;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId), new XmlKvPair("labor_time_remains", laborTimeRemains)});
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(INTERVAL));
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
                StateChange(ActionState.Failed);
            }
        }

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

                #region WeaponExport
                int weaponExport = 0;
                int WeaponExportMarket = 0;
                #endregion
                /*********************************** Loop1 *******************************************/
                foreach (var structure in city)
                {
                    #region Repair

                    if (ObjectTypeFactory.IsStructureType("RepairBuilding", structure))
                        repairPower += Formula.RepairRate(structure);

                    #endregion

                    #region Labor

                    if (structure.Stats.Labor > 0)
                        laborTotal += structure.Stats.Labor;

                    #endregion

                    #region WeaponExport
                    weaponExport += structure.Technologies.GetEffects(EffectCode.WeaponExport, EffectInheritance.Self).DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]);
                    if( ObjectTypeFactory.IsStructureType("Market",structure) )
                    {
                        WeaponExportMarket += structure.Lvl;
                    }
                    #endregion
                }

                /********************************* Post Loop1 ****************************************/

                #region Upkeep

                if (Config.resource_upkeep)
                {
                    if (city.Resource.Crop.Upkeep > city.Resource.Crop.Rate)
                    {
                        int upkeepCost = Math.Max(1, (int)((INTERVAL/3600f)/Config.seconds_per_unit)*(city.Resource.Crop.Upkeep - city.Resource.Crop.Rate));

                        if (city.Resource.Crop.Value < upkeepCost)
                            city.Worker.DoPassive(city, new StarveAction(city.Id), false);

                        city.Resource.Crop.Subtract(upkeepCost);
                    }
                }

                #endregion

                #region Resource: Fast Income

                if (Config.resource_fast_income)
                {
                    var resource = new Resource(15000, city.Resource.Gold.Value < 99500 ? 250 : 0, 15000, 15000, 0);
                    city.Resource.Add(resource);
                }

                #endregion

                #region Labor

                if (city.Owner.Session != null || DateTime.Now.Subtract(city.Owner.LastLogin).TotalDays <= 2)
                {
                    laborTimeRemains += INTERVAL;
                    int laborRate = Formula.GetLaborRate(laborTotal, city);
                    int laborProduction = laborTimeRemains/laborRate;
                    if (laborProduction > 0)
                    {
                        laborTimeRemains -= laborProduction*laborRate;
                        city.Resource.Labor.Add(laborProduction);
                    }
                }

                #endregion
                
                #region WeaponExport
                if (weaponExport * WeaponExportMarket > 0)
                {
                    city.Resource.Gold.Add(weaponExport*WeaponExportMarket);
                }

                #endregion

                /********************************** Pre Loop2 ****************************************/

                /*********************************** Loop2 *******************************************/
                foreach (var structure in city)
                {
                    #region Repair

                    if (repairPower > 0)
                    {
                        if (structure.Stats.Base.Battle.MaxHp > structure.Stats.Hp && !ObjectTypeFactory.IsStructureType("NonRepairable", structure) &&
                            structure.State.Type != ObjectState.Battle)
                        {
                            structure.BeginUpdate();
                            structure.Stats.Hp = (ushort)Math.Min(structure.Stats.Hp + repairPower, structure.Stats.Base.Battle.MaxHp);
                            structure.EndUpdate();
                        }
                    }

                    #endregion
                }
                /********************************* Post Loop2 ****************************************/
 
                city.EndUpdate();

                // Stop city action if player has not login for more than a week
                if (city.Owner.Session != null && DateTime.UtcNow.Subtract(city.Owner.LastLogin).TotalDays > 7)
                    StateChange(ActionState.Completed);
                else
                {
                    beginTime = DateTime.UtcNow;
                    endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : INTERVAL*Config.seconds_per_unit);
                    StateChange(ActionState.Fired);
                }
            }
        }
    }
}