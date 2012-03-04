#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public enum AttackMode
    {
        Weak = 0,
        Normal = 1,
        Strong = 2
    }

    public class AttackChainAction : ChainAction
    {
        private readonly uint cityId;
        private readonly AttackMode mode;
        private readonly byte stubId;
        private readonly uint targetCityId;
        private readonly uint targetStructureId;
        private int initialTroopValue;

        public uint From
        {
            get
            {
                return cityId;
            }
        }

        public uint To
        {
            get
            {
                return targetCityId;
            }
        }

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
            ICity city;
            ITroopStub stub;
            ICity targetCity;
            IStructure targetStructure;

            if (!World.Current.TryGetObjects(cityId, stubId, out city, out stub) ||
                !World.Current.TryGetObjects(targetCityId, targetStructureId, out targetCity, out targetStructure))
                return Error.ObjectNotFound;

            int currentAttacks = city.Worker.PassiveActions.Values.Count(action => action is AttackChainAction);
            if (currentAttacks > 20)
            {
                return Error.TooManyTroops;
            }

            // Can't attack if target is under newbie protection
#if !DEBUG
            if (Formula.Current.IsNewbieProtected(targetCity.Owner))
                return Error.PlayerNewbieProtection;
#endif

            // Can't attack cities that are being deleted
            if (targetCity.Deleted != City.DeletedState.NotDeleted)
                return Error.ObjectNotAttackable;

            // Can't attack "Unattackable" Objects
            if (Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Unattackable", targetStructure))
                return Error.ObjectNotAttackable;

            // Can't attack "Undestroyable" Objects if they're level 1
            if (targetStructure.Lvl <= 1 && Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Undestroyable", targetStructure))
                return Error.StructureUndestroyable;

            // Can't attack tribes mate
            if (city.Owner.Tribesman != null && targetCity.Owner.Tribesman != null && city.Owner.Tribesman.Tribe == targetCity.Owner.Tribesman.Tribe)
                return Error.AssignmentCantAttackFriend;

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats(TroopBattleGroup.Attack);
            stub.EndUpdate();

            initialTroopValue = stub.Value;

            city.Worker.References.Add(stub.TroopObject, this);
            city.Worker.Notifications.Add(stub.TroopObject, this, targetCity);

            var tma = new TroopMovePassiveAction(cityId, stub.TroopObject.ObjectId, targetStructure.X, targetStructure.Y, false, true);

            ExecuteChainAndWait(tma, AfterTroopMoved);
            if (targetCity.Owner.Tribesman != null)
                targetCity.Owner.Tribesman.Tribe.SendUpdate();

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                ICity targetCity;

                if (!World.Current.TryGetObjects(cityId, out city))
                    throw new Exception("City is missing");

                if (!World.Current.TryGetObjects(targetCityId, out targetCity))
                {
                    //If the target is missing, walk back
                    using (Concurrency.Current.Lock(city))
                    {
                        ITroopStub stub = city.Troops[stubId];
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

                using (Concurrency.Current.Lock(lockAllStationed, null, city, targetCity))
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
                Dictionary<uint, ICity> cities;
                using (Concurrency.Current.Lock(out cities, cityId, targetCityId))
                {
                    ICity city = cities[cityId];
                    ITroopStub stub;
                    ICity targetCity = cities[targetCityId];

                    //Remove notification to target city once battle is over
                    city.Worker.Notifications.Remove(this);

                    //Remove Incoming Icon from the target's tribe
                    if (targetCity.Owner.Tribesman != null)
                        targetCity.Owner.Tribesman.Tribe.SendUpdate();


                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    // Check if troop is still alive
                    if (stub.TotalCount > 0)
                    {
                        // Calculate how many attack points to give to the city
                        city.BeginUpdate();
                        Procedure.Current.GiveAttackPoints(city, stub.TroopObject.Stats.AttackPoint, initialTroopValue, stub.Value);
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
                        Procedure.Current.TroopObjectDelete(stub.TroopObject, false);

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
                ICity city;
                using (Concurrency.Current.Lock(cityId, out city))
                {
                    ITroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    // Remove notification
                    stub.City.Worker.Notifications.Remove(this);

                    // If city is not in battle then add back to city otherwise join local battle
                    if (city.Battle == null)
                    {
                        city.Worker.References.Remove(stub.TroopObject, this);
                        Procedure.Current.TroopObjectDelete(stub.TroopObject, true);
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
                ICity city;
                using (Concurrency.Current.Lock(cityId, out city))
                {
                    ITroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception("Stub should still exist");

                    city.Worker.References.Remove(stub.TroopObject, this);

                    if (stub.TotalCount == 0)
                        Procedure.Current.TroopObjectDelete(stub.TroopObject, false);
                    else
                        Procedure.Current.TroopObjectDelete(stub.TroopObject, true);

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