using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions {
    class ObjectRemoveAction : ScheduledPassiveAction {

        uint cityId;
        uint objectId;
        bool wasKilled;

        public ObjectRemoveAction(uint cityId, uint objectId, bool wasKilled) {
            this.cityId = cityId;
            this.objectId = objectId;
            this.wasKilled = wasKilled;
        }

        public ObjectRemoveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                          Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            objectId = uint.Parse(properties["object_id"]);
            wasKilled = bool.Parse(properties["was_killed"]);
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        public override Error Execute() {
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow;

            return Error.OK;
        }

        public override void Callback(object custom) {
            City city;
            GameObject obj;
            
            using (new MultiObjectLock(cityId, out city)) {
                if (city == null)
                    throw new Exception("City is missing");

                if (!city.TryGetObject(objectId, out obj)) {
                    throw new Exception("Obj is missing");
                }   
            }

            // Cancel all active actions
            while (true) {
                GameAction action;

                using (new MultiObjectLock(cityId, out city)) {
                    if (city == null)
                        throw new Exception("City is missing");

                    GameObject obj1 = obj;
                    action = city.Worker.ActiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj1);

                    if (action == null) break;
                }

                action.WorkerRemoved(wasKilled);
            }

            // Cancel all passive actions
            while (true) {
                GameAction action;

                using (new MultiObjectLock(cityId, out city)) {
                    if (city == null)
                        throw new Exception("City is missing");

                    GameObject obj1 = obj;
                    action = city.Worker.PassiveActions.Values.FirstOrDefault(x => x.WorkerObject == obj1);

                    if (action == null) break;
                }

                action.WorkerRemoved(wasKilled);
            }

            using (new MultiObjectLock(cityId, out city)) {
                if (city == null)
                    throw new Exception("City is missing");                    
                
                if (!city.TryGetObject(objectId, out obj))
                    throw new Exception("Obj is missing");

                if (city.Worker.GetActions(obj).Count() != 0)
                    throw new Exception("Not all actions were cancelled for this obj");

                // Finish cleaning object
                if (obj is TroopObject)
                    city.DoRemove(obj as TroopObject);
                else if (obj is Structure)
                    city.DoRemove(obj as Structure);

                StateChange(ActionState.COMPLETED);
            }
        }

        public override void UserCancelled() {            
        }

        public override void WorkerRemoved(bool wasKilled) {
            throw new Exception("City was destroyed?");
        }

        public override ActionType Type {
            get { return ActionType.OBJECT_REMOVE; }
        }

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                      new XMLKVPair("city_id", cityId),
                                                      new XMLKVPair("object_id", objectId),
                                                      new XMLKVPair("was_killed", wasKilled),
                                                  });
            }
        }
    }
}
