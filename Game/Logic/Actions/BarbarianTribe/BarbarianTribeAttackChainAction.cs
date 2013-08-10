#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
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
    public class BarbarianTribeAttackChainAction : ChainAction
    {
        private readonly IActionFactory actionFactory;

        private readonly BattleProcedure battleProcedure;

        private readonly uint cityId;

        private uint troopObjectId;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly Procedure procedure;

        private readonly uint targetObjectId;

        private readonly ITroopObjectInitializer troopObjectInitializer;

        public BarbarianTribeAttackChainAction(uint cityId,
                                               uint targetObjectId,
                                               ITroopObjectInitializer troopObjectInitializer,
                                               IActionFactory actionFactory,
                                               Procedure procedure,
                                               ILocker locker,
                                               IGameObjectLocator gameObjectLocator,
                                               BattleProcedure battleProcedure)
        {
            this.cityId = cityId;
            this.targetObjectId = targetObjectId;
            this.troopObjectInitializer = troopObjectInitializer;
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
        }

        public BarbarianTribeAttackChainAction(uint id,
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
            targetObjectId = uint.Parse(properties["target_object_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.BarbarianTribeAttackChain;
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
                                new XmlKvPair("target_object_id", targetObjectId)
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
            IBarbarianTribe barbarianTribe;

            if (!troopObjectInitializer.GetTroopObject(out troopObject))
            {
                return Error.TroopChanged;
            }

            troopObjectId = troopObject.ObjectId;

            if (!gameObjectLocator.TryGetObjects(cityId, out city) || !gameObjectLocator.TryGetObjects(targetObjectId, out barbarianTribe))
            {
                troopObjectInitializer.DeleteTroopObject(troopObject);
                return Error.ObjectNotFound;
            }

            if (battleProcedure.HasTooManyAttacks(city))
            {
                troopObjectInitializer.DeleteTroopObject(troopObject);
                return Error.TooManyTroops;
            }

            if (barbarianTribe.CampRemains == 0 || !barbarianTribe.InWorld)
            {
                troopObjectInitializer.DeleteTroopObject(troopObject);
                return Error.BarbarianTribeNoCampsRemaining;
            }

            city.References.Add(troopObject, this);
            city.Notifications.Add(troopObject, this);

            var moveAction = actionFactory.CreateTroopMovePassiveAction(cityId,
                                                                        troopObject.ObjectId,
                                                                        barbarianTribe.X,
                                                                        barbarianTribe.Y,
                                                                        isReturningHome: false,
                                                                        isAttacking: true);

            ExecuteChainAndWait(moveAction, AfterTroopMoved);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Fired)
            {
                IBarbarianTribe targetBarbarianTribe;
                // Verify the target is still good, otherwise we walk back immediately
                if (!gameObjectLocator.TryGetObjects(targetObjectId, out targetBarbarianTribe) || targetBarbarianTribe.CampRemains == 0)
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
                IBarbarianTribe targetBarbarianTribe;
                ITroopObject troopObject;

                if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
                {
                    throw new Exception("City or troop object is missing");
                }

                if (!gameObjectLocator.TryGetObjects(targetObjectId, out targetBarbarianTribe))
                {
                    //If the target is missing, walk back
                    using (locker.Lock(city))
                    {
                        TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id,
                                                                                                troopObject.ObjectId,
                                                                                                city.X,
                                                                                                city.Y,
                                                                                                true,
                                                                                                true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                        return;
                    }
                }

                using (locker.Lock(city, targetBarbarianTribe))
                {
                    var bea = actionFactory.CreateBarbarianTribeEngageAttackPassiveAction(cityId, troopObject.ObjectId, targetObjectId);
                    ExecuteChainAndWait(bea, AfterBattle);
                }
            }
        }

        private void AfterBattle(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                using (locker.Lock(cityId, out city))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception("Troop object should still exist");
                    }

                    // Calculate how many attack points to give to the city
                    city.BeginUpdate();
                    procedure.GiveAttackPoints(city,troopObject.Stats.AttackPoint);
                    city.EndUpdate();
                    
                    // Check if troop is still alive
                    if (troopObject.Stub.TotalCount > 0)
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
                    else
                    {
                        city.References.Remove(troopObject, this);

                        // Remove troop since he's dead
                        city.BeginUpdate();                        
                        procedure.TroopObjectDelete(troopObject, false);
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
                    // If city is not in battle then add back to city otherwise join local battle
                    if (city.Battle == null)
                    {
                        city.References.Remove(troopObject, this);
                        city.Notifications.Remove(this);
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
                    city.References.Remove(troopObject, this);
                    city.Notifications.Remove(this);

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