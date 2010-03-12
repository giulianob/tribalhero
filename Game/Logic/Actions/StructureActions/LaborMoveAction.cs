#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class LaborMoveAction : ScheduledActiveAction {
        private readonly bool cityToStructure;
        private readonly uint cityId;
        private readonly uint structureId;

        public LaborMoveAction(uint cityId, uint structureId, bool cityToStructure, ushort count) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.cityToStructure = cityToStructure;
            ActionCount = count;
        }

        public LaborMoveAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                               byte workerIndex, ushort actionCount, IDictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityToStructure = bool.Parse(properties["city_to_structure"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        #region IAction Members

        public override ActionType Type {
            get { return ActionType.LABOR_MOVE; }
        }

        public override Error Execute() {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            if (cityToStructure) {
                structure.City.BeginUpdate();
                structure.City.Resource.Labor.Subtract(ActionCount);
                structure.City.EndUpdate();
            } else {
                structure.BeginUpdate();
                structure.Stats.Labor -= (byte) ActionCount;
                structure.EndUpdate();

                structure.City.BeginUpdate();
                Procedure.AdjustCityResourceRates(structure, -1 * ActionCount); // labor got taken out immediately                
                structure.City.EndUpdate();
            }

            // add to queue for completion
            beginTime = DateTime.Now;
            endTime = DateTime.Now.AddSeconds(Formula.LaborMoveTime(structure, (byte) ActionCount, structure.Technologies));

            return Error.OK;
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        #region ISchedule Members

        public override void Callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                if (cityToStructure) {
                    structure.BeginUpdate();
                    structure.Stats.Labor += (byte) ActionCount;
                    structure.EndUpdate();

                    structure.City.BeginUpdate();
                    Procedure.AdjustCityResourceRates(structure, ActionCount); // labor got put in here
                    structure.City.EndUpdate();
                } else {
                    structure.City.BeginUpdate();
                    structure.City.Resource.Labor.Add(ActionCount);
                    structure.City.EndUpdate();
                }
                StateChange(ActionState.COMPLETED);
                return;
            }
        }

        #endregion

        public override void Interrupt(ActionInterrupt state) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                Global.Scheduler.Del(this);
                switch (state) {
                    case ActionInterrupt.KILLED:
                        StateChange(ActionState.FAILED);
                        break;
                    case ActionInterrupt.CANCEL:
                        if (cityToStructure) {
                            structure.City.BeginUpdate();
                            structure.City.Resource.Labor.Add(ActionCount);
                            structure.City.EndUpdate();
                        } else {
                            structure.BeginUpdate();
                            structure.Stats.Labor += (byte) ActionCount;
                            structure.EndUpdate();
                            
                            structure.City.BeginUpdate();
                            Procedure.AdjustCityResourceRates(structure, 1 * ActionCount);
                            structure.City.EndUpdate();
                        }
                        StateChange(ActionState.INTERRUPTED);
                        break;
                }
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                      new XMLKVPair("city_to_structure", cityToStructure),
                                                      new XMLKVPair("city_id", cityId),
                                                      new XMLKVPair("structure_id", structureId)
                    });
            }
        }

        #endregion
    }
}