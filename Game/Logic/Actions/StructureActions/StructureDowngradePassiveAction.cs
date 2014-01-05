#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class StructureDowngradePassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;

        private readonly uint structureId;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly Procedure procedure;

        private readonly CallbackProcedure callbackProcedure;

        private readonly ILocker locker;

        public StructureDowngradePassiveAction(uint cityId,
                                               uint structureId,
                                               ILocker locker,
                                               IStructureCsvFactory structureCsvFactory,
                                               Procedure procedure,
											   CallbackProcedure callbackProcedure)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.callbackProcedure = callbackProcedure;
            this.locker = locker;
            this.structureCsvFactory = structureCsvFactory;
            this.procedure = procedure;
        }

        public StructureDowngradePassiveAction(uint id,
                                               DateTime beginTime,
                                               DateTime nextTime,
                                               DateTime endTime,
                                               bool isVisible,
                                               string nlsDescription,
                                               IDictionary<string, string> properties,
                                               CallbackProcedure callbackProcedure,
                                               ILocker locker,
                                               IStructureCsvFactory structureCsvFactory,
                                               Procedure procedure)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.callbackProcedure = callbackProcedure;
            this.locker = locker;
            this.structureCsvFactory = structureCsvFactory;
            this.procedure = procedure;
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureDowngradePassive;
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            endTime = DateTime.UtcNow;
            BeginTime = DateTime.UtcNow;

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            locker.Lock(cityId, out city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            });
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

            locker.Lock(cityId, structureId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                city.BeginUpdate();
                structure.BeginUpdate();
                structure.IsBlocked = 0;
                ushort oldLabor = structure.Stats.Labor;
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl - 1));
                structure.Stats.Hp = structure.Stats.Base.Battle.MaxHp;
                structure.Stats.Labor = Math.Min(oldLabor, structure.Stats.Base.MaxLabor);

                procedure.OnStructureUpgradeDowngrade(structure);

                structure.EndUpdate();
                city.EndUpdate();


                callbackProcedure.OnStructureDowngrade(structure);

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
                        {new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId)});
            }
        }

        #endregion
    }
}