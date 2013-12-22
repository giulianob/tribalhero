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

        private readonly CallbackProcedure callbackProcedure;
        
        private readonly IStructureCsvFactory structureCsvFactory;

        public StructureChangePassiveAction(Formula formula,
                                            IWorld world,
                                            ILocker locker,
                                            Procedure procedure,
                                            CallbackProcedure callbackProcedure,
                                            IStructureCsvFactory structureCsvFactory)
        {
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.procedure = procedure;
            this.callbackProcedure = callbackProcedure;
            this.structureCsvFactory = structureCsvFactory;
        }

        public StructureChangePassiveAction(uint cityId,
                                            uint objectId,
                                            int seconds,
                                            ushort newType,
                                            byte newLvl,
                                            Formula formula,
                                            IWorld world,
                                            ILocker locker,
                                            Procedure procedure,
                                            CallbackProcedure callbackProcedure,
                                            IStructureCsvFactory structureCsvFactory)
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
            this.callbackProcedure = callbackProcedure;
            this.structureCsvFactory = structureCsvFactory;
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
                                            Procedure procedure,
                                            CallbackProcedure callbackProcedure,
                                            IStructureCsvFactory structureCsvFactory)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.formula = formula;
            this.world = world;
            this.locker = locker;
            this.procedure = procedure;
            this.callbackProcedure = callbackProcedure;
            this.structureCsvFactory = structureCsvFactory;
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

        public override void Callback(object custom)
        {
            ICity city;
            IStructure structure;

            // Block structure
            var isOk = locker.Lock(cityId, objectId, out city, out structure).Do(() =>
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

            locker.Lock(cityId, objectId, out city, out structure).Do(() =>
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

                procedure.StructureChange(structure, type, lvl, callbackProcedure, structureCsvFactory);
                
                StateChange(ActionState.Completed);
            });
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
            locker.Lock(cityId, objectId, out city, out structure).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                StateChange(ActionState.Failed);
            });
        }
    }
}