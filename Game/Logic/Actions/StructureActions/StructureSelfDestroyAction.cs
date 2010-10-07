using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Util;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Actions {
    public class StructureSelfDestroyAction: ScheduledPassiveAction,IScriptable {
        uint cityId;
        uint objectId;
        TimeSpan ts;

        public StructureSelfDestroyAction() {
        }

        public StructureSelfDestroyAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                          Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
        }

        #region IScriptable Members

        public void ScriptInit(GameObject obj, string[] parms) {
            City city;
            Structure structure;

            if (!(obj is Structure))
                throw new Exception();
            cityId = obj.City.Id;
            objectId = obj.ObjectId;
            ts = TimeSpan.FromSeconds(int.Parse(parms[0]));
                
            if (!Global.World.TryGetObjects(cityId, objectId, out city, out structure))
                return;
            city.Worker.DoPassive(structure,this,true);
        }

        #endregion
        
        public override void Callback(object custom) {
            City city;
            Structure structure;

            // Block structure
            using (new MultiObjectLock(cityId, objectId, out city, out structure)) {
                if (!IsValid())
                    return;

                //city.Worker.References.Remove(structure, this);

                if (structure == null) {
                    StateChange(ActionState.COMPLETED);
                    return;
                }

                if (structure.State.Type == ObjectState.BATTLE) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                city.BeginUpdate();
                structure.BeginUpdate();

                Global.World.Remove(structure);
                city.ScheduleRemove(structure, false);

                structure.EndUpdate();
                city.EndUpdate();

                StateChange(ActionState.COMPLETED);
            }
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            City city;
            Structure structure;

            endTime = SystemClock.Now.AddSeconds(ts.TotalSeconds);
            beginTime = SystemClock.Now;
            
            if (!Global.World.TryGetObjects(cityId, objectId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            return Error.OK;
        }

        public override void UserCancelled() {
            throw new Exception("This action cannot be cancelled!");
        }

        public override void WorkerRemoved(bool wasKilled) {
            //throw new Exception("City was destroyed?");
        }

        public override ActionType Type {
            get { return ActionType.STRUCTURE_SELF_DESTROY; }
        }

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                      new XMLKVPair("city_id", cityId),
                                                      new XMLKVPair("object_id", objectId),
                                                  });
            }
        }
    }
}
