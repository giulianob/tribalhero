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
using Ninject.Extensions.Logging;

#endregion

namespace Game.Logic.Actions
{
    public class StructureChangePassiveAction : ScheduledPassiveAction, IScriptable
    {
        private readonly Formula formula;

        private readonly IWorld world;

        private readonly ILocker locker;

        private readonly Procedure procedure;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private uint cityId;

        private byte lvl;

        private uint objectId;

        private TimeSpan ts;

        private ushort type;

        public StructureChangePassiveAction(Formula formula, IWorld world, ILocker locker, Procedure procedure)
        {
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.procedure = procedure;
        }

        public StructureChangePassiveAction(uint cityId,
                                            uint objectId,
                                            int seconds,
                                            ushort newType,
                                            byte newLvl,
                                            Formula formula,
                                            IWorld world,
                                            ILocker locker,
                                            Procedure procedure)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            ts = TimeSpan.FromSeconds(seconds);
            type = newType;
            lvl = newLvl;
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.procedure = procedure;
        }

        public StructureChangePassiveAction(uint id,
                                            DateTime beginTime,
                                            DateTime nextTime,
                                            DateTime endTime,
                                            bool isVisible,
                                            string nlsDescription,
                                            Dictionary<string, string> properties,
                                            Formula formula,
                                            IWorld world,
                                            ILocker locker,
                                            Procedure procedure)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.procedure = procedure;
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
            type = ushort.Parse(properties["type"]);
            lvl = byte.Parse(properties["lvl"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureChangePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("object_id", objectId),
                                new XmlKvPair("type", type), new XmlKvPair("lvl", lvl)
                        });
            }
        }

        #region IScriptable Members

        public void ScriptInit(IGameObject obj, string[] parms)
        {
            ICity city;
            IStructure structure;

            if (!(obj is IStructure))
            {
                throw new Exception();
            }

            cityId = obj.City.Id;
            objectId = obj.ObjectId;

            ts = formula.ReadCsvTimeFormat(parms[0]);
            type = ushort.Parse(parms[1]);
            lvl = byte.Parse(parms[2]);

            if (!world.TryGetObjects(cityId, objectId, out city, out structure))
            {
                return;
            }

            city.Worker.DoPassive(structure, this, true);
        }

        #endregion

        public override void Callback(object custom)
        {
            ICity city;
            IStructure structure;

            // Block structure
            using (locker.Lock(cityId, objectId, out city, out structure))
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

            using (locker.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                if (structure == null)
                {
                    logger.Warn("StructureChange did not find structure");
                    StateChange(ActionState.Completed);
                    return;
                }

                structure.City.BeginUpdate();
                structure.BeginUpdate();
                structure.IsBlocked = 0;
                procedure.StructureChange(structure, type, lvl);
                structure.EndUpdate();
                structure.City.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            IStructure structure;

            endTime = SystemClock.Now.AddSeconds(CalculateTime(ts.TotalSeconds));
            BeginTime = SystemClock.Now;

            if (!world.TryGetObjects(cityId, objectId, out city, out structure))
            {
                return Error.ObjectNotFound;
            }

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            ICity city;
            IStructure structure;
            using (locker.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            }
        }
    }
}