#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class TechnologyUpgradeAction : ScheduledActiveAction, IScriptable {
        private uint cityId;
        private uint structureId;
        private uint techId;

        private bool isSelfInit;

        public TechnologyUpgradeAction() { }

        public TechnologyUpgradeAction(uint cityId, uint structureId, uint techId) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.techId = techId;
        }

        public TechnologyUpgradeAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime,
                                       int workerType, byte workerIndex, ushort actionCount,
                                       Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            techId = uint.Parse(properties["tech_id"]);
        }

        public override Error Validate(string[] parms) {
            byte maxLevel = byte.Parse(parms[1]);

            if (uint.Parse(parms[0]) != techId)
                return Error.ACTION_INVALID;

            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            Technology tech;
            if (!structure.Technologies.TryGetTechnology(techId, out tech))        
                return Error.OK;                       

            if (tech.Level >= maxLevel)
                return Error.TECHNOLOGY_MAX_LEVEL_REACHED;

            return Error.OK;
        }

        public override Error Execute() {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            Technology tech;
            TechnologyBase techBase;
            if (structure.Technologies.TryGetTechnology(techId, out tech)) {
                techBase = TechnologyFactory.GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));
            }            
            else {
                techBase = TechnologyFactory.GetTechnologyBase(techId, 1);
            }

            if (techBase == null)
                return Error.OBJECT_NOT_FOUND;

            if (isSelfInit) {
                beginTime = DateTime.UtcNow;
                endTime = DateTime.UtcNow;
            }
            else {
                if (!city.Resource.HasEnough(techBase.resources))
                    return Error.RESOURCE_NOT_ENOUGH;

                city.BeginUpdate();
                city.Resource.Subtract(techBase.resources);
                city.EndUpdate();

                beginTime = DateTime.UtcNow;
                endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : techBase.time * Config.seconds_per_unit);
            }

            return Error.OK;
        }

        private void InterruptCatchAll(bool wasKilled) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                if (!IsValid())
                    return;

                Technology tech;
                TechnologyBase techBase;
                if (city.Technologies.TryGetTechnology(techId, out tech))
                    techBase = TechnologyFactory.GetTechnologyBase(tech.Type, (byte) (tech.Level + 1));
                else
                    techBase = TechnologyFactory.GetTechnologyBase(techId, 1);

                if (techBase == null) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                if (!wasKilled) {
                    city.BeginUpdate();
                    city.Resource.Add(techBase.resources/2);
                    city.EndUpdate();
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

        public override ActionType Type {
            get { return ActionType.TECHNOLOGY_UPGRADE; }
        }

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

                Technology tech;
                if (structure.Technologies.TryGetTechnology(techId, out tech)) {
                    TechnologyBase techBase = TechnologyFactory.GetTechnologyBase(tech.Type, (byte) (tech.Level + 1));

                    if (techBase == null) {
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    structure.Technologies.BeginUpdate();
                    if (!structure.Technologies.Upgrade(new Technology(techBase))) {
                        structure.EndUpdate();
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    structure.Technologies.EndUpdate();
                }
                else {
                    TechnologyBase techBase = TechnologyFactory.GetTechnologyBase(techId, 1);

                    if (techBase == null) {
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    structure.Technologies.BeginUpdate();
                    if (!structure.Technologies.Add(new Technology(techBase))) {
                        structure.EndUpdate();
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    structure.Technologies.EndUpdate();                    
                }

                StateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        #region ICanInit Members

        public void ScriptInit(GameObject obj, string[] parms) {
            throw new Exception("have to add logic so that upgrading after a building has been knocked down won't upgrade this tech again");

            if ((obj = obj as Structure) == null)
                throw new Exception();
            cityId = obj.City.Id;
            structureId = obj.ObjectId;
            techId = uint.Parse(parms[0]);
            isSelfInit = true;
            Execute();
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("tech_id", techId), new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("structure_id", structureId)
                                                            });
            }
        }

        #endregion
    }
}