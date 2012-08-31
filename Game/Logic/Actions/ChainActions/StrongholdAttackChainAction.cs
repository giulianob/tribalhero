#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class StrongholdAttackChainAction : ChainAction
    {
        private readonly uint cityId;

        private readonly uint troopObjectId;

        private readonly uint targetStrongholdId;

        private int initialTroopValue;

        private readonly AttackMode mode;

        private readonly IActionFactory actionFactory;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly BattleProcedure battleProcedure;

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
                return targetStrongholdId;
            }
        }

        public StrongholdAttackChainAction(uint cityId,
                                           uint troopObjectId,
                                           uint targetStrongholdId,
                                           AttackMode mode,
                                           IActionFactory actionFactory,
                                           Procedure procedure,
                                           ILocker locker,
                                           IGameObjectLocator gameObjectLocator,
                                           BattleProcedure battleProcedure)
        {
            this.cityId = cityId;
            this.targetStrongholdId = targetStrongholdId;
            this.troopObjectId = troopObjectId;
            this.mode = mode;
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
        }

        public StrongholdAttackChainAction(uint id,
                                           string chainCallback,
                                           PassiveAction current,
                                           ActionState chainState,
                                           bool isVisible,
                                           IDictionary<string, string> properties,
                                           IActionFactory actionFactory,
                                           Procedure procedure,
                                           ILocker locker,
                                           IGameObjectLocator gameObjectLocator,
                                           BattleProcedure battleProcedure)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            mode = (AttackMode)uint.Parse(properties["mode"]);
            targetStrongholdId = uint.Parse(properties["target_stronghold_id"]);
            initialTroopValue = int.Parse(properties["initial_troop_value"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StrongholdAttackChain;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[]
                {
                        new XmlKvPair("city_id", cityId), new XmlKvPair("troop_object_id", troopObjectId),
                        new XmlKvPair("target_stronghold_id", targetStrongholdId), new XmlKvPair("mode", (byte)mode),
                        new XmlKvPair("initial_troop_value", initialTroopValue)
                });
            }
        }

        public override Error Execute()
        {
            ICity city;
            ITroopObject troopObject;
            IStronghold targetStronghold;

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
            {
                return Error.ObjectNotFound;
            }

            if (battleProcedure.HasTooManyAttacks(city))
            {
                return Error.TooManyTroops;
            }

            var canStrongholdBeAttacked = battleProcedure.CanStrongholdBeAttacked(city, targetStronghold);
            if (canStrongholdBeAttacked != Error.Ok)
            {
                return canStrongholdBeAttacked;
            }

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(TroopBattleGroup.Attack);
            troopObject.Stub.EndUpdate();

            initialTroopValue = troopObject.Stub.Value;

            city.References.Add(troopObject, this);

            // TODO: Figure out notifications
            //city.Notifications.Add(troopObject, this, targetCity);

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId, troopObject.ObjectId, targetStronghold.X, targetStronghold.Y, false, true);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            if (targetStronghold.Tribe != null)
            {
                targetStronghold.Tribe.SendUpdate();
            }

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Fired)
            {
                // Verify the target is still good, otherwise we walk back immediately

                ICity city;
                IStronghold targetStronghold;
                ITroopObject troopObject;

                if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
                {
                    throw new Exception("City or troop object is missing");
                }

                // If the stronghold is not there or we are unable to attack/defense it, then cancel the current TroopMoveAction
                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold) ||
                    (battleProcedure.CanStrongholdBeAttacked(city, targetStronghold) != Error.Ok &&
                     battleProcedure.CanStrongholdBeDefended(city, targetStronghold) != Error.Ok))
                {
                    CancelCurrentChain();
                }
            }
            else if (state == ActionState.Failed)
            {
                // If TroopMove failed it's because we cancelled it and the target is invalid. Walk back home
                ICity city;
                ITroopObject troopObject;

                using (locker.Lock(cityId, troopObjectId, out city, out troopObject))
                {
                    TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                    ExecuteChainAndWait(tma, AfterTroopMovedHome);
                }
            }
            else if (state == ActionState.Completed)
            {
                ICity city;
                IStronghold targetStronghold;
                ITroopObject troopObject;

                if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
                {
                    throw new Exception("City or troop object is missing");
                }

                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
                {
                    throw new Exception("Stronghold is missing");
                }
                
                if (targetStronghold.GateOpenTo == city.Owner.Tribesman.Tribe)
                {
                    // If stronghold's gate is open to the tribe, then it should engage the stronghold

                    CallbackLock.CallbackLockHandler lockAll = delegate { return targetStronghold.LockList.ToArray(); };

                    using (locker.Lock(lockAll, null, city, targetStronghold))
                    {
                        //var bea = actionFactory.CreateStrongholdEngagePassiveAction(cityId, troopObject.ObjectId, targetStrongholdId, mode);
                        //ExecuteChainAndWait(bea, AfterGateBattle);
                    }
                }                        
                else if (targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                {
                    // If tribe already owns stronghold, then it should defend it

                    if (targetStronghold.GateOpenTo != null && targetStronghold.Battle != null)
                    {
                        // TODO: Join the battle
                    }
                    else
                    {
                        // TODO: Station yourself
                    }
                }                        
                else if (targetStronghold.GateOpenTo == null)
                {
                    // If gate isn't open to anyone then engage the gate

                    CallbackLock.CallbackLockHandler lockAll = delegate { return targetStronghold.LockList.ToArray(); };

                    using (locker.Lock(lockAll, null, city, targetStronghold))
                    {
                        var bea = actionFactory.CreateStrongholdEngageGateAttackPassiveAction(cityId, troopObject.ObjectId, targetStrongholdId);
                        ExecuteChainAndWait(bea, AfterGateBattle);
                    }
                }                        
                else
                {
                    // Walk back to city if none of the above conditions apply

                    using (locker.Lock(cityId, troopObjectId, out city, out troopObject))
                    {
                        TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    }
                }
            }
        }

        private void AfterGateBattle(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                Dictionary<uint, ICity> cities;
                using (locker.Lock(out cities, cityId, targetStrongholdId))
                {
                    if (cities == null)
                    {
                        throw new Exception("City not found");
                    }

                    ICity city = cities[cityId];
                    ICity targetCity = cities[targetStrongholdId];

                    //Remove notification to target city once battle is over
                    city.Notifications.Remove(this);

                    //Remove Incoming Icon from the target's tribe
                    if (targetCity.Owner.Tribesman != null)
                    {
                        targetCity.Owner.Tribesman.Tribe.SendUpdate();
                    }

                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception("Troop object should still exist");
                    }

                    // Check if troop is still alive
                    if (troopObject.Stub.TotalCount > 0)
                    {
                        // Calculate how many attack points to give to the city
                        city.BeginUpdate();
                        procedure.GiveAttackPoints(city, troopObject.Stats.AttackPoint, initialTroopValue, troopObject.Stub.Value);
                        city.EndUpdate();

                        // Send troop back home
                        var tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);

                        // Add notification just to the main city
                        city.Notifications.Add(troopObject, this);
                    }
                    else
                    {
                        targetCity.BeginUpdate();
                        city.BeginUpdate();

                        // Give back the loot to the target city
                        targetCity.Resource.Add(troopObject.Stats.Loot);

                        // Remove this actions reference from the troop
                        city.References.Remove(troopObject, this);

                        // Remove troop since he's dead
                        procedure.TroopObjectDelete(troopObject, false);

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
                ITroopObject troopObject;
                using (locker.Lock(cityId, troopObjectId, out city, out troopObject))
                {
                    // Remove notification
                    city.Notifications.Remove(this);

                    // If city is not in battle then add back to city otherwise join local battle
                    if (city.Battle == null)
                    {
                        city.References.Remove(troopObject, this);
                        procedure.TroopObjectDelete(troopObject, true);
                        StateChange(ActionState.Completed);
                    }
                    else
                    {
                        var eda = actionFactory.CreateCityEngageDefensePassiveAction(cityId, troopObject.ObjectId);
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
                ITroopObject troopObject;
                using (locker.Lock(cityId, troopObjectId, out city, out troopObject))
                {
                    city.References.Remove(troopObject, this);

                    procedure.TroopObjectDelete(troopObject, troopObject.Stub.TotalCount != 0);

                    StateChange(ActionState.Completed);
                }
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override ActionCategory Category
        {
            get
            {
                return ActionCategory.Attack;
            }
        }
    }
}