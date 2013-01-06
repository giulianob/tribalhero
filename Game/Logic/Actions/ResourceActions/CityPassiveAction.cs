#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class CityPassiveAction : ScheduledPassiveAction
    {
        public const int PLAYER_IDLE_DAYS = 2;

        private const int INTERVAL_IN_SECONDS = 1800;

        private readonly IActionFactory actionFactory;

        private readonly uint cityId;

        private readonly Formula formula;

        private readonly ILocker locker;

        private readonly ObjectTypeFactory objectTypeFactory;

        private bool everyOther;

        private int laborTimeRemains;

        public CityPassiveAction(uint cityId,
                                 ObjectTypeFactory objectTypeFactory,
                                 ILocker locker,
                                 Formula formula,
                                 IActionFactory actionFactory)
        {
            this.cityId = cityId;
            this.objectTypeFactory = objectTypeFactory;
            this.locker = locker;
            this.formula = formula;
            this.actionFactory = actionFactory;

            CreateSubscriptions();
        }

        public CityPassiveAction(uint id,
                                 DateTime beginTime,
                                 DateTime nextTime,
                                 DateTime endTime,
                                 bool isVisible,
                                 string nlsDescription,
                                 Dictionary<string, string> properties,
                                 ObjectTypeFactory objectTypeFactory,
                                 ILocker locker,
                                 Formula formula,
                                 IActionFactory actionFactory)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.objectTypeFactory = objectTypeFactory;
            this.locker = locker;
            this.formula = formula;
            this.actionFactory = actionFactory;
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
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("labor_time_remains", laborTimeRemains),
                                new XmlKvPair("every_other", everyOther)
                        });
            }
        }

        private event Init InitVars;

        private event StructureLoop FirstLoop;

        private event PostLoop PostFirstLoop;

        private event StructureLoop SecondLoop;

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            beginTime = SystemClock.Now;
            endTime = SystemClock.Now.AddSeconds(CalculateTime(INTERVAL_IN_SECONDS));
            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            using (locker.Lock(cityId, out city))
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
            using (locker.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                if (Config.actions_skip_city_actions && city.Owner.Session == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                everyOther = !everyOther;

                city.BeginUpdate();

                if (InitVars != null)
                {
                    InitVars(city);
                }

                if (FirstLoop != null)
                {
                    foreach (var structure in city)
                    {
                        FirstLoop(city, structure);
                    }
                }

                if (PostFirstLoop != null)
                {
                    PostFirstLoop(city);
                }

                if (SecondLoop != null)
                {
                    foreach (var structure in city)
                    {
                        SecondLoop(city, structure);
                    }
                }

                city.EndUpdate();

                // Stop city action if player has not login for more than a week
                if (city.Owner.Session != null && SystemClock.Now.Subtract(city.Owner.LastLogin).TotalDays > 7)
                {
                    StateChange(ActionState.Completed);
                }
                else
                {
                    beginTime = SystemClock.Now;
                    endTime = SystemClock.Now.AddSeconds(CalculateTime(INTERVAL_IN_SECONDS));
                    StateChange(ActionState.Fired);
                }
            }
        }

        private void Labor()
        {
            int laborTotal = 0;

            InitVars += city => { laborTotal = city.Resource.Labor.Value; };

            FirstLoop += (city, structure) =>
                {
                    if (structure.Stats.Labor > 0)
                    {
                        laborTotal += structure.Stats.Labor;
                    }
                };

            PostFirstLoop += city =>
                {
                    if (city.Owner.IsIdle)
                    {
                        return;
                    }

                    laborTimeRemains += (int)CalculateTime(INTERVAL_IN_SECONDS);
                    int laborRate = formula.GetLaborRate(laborTotal, city);
                    if (laborRate <= 0)
                    {
                        return;
                    }

                    int laborProduction = laborTimeRemains / laborRate;
                    if (laborProduction <= 0)
                    {
                        return;
                    }

                    laborTimeRemains -= laborProduction * laborRate;
                    city.Resource.Labor.Add(laborProduction);
                };
        }

        private void Repair()
        {
            ushort repairPower = 0;

            InitVars += city => repairPower = 0;

            FirstLoop += (city, structure) =>
                {
                    if (objectTypeFactory.IsStructureType("RepairBuilding", structure))
                    {
                        repairPower += formula.RepairRate(structure);
                    }
                };

            SecondLoop += (city, structure) =>
                {
                    if (repairPower <= 0)
                    {
                        return;
                    }

                    if (structure.Stats.Base.Battle.MaxHp <= structure.Stats.Hp ||
                        objectTypeFactory.IsStructureType("NonRepairable", structure) ||
                        structure.State.Type == ObjectState.Battle)
                    {
                        return;
                    }

                    structure.BeginUpdate();
                    structure.Stats.Hp =
                            (ushort)Math.Min(structure.Stats.Hp + repairPower, structure.Stats.Base.Battle.MaxHp);
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

                    int cropCost = -city.Resource.Crop.GetAmountReceived((int)(CalculateTime(INTERVAL_IN_SECONDS) * 1000));
                    Resource upkeepCost = new Resource(crop: Math.Max(0, cropCost));

                    if (upkeepCost.Empty)
                    {
                        return;
                    }

                    if (!city.Resource.HasEnough(upkeepCost))
                    {
                        city.Worker.DoPassive(city, actionFactory.CreateStarvePassiveAction(city.Id), false);
                    }

                    city.Resource.Subtract(upkeepCost);
                };
        }

        private void FastIncome()
        {
            PostFirstLoop += city =>
                {
                    if (!Config.resource_fast_income || Config.server_production)
                    {
                        return;
                    }

                    var resource = new Resource(15000, city.Resource.Gold.Value < 99999 ? 99999 : 0, 15000, 15000, 0);
                    city.Resource.Add(resource);
                };
        }

        private void WeaponExport()
        {
            PostFirstLoop += city =>
                {
                    var weaponExportMax =
                            city.Technologies.GetEffects(EffectCode.WeaponExport)
                                .DefaultIfEmpty()
                                .Max(x => x == null ? 0 : (int)x.Value[0]);
                    int gold = formula.GetWeaponExportLaborProduce(weaponExportMax, city.Resource.Labor.Value);
                    if (gold <= 0)
                    {
                        return;
                    }
                    city.Resource.Gold.Add(gold);
                };
        }

        private void AlignmentPoint()
        {
            PostFirstLoop += city =>
                {
                    if (Math.Abs(city.AlignmentPoint - 50m) < (Config.ap_deduction_per_hour / 2))
                    {
                        city.AlignmentPoint = 50m;
                    }
                    else
                    {
                        city.AlignmentPoint += city.AlignmentPoint > 50m
                                                       ? -(Config.ap_deduction_per_hour / 2)
                                                       : (Config.ap_deduction_per_hour / 2);
                    }
                };
        }

        private delegate void Init(ICity city);

        private delegate void PostLoop(ICity city);

        private delegate void StructureLoop(ICity city, IStructure structure);
    }
}