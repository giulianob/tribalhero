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
    public class StructureChangeActiveAction : ScheduledActiveAction
    {
        private uint cityId;

        private byte lvl;

        private uint structureId;

        private uint type;

        private Resource cost;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        private readonly CallbackProcedure callbackProcedure;

        public StructureChangeActiveAction(IStructureCsvFactory structureCsvFactory,
                                           Formula formula,
                                           IWorld world,
                                           Procedure procedure,
                                           ILocker locker,
                                           CallbackProcedure callbackProcedure)
        {
            this.structureCsvFactory = structureCsvFactory;
            this.formula = formula;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
            this.callbackProcedure = callbackProcedure;
        }

        public StructureChangeActiveAction(uint cityId,
                                           uint structureId,
                                           uint type,
                                           byte lvl,
                                           IStructureCsvFactory structureCsvFactory,
                                           Formula formula,
                                           IWorld world,
                                           Procedure procedure,
                                           ILocker locker,
                                           CallbackProcedure callbackProcedure)
            : this(structureCsvFactory, formula, world, procedure, locker, callbackProcedure)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            this.lvl = lvl;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
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
            locker.Lock(cityId, out city).Do(() =>
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
            IStructure structure;

            // Block structure
            var isOk = locker.Lock(cityId, structureId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return false;
                }

                if (structure.CheckBlocked(ActionId))
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

            locker.Lock(cityId, out city).Do(() =>
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
                
                procedure.StructureChange(structure, (ushort)type, lvl, callbackProcedure, structureCsvFactory);

                StateChange(ActionState.Completed);
            });
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