#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    public enum AttackMode {
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
        private int initialTroopValue;

        public AttackAction(uint cityId, byte stubId, uint targetCityId, uint targetStructureId, AttackMode mode) {
            this.cityId = cityId;
            this.targetCityId = targetCityId;
            this.targetStructureId = targetStructureId;
            this.stubId = stubId;
            this.mode = mode;
        }

        public AttackAction(uint id, string chainCallback, PassiveAction current, ActionState chainState,
                            bool isVisible, IDictionary<string, string> properties)
            : base(id, chainCallback, current, chainState, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["stub_id"]);
            mode = (AttackMode) uint.Parse(properties["mode"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
            targetStructureId = uint.Parse(properties["target_object_id"]);
            initialTroopValue = int.Parse(properties["initial_troop_value"]);
        }

        public override Error Execute() {
            City city;
            TroopStub stub;
            City targetCity;
            Structure targetStructure;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, targetStructureId, out targetCity, out targetStructure))
                return Error.OBJECT_NOT_FOUND;

            // Can't attack if target is under newbie protection
#if !DEBUG
            if (SystemClock.Now.Subtract(targetStructure.City.Owner.Created).TotalSeconds < Config.newbie_protection)
                return Error.PLAYER_NEWBIE_PROTECTION;
#endif

            // Can't attack "Unattackable" Objects
            if (ObjectTypeFactory.IsStructureType("Unattackable", targetStructure))
                return Error.OBJECT_NOT_ATTACKABLE;

            // Can't attack "Undestroyable" Objects if they're level 1
            if (targetStructure.Lvl <= 1 && ObjectTypeFactory.IsStructureType("Undestroyable", targetStructure))
                return Error.OBJECT_NOT_ATTACKABLE;          

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats(TroopBattleGroup.ATTACK);
            stub.EndUpdate();

            initialTroopValue = stub.Value;

            city.Worker.References.Add(stub.TroopObject, this);
            city.Worker.Notifications.Add(stub.TroopObject, this, targetCity);

            TroopMoveAction tma = new TroopMoveAction(cityId, stub.TroopObject.ObjectId, targetStructure.X,
                                                      targetStructure.Y);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.OK;
        }

        private void AfterTroopMoved(ActionState state) {
            if (state == ActionState.COMPLETED) {
                City city;
                City targetCity;

                if (!Global.World.TryGetObjects(cityId, out city)) {
                    throw new Exception("City is missing");
                }

                if (!Global.World.TryGetObjects(targetCityId, out targetCity)) {
                    //If the target is missing, walk back
                    using (new MultiObjectLock(city)) {
                        TroopStub stub = city.Troops[stubId];
                        TroopMoveAction tma = new TroopMoveAction(stub.City.Id, stub.TroopObject.ObjectId, city.MainBuilding.X, city.MainBuilding.Y);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                        return;
                    }
                }

                // Get all of the stationed city id's from the target city since they will be used by the engage attack action
                CallbackLock.CallbackLockHandler lockAllStationed = delegate {
                    List<ILockable> toBeLocked = new List<ILockable>();
                    foreach (TroopStub stationedStub in targetCity.Troops.StationedHere()) {
                        toBeLocked.Add(stationedStub.City);
                    }

                    return toBeLocked.ToArray();
                };
                                
                using (new CallbackLock(lockAllStationed, null, city, targetCity)) {
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

                    //Remove notification to target city once battle is over
                    city.Worker.Notifications.Remove(this);

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    // Check if troop is still alive
                    if (stub.TotalCount > 0) {
                        // Calculate how many attack points to give to the city
                        city.BeginUpdate();
                        Procedure.GiveAttackPoints(city, stub.TroopObject.Stats.AttackPoint, initialTroopValue, stub.Value);
                        city.EndUpdate();

                        // Send troop back home
                        TroopMoveAction tma = new TroopMoveAction(stub.City.Id, stub.TroopObject.ObjectId,
                                                                  city.MainBuilding.X, city.MainBuilding.Y);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    } else {
                        targetCity.BeginUpdate();
                        city.BeginUpdate();
                        
                        // Give back the loot to the target city
                        targetCity.Resource.Add(stub.TroopObject.Stats.Loot);

                        // Remove this actions reference from the troop
                        city.Worker.References.Remove(stub.TroopObject, this);
                        
                        // Remove troop since he's dead
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

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");                    

                    if (city.Battle == null) {
                        city.Worker.References.Remove(stub.TroopObject, this);
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

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    city.Worker.References.Remove(stub.TroopObject, this);

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
                                                new XMLKVPair("mode", (byte) mode),
                                                new XMLKVPair("initial_troop_value", initialTroopValue)
                    });
            }
        }

        #endregion
    }
}