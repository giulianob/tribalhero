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
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class StructureChangeActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;

        private readonly byte lvl;

        private readonly uint structureId;

        private readonly uint type;

        private Resource cost;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        public StructureChangeActiveAction(uint cityId, uint structureId, uint type, byte lvl, IStructureCsvFactory structureCsvFactory, Formula formula, IWorld world, Procedure procedure, ILocker locker)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            this.lvl = lvl;
            this.structureCsvFactory = structureCsvFactory;
            this.formula = formula;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
        }

        public StructureChangeActiveAction(uint id,
                                           DateTime beginTime,
                                           DateTime nextTime,
                                           DateTime endTime,
                                           int workerType,
                                           byte workerIndex,
                                           ushort actionCount,
                                           IDictionary<string, string> properties,
                                           IStructureCsvFactory structureCsvFactory,
                                           Formula formula,
                                           IWorld world,
                                           Procedure procedure,
                                           ILocker locker)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.structureCsvFactory = structureCsvFactory;
            this.formula = formula;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            lvl = byte.Parse(properties["lvl"]);
            type = uint.Parse(properties["type"]);
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
                return ActionType.StructureChangeActive;
            }
        }

        public override Error Validate(string[] parms)
        {
            if (type == uint.Parse(parms[0]) && lvl == uint.Parse(parms[1]))
            {
                return Error.Ok;
            }
            return Error.ActionInvalid;
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;
            if (!world.TryGetObjects(cityId, structureId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            cost = formula.StructureCost(structure.City, structureCsvFactory.GetBaseStats((ushort)type, lvl));
            if (cost == null)
            {
                return Error.ObjectNotFound;
            }

            if (!structure.City.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(cost);
            structure.City.EndUpdate();

            var buildTime = formula.BuildTime(structureCsvFactory.GetTime((ushort)type, lvl), city, structure.Technologies);

            endTime = SystemClock.Now.AddSeconds(CalculateTime(buildTime));
            BeginTime = SystemClock.Now;

            return Error.Ok;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            using (locker.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(formula.GetActionCancelResource(BeginTime, cost));
                    city.EndUpdate();
                }

                StateChange(ActionState.Failed);
            }
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
            IStructure structure;

            // Block structure
            using (locker.Lock(cityId, structureId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                if (structure.CheckBlocked(ActionId))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                structure.IsBlocked = ActionId;
                structure.EndUpdate();
            }

            structure.City.Worker.Remove(structure, new GameAction[] {this});

            using (locker.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                city.BeginUpdate();
                structure.BeginUpdate();
                structure.IsBlocked = 0;
                procedure.StructureChange(structure, (ushort)type, lvl);
                structure.EndUpdate();
                city.EndUpdate();

                StateChange(ActionState.Completed);
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
                                new XmlKvPair("type", type), new XmlKvPair("lvl", lvl), new XmlKvPair("city_id", cityId),
                                new XmlKvPair("structure_id", structureId), new XmlKvPair("wood", cost.Wood),
                                new XmlKvPair("crop", cost.Crop), new XmlKvPair("iron", cost.Iron),
                                new XmlKvPair("gold", cost.Gold), new XmlKvPair("labor", cost.Labor),
                        });
            }
        }

        #endregion
    }
}