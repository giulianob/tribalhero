#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class DefenseAction : ChainAction {
        private uint cityId;
        private uint targetCityId;
        private byte stubId;

        public DefenseAction(uint cityId, byte stubId, uint targetCityId) {
            this.cityId = cityId;
            this.stubId = stubId;
            this.targetCityId = targetCityId;
        }

        public DefenseAction(ushort id, string chainCallback, PassiveAction current, ActionState chainState,
                             bool isVisible, Dictionary<string, string> properties)
            : base(id, chainCallback, current, chainState, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["troop_stub_id"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
        }

        public override Error execute() {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                return Error.OBJECT_NOT_FOUND;

            City targetCity;
            if (!Global.World.TryGetObjects(targetCityId, out targetCity))
                return Error.OBJECT_NOT_FOUND;

            if (city.Battle != null)
                return Error.CITY_IN_BATTLE;

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats();
            stub.EndUpdate();

            city.Worker.References.add(stub.TroopObject, this);
            city.Worker.Notifications.add(stub.TroopObject, this, targetCity);

            TroopMoveAction tma = new TroopMoveAction(cityId, stub.TroopObject.ObjectId, targetCity.MainBuilding.X,
                                                      targetCity.MainBuilding.Y);

            ExecuteChainAndWait(tma, new ChainCallback(AfterTroopMoved));

            return Error.OK;
        }

        private void AfterTroopMoved(ActionState state) {
            if (state == ActionState.COMPLETED) {
                Dictionary<uint, City> cities;

                using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                    City city = cities[cityId];
                    City targetCity = cities[targetCityId];

                    TroopStub stub;
                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();

                    city.Worker.References.remove(stub.TroopObject, this);
                    city.Worker.Notifications.remove(this);

                    Procedure.TroopObjectStation(stub.TroopObject, targetCity);

                    if (targetCity.Battle != null) {
                        List<TroopStub> list = new List<TroopStub>();

                        stub.BeginUpdate();
                        stub.State = TroopStub.TroopState.BATTLE_STATIONED;
                        stub.EndUpdate();

                        list.Add(stub);

                        targetCity.Battle.addToDefense(list);
                    }
                    stateChange(ActionState.COMPLETED);
                }
            }
        }

        public override ActionType Type {
            get { return ActionType.DEFENSE; }
        }

        public override Error validate(string[] parms) {
            return Error.OK;
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new XMLKVPair[] {
                                                                new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("target_city_id", targetCityId),
                                                                new XMLKVPair("troop_stub_id", stubId)
                                                            });
            }
        }

        #endregion
    }
}