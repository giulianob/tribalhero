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
        private uint cityId;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly IWorld world;

        private readonly ILocker locker;

        private readonly Formula formula;

        private uint structureId;

        public StructureDowngradeActiveAction(IObjectTypeFactory objectTypeFactory,
                                              IStructureCsvFactory structureCsvFactory,
                                              IWorld world,
                                              ILocker locker,
                                              Formula formula)
        {
            this.objectTypeFactory = objectTypeFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.world = world;
            this.locker = locker;
            this.formula = formula;
        }

        public StructureDowngradeActiveAction(uint cityId,
                                              uint structureId,
                                              IObjectTypeFactory objectTypeFactory,
                                              IStructureCsvFactory structureCsvFactory,
                                              IWorld world,
                                              ILocker locker,
                                              Formula formula)
            : this(objectTypeFactory, structureCsvFactory, world, locker, formula)
        {
            this.cityId = cityId;
            this.structureId = structureId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
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

            var buildTime = formula.BuildTime(structureCsvFactory.GetTime(structure.Type, (byte)(structure.Lvl + 1)), city, structure.Technologies);
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(buildTime));
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
            var isOk = locker.Lock(cityId, structureId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return false;
                }

                if (structure == null)
                {
                    StateChange(ActionState.Completed);
                    return false;
                }

                if (WorkerObject.WorkerId != structureId)
                {
                    city.References.Remove(structure, this);
                }

                if (structure.CheckBlocked(ActionId))
                {
                    StateChange(ActionState.Failed);
                    return false;
                }

                if (structure.State.Type == ObjectState.Battle)
                {
                    StateChange(ActionState.Failed);
                    return false;
                }

                structure.BeginUpdate();
                structure.IsBlocked = ActionId;
                structure.EndUpdate();

                return true;
            });

            if (!isOk)
            {
                return;
            }

            structure.City.Worker.Remove(structure, new GameAction[] {this});

            locker.Lock(cityId, structureId, out city, out structure).Do(() =>
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
            });
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        private void InterruptCatchAll()
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
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
            });
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