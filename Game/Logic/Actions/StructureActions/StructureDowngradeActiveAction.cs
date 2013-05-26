#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class StructureDowngradeActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly StructureCsvFactory structureCsvFactory;

        private readonly World world;

        private readonly ILocker locker;

        private readonly Formula formula;

        private readonly uint structureId;

        public StructureDowngradeActiveAction(uint cityId,
                                              uint structureId,
                                              ObjectTypeFactory objectTypeFactory,
                                              StructureCsvFactory structureCsvFactory,
                                              World world,
                                              ILocker locker,
                                              Formula formula)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.objectTypeFactory = objectTypeFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.world = world;
            this.locker = locker;
            this.formula = formula;
        }

        public StructureDowngradeActiveAction(uint id,
                                              DateTime beginTime,
                                              DateTime nextTime,
                                              DateTime endTime,
                                              int workerType,
                                              byte workerIndex,
                                              ushort actionCount,
                                              Dictionary<string, string> properties,
                                              ObjectTypeFactory objectTypeFactory,
                                              StructureCsvFactory structureCsvFactory,
                                              World world,
                                              ILocker locker,
                                              Formula formula)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.objectTypeFactory = objectTypeFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.world = world;
            this.locker = locker;
            this.formula = formula;
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
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
                return ActionType.StructureDowngradeActive;
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

            if (objectTypeFactory.IsStructureType("MainBuilding", structure))
            {
                return Error.StructureUndowngradable;
            }

            if (objectTypeFactory.IsStructureType("NonUserDestroyable", structure))
            {
                return Error.StructureUndowngradable;
            }

            if (objectTypeFactory.IsStructureType("Undestroyable", structure))
            {
                return Error.StructureUndestroyable;
            }

            endTime =
                    DateTime.UtcNow.AddSeconds(
                                               CalculateTime(
                                                             formula.BuildTime(
                                                                                       structureCsvFactory.GetTime(
                                                                                                                structure
                                                                                                                        .Type,
                                                                                                                (byte)
                                                                                                                (structure
                                                                                                                         .Lvl +
                                                                                                                 1)),
                                                                                       city,
                                                                                       structure.Technologies)));
            BeginTime = DateTime.UtcNow;

            if (WorkerObject.WorkerId != structureId)
            {
                city.References.Add(structure, this);
            }

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            IStructure structure;

            // Block structure
            using (locker.Lock(cityId, structureId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                if (structure == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                if (WorkerObject.WorkerId != structureId)
                {
                    city.References.Remove(structure, this);
                }

                if (structure.CheckBlocked(ActionId))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (structure.State.Type == ObjectState.Battle)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                structure.IsBlocked = ActionId;
                structure.EndUpdate();
            }

            structure.City.Worker.Remove(structure, new GameAction[] {this});

            using (locker.Lock(cityId, structureId, out city, out structure))
            {
                city.BeginUpdate();
                structure.BeginUpdate();

                // Unblock structure since we're done with it in this action and ObjectRemoveAction will take it from here
                structure.IsBlocked = 0;

                // Send any laborers back
                city.Resource.Labor.Add(structure.Stats.Labor);
                structure.Stats.Labor = 0;

                // Destroy structure
                world.Regions.Remove(structure);
                city.ScheduleRemove(structure, false);

                structure.EndUpdate();
                city.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        private void InterruptCatchAll()
        {
            ICity city;
            using (locker.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (WorkerObject.WorkerId != structureId && city.TryGetStructure(structureId, out structure))
                {
                    city.References.Remove(structure, this);
                }

                StateChange(ActionState.Failed);
            }
        }

        public override void UserCancelled()
        {
            InterruptCatchAll();
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll();
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId)});
            }
        }

        #endregion
    }
}