#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class CityAttackChainAction : ChainAction
    {
        private readonly uint cityId;

        private readonly uint troopObjectId;

        private readonly uint targetCityId;

        private readonly uint targetStructureId;

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
                return targetCityId;
            }
        }

        public CityAttackChainAction(uint cityId,
                                 uint troopObjectId,
                                 uint targetCityId,
                                 uint targetStructureId,
                                 AttackMode mode,
                                 IActionFactory actionFactory,
                                 Procedure procedure,
                                 ILocker locker,
                                 IGameObjectLocator gameObjectLocator,
                                 BattleProcedure battleProcedure)
        {
            this.cityId = cityId;
            this.targetCityId = targetCityId;
            this.targetStructureId = targetStructureId;
            this.troopObjectId = troopObjectId;
            this.mode = mode;
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.battleProcedure = battleProcedure;
        }

        public CityAttackChainAction(uint id,
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
            targetCityId = uint.Parse(properties["target_city_id"]);
            targetStructureId = uint.Parse(properties["target_object_id"]);
            initialTroopValue = int.Parse(properties["initial_troop_value"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.CityAttackChain;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("troop_object_id", troopObjectId), new XmlKvPair("target_city_id", targetCityId),
                                new XmlKvPair("target_object_id", targetStructureId), new XmlKvPair("mode", (byte)mode),
                                new XmlKvPair("initial_troop_value", initialTroopValue)
                        });
            }
        }

        public override Error Execute()
        {
            ICity city;
            ITroopObject troopObject;
            ICity targetCity;
            IStructure targetStructure;

            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject) ||
                !gameObjectLocator.TryGetObjects(targetCityId, targetStructureId, out targetCity, out targetStructure))
            {
                return Error.ObjectNotFound;
            }

            if (city.Owner.PlayerId == targetCity.Owner.PlayerId)
            {
                return Error.AttackSelf;
            }

            int currentAttacks = city.Worker.PassiveActions.Values.Count(action => action is CityAttackChainAction);
            if (currentAttacks > 20)
            {
                return Error.TooManyTroops;
            }

            var cityAttackResult = battleProcedure.CanCityBeAttacked(city, targetCity);
            if (cityAttackResult != Error.Ok)
            {
                return cityAttackResult;
            }

            var structureAttackResult = battleProcedure.CanStructureBeAttacked(targetStructure);
            if (structureAttackResult != Error.Ok)
            {
                return structureAttackResult;
            }

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(TroopBattleGroup.Attack);
            troopObject.Stub.EndUpdate();

            initialTroopValue = troopObject.Stub.Value;

            city.References.Add(troopObject, this);
            city.Notifications.Add(troopObject, this, targetCity);

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId, troopObject.ObjectId, targetStructure.X, targetStructure.Y, false, true);

            ExecuteChainAndWait(tma, AfterTroopMoved);
            if (targetCity.Owner.Tribesman != null)
            {
                targetCity.Owner.Tribesman.Tribe.SendUpdate();
            }

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                ICity targetCity;
                ITroopObject troopObject;

                if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
                {
                    throw new Exception("City or troop object is missing");
                }

                if (!gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
                {
                    //If the target is missing, walk back
                    using (locker.Lock(city))
                    {
                        TroopMovePassiveAction tma = actionFactory.CreateTroopMovePassiveAction(city.Id, troopObject.ObjectId, city.X, city.Y, true, true);
                        ExecuteChainAndWait(tma, AfterTroopMovedHome);
                        return;
                    }
                }

                // Get all of the stationed city id's from the target city since they will be used by the engage attack action
                CallbackLock.CallbackLockHandler lockAllStationed =
                        delegate { return targetCity.Troops.StationedHere().Select(stationedStub => stationedStub.City).Cast<ILockable>().ToArray(); };

                using (locker.Lock(lockAllStationed, null, city, targetCity))
                {
                    var bea = actionFactory.CreateCityEngageAttackPassiveAction(cityId, troopObject.ObjectId, targetCityId, mode);
                    ExecuteChainAndWait(bea, AfterBattle);
                }
            }
        }

        private void AfterBattle(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                Dictionary<uint, ICity> cities;
                using (locker.Lock(out cities, cityId, targetCityId))
                {
                    if (cities == null)
                    {
                        throw new Exception("City not found");
                    }

                    ICity city = cities[cityId];
                    ICity targetCity = cities[targetCityId];

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