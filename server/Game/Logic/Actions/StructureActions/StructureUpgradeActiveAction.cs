#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class StructureUpgradeActiveAction : ScheduledActiveAction
    {
        private uint cityId;

        private uint structureId;

        private Resource cost;

        private ushort type;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        private readonly IRequirementCsvFactory requirementCsvFactory;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly CallbackProcedure callbackProcedure;

        private readonly InstantProcedure instantProcedure;

        public StructureUpgradeActiveAction(IStructureCsvFactory structureCsvFactory,
                                            Formula formula,
                                            IWorld world,
                                            Procedure procedure,
                                            ILocker locker,
                                            IRequirementCsvFactory requirementCsvFactory,
                                            IObjectTypeFactory objectTypeFactory,
                                            CallbackProcedure callbackProcedure,
                                            InstantProcedure instantProcedure)
        {
            this.structureCsvFactory = structureCsvFactory;
            this.formula = formula;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
            this.requirementCsvFactory = requirementCsvFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.callbackProcedure = callbackProcedure;
            this.instantProcedure = instantProcedure;
        }

        public StructureUpgradeActiveAction(uint cityId,
                                            uint structureId,
                                            IStructureCsvFactory structureCsvFactory,
                                            Formula formula,
                                            IWorld world,
                                            Procedure procedure,
                                            ILocker locker,
                                            IRequirementCsvFactory requirementCsvFactory,
                                            IObjectTypeFactory objectTypeFactory,
                                            CallbackProcedure callbackProcedure,
                                            InstantProcedure instantProcedure)
            : this(structureCsvFactory, formula, world, procedure, locker, requirementCsvFactory, objectTypeFactory, callbackProcedure, instantProcedure)
        {
            this.cityId = cityId;
            this.structureId = structureId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            cost = new Resource(int.Parse(properties["crop"]),
                                int.Parse(properties["gold"]),
                                int.Parse(properties["iron"]),
                                int.Parse(properties["wood"]),
                                int.Parse(properties["labor"]));
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.StandAlone;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureUpgradeActive;
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            var maxConcurrentUpgradesResult = formula.CityMaxConcurrentBuildActions(type, ActionId, city, objectTypeFactory);

            if (maxConcurrentUpgradesResult != Error.Ok)
            {
                return maxConcurrentUpgradesResult;
            }
            
            var stats = structureCsvFactory.GetBaseStats(structure.Type, (byte)(structure.Lvl + 1));
            if (stats == null)
            {
                return Error.ObjectStructureNotFound;                
            }

            cost = formula.StructureCost(city, stats);
            type = structure.Type;

            // layout requirement
            if (!requirementCsvFactory
                         .GetLayoutRequirement(structure.Type, (byte)(structure.Lvl + 1))
                         .Validate(structure, structure.Type, structure.PrimaryPosition.X, structure.PrimaryPosition.Y, structure.Size))
            {
                return Error.LayoutNotFullfilled;
            }

            if (!structure.City.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            if (instantProcedure.BuildNext(city, structure))
            {
                endTime = BeginTime = SystemClock.Now;
            }
            else
            {
                var buildTime = formula.BuildTime(structureCsvFactory.GetTime(structure.Type, (byte)(structure.Lvl + 1)), city, structure.Technologies);
                endTime = SystemClock.Now.AddSeconds(CalculateTime(buildTime));
                BeginTime = SystemClock.Now;
            }

            return Error.Ok;
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

                structure.City.BeginUpdate();
                structure.BeginUpdate();
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));

                procedure.OnStructureUpgradeDowngrade(structure);

                structure.EndUpdate();                               
                structure.City.EndUpdate();
                
                callbackProcedure.OnStructureUpgrade(structure);

                StateChange(ActionState.Completed);
            });
        }

        public override Error Validate(string[] parms)
        {
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
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(formula.GetActionCancelResource(BeginTime, cost));
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

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), 
                                new XmlKvPair("structure_id", structureId),
                                new XmlKvPair("wood", cost.Wood), 
                                new XmlKvPair("crop", cost.Crop),
                                new XmlKvPair("iron", cost.Iron), 
                                new XmlKvPair("gold", cost.Gold),
                                new XmlKvPair("labor", cost.Labor)
                        });
            }
        }

        public override Error SystemCancelable
        {
            get
            {
                return Error.Ok;
            }
        }

        #endregion
    }
}