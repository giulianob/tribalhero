#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    public enum AttackMode
    {
        Weak = 0,
        Normal = 1,
        Strong = 2
    }

    class AttackChainAction : ChainAction
    {
        private readonly uint cityId;
        private readonly AttackMode mode;
        private readonly byte stubId;
        private readonly uint targetCityId;
        private readonly uint targetStructureId;
        private int initialTroopValue;

        public AttackChainAction(uint cityId, byte stubId, uint targetCityId, uint targetStructureId, AttackMode mode)
        {
            this.cityId = cityId;
            this.targetCityId = targetCityId;
            this.targetStructureId = targetStructureId;
            this.stubId = stubId;
            this.mode = mode;
        }

        public AttackChainAction(uint id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible, IDictionary<string, string> properties)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["stub_id"]);
            mode = (AttackMode)uint.Parse(properties["mode"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
            targetStructureId = uint.Parse(properties["target_object_id"]);
            initialTroopValue = int.Parse(properties["initial_troop_value"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.AttackChain;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                                                {
                                                        new XmlKvPair("city_id", cityId), new XmlKvPair("stub_id", stubId),
                                                        new XmlKvPair("target_city_id", targetCityId), new XmlKvPair("target_object_id", targetStructureId),
                                                        new XmlKvPair("mode", (byte)mode), new XmlKvPair("initial_troop_value", initialTroopValue)
                                                });
            }
        }

        public override Error Execute()
        {
            City city;
            TroopStub stub;
            City targetCity;
            Structure targetStructure;

            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub) ||
                !Global.World.TryGetObjects(targetCityId, targetStructureId, out targetCity, out targetStructure))
                return Error.ObjectNotFound;

            if (city.Troops.MyStubs().Count() >= 12)
                return Error.TooManyTroops;

            // Can't attack if target is under newbie protection
#if !DEBUG
            if (targetCity.AttackPoint == 0 && SystemClock.Now.Subtract(targetStructure.City.Owner.Created).TotalSeconds < Config.newbie_protection)
                return Error.PlayerNewbieProtection;
#endif

            // Can't attack "Unattackable" Objects
            if (ObjectTypeFactory.IsStructureType("Unattackable", targetStructure))
                return Error.ObjectNotAttackable;

            // Can't attack "Undestroyable" Objects if they're level 1
            if (targetStructure.Lvl <= 1 && ObjectTypeFactory.IsStructureType("Undestroyable", targetStructure))
                return Error.StructureUndestroyable;

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats(TroopBattleGroup.Attack);
            stub.EndUpdate();

            initialTroopValue = stub.Value;

            city.Worker.References.Add(stub.TroopObject, this);
            city.Worker.Notifications.Add(stub.TroopObject, this, targetCity);

            var tma = new TroopMovePassiveAction(cityId, stub.TroopObject.ObjectId, targetStructure.X, targetStructure.Y, false, true);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                City city;
                City targetCity;

                if (!Global.World.TryGetObjects(cityId, out city))
                    throw new Exception("City is missing");

                if (!Global.World.TryGetObjects(targetCityId, out targetCity))
                {
                    //If the target is missing, walk back
                    using (new MultiObjectLock(city))
                    {
                        TroopStub stub = city.Troops[stubId];
                        TroopMovePassiveAction tma = new TroopMovePassiveAction(stub.City.Id, stub.TroopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                        return;
                    }
                }

                // Get all of the stationed city id's from the target city since they will be used by the engage attack action
                CallbackLock.CallbackLockHandler lockAllStationed = delegate
                    {
                        var toBeLocked = new List<ILockable>();
                        foreach (var stationedStub in targetCity.Troops.StationedHere())
                            toBeLocked.Add(stationedStub.City);

                        return toBeLocked.ToArray();
                    };

                using (new CallbackLock(lockAllStationed, null, city, targetCity))
                {
                    var bea = new EngageAttackPassiveAction(cityId, stubId, targetCityId, mode);
                    ExecuteChainAndWait(bea, AfterBattle);
                }
            }
        }

        private void AfterBattle(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                Dictionary<uint, City> cities;
                using (new MultiObjectLock(out cities, cityId, targetCityId))
                {
                    City city = cities[cityId];
                    TroopStub stub;
                    City targetCity = cities[targetCityId];

                    //Remove notification to target city once battle is over
                    city.Worker.Notifications.Remove(this);

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    // Check if troop is still alive
                    if (stub.TotalCount > 0)
                    {
                        // Calculate how many attack points to give to the city
                        city.BeginUpdate();
                        Procedure.GiveAttackPoints(city, stub.TroopObject.Stats.AttackPoint, initialTroopValue, stub.Value);
                        city.EndUpdate();

                        // Send troop back home
                        var tma = new TroopMovePassiveAction(stub.City.Id, stub.TroopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);

                        // Add notification just to the main city
                        stub.City.Worker.Notifications.Add(stub.TroopObject, this);
                    }
                    else
                    {
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

                        StateChange(ActionState.Completed);
                    }
                }
            }
        }

        private void AfterTroopMovedHome(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                City city;
                using (new MultiObjectLock(cityId, out city))
                {
                    TroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    // Remove notification
                    stub.City.Worker.Notifications.Remove(this);

                    // If city is not in battle then add back to city otherwise join local battle
                    if (city.Battle == null)
                    {
                        city.Worker.References.Remove(stub.TroopObject, this);
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                        StateChange(ActionState.Completed);
                    }
                    else
                    {
                        var eda = new EngageDefensePassiveAction(cityId, stubId);
                        ExecuteChainAndWait(eda, AfterEngageDefense);
                    }
                }
            }
        }

        private void AfterEngageDefense(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                City city;
                using (new MultiObjectLock(cityId, out city))
                {
                    TroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    city.Worker.References.Remove(stub.TroopObject, this);

                    if (stub.TotalCount == 0)
                        Procedure.TroopObjectDelete(stub.TroopObject, false);
                    else
                        Procedure.TroopObjectDelete(stub.TroopObject, true);

                    StateChange(ActionState.Completed);
                }
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }
    }
}