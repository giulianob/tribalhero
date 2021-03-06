#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Logic.Triggers;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class TechnologyUpgradeActiveAction : ScheduledActiveAction
    {
        private uint cityId;

        private uint structureId;

        private uint techId;

        private readonly IWorld world;

        private readonly Formula formula;

        private readonly ILocker locker;

        private readonly TechnologyFactory technologyFactory;

        private readonly CallbackProcedure callbackProcedure;

        public TechnologyUpgradeActiveAction(IWorld world,
                                             Formula formula,
                                             ILocker locker,
                                             TechnologyFactory technologyFactory,
                                             CallbackProcedure callbackProcedure)
        {
            this.world = world;
            this.formula = formula;
            this.locker = locker;
            this.technologyFactory = technologyFactory;
            this.callbackProcedure = callbackProcedure;
        }

        public TechnologyUpgradeActiveAction(uint cityId,
                                             uint structureId,
                                             uint techId,
                                             IWorld world,
                                             Formula formula,
                                             ILocker locker,
                                             TechnologyFactory technologyFactory,
                                             CallbackProcedure callbackProcedure)
            : this(world, formula, locker, technologyFactory, callbackProcedure)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.techId = techId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            techId = uint.Parse(properties["tech_id"]);
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Normal;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.TechnologyUpgradeActive;
            }
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("tech_id", techId), 
                                new XmlKvPair("city_id", cityId),
                                new XmlKvPair("structure_id", structureId)
                        });
            }
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            byte maxLevel = byte.Parse(parms[1]);

            if (uint.Parse(parms[0]) != techId)
            {
                return Error.ActionInvalid;
            }

            ICity city;
            IStructure structure;
            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            Technology tech;
            if (!structure.Technologies.TryGetTechnology(techId, out tech))
            {
                return Error.Ok;
            }

            if (tech.Level >= maxLevel)
            {
                return Error.TechnologyMaxLevelReached;
            }

            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;
            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            Technology tech;
            TechnologyBase techBase;
            if (structure.Technologies.TryGetTechnology(techId, out tech))
            {
                techBase = technologyFactory.GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));
            }
            else
            {
                techBase = technologyFactory.GetTechnologyBase(techId, 1);
            }

            if (techBase == null)
            {
                return Error.ObjectNotFound;
            }

            if (!city.Resource.HasEnough(techBase.Resources))
            {
                return Error.ResourceNotEnough;
            }

            city.BeginUpdate();
            city.Resource.Subtract(techBase.Resources);
            city.EndUpdate();

            BeginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(formula.BuildTime((int)techBase.Time, city, city.Technologies)));

            return Error.Ok;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (!world.TryGetObjects(cityId, structureId, out city, out structure))
                {
                    return;
                }

                Technology tech;
                TechnologyBase techBase;
                if (structure.Technologies.TryGetTechnology(techId, out tech))
                {
                    techBase = technologyFactory.GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));
                }
                else
                {
                    techBase = technologyFactory.GetTechnologyBase(techId, 1);
                }

                if (techBase == null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(formula.GetActionCancelResource(BeginTime, techBase.Resources));
                    city.EndUpdate();
                }

                StateChange(ActionState.Failed);
            });
        }

        public override void UserCancelled()
        {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll(wasKilled);
        }

        public override void Callback(object custom)
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                Technology tech;
                TechnologyBase techBase;
                if (structure.Technologies.TryGetTechnology(techId, out tech))
                {
                    techBase = technologyFactory
                            .GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));

                    if (techBase == null)
                    {
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.BeginUpdate();
                    if (!structure.Technologies.Upgrade(new Technology(techBase)))
                    {
                        structure.EndUpdate();
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.EndUpdate();
                }
                else
                {
                    techBase = technologyFactory.GetTechnologyBase(techId, 1);

                    if (techBase == null)
                    {
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.BeginUpdate();
                    if (!structure.Technologies.Add(new Technology(techBase)))
                    {
                        structure.EndUpdate();
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.EndUpdate();
                }

                callbackProcedure.OnTechnologyUpgrade(structure, techBase);
                StateChange(ActionState.Completed);
            });
        }
    }
}