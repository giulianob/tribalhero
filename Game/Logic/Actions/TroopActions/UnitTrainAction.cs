#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Fighting;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class UnitTrainAction : ScheduledActiveAction {
        private ushort type;
        private uint cityId;
        private uint structureId;
        private Resource cost;

        public UnitTrainAction(uint cityId, uint structureId, ushort type, ushort count) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.type = type;
            ActionCount = count;
        }

        public UnitTrainAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                               byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            type = ushort.Parse(properties["type"]);
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
        }

        #region IAction Members

        public override ActionType Type {
            get { return ActionType.UNIT_TRAIN; }
        }

        public override Error Execute() {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_STRUCTURE_NOT_FOUND;

            cost = Formula.UnitCost(structure.City, type, structure.City.Template[type].Lvl);
            Resource totalCost = cost*ActionCount;
            if (!structure.City.Resource.HasEnough(totalCost))
                return Error.RESOURCE_NOT_ENOUGH;

            structure.City.BeginUpdate();
            structure.City.Resource.Subtract(totalCost);
            structure.City.EndUpdate();

            int buildtime = Formula.TrainTime((int) UnitFactory.getTime(type, 1), (structure.Lvl - 1)*3,
                                              structure.Technologies);

            // add to queue for completion
            nextTime = DateTime.Now.AddSeconds((double) buildtime);
            beginTime = DateTime.Now;
            endTime = DateTime.Now.AddSeconds((double) buildtime*ActionCount);

            return Error.OK;
        }

        public override Error Validate(string[] parms) {
            if (ushort.Parse(parms[0]) != type)
                return Error.ACTION_INVALID;
            return Error.OK;
        }

        #endregion

        #region ISchedule Members

        public override void callback(object custom) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                structure.City.DefaultTroop.BeginUpdate();
                structure.City.DefaultTroop.addUnit(FormationType.Normal, type, 1);
                structure.City.DefaultTroop.EndUpdate();

                --ActionCount;
                if (ActionCount == 0) {
                    StateChange(ActionState.COMPLETED);
                    return;
                }

                int buildtime = Formula.TrainTime((int) UnitFactory.getTime(type, 1), (structure.Lvl - 1)*3,
                                                  structure.Technologies);
                nextTime = nextTime.AddSeconds(buildtime);
                StateChange(ActionState.RESCHEDULED);
            }
        }

        #endregion

        public override void Interrupt(ActionInterrupt state) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                Global.Scheduler.del(this);

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                switch (state) {
                    case ActionInterrupt.KILLED:
                        StateChange(ActionState.FAILED);
                        break;
                    case ActionInterrupt.CANCEL:
                        Resource totalCost = cost*ActionCount;
                        structure.City.BeginUpdate();
                        structure.City.Resource.Add(totalCost/2);
                        structure.City.EndUpdate();
                        StateChange(ActionState.INTERRUPTED);
                        break;
                }
            }
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new XMLKVPair[] {
                                                                new XMLKVPair("type", type), new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("structure_id", structureId)
                                                            });
            }
        }

        #endregion
    }
}