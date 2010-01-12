#region

using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    enum AttackMode {
        WEAK = 0,
        NORMAL = 1,
        STRONG = 2
    }

    class AttackAction : ChainAction {
        private readonly uint cityId;
        private readonly byte stubId;
        private readonly uint targetCityId;
        private readonly uint targetStructureId;
        private readonly AttackMode mode;

        public AttackAction(uint cityId, byte stubId, uint targetCityId, uint targetStructureId, AttackMode mode) {
            this.cityId = cityId;
            this.targetCityId = targetCityId;
            this.targetStructureId = targetStructureId;
            this.stubId = stubId;
            this.mode = mode;
        }

        public AttackAction(ushort id, string chainCallback, PassiveAction current, ActionState chainState,
                            bool isVisible, IDictionary<string, string> properties)
            : base(id, chainCallback, current, chainState, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["stub_id"]);
            mode = (AttackMode) uint.Parse(properties["mode"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
            targetStructureId = uint.Parse(properties["target_object_id"]);
        }

        public override Error Execute() {
            City city;
            TroopStub stub;
            City targetCity;
            Structure targetStructure;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, targetStructureId, out targetCity, out targetStructure))
                return Error.OBJECT_NOT_FOUND;

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats();
            stub.EndUpdate();

            city.Worker.References.add(stub.TroopObject, this);
            city.Worker.Notifications.add(stub.TroopObject, this, targetCity);

            TroopMoveAction tma = new TroopMoveAction(cityId, stub.TroopObject.ObjectId, targetStructure.X,
                                                      targetStructure.Y);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.OK;
        }

        private void AfterTroopMoved(ActionState state) {
            if (state == ActionState.COMPLETED) {                        
                List<ILockable> toBeLocked = new List<ILockable>();
                Dictionary<uint, City> cities;
                //This is a 2 step process because we need to find all the cities that need to be locked first

                //1. Get all of the stationed city id's from the target city since they will be used by the engage attack action
                using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                    City city = cities[cityId];
                    City targetCity = cities[targetCityId];

                    TroopStub stub = city.Troops[stubId];

                    //If the target is missing, walk back
                    if (targetCity == null) {
                        TroopMoveAction tma = new TroopMoveAction(stub.City.CityId, stub.TroopObject.ObjectId,
                                                                  city.MainBuilding.X, city.MainBuilding.Y);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                        return;
                    }

                    foreach (TroopStub stationedStub in targetCity.Troops.StationedHere()) {
                        toBeLocked.Add(stationedStub.City);
                    }

                    toBeLocked.Add(city);
                    toBeLocked.Add(targetCity);
                }

                //2. Lock them all
                using (new MultiObjectLock(toBeLocked.ToArray())) {
                    EngageAttackAction bea = new EngageAttackAction(cityId, stubId, targetCityId, mode);
                    ExecuteChainAndWait(bea, AfterBattle);
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
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    //Remove notification to target city once battle is over
                    city.Worker.Notifications.remove(this);

                    if (stub.TotalCount > 0) {
                        TroopMoveAction tma = new TroopMoveAction(stub.City.CityId, stub.TroopObject.ObjectId,
                                                                  city.MainBuilding.X, city.MainBuilding.Y);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    } else {
                        targetCity.BeginUpdate();
                        city.BeginUpdate();

                        targetCity.Resource.Add(stub.TroopObject.Stats.Loot);

                        city.Worker.References.remove(stub.TroopObject, this);

                        Procedure.TroopObjectDelete(stub.TroopObject, false);

                        targetCity.EndUpdate();
                        city.EndUpdate();

                        StateChange(ActionState.COMPLETED);
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
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    if (city.Battle == null) {
                        city.Worker.References.remove(stub.TroopObject, this);
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                        StateChange(ActionState.COMPLETED);
                    } else {
                        EngageDefenseAction eda = new EngageDefenseAction(cityId, stubId);
                        ExecuteChainAndWait(eda, AfterEngageDefense);
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
                        StateChange(ActionState.FAILED);
                        return;
                    }

                    city.Worker.References.remove(stub.TroopObject, this);
                    if (stub.TotalCount == 0)
                        Procedure.TroopObjectDelete(stub.TroopObject, false);
                    else
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                    StateChange(ActionState.COMPLETED);
                }
            }
        }

        public override ActionType Type {
            get { return ActionType.ATTACK; }
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        #region IPersistable Members

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("city_id", cityId), new XMLKVPair("stub_id", stubId),
                                                                new XMLKVPair("target_city_id", targetCityId),
                                                                new XMLKVPair("target_object_id", targetStructureId),
                                                                new XMLKVPair("mode", (byte) mode)
                                                            });
            }
        }

        #endregion
    }
}