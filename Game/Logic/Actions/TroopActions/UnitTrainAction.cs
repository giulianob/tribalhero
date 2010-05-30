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

        public UnitTrainAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
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

            int buildtime = Formula.TrainTime(UnitFactory.GetTime(type, 1), structure.Lvl, structure.Technologies);

            // add to queue for completion
            nextTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : buildtime);
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 * ActionCount : (double) buildtime * ActionCount);

            return Error.OK;
        }

        public override Error Validate(string[] parms) {
            if (ushort.Parse(parms[0]) != type)
                return Error.ACTION_INVALID;

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

                structure.City.DefaultTroop.BeginUpdate();
                structure.City.DefaultTroop.AddUnit(FormationType.NORMAL, type, 1);
                structure.City.DefaultTroop.EndUpdate();

                --ActionCount;
                if (ActionCount == 0) {
                    StateChange(ActionState.COMPLETED);
                    return;
                }

                int buildtime = Formula.TrainTime(UnitFactory.GetTime(type, 1), structure.Lvl, structure.Technologies);
                nextTime = nextTime.AddSeconds(Config.actions_instant_time ? 3 : buildtime);
                StateChange(ActionState.RESCHEDULED);
            }
        }

        #endregion

        private void InterruptCatchAll(bool wasKilled) {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                if (!wasKilled) {
                    Resource totalCost = cost*ActionCount;
                    structure.City.BeginUpdate();
                    structure.City.Resource.Add(totalCost/2);
                    structure.City.EndUpdate();
                }

                StateChange(ActionState.FAILED);
            }
        }

        public override void UserCancelled() {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled) {
            InterruptCatchAll(wasKilled);
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("type", type), new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("structure_id", structureId)
                                                            });
            }
        }

        #endregion
    }
}