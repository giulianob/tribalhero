#region

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly uint cityId;

        private readonly uint structureId;

        private Resource cost;

        private ushort type;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        private readonly IRequirementCsvFactory requirementCsvFactory;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly InitFactory initFactory;

        public StructureUpgradeActiveAction(uint cityId,
                                            uint structureId,
                                            IStructureCsvFactory structureCsvFactory,
                                            Formula formula,
                                            IWorld world,
                                            Procedure procedure,
                                            ILocker locker,
                                            IRequirementCsvFactory requirementCsvFactory,
                                            IObjectTypeFactory objectTypeFactory,
                                            InitFactory initFactory)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.structureCsvFactory = structureCsvFactory;
            this.formula = formula;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
            this.requirementCsvFactory = requirementCsvFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.initFactory = initFactory;
        }

        public StructureUpgradeActiveAction(uint id,
                                            DateTime beginTime,
                                            DateTime nextTime,
                                            DateTime endTime,
                                            int workerType,
                                            byte workerIndex,
                                            ushort actionCount,
                                            Dictionary<string, string> properties,
                                            IStructureCsvFactory structureCsvFactory,
                                            Formula formula,
                                            IWorld world,
                                            Procedure procedure,
                                            ILocker locker,
                                            IRequirementCsvFactory requirementCsvFactory,
                                            IObjectTypeFactory objectTypeFactory,
                                            InitFactory initFactory)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.structureCsvFactory = structureCsvFactory;
            this.formula = formula;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
            this.requirementCsvFactory = requirementCsvFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.initFactory = initFactory;
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

            int maxConcurrentUpgrades = formula.ConcurrentBuildUpgrades(city.MainBuilding.Lvl);

            if (!objectTypeFactory.IsObjectType("UnlimitedBuilding", type) &&
                city.Worker.ActiveActions.Values.Count(
                                                       action =>
                                                       action.ActionId != ActionId &&
                                                       (action.Type == ActionType.StructureUpgradeActive ||
                                                        (action.Type == ActionType.StructureBuildActive &&
                                                         objectTypeFactory
                                                                 .IsObjectType("UnlimitedBuilding",
                                                                               ((StructureBuildActiveAction)action)
                                                                                       .BuildType)))) >=
                maxConcurrentUpgrades)
            {
                return Error.ActionTotalMaxReached;
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

            var buildTime = formula.BuildTime(structureCsvFactory.GetTime(structure.Type, (byte)(structure.Lvl + 1)), city, structure.Technologies);
            endTime = SystemClock.Now.AddSeconds(CalculateTime(buildTime));
            BeginTime = SystemClock.Now;

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

                structure.BeginUpdate();
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));
                structure.EndUpdate();

                initFactory.InitGameObject(InitCondition.OnUpgrade, structure, structure.Type, structure.Lvl);

                structure.City.BeginUpdate();
                procedure.OnStructureUpgradeDowngrade(structure);
                structure.City.EndUpdate();

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

        #endregion
    }
}