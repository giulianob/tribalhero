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

        private readonly Formula formula;

        private readonly BattleProcedure battleProcedure;

        private readonly uint cityId;

        private readonly uint troopObjectId;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly BarbarianTribeBattleProcedure barbarianTribeBattleProcedure;

        private readonly ILocker locker;

        private readonly AttackMode mode;

        private readonly Procedure procedure;

        private readonly uint targetObjectId;

        public BarbarianTribeAttackChainAction(uint cityId,
                                               uint troopObjectId,
                                               uint targetObjectId,
                                               AttackMode mode,
                                               IActionFactory actionFactory,
                                               Procedure procedure,
                                               ILocker locker,
                                               IGameObjectLocator gameObjectLocator,
                                               BarbarianTribeBattleProcedure barbarianTribeBattleProcedure,
                                               Formula formula,
                                               BattleProcedure battleProcedure)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.targetObjectId = targetObjectId;
            this.mode = mode;
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.barbarianTribeBattleProcedure = barbarianTribeBattleProcedure;
            this.formula = formula;
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
                                               BarbarianTribeBattleProcedure barbarianTribeBattleProcedure,
                                               Formula formula,
                                               BattleProcedure battleProcedure)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.barbarianTribeBattleProcedure = barbarianTribeBattleProcedure;
            this.formula = formula;
            this.battleProcedure = battleProcedure;
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            mode = (AttackMode)uint.Parse(properties["mode"]);
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
                                new XmlKvPair("target_object_id", targetObjectId),
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
            IBarbarianTribe barbarianTribe;            

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetObjectId, out barbarianTribe))
            {
                return Error.ObjectNotFound;
            }

            if (battleProcedure.HasTooManyAttacks(city))
            {
                return Error.TooManyTroops;
            }

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(TroopBattleGroup.Attack);
            troopObject.Stub.RetreatCount = (ushort)formula.GetAttackModeTolerance(troopObject.Stub.TotalCount, mode);
            troopObject.Stub.EndUpdate();

            city.References.Add(troopObject, this);
            city.Notifications.Add(troopObject, this);

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId, troopObject.ObjectId, barbarianTribe.X, barbarianTribe.Y, false, true);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Fired)
            {
                IBarbarianTribe targetBarbarianTribe;
                // Verify the target is still good, otherwise we walk back immediately
                if (!gameObjectLocator.TryGetObjects(targetObjectId, out targetBarbarianTribe))
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
                    var bea = actionFactory.CreateBarbarianTribeEngageAttackPassiveAction(cityId, troopObject.ObjectId, targetObjectId, mode);
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