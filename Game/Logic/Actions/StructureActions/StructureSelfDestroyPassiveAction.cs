#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class StructureSelfDestroyPassiveAction : ScheduledPassiveAction, IScriptable
    {
        private uint cityId;
        private uint objectId;
        private TimeSpan ts;

        public StructureSelfDestroyPassiveAction()
        {
        }

        public StructureSelfDestroyPassiveAction(uint id,
                                          DateTime beginTime,
                                          DateTime nextTime,
                                          DateTime endTime,
                                          bool isVisible,
                                          string nlsDescription, 
                                          Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureSelfDestroyPassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId), new XmlKvPair("object_id", objectId),});
            }
        }

        #region IScriptable Members

        public void ScriptInit(GameObject obj, string[] parms)
        {
            City city;
            Structure structure;

            if (!(obj is Structure))
                throw new Exception();
            cityId = obj.City.Id;
            objectId = obj.ObjectId;
            ts = TimeSpan.FromSeconds(int.Parse(parms[0]));
            NlsDescription = parms[1];

            if (!Global.World.TryGetObjects(cityId, objectId, out city, out structure))
                return;

            city.Worker.DoPassive(structure, this, true);
        }

        #endregion

        public override void Callback(object custom)
        {
            City city;
            Structure structure;

            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;

                if (structure == null)
                {
                    StateChange(ActionState.Completed);
                    return;
                }

                if (structure.State.Type == ObjectState.Battle)
                {
                    endTime = DateTime.UtcNow.AddMinutes(5);
                    StateChange(ActionState.Rescheduled);
                    return;
                }

                city.BeginUpdate();
                structure.BeginUpdate();

                Global.World.Remove(structure);
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

        public override Error Execute()
        {
            City city;
            Structure structure;

            endTime = SystemClock.Now.AddSeconds(ts.TotalSeconds);
            BeginTime = SystemClock.Now;

            if (!Global.World.TryGetObjects(cityId, objectId, out city, out structure))
                return Error.ObjectNotFound;

            return Error.Ok;
        }

        public override void UserCancelled()
        {            
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            City city;
            Structure structure;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;                

                StateChange(ActionState.Failed);
            }
        }
    }
}