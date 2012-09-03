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

            city.Notifications.Add(troopObject, this);

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

                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
                {
                    CancelCurrentChain();
                    return;
                }

                bool invalidTarget;
                using (locker.Lock(city, targetStronghold))
                {
                    invalidTarget = battleProcedure.CanStrongholdBeAttacked(city, targetStronghold) != Error.Ok &&
                                    battleProcedure.CanStrongholdBeDefended(city, targetStronghold) != Error.Ok;
                }

                // If the stronghold is not there or we are unable to attack/defense it, then cancel the current TroopMoveAction
                if (invalidTarget)
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

                CallbackLock.CallbackLockHandler lockAll = delegate { return targetStronghold.LockList.ToArray(); };

                using (locker.Lock(lockAll, null, city, targetStronghold))
                {
                    if (targetStronghold.GateOpenTo == city.Owner.Tribesman.Tribe)
                    {
                        // If stronghold's gate is open to the tribe, then it should engage the stronghold
                        JoinOrCreateStrongholdMainBattle(city);
                    }
                    else if (targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                    {
                        StationTroopInStronghold(troopObject, targetStronghold);

                        // If tribe already owns stronghold, then it should defend it
                        if (targetStronghold.GateOpenTo != null && targetStronghold.MainBattle != null)
                        {
                            throw new Exception("Need to join the current battle on the defensive side");
                        }
                    }
                    else if (targetStronghold.GateOpenTo == null)
                    {
                        // If gate isn't open to anyone then engage the gate
                        var bea = actionFactory.CreateStrongholdEngageGateAttackPassiveAction(cityId, troopObject.ObjectId, targetStrongholdId);
                        ExecuteChainAndWait(bea, AfterGateBattle);
                    }
                    else
                    {
                        // Walk back to city if none of the above conditions apply

                        TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    }
                }
            }
        }

        private void JoinOrCreateStrongholdMainBattle(ICity city)
        {
            ITroopObject troopObject;
            if (!city.TryGetTroop(troopObjectId, out troopObject))
            {
                throw new Exception("Troop object should still exist");
            }

            var bea = actionFactory.CreateStrongholdEngageMainAttackPassiveAction(cityId, troopObject.ObjectId, targetStrongholdId, mode);
            ExecuteChainAndWait(bea, AfterMainBattle);
        }

        private void AfterMainBattle(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                IStronghold targetStronghold;

                if (!gameObjectLocator.TryGetObjects(cityId, out city))
                {
                    throw new Exception("City not found");
                }

                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
                {
                    throw new Exception("Stronghold not found");
                }

                CallbackLock.CallbackLockHandler lockAll = delegate { return targetStronghold.LockList.ToArray(); };

                using (locker.Lock(lockAll, null, city, targetStronghold))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception("Troop object should still exist");
                    }

                    // Remove troop if he's dead
                    if (troopObject.Stub.TotalCount == 0)
                    {
                        city.BeginUpdate();
                        // Remove this actions reference from the troop
                        city.References.Remove(troopObject, this);
                        // Remove troop since he's dead
                        procedure.TroopObjectDelete(troopObject, false);
                        city.EndUpdate();

                        StateChange(ActionState.Completed);
                        return;
                    }

                    if (city.Owner.IsInTribe && targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                    {
                        // If our city is the one that now owns the stronghold then station there                        
                        StationTroopInStronghold(troopObject, targetStronghold);
                    }
                    else
                    {
                        // Send troop back home
                        var tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    }
                }
            }
        }

        private void StationTroopInStronghold(ITroopObject troopObject, IStronghold stronghold)
        {
            procedure.TroopObjectStation(troopObject, stronghold);
            StateChange(ActionState.Completed);
        }

        private void AfterGateBattle(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                IStronghold targetStronghold;

                if (!gameObjectLocator.TryGetObjects(cityId, out city))
                {
                    throw new Exception("City not found");
                }

                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
                {
                    throw new Exception("Stronghold not found");
                }

                CallbackLock.CallbackLockHandler lockAll = delegate { return targetStronghold.LockList.ToArray(); };

                using (locker.Lock(lockAll, null, city, targetStronghold))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception("Troop object should still exist");
                    }

                    // Remove troop if he's dead
                    if (troopObject.Stub.TotalCount == 0)
                    {
                        city.BeginUpdate();
                        // Remove this actions reference from the troop
                        city.References.Remove(troopObject, this);
                        // Remove troop since he's dead
                        procedure.TroopObjectDelete(troopObject, false);
                        city.EndUpdate();

                        StateChange(ActionState.Completed);
                        return;
                    }

                    if (city.Owner.IsInTribe && targetStronghold.GateOpenTo == city.Owner.Tribesman.Tribe)
                    {
                        // If our city is the one that now has access to the stronghold then join the real battle
                        JoinOrCreateStrongholdMainBattle(city);
                    }
                    else
                    {
                        // Send troop back home
                        var tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
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