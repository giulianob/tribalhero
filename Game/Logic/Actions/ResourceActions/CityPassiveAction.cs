#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using System.Linq;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class CityPassiveAction : ScheduledPassiveAction
    {

        private delegate void Init(ICity city);
        private delegate void PostLoop(ICity city);
        private delegate void StructureLoop(ICity city, IStructure structure);

        private event Init InitVars;

        private event StructureLoop FirstLoop;
        private event PostLoop PostFirstLoop;
        private event StructureLoop SecondLoop;

        private const int INTERVAL_IN_SECONDS = 1800;
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
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(INTERVAL_IN_SECONDS));
            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
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
            AlignmentPoint();
        }

        public override void Callback(object custom)
        {            
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                    return;

                if (Config.actions_skip_city_actions && city.Owner.Session == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                everyOther = !everyOther;

                city.BeginUpdate();

                if (InitVars != null)
                    InitVars(city);

                if (FirstLoop != null)
                    foreach (var structure in city)
                        FirstLoop(city, structure);

                if (PostFirstLoop != null)
                    PostFirstLoop(city);

                if (SecondLoop != null)
                    foreach (var structure in city)
                        SecondLoop(city, structure);

                city.EndUpdate();

                // Stop city action if player has not login for more than a week
                if (city.Owner.Session != null && DateTime.UtcNow.Subtract(city.Owner.LastLogin).TotalDays > 7)
                    StateChange(ActionState.Completed);
                else
                {
                    beginTime = DateTime.UtcNow;
                    endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : INTERVAL_IN_SECONDS * Config.seconds_per_unit);
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
                    if (city.Owner.Session == null && DateTime.UtcNow.Subtract(city.Owner.LastLogin).TotalDays > 2)
                        return;

                    laborTimeRemains += INTERVAL_IN_SECONDS;
                    int laborRate = Formula.Current.GetLaborRate(laborTotal, city);
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
                        repairPower += Formula.Current.RepairRate(structure);
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
                    if (!Config.troop_starve)
                    {
                        return;
                    }

                    Resource upkeepCost = new Resource(Math.Max(0, -1 * city.Resource.Crop.GetAmountReceived(INTERVAL_IN_SECONDS*1000)), 0, 0, 0, 0);

                    if (upkeepCost.Empty)
                    {
                        return;
                    }

                    if (!city.Resource.HasEnough(upkeepCost))
                    {
                        city.Worker.DoPassive(city, new StarvePassiveAction(city.Id), false);
                    }

                    city.Resource.Subtract(upkeepCost);

                };
        }

        private void FastIncome()
        {
           PostFirstLoop += city =>
            {
                if (!Config.resource_fast_income)
                    return;

                var resource = new Resource(15000, city.Resource.Gold.Value < 99999 ? 99999 : 0, 15000, 15000, 0);
                city.Resource.Add(resource);
            };
        }

        private void WeaponExport()
        {

            PostFirstLoop += city =>
                {
                    var weaponExportMax = city.Technologies.GetEffects(EffectCode.WeaponExport).DefaultIfEmpty().Max(x =>x==null?0:(int)x.Value[0]);
                    int gold = Formula.Current.GetWeaponExportLaborProduce(weaponExportMax, city.Resource.Labor.Value);
                    if (gold <= 0)
                        return;
                    city.Resource.Gold.Add(gold);
                };
        }

        private void AlignmentPoint()
        {
            PostFirstLoop += city =>
            {
                if (Math.Abs(city.AlignmentPoint - 50m) < .125m)
                {
                    city.AlignmentPoint = 50m;
                }
                else
                {
                    city.AlignmentPoint += city.AlignmentPoint > 50m ? -.125m : +.125m;
                }
            };
        }

    }
}