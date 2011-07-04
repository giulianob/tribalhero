#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class StructureDowngradeActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly uint structureId;

        public StructureDowngradeActiveAction(uint cityId, uint structureId)
        {
            this.cityId = cityId;
            this.structureId = structureId;
        }

        public StructureDowngradeActiveAction(uint id,
                                            DateTime beginTime,
                                            DateTime nextTime,
                                            DateTime endTime,
                                            int workerType,
                                            byte workerIndex,
                                            ushort actionCount,
                                            Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        public override ConcurrencyType Concurrency
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
            City city;
            Structure structure;

            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            if (ObjectTypeFactory.IsStructureType("Undestroyable", structure) && structure.Lvl <= 1)
                return Error.StructureUndestroyable;

            endTime =
                    DateTime.UtcNow.AddSeconds(
                                               CalculateTime(Formula.BuildTime(StructureFactory.GetTime(structure.Type, (byte)(structure.Lvl + 1)),
                                                                               city,
                                                                               structure.Technologies)));
            BeginTime = DateTime.UtcNow;

            if (WorkerObject.WorkerId != structureId)
                city.Worker.References.Add(structure, this);

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            City city;
            Structure structure;

            // Block structure
            using (new MultiObjectLock(cityId, structureId, out city, out structure))
            {
                if (!IsValid())
                    return;

                if (WorkerObject.WorkerId != structureId)
                    city.Worker.References.Remove(structure, this);

                if (structure == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                if (ObjectTypeFactory.IsStructureType("Undestroyable", structure) && structure.Lvl <= 1)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (structure.State.Type == ObjectState.Battle && structure.Lvl <= 1)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                structure.IsBlocked = true;
                structure.EndUpdate();
            }

            structure.City.Worker.Remove(structure, new GameAction[] {this});

            using (new MultiObjectLock(cityId, structureId, out city, out structure))
            {
                city.BeginUpdate();
                structure.BeginUpdate();

                // We need to unblock the structure here because the ScheduleRemove won't work if it's already blocked. Sort of a hack.
                structure.IsBlocked = false;

                // If structure is level 0 then we don't even try to give any laborers back or anything, just remove it
                if (structure.Lvl == 0)
                {
                    Global.World.Remove(structure);
                    city.ScheduleRemove(structure, false);
                }
                else
                {
                    ushort oldLabor = structure.Stats.Labor;
                    StructureFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl - 1));
                    structure.Stats.Hp = structure.Stats.Base.Battle.MaxHp;
                    structure.Stats.Labor = Math.Min(oldLabor, structure.Stats.Base.MaxLabor);
                    Procedure.AdjustCityResourceRates(structure, structure.Stats.Labor - oldLabor);
                    if (oldLabor > structure.Stats.Base.MaxLabor)
                        city.Resource.Labor.Add(oldLabor - structure.Stats.Base.MaxLabor);

                    Procedure.SetResourceCap(structure.City);

                    if (structure.Lvl > 0)
                        InitFactory.InitGameObject(InitCondition.OnDowngrade, structure, structure.Type, structure.Lvl);
                    else
                    {
                        Global.World.Remove(structure);
                        city.ScheduleRemove(structure, false);
                    }
                }

                structure.EndUpdate();
                city.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                Structure structure;
                if (WorkerObject.WorkerId != structureId && city.TryGetStructure(structureId, out structure))
                    city.Worker.References.Remove(structure, this);

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

        #region IPersistable

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId)});
            }
        }

        #endregion
    }
}