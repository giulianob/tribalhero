using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Logic.Procedures;
using Game.Util;
using Game.Database;

namespace Game.Logic.Actions {
    enum AttackMode {
        WEAK = 0,
        NORMAL = 1,
        STRONG = 2
    }

    class AttackAction : ChainAction {
        uint cityId;
        byte stubId;        
        uint targetCityId;
        uint targetStructureId;        
        AttackMode mode;

        public AttackAction(uint cityId, byte stubId, uint targetCityId, uint targetStructureId, AttackMode mode) {
            this.cityId = cityId;
            this.targetCityId = targetCityId;
            this.targetStructureId = targetStructureId;
            this.stubId = stubId;
            this.mode = mode;
        }

        public AttackAction(ushort id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible, Dictionary<string, string> properties)
            : base(id, chainCallback, current, chainState, isVisible) {
            cityId = uint.Parse(properties["city_id"]);            
            stubId = byte.Parse(properties["stub_id"]);
            mode = (AttackMode)uint.Parse(properties["mode"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
            targetStructureId = uint.Parse(properties["target_object_id"]);
        }

        public override Error execute() {
            City city;
            TroopStub stub;
            City targetCity;
            Structure targetStructure;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, targetStructureId, out targetCity, out targetStructure)) {
                return Error.OBJECT_NOT_FOUND;
            }

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats();
            stub.EndUpdate();

            city.Worker.References.add(stub.TroopObject, this);
            city.Worker.Notifications.add(stub.TroopObject, this, targetCity);

            TroopMoveAction tma = new TroopMoveAction(cityId, stub.TroopObject.ObjectID, targetStructure.X, targetStructure.Y);

            ExecuteChainAndWait(tma, new ChainCallback(this.AfterTroopMoved));

            return Error.OK;
        }

        private void AfterTroopMoved(ActionState state) {
            if (state == ActionState.COMPLETED) {
                Dictionary<uint, City> cities;
                using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                    EngageAttackAction bea = new EngageAttackAction(cityId, stubId, targetCityId, mode);
                    ExecuteChainAndWait(bea, new ChainCallback(this.AfterBattle));
                }
            }
        }

        private void AfterBattle(ActionState state) {
            if (state == ActionState.COMPLETED) {
                Dictionary<uint, City> cities;
                using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                    City city = cities[cityId];
                    TroopStub stub;
                    City targetCity = cities[targetCityId];

                    if (!city.Troops.TryGetStub(stubId, out stub)) {
                        stateChange(ActionState.FAILED);
                        return;
                    }

                    //Remove notification to target city once battle is over
                    city.Worker.Notifications.remove(this);

                    if (stub.TotalCount > 0) {
                        TroopMoveAction tma = new TroopMoveAction(stub.City.CityId, stub.TroopObject.ObjectID, city.MainBuilding.X, city.MainBuilding.Y);
                        ExecuteChainAndWait(tma, new ChainCallback(this.AfterTroopMovedHome));
                    }
                    else {
                        targetCity.BeginUpdate();
                        city.BeginUpdate();

                        targetCity.Resource.Add(stub.TroopObject.Stats.Loot);

                        city.Worker.References.remove(stub.TroopObject, this);                        

                        Procedure.TroopObjectDelete(stub.TroopObject, false);

                        targetCity.EndUpdate();
                        city.EndUpdate();

                        stateChange(ActionState.COMPLETED);
                    }
                }
            }
        }

        private void AfterTroopMovedHome(ActionState state) {
            if (state == ActionState.COMPLETED) {
                City city;
                using (new MultiObjectLock(cityId, out city)) {                    
                    TroopStub stub;                    

                    if (!city.Troops.TryGetStub(stubId, out stub)) {
                        stateChange(ActionState.FAILED);
                        return;
                    }

                    if (city.Battle == null) {
                        city.Worker.References.remove(stub.TroopObject, this);
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                        stateChange(ActionState.COMPLETED);
                    }
                    else {
                        EngageDefenseAction eda = new EngageDefenseAction(cityId, stubId);
                        ExecuteChainAndWait(eda, new ChainCallback(this.AfterEngageDefense));
                    }
                }
            }
        }

        private void AfterEngageDefense(ActionState state) {
            if (state == ActionState.COMPLETED) {
                City city;
                using (new MultiObjectLock(cityId, out city)) {
                    TroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub)) {
                        stateChange(ActionState.FAILED);
                        return;
                    }

                    city.Worker.References.remove(stub.TroopObject, this);
                    if (stub.TotalCount == 0) {
                        Procedure.TroopObjectDelete(stub.TroopObject, false);
                    }
                    else {
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                    }
                    stateChange(ActionState.COMPLETED);
                }
            }
        }
        
        public override ActionType Type {
            get { return ActionType.ATTACK; }
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                        new XMLKVPair("city_id", cityId),
                        new XMLKVPair("stub_id", stubId),
                        new XMLKVPair("target_city_id", targetCityId),
                        new XMLKVPair("target_object_id", targetStructureId),
                        new XMLKVPair("mode", (byte)mode)
                    }
                );
            }
        }

        #endregion
    }
}
