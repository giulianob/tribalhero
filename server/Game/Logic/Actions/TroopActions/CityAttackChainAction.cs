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
    public class CityAttackChainAction : ChainAction
    {
        private readonly IActionFactory actionFactory;

        private readonly BattleProcedure battleProcedure;

        private readonly Formula formula;

        private uint cityId;

        private readonly ITroopObjectInitializer troopObjectInitializer;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly CityBattleProcedure cityBattleProcedure;

        private readonly ILocker locker;

        private readonly Procedure procedure;

        private uint targetCityId;

        private readonly Position tmpTarget;

        private uint troopObjectId;

        public CityAttackChainAction(IActionFactory actionFactory,
                                     Procedure procedure,
                                     ILocker locker,
                                     IGameObjectLocator gameObjectLocator,
                                     CityBattleProcedure cityBattleProcedure,
                                     BattleProcedure battleProcedure,
                                     Formula formula)
        {
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.cityBattleProcedure = cityBattleProcedure;
            this.battleProcedure = battleProcedure;
            this.formula = formula;
        }

        public CityAttackChainAction(uint cityId,
                                     ITroopObjectInitializer troopObjectInitializer,
                                     uint targetCityId,
                                     Position target,
                                     IActionFactory actionFactory,
                                     Procedure procedure,
                                     ILocker locker,
                                     IGameObjectLocator gameObjectLocator,
                                     CityBattleProcedure cityBattleProcedure,
                                     BattleProcedure battleProcedure,
                                     Formula formula)
            : this(actionFactory, procedure, locker, gameObjectLocator, cityBattleProcedure, battleProcedure,formula)
        {
            this.tmpTarget = target;
            this.cityId = cityId;
            this.troopObjectInitializer = troopObjectInitializer;
            this.targetCityId = targetCityId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
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
                                new XmlKvPair("city_id", cityId), 
                                new XmlKvPair("troop_object_id", troopObjectId),
                                new XmlKvPair("target_city_id", targetCityId)
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
            ICity targetCity;

            if (!gameObjectLocator.TryGetObjects(cityId, out city) ||
                !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
            {
                return Error.CityNotFound;
            }

            var targetStructure = gameObjectLocator.Regions.GetObjectsInTile(tmpTarget.X, tmpTarget.Y).OfType<IStructure>().FirstOrDefault(s => s.City == targetCity);

            if (targetStructure == null)
            {
                return Error.StructureNotFound;
            }

            if (city.Owner.PlayerId == targetCity.Owner.PlayerId)
            {
                return Error.AttackSelf;
            }
            
            if (battleProcedure.HasTooManyAttacks(city))
            {
                return Error.TooManyTroops;
            }

            var cityAttackResult = cityBattleProcedure.CanCityBeAttacked(city, targetCity);
            if (cityAttackResult != Error.Ok)
            {
                return cityAttackResult;
            }

            var structureAttackResult = cityBattleProcedure.CanStructureBeAttacked(targetStructure);
            if (structureAttackResult != Error.Ok)
            {
                return structureAttackResult;
            }

            ITroopObject troopObject;
            var troopInitializeResult = troopObjectInitializer.GetTroopObject(out troopObject);
            if (troopInitializeResult != Error.Ok)
            {
                return troopInitializeResult;
            }

            troopObjectId = troopObject.ObjectId;

            city.References.Add(troopObject, this);
            city.Notifications.Add(troopObject, this, targetCity);

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId,
                                                                 troopObject.ObjectId,
                                                                 tmpTarget.X,
                                                                 tmpTarget.Y,
                                                                 false,
                                                                 true);

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
                    locker.Lock(city).Do(() =>
                    {
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

                // Get all of the stationed city id's from the target city since they will be used by the engage attack action
                CallbackLock.CallbackLockHandler lockAllStationed = delegate
                {
                    return targetCity.Troops.StationedHere()
                                     .Select(stationedStub => stationedStub.City)
                                     .Cast<ILockable>()
                                     .ToArray();
                };

                locker.Lock(lockAllStationed, null, city, targetCity).Do(() =>
                {
                    var bea = actionFactory.CreateCityEngageAttackPassiveAction(cityId,
                                                                                troopObject.ObjectId,
                                                                                targetCityId);
                    ExecuteChainAndWait(bea, AfterBattle);
                });
            }
        }

        private void AfterBattle(ActionState state)
        {
            if (state != ActionState.Completed)
            {
                return;
            }

            Dictionary<uint, ICity> cities;
            locker.Lock(out cities, cityId, targetCityId).Do(() =>
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

                // Calculate how many attack points to give to the city
                city.BeginUpdate();
                city.AttackPoint += troopObject.Stats.AttackPoint;
                city.EndUpdate();

                // Check if troop is still alive
                if (troopObject.Stub.TotalCount > 0)
                {
                    // Add notification for walking back
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
                else
                {
                    //Remove notification to target city once battle is over
                    city.References.Remove(troopObject, this);

                    targetCity.BeginUpdate();
                    city.BeginUpdate();

                    // Give back the loot to the target city
                    targetCity.Resource.Add(troopObject.Stats.Loot);

                    // Remove troop since he's dead
                    procedure.TroopObjectDelete(troopObject, false);

                    targetCity.EndUpdate();
                    city.EndUpdate();

                    StateChange(ActionState.Completed);
                }
            });
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
                city.References.Remove(troopObject, this);
                city.Notifications.Remove(this);

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