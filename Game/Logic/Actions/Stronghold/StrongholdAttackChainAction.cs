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
        private readonly IActionFactory actionFactory;

        private readonly BattleProcedure battleProcedure;

        private readonly StrongholdBattleProcedure strongholdBattleProcedure;

        private uint cityId;

        private readonly ITroopObjectInitializer troopObjectInitializer;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly bool tempForceAttack;

        private readonly Procedure procedure;

        private uint targetStrongholdId;

        private uint troopObjectId;

        public StrongholdAttackChainAction(IActionFactory actionFactory,
                                           Procedure procedure,
                                           ILocker locker,
                                           IGameObjectLocator gameObjectLocator,
                                           BattleProcedure battleProcedure,
                                           StrongholdBattleProcedure strongholdBattleProcedure)
        {
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
            this.strongholdBattleProcedure = strongholdBattleProcedure;
        }

        public StrongholdAttackChainAction(uint cityId,
                                           ITroopObjectInitializer troopObjectInitializer,
                                           uint targetStrongholdId,
                                           bool forceAttack,
                                           IActionFactory actionFactory,
                                           Procedure procedure,
                                           ILocker locker,
                                           IGameObjectLocator gameObjectLocator,
                                           BattleProcedure battleProcedure,
                                           StrongholdBattleProcedure strongholdBattleProcedure)
            : this(actionFactory, procedure, locker, gameObjectLocator, battleProcedure, strongholdBattleProcedure)
        {
            this.cityId = cityId;
            this.troopObjectInitializer = troopObjectInitializer;
            this.targetStrongholdId = targetStrongholdId;            
            this.tempForceAttack = forceAttack;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
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
                                new XmlKvPair("city_id", cityId), 
                                new XmlKvPair("troop_object_id", troopObjectId),
                                new XmlKvPair("target_stronghold_id", targetStrongholdId),
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
            IStronghold targetStronghold;

            if (!gameObjectLocator.TryGetObjects(cityId, out city) ||
                !gameObjectLocator.TryGetObjects(targetStrongholdId, out targetStronghold))
            {
                return Error.ObjectNotFound;
            }

            if (battleProcedure.HasTooManyAttacks(city))
            {
                return Error.TooManyTroops;
            }

            var canStrongholdBeAttacked = strongholdBattleProcedure.CanStrongholdBeAttacked(city, targetStronghold, this.tempForceAttack);
            if (canStrongholdBeAttacked != Error.Ok)
            {
                return canStrongholdBeAttacked;
            }

            ITroopObject troopObject;
            var troopInitializeResult = troopObjectInitializer.GetTroopObject(out troopObject);
            if (troopInitializeResult != Error.Ok)
            {
                return troopInitializeResult;
            }

            troopObjectId = troopObject.ObjectId;

            if (!troopObject.Stub.HasFormation(FormationType.Attack))
            {
                troopObjectInitializer.DeleteTroopObject();
                return Error.Unexpected;                
            }

            city.References.Add(troopObject, this);

            city.Notifications.Add(troopObject, this, targetStronghold);

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId,
                                                                 troopObject.ObjectId,
                                                                 targetStronghold.PrimaryPosition.X,
                                                                 targetStronghold.PrimaryPosition.Y,
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

                /*
                 * Commenting out this logic to return if stronghold is not attackable/defendable until we are able to correctly cancel assignments. At the moment, 
                 * there are cases where when troops are in an assignment and a stronghold is taken over,
                 * the ones that are on the walk will return and then the ones that are dispatched will be sent to the stronghold.
                 * 
                bool invalidTarget;
                using (locker.Lock(city, targetStronghold))
                {
                    invalidTarget = strongholdBattleProcedure.CanStrongholdBeAttacked(city, targetStronghold) != Error.Ok &&
                                    strongholdBattleProcedure.CanStrongholdBeDefended(city, targetStronghold) != Error.Ok;
                }
                */
                
                // If player is no longer in a tribe, return
                if (!city.Owner.IsInTribe)
                {
                    CancelCurrentChain();
                }

                return;
            }
            
            if (state == ActionState.Failed)
            {
                // If TroopMove failed it's because we cancelled it and the target is invalid. Walk back home
                ICity city;
                ITroopObject troopObject;

                locker.Lock(cityId, troopObjectId, out city, out troopObject).Do(() =>
                {
                    // Remove from remote stronghold and add only to this troop
                    city.Notifications.Remove(this);
                    city.Notifications.Add(troopObject, this);

                    TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                                            troopObject.ObjectId,
                                                                                            city.PrimaryPosition.X,
                                                                                            city.PrimaryPosition.Y,
                                                                                            true,
                                                                                            true);
                    ExecuteChainAndWait(tma, AfterTroopMovedHome);
                });

                return;
            }
            
            if (state == ActionState.Completed)
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

                locker.Lock(lockAll, null, city, targetStronghold).Do(() =>
                {
                    if (city.Owner.IsInTribe)
                    {
                        if (targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                        {
                            if (targetStronghold.MainBattle != null)
                            {
                                MoveFromAttackToDefenseFormation(troopObject);
                                battleProcedure.AddReinforcementToBattle(targetStronghold.MainBattle, troopObject.Stub, FormationType.Defense);
                                StationTroopInStronghold(troopObject, targetStronghold, TroopState.BattleStationed);
                            }
                            else
                            {
                                StationTroopInStronghold(troopObject, targetStronghold);
                            }

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
                    city.Notifications.Add(troopObject, this);

                    // Walk back to city if none of the above conditions apply
                    TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                                            troopObject.ObjectId,
                                                                                            city.PrimaryPosition.X,
                                                                                            city.PrimaryPosition.Y,
                                                                                            true,
                                                                                            true);
                    ExecuteChainAndWait(tma, AfterTroopMovedHome);
                });
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
                                                                                  targetStrongholdId);
            ExecuteChainAndWait(bea, AfterMainBattle);
        }

        private void AfterMainBattle(ActionState state)
        {
            if (state != ActionState.Completed)
            {
                return;
            }

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

            CallbackLock.CallbackLockHandler lockAll = delegate
            {
                var locks = targetStronghold.LockList.ToList();
                if (city.Owner.IsInTribe)
                {
                    locks.Add(city.Owner.Tribesman.Tribe);
                }

                return locks.ToArray();
            };

            locker.Lock(lockAll, null, city, targetStronghold).Do(() =>
            {
                ITroopObject troopObject;
                if (!city.TryGetTroop(troopObjectId, out troopObject))
                {
                    throw new Exception("Troop object should still exist");
                }

                //Remove notification once battle is over
                city.Notifications.Remove(this);

                // Attack points from SH battles go to tribe
                if (city.Owner.IsInTribe)
                {
                    city.Owner.Tribesman.Tribe.AttackPoint += troopObject.Stats.AttackPoint;
                }

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
                    city.Notifications.Add(troopObject, this);
                    // Send troop back home
                    var tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                         troopObject.ObjectId,
                                                                         city.PrimaryPosition.X,
                                                                         city.PrimaryPosition.Y,
                                                                         true,
                                                                         true);
                    ExecuteChainAndWait(tma, AfterTroopMovedHome);
                }
            });
        }

        private void MoveFromAttackToDefenseFormation(ITroopObject troopObject)
        {
            if (!troopObject.Stub.HasFormation(FormationType.Defense))
            {
                troopObject.Stub.BeginUpdate();
                troopObject.Stub.ChangeFormation(FormationType.Attack, FormationType.Defense);
                troopObject.Stub.EndUpdate();
            }
        }

        private void StationTroopInStronghold(ITroopObject troopObject, IStronghold stronghold, TroopState stubState = TroopState.Stationed)
        {
            MoveFromAttackToDefenseFormation(troopObject);

            procedure.TroopObjectStation(troopObject, stronghold);
            if (troopObject.Stub.State != stubState)
            {
                troopObject.Stub.BeginUpdate();
                troopObject.Stub.State = stubState;
                troopObject.Stub.EndUpdate();
            }

            StateChange(ActionState.Completed);
        }

        private void AfterGateBattle(ActionState state)
        {
            if (state != ActionState.Completed)
            {
                return;
            }

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

            locker.Lock(lockAll, null, city, targetStronghold).Do(() =>
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
                else if (city.Owner.IsInTribe && targetStronghold.Tribe == city.Owner.Tribesman.Tribe)
                {
                    StationTroopInStronghold(troopObject, targetStronghold);
                }
                else
                {
                    //Remove notification to target once battle is over
                    city.Notifications.Remove(this);

                    city.Notifications.Add(troopObject, this);

                    // Send troop back home
                    var tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.PrimaryPosition.X, city.PrimaryPosition.Y, true, true);
                    ExecuteChainAndWait(tma, AfterTroopMovedHome);
                }
            });
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
            if (state != ActionState.Completed)
            {
                return;
            }

            ICity city;
            ITroopObject troopObject;
            locker.Lock(cityId, troopObjectId, out city, out troopObject).Do(() =>
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
            });
        }

        private void AfterEngageDefense(ActionState state)
        {
            if (state != ActionState.Completed)
            {
                return;
            }
            ICity city;
            ITroopObject troopObject;
            locker.Lock(cityId, troopObjectId, out city, out troopObject).Do(() =>
            {
                procedure.TroopObjectDelete(troopObject, troopObject.Stub.TotalCount != 0);
                StateChange(ActionState.Completed);
            });
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }
    }
}