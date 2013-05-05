#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class StructureDowngradePassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;

        private readonly uint structureId;

        public StructureDowngradePassiveAction(uint cityId, uint structureId)
        {
            this.cityId = cityId;
            this.structureId = structureId;
        }

        public StructureDowngradePassiveAction(uint id,
                                               DateTime beginTime,
                                               DateTime nextTime,
                                               DateTime endTime,
                                               bool isVisible,
                                               string nlsDescription,
                                               IDictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
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
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            }
        }

        public override void Callback(object custom)
        {
            ICity city;
            IStructure structure;

            // Block structure
            using (Concurrency.Current.Lock(cityId, structureId, out city, out structure))
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

            structure.City.Worker.Remove(structure, new[] {this});

            using (Concurrency.Current.Lock(cityId, structureId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                city.BeginUpdate();
                structure.BeginUpdate();
                structure.IsBlocked = 0;
                ushort oldLabor = structure.Stats.Labor;
                Ioc.Kernel.Get<StructureFactory>().GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl - 1));
                structure.Stats.Hp = structure.Stats.Base.Battle.MaxHp;
                structure.Stats.Labor = Math.Min(oldLabor, structure.Stats.Base.MaxLabor);

                Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnDowngrade, structure, structure.Type, structure.Lvl);

                Procedure.Current.OnStructureUpgradeDowngrade(structure);

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
                        {new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId)});
            }
        }

        #endregion
    }
}