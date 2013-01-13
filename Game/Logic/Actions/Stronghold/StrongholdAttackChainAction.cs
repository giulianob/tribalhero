#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Formulas;
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
        private readonly IActionFactory actionFactory;

        private readonly BattleProcedure battleProcedure;

        private readonly StrongholdBattleProcedure strongholdBattleProcedure;

        private readonly uint cityId;

        private readonly Formula formula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly AttackMode mode;

        private readonly Procedure procedure;

        private readonly uint targetStrongholdId;

        private readonly uint troopObjectId;

        public StrongholdAttackChainAction(uint cityId,
                                           uint troopObjectId,
                                           uint targetStrongholdId,
                                           AttackMode mode,
                                           IActionFactory actionFactory,
                                           Procedure procedure,
                                           ILocker locker,
                                           IGameObjectLocator gameObjectLocator,
                                           BattleProcedure battleProcedure,
                                           StrongholdBattleProcedure strongholdBattleProcedure,
                                           Formula formula)
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
            this.strongholdBattleProcedure = strongholdBattleProcedure;
            this.formula = formula;
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
                                           BattleProcedure battleProcedure,
                                           StrongholdBattleProcedure strongholdBattleProcedure,
                                           Formula formula)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.strongholdBattleProcedure = strongholdBattleProcedure;
            this.formula = formula;
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            mode = (AttackMode)uint.Parse(properties["mode"]);
            targetStrongholdId = uint.Parse(properties["target_stronghold_id"]);
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
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("troop_object_id", troopObjectId),
                                new XmlKvPair("target_stronghold_id", targetStrongholdId),
                                new XmlKvPair("mode", (byte)mode)
                        });
            }
        }

        public override ActionCategory Category
        {
            get
            {
                return ActionCategory.Attack;
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

            if (!troopObject.Stub.HasFormation(FormationType.Attack))
            {
                return Error.Unexpected;                
            }

            if (battleProcedure.HasTooManyAttacks(city))
            {
                return Error.TooManyTroops;
            }

            var canStrongholdBeAttacked = strongholdBattleProcedure.CanStrongholdBeAttacked(city, targetStronghold);
            if (canStrongholdBeAttacked != Error.Ok)
            {
                return canStrongholdBeAttacked;
            }

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(TroopBattleGroup.Attack);
            troopObject.Stub.RetreatCount = (ushort)formula.GetAttackModeTolerance(troopObject.Stub.TotalCount, mode);
            troopObject.Stub.EndUpdate();

            city.References.Add(troopObject, this);

            city.Notifications.Add(troopObject, this, targetStronghold);

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId,
                                                                 troopObject.ObjectId,
                                                                 targetStronghold.X,
                                                                 targetStronghold.Y,
                                                                 false,
                                                                 true);

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
                    invalidTarget = strongholdBattleProcedure.CanStrongholdBeAttacked(city, targetStronghold) != Error.Ok &&
                                    strongholdBattleProcedure.CanStrongholdBeDefended(city, targetStronghold) != Error.Ok;
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
                    // Remove from remote stronghold and add only to this troop
                    city.Notifications.Remove(this);
                    city.Notifications.Add(troopObject, this);

                    TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                                            troopObject.ObjectId,
                                                                                            city.X,
                                                                                            city.Y,
                                                                                            true,
                                                                                            true);
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
                    if (city.Owner.IsInTribe)
                    {
                        if (targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                        {
                            StationTroopInStronghold(troopObject, targetStronghold);
                            return;
                        }

                        if (targetStronghold.GateOpenTo == city.Owner.Tribesman.Tribe)
                        {
                            // If stronghold's gate is open to the tribe, then it should engage the stronghold
                            JoinOrCreateStrongholdMainBattle(city);
                            return;
                        }

                        if (targetStronghold.GateOpenTo == null)
                        {
                            // If gate isn't open to anyone then engage the gate
                            var bea = actionFactory.CreateStrongholdEngageGateAttackPassiveAction(cityId,
                                                                                                  troopObject.ObjectId,
                                                                                                  targetStrongholdId);
                            ExecuteChainAndWait(bea, AfterGateBattle);
                            return;
                        }
                    }

                    city.Notifications.Remove(this);

                    // Walk back to city if none of the above conditions apply
                    TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                                            troopObject.ObjectId,
                                                                                            city.X,
                                                                                            city.Y,
                                                                                            true,
                                                                                            true);
                    ExecuteChainAndWait(tma, AfterTroopMovedHome);
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

            var bea = actionFactory.CreateStrongholdEngageMainAttackPassiveAction(cityId,
                                                                                  troopObject.ObjectId,
                                                                                  targetStrongholdId,
                                                                                  mode);
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

                    //Remove notification once battle is over
                    city.Notifications.Remove(this);

                    // Calculate how many attack points to give to the city
                    city.BeginUpdate();
                    procedure.GiveAttackPoints(city,troopObject.Stats.AttackPoint);
                    city.EndUpdate();

                    // Remove troop if he's dead
                    if (TroopIsDead(troopObject, city))
                    {
                        StateChange(ActionState.Completed);
                        return;
                    }

                    if (city.Owner.IsInTribe && targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                    {
                        // If our city is the one that now owns the stronghold then station there and we're done               
                        StationTroopInStronghold(troopObject, targetStronghold);
                    }
                    else
                    {
                        // Send troop back home
                        var tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                             troopObject.ObjectId,
                                                                             city.X,
                                                                             city.Y,
                                                                             true,
                                                                             true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    }
                }
            }
        }

        private void StationTroopInStronghold(ITroopObject troopObject, IStronghold stronghold)
        {
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.ChangeFormation(FormationType.Attack, FormationType.Defense);
            troopObject.Stub.EndUpdate();

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
                    if (TroopIsDead(troopObject, city))
                    {
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
                        //Remove notification to target once battle is over
                        city.Notifications.Remove(this);

                        // Send troop back home
                        var tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                             troopObject.ObjectId,
                                                                             city.X,
                                                                             city.Y,
                                                                             true,
                                                                             true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                    }
                }
            }
        }

        private bool TroopIsDead(ITroopObject troopObject, ICity city)
        {
            if (troopObject.Stub.TotalCount != 0)
            {
                return false;
            }

            // Remove troop since he's dead
            city.BeginUpdate();
            procedure.TroopObjectDelete(troopObject, false);
            city.EndUpdate();

            return true;
        }

        private void AfterTroopMovedHome(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                ITroopObject troopObject;
                using (locker.Lock(cityId, troopObjectId, out city, out troopObject))
                {
                    // If city is not in battle then add back to city otherwise join local battle
                    if (city.Battle == null)
                    {
                        procedure.TroopObjectDelete(troopObject, true);
                        StateChange(ActionState.Completed);
                    }
                    else
                    {
                        var eda = actionFactory.CreateCityEngageDefensePassiveAction(cityId, troopObject.ObjectId, FormationType.Attack);
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
                    procedure.TroopObjectDelete(troopObject, troopObject.Stub.TotalCount != 0);
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