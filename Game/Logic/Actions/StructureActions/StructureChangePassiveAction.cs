#region

using System;
using System.Collections.Generic;
using System.Threading;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class StructureChangePassiveAction : ScheduledPassiveAction, IScriptable
    {
        private uint cityId;
        private uint objectId;
        private TimeSpan ts;
        private ushort type;
        private byte lvl;
        
        public StructureChangePassiveAction()
        {
        }

        public StructureChangePassiveAction(uint cityId, uint objectId, int seconds, ushort newType, byte newLvl)
        {
            this.cityId = cityId;
            this.objectId = objectId;
            ts = TimeSpan.FromSeconds(seconds);
            type = newType;
            lvl = newLvl;
        }

        public StructureChangePassiveAction(uint id,
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
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId), new XmlKvPair("object_id", objectId), new XmlKvPair("type", type), new XmlKvPair("lvl", lvl)});
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

            ts = Formula.ReadCsvTimeFormat(parms[0]);
            type = ushort.Parse(parms[1]);
            lvl = byte.Parse(parms[2]);            

            if (!Global.World.TryGetObjects(cityId, objectId, out city, out structure))
                return;

            city.Worker.DoPassive(structure, this, true);
        }

        #endregion

        public override void Callback(object custom)
        {
            City city;
            Structure structure;

            // Block structure
            using (Concurrency.Current.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;

                if (structure.IsBlocked)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                structure.BeginUpdate();
                structure.IsBlocked = true;
                structure.EndUpdate();
            }

            structure.City.Worker.Remove(structure, new GameAction[] { this });
            
            using (Concurrency.Current.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;

                if (structure == null)
                {
                    Global.Logger.Warn("StructureChange did not find structure");
                    StateChange(ActionState.Completed);
                    return;
                }

                structure.City.BeginUpdate();
                structure.BeginUpdate();
                Procedures.Procedure.StructureChange(structure, type, lvl);
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
            City city;
            Structure structure;

            endTime = SystemClock.Now.AddSeconds(CalculateTime(ts.TotalSeconds, false));
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
            using (Concurrency.Current.Lock(cityId, objectId, out city, out structure))
            {
                if (!IsValid())
                    return;                

                StateChange(ActionState.Failed);
            }
        }
    }
}