using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Logic.Procedures;
using Game.Database;
using Game.Setup;
using Game.Util;

namespace Game.Logic.Actions {
    public class RetreatAction : ChainAction {
        uint cityId;
        byte stubId;

        public RetreatAction(uint cityId, byte stubId) {
            this.cityId = cityId;
            this.stubId = stubId;
        }

        public RetreatAction(ushort id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible, Dictionary<string, string> properties)
            : base(id, chainCallback, current, chainState, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["troop_stub_id"]);
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        public override Error execute() {

            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub)) {
                throw new Exception();                    
            }

            stub.City.Worker.References.add(stub.TroopObject, this);
            stub.City.Worker.Notifications.add(stub.TroopObject, this);

            TroopMoveAction tma = new TroopMoveAction(cityId, stub.TroopObject.ObjectID, stub.City.MainBuilding.X, stub.City.MainBuilding.Y);
            ExecuteChainAndWait(tma, new ChainCallback(this.AfterTroopMoved));

            return Error.OK;
        }

        private void AfterTroopMoved(ActionState state) {
            if (state == ActionState.COMPLETED) {
                City city;                
                using (new MultiObjectLock(cityId, out city)) {
                    TroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();
                                        
                    if (stub.City.Battle == null) {
                        stub.City.Worker.References.remove(stub.TroopObject, this);
                        stub.City.Worker.Notifications.remove(this);
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

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();

                    stub.City.Worker.References.remove(stub.TroopObject, this);
                    stub.City.Worker.Notifications.remove(this);
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
            get { return ActionType.RETREAT; }
        }

        public override string Properties {
            get {
                return XMLSerializer.Serialize(new XMLKVPair[] {
                        new XMLKVPair("city_id", cityId),
                        new XMLKVPair("troop_stub_id", stubId)
                    }
                );
            }
        }
    }
}
