#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using System.Linq;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    class CityPassiveAction : ScheduledPassiveAction
    {

        private delegate void Init(City city);
        private delegate void PreLoop(City city);
        private delegate void PostLoop(City city);
        private delegate void StructureLoop(City city, Structure structure);

        private event Init InitVars;
        private event PreLoop PreFirstLoop;
        private event StructureLoop FirstLoop;
        private event PostLoop PostFirstLoop;
        private event PreLoop PreSecondLoop;
        private event StructureLoop SecondLoop;
        private event PostLoop PostSecondLoop;


        private const int INTERVAL = 1800;
        private readonly uint cityId;
        
        private int laborTimeRemains;
        private bool everyOther;

        public CityPassiveAction(uint cityId)
        {
            this.cityId = cityId;

            CreateSubscriptions();
        }

        public CityPassiveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, string nlsDescription, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            cityId = uint.Parse(properties["city_id"]);
            laborTimeRemains = int.Parse(properties["labor_time_remains"]);

            CreateSubscriptions();
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.CityPassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] { new XmlKvPair("city_id", cityId), new XmlKvPair("labor_time_remains", laborTimeRemains), new XmlKvPair("every_other", everyOther) });
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

        private void CreateSubscriptions()
        {
            Repair();
            Labor();
            Upkeep();
            WeaponExport();
            FastIncome();
        }

        public override void Callback(object custom)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                everyOther = !everyOther;

                city.BeginUpdate();

                if (InitVars != null)
                    InitVars(city);

                if (PreFirstLoop != null)
                    PreFirstLoop(city);

                if (FirstLoop != null)
                    foreach (var structure in city)
                        FirstLoop(city, structure);

                if (PostFirstLoop != null)
                    PostFirstLoop(city);

                if (PreSecondLoop != null)
                    PreSecondLoop(city);

                if (SecondLoop != null)
                    foreach (var structure in city)
                        SecondLoop(city, structure);

                if (PostSecondLoop != null)
                    PostSecondLoop(city);

                city.EndUpdate();

                // Stop city action if player has not login for more than a week
                if (city.Owner.Session != null && DateTime.UtcNow.Subtract(city.Owner.LastLogin).TotalDays > 7)
                    StateChange(ActionState.Completed);
                else
                {
                    beginTime = DateTime.UtcNow;
                    endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : INTERVAL * Config.seconds_per_unit);
                    StateChange(ActionState.Fired);
                }
            }
        }

        private void Labor()
        {
            int laborTotal = 0;

            InitVars += city =>
                { laborTotal = city.Resource.Labor.Value; };

            FirstLoop += (city, structure) =>
                {
                    if (structure.Stats.Labor > 0)
                        laborTotal += structure.Stats.Labor;
                };

            PostFirstLoop += city =>
                {
                    if (city.Owner.Session == null && DateTime.Now.Subtract(city.Owner.LastLogin).TotalDays > 2)
                        return;

                    laborTimeRemains += INTERVAL;
                    int laborRate = Formula.GetLaborRate(laborTotal, city);
                    int laborProduction = laborTimeRemains/laborRate;
                    if (laborProduction <= 0)
                        return;

                    laborTimeRemains -= laborProduction*laborRate;
                    city.Resource.Labor.Add(laborProduction);
                };
        }

        private void Repair()
        {
            ushort repairPower = 0;

            InitVars += city => repairPower = 0;

            FirstLoop += (city, structure) =>
                {
                    if (Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("RepairBuilding", structure))
                        repairPower += Formula.RepairRate(structure);
                };

            SecondLoop += (city, structure) =>
                {
                    if (repairPower <= 0)
                        return;

                    if (structure.Stats.Base.Battle.MaxHp <= structure.Stats.Hp || Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("NonRepairable", structure) ||
                        structure.State.Type == ObjectState.Battle)
                        return;

                    structure.BeginUpdate();
                    structure.Stats.Hp = (ushort)Math.Min(structure.Stats.Hp + repairPower, structure.Stats.Base.Battle.MaxHp);
                    structure.EndUpdate();
                };
        }

        private void Upkeep()
        {
            PostFirstLoop += city =>
                {
                    if (!Config.resource_upkeep)
                        return;
                    
                    if (city.Resource.Crop.Upkeep <= city.Resource.Crop.Rate)
                        return;

                    int upkeepCost = Math.Max(1, (int)((INTERVAL/3600f)/Config.seconds_per_unit)*(city.Resource.Crop.Upkeep - city.Resource.Crop.Rate));

                    if (city.Resource.Crop.Value < upkeepCost)
                        city.Worker.DoPassive(city, new StarvePassiveAction(city.Id), false);

                    city.Resource.Crop.Subtract(upkeepCost);
                };
        }

        private void FastIncome()
        {
           PostFirstLoop += city =>
            {
                if (!Config.resource_fast_income)
                    return;

                var resource = new Resource(15000, city.Resource.Gold.Value < 99500 ? 250 : 0, 15000, 15000, 0);
                city.Resource.Add(resource);
            };
        }

        private void WeaponExport()
        {
            int weaponExportMarket = 0;
            int weaponExportMax = 0;

            InitVars += city =>
                {
                    weaponExportMax = 0;
                    weaponExportMarket = 0;
                };

            FirstLoop += (city, structure) =>
                {
                    if (!everyOther)
                        return;

                    var effects = structure.Technologies.GetEffects(EffectCode.WeaponExport, EffectInheritance.Self);

                    if (effects.Count > 0)
                    {
                        int weaponExportLvl = effects.Max(x => (int)x.Value[0]);
                        weaponExportMax = Math.Max(weaponExportMax, weaponExportLvl);
                    }

                    if (Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Market", structure))
                        weaponExportMarket += structure.Lvl;
                };

            PostFirstLoop += city =>
                {
                    if (city.Resource.Gold.Value > weaponExportMax * 500) return;
                    int gold = weaponExportMax*weaponExportMarket;
                    gold += Formula.GetWeaponExportLaborProduce(weaponExportMax, city.Resource.Labor.Value);
                    if (gold <= 0)
                        return;
                    city.Resource.Gold.Add(gold);
                };
        }
    }
}