#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
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
    public enum AttackMode
    {
        Weak = 0,

        Normal = 1,

        Strong = 2
    }

    public class AttackChainAction : ChainAction
    {
        private readonly uint cityId;

        private readonly uint troopObjectId;

        private readonly uint targetCityId;

        private readonly uint targetStructureId;

        private int initialTroopValue;

        private readonly AttackMode mode;

        private readonly Formula formula;

        private readonly IActionFactory actionFactory;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        private readonly IGameObjectLocator gameObjectLocator;

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

        public AttackChainAction(uint cityId,
                                 uint troopObjectId,
                                 uint targetCityId,
                                 uint targetStructureId,
                                 AttackMode mode,
                                 Formula formula,
                                 IActionFactory actionFactory,
                                 ObjectTypeFactory objectTypeFactory,
                                 Procedure procedure,
                                 ILocker locker,
                                 IGameObjectLocator gameObjectLocator)
        {
            this.cityId = cityId;
            this.targetCityId = targetCityId;
            this.targetStructureId = targetStructureId;
            this.troopObjectId = troopObjectId;
            this.mode = mode;
            this.formula = formula;
            this.actionFactory = actionFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
        }

        public AttackChainAction(uint id,
                                 string chainCallback,
                                 PassiveAction current,
                                 ActionState chainState,
                                 bool isVisible,
                                 IDictionary<string, string> properties,
                                 Formula formula,
                                 IActionFactory actionFactory,
                                 ObjectTypeFactory objectTypeFactory,
                                 Procedure procedure,
                                 ILocker locker,
                                 IGameObjectLocator gameObjectLocator)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.formula = formula;
            this.actionFactory = actionFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
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

            int currentAttacks = city.Worker.PassiveActions.Values.Count(action => action is AttackChainAction);
            if (currentAttacks > 20)
            {
                return Error.TooManyTroops;
            }

            // Can't attack if target is under newbie protection
            if (formula.IsNewbieProtected(targetCity.Owner))
            {
                return Error.PlayerNewbieProtection;
            }

            // Can't attack cities that are being deleted
            if (targetCity.Deleted != City.DeletedState.NotDeleted)
            {
                return Error.ObjectNotAttackable;
            }

            // Can't attack "Unattackable" Objects
            if (objectTypeFactory.IsStructureType("Unattackable", targetStructure))
            {
                return Error.ObjectNotAttackable;
            }

            // Can't attack "Undestroyable" Objects if they're level 1
            if (targetStructure.Lvl <= 1 && objectTypeFactory.IsStructureType("Undestroyable", targetStructure))
            {
                return Error.StructureUndestroyable;
            }

            // Can't attack tribes mate
            if (city.Owner.Tribesman != null && targetCity.Owner.Tribesman != null && city.Owner.Tribesman.Tribe == targetCity.Owner.Tribesman.Tribe)
            {
                return Error.AssignmentCantAttackFriend;
            }

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(TroopBattleGroup.Attack);
            troopObject.Stub.EndUpdate();

            initialTroopValue = troopObject.Stub.Value;

            city.Worker.References.Add(troopObject, this);
            city.Worker.Notifications.Add(troopObject, this, targetCity);

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
                CallbackLock.CallbackLockHandler lockAllStationed = delegate
                    {
                        return targetCity.Troops.StationedHere().Select(stationedStub => stationedStub.City).Cast<ILockable>().ToArray();
                    };

                using (locker.Lock(lockAllStationed, null, city, targetCity))
                {
                    var bea = actionFactory.CreateEngageAttackPassiveAction(cityId, troopObject.ObjectId, targetCityId, mode);
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
                    city.Worker.Notifications.Remove(this);

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
                        city.Worker.Notifications.Add(troopObject, this);
                    }
                    else
                    {
                        targetCity.BeginUpdate();
                        city.BeginUpdate();

                        // Give back the loot to the target city
                        targetCity.Resource.Add(troopObject.Stats.Loot);

                        // Remove this actions reference from the troop
                        city.Worker.References.Remove(troopObject, this);

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
                    city.Worker.Notifications.Remove(this);

                    // If city is not in battle then add back to city otherwise join local battle
                    if (city.Battle == null)
                    {
                        city.Worker.References.Remove(troopObject, this);
                        procedure.TroopObjectDelete(troopObject, true);
                        StateChange(ActionState.Completed);
                    }
                    else
                    {
                        var eda = actionFactory.CreateEngageDefensePassiveAction(cityId, troopObject.ObjectId);
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

                    city.Worker.References.Remove(troopObject, this);

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