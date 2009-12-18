using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Module;
using Game.Util;
using Game.Logic.Procedures;

namespace Game.Logic.Actions {
    class LaborMoveAction : ScheduledActiveAction {
        bool city2structure;
        uint cityId;
        uint structureId;

        public LaborMoveAction(uint cityId, uint structureId, bool city2structure, ushort count) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.city2structure = city2structure;
            this.actionCount = count;
        }

        public LaborMoveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType, byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            city2structure = bool.Parse(properties["city2structure"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        #region IAction Members

        public override ActionType Type {
            get { return ActionType.LABOR_MOVE; }
        }

        public override Error execute() {

            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            if (city2structure) {
                structure.City.BeginUpdate();
                structure.City.Resource.Labor.Subtract(actionCount);
                structure.City.EndUpdate();
            }
            else {
                structure.BeginUpdate();
                structure.Stats.Labor -= (byte)actionCount;
                structure.EndUpdate();

                structure.City.BeginUpdate();
                Procedure.OnLaborUpdate(structure, -1 * actionCount);  // labor got taken out immediately                
                structure.City.EndUpdate();
            }

            // add to queue for completion
            beginTime = DateTime.Now;
            endTime = DateTime.Now.AddSeconds(Formula.LaborMoveTime(structure, (byte)actionCount, structure.Technologies));

            return Error.OK;
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        #region ISchedule Members
        public override void callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!isValid()) return;

                if (!city.tryGetStructure(structureId, out structure)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                if (city2structure) {
                    structure.BeginUpdate();
                    structure.Stats.Labor += (byte)actionCount;
                    structure.EndUpdate();

                    structure.City.BeginUpdate();
                    Procedure.OnLaborUpdate(structure, actionCount);  // labor got put in here
                    structure.City.EndUpdate();
                } else {
                    structure.City.BeginUpdate();
                    structure.City.Resource.Labor.Add(actionCount);
                    structure.City.EndUpdate();
                }
                stateChange(ActionState.COMPLETED);
                return;
            }
        }
        #endregion

        public override void interrupt(ActionInterrupt state) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!city.tryGetStructure(structureId, out structure)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                Global.Scheduler.del(this);
                switch (state) {
                    case ActionInterrupt.KILLED:
                        stateChange(ActionState.FAILED);
                        break;
                    case ActionInterrupt.CANCEL:
                        if (city2structure) {
                            structure.City.BeginUpdate();
                            structure.City.Resource.Labor.Add(actionCount);
                            structure.City.EndUpdate();
                        } else {
                            structure.BeginUpdate();
                            structure.Stats.Labor += (byte)actionCount;
                            structure.EndUpdate();

                            structure.City.BeginUpdate();
                            Procedure.OnLaborUpdate(structure, -1 * actionCount);  // need to add production back to structures
                            structure.City.EndUpdate();
                        }
                        stateChange(ActionState.INTERRUPTED);
                        break;
                }
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                    new XMLKVPair("city2structure", city2structure),
                    new XMLKVPair("city_id", cityId),
                    new XMLKVPair("structure_id", structureId)
                });
            }
        }

        #endregion
    }
}
