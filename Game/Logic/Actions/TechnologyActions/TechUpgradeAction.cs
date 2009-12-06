using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions {
    class TechnologyUpgradeAction : ScheduledActiveAction, IScriptable {
        uint cityId;
        uint structureId;        
        uint techId;
        
        bool isSelfInit = false;

        public TechnologyUpgradeAction() {
        }

        public TechnologyUpgradeAction(uint cityId, uint structureId, uint techId) {
            this.cityId = cityId;
            this.structureId = structureId;
            this.techId = techId;
        }

        public TechnologyUpgradeAction(ushort id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType, byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            techId = uint.Parse(properties["tech_id"]);
        }

        public override Error validate(string[] parms) {

            City city;
            Structure structure; 
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            Technology tech;
            if (!structure.Technologies.TryGetTechnology(techId, out tech))
                return Error.TECHNOLOGY_NOT_FOUND;

            if (tech == null) return Error.ACTION_INVALID;
            if (uint.Parse(parms[0]) != tech.Type) return Error.ACTION_INVALID;
            if (tech.Level >= byte.Parse(parms[1])) return Error.ACTION_INVALID;
            return Error.OK;
        }

        public override Error execute() {

            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.OBJECT_NOT_FOUND;

            Technology tech;
            if (!structure.Technologies.TryGetTechnology(techId, out tech))
                return Error.OBJECT_NOT_FOUND;

            TechnologyBase techBase = TechnologyFactory.getTechnologyBase(tech.Type, (byte)(tech.Level + 1));

            if (techBase == null)
                return Error.OBJECT_NOT_FOUND;            

            if (isSelfInit) {
                beginTime = DateTime.Now;
                endTime = DateTime.Now;
            }
            else {
                if (!city.Resource.HasEnough(techBase.resources)) {
                    return Error.RESOURCE_NOT_ENOUGH;
                }
                city.Resource.Subtract(techBase.resources);

                Global.dbManager.Save(city);

                beginTime = DateTime.Now;
                endTime = DateTime.Now.AddSeconds(techBase.time * Setup.Config.seconds_per_unit);
            }

            return Error.OK;
        }

        public override void interrupt(ActionInterrupt state) {
            City city;
            using (new MultiObjectLock(cityId, out city)) {
                Global.Scheduler.del(this);

                Technology tech;
                if (!city.Technologies.TryGetTechnology(techId, out tech)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                TechnologyBase techBase = TechnologyFactory.getTechnologyBase(tech.Type, (byte)(tech.Level + 1));

                if (techBase == null) {
                    stateChange(ActionState.FAILED);
                    return;
                }
                
                switch (state) {
                    case ActionInterrupt.KILLED:
                        Global.Scheduler.del(this);
                        stateChange(ActionState.FAILED);
                        break;
                    case ActionInterrupt.CANCEL:
                        Global.Scheduler.del(this);
                        city.Resource.Add(techBase.resources / 2);
                        Global.dbManager.Save(city);
                        stateChange(ActionState.INTERRUPTED);
                        break;
                }
            }
        }

        public override ActionType Type {
            get { return ActionType.TECHNOLOGY_UPGRADE; }
        }

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

                Technology tech;
                if (!structure.Technologies.TryGetTechnology(techId, out tech)) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                TechnologyBase techBase = TechnologyFactory.getTechnologyBase(tech.Type, (byte)(tech.Level + 1));

                if (techBase == null) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                if (!structure.Technologies.upgrade(new Technology(techBase))) {
                    stateChange(ActionState.FAILED);
                    return;
                }

                Global.dbManager.Save(structure.Technologies);

                stateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        #region ICanInit Members

        public void ScriptInit(GameObject obj, string[] parms) {
            if ((obj = obj as Structure) == null) throw new Exception();
            cityId = obj.City.CityId;
            structureId = obj.ObjectID;
            techId = uint.Parse(parms[0]);
            isSelfInit = true;
            this.execute();
        }

        #endregion

        #region IPersistable

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                    new XMLKVPair("tech_id", techId),
                    new XMLKVPair("city_id", cityId),
                    new XMLKVPair("structure_id", structureId)
                });
            }
        }

        #endregion
    }
}
