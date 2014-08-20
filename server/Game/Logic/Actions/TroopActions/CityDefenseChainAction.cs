#region

using System;
using System.Collections.Generic;
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
    public class CityDefenseChainAction : ChainAction
    {
        private readonly BattleProcedure battleProcedure;

        private readonly IActionFactory actionFactory;

        private readonly ITroopObjectInitializer troopObjectInitializer;

        private readonly ILocker locker;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private uint cityId;
        
        private uint troopObjectId;

        private uint targetCityId;

        public CityDefenseChainAction(BattleProcedure battleProcedure,
                                      IActionFactory actionFactory,
                                      ILocker locker,
                                      IWorld world,
                                      Procedure procedure)
        {
            this.battleProcedure = battleProcedure;
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.world = world;
            this.procedure = procedure;
        }

        public CityDefenseChainAction(uint cityId,
                                      ITroopObjectInitializer troopObjectInitializer,
                                      uint targetCityId,
                                      BattleProcedure battleProcedure,
                                      IActionFactory actionFactory,
                                      ILocker locker,
                                      IWorld world,
                                      Procedure procedure)
            : this(battleProcedure, actionFactory, locker, world, procedure)
        {
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
                return ActionType.CityDefenseChain;
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
                            new XmlKvPair("target_city_id", targetCityId),
                            new XmlKvPair("troop_object_id", troopObjectId)
                        });
            }
        }

        public override Error SystemCancelable
        {
            get
            {
                return Error.UncancelableCityDefenseChain;
            }
        }

        public override ActionCategory Category
        {
            get
            {
                return ActionCategory.Defense;
            }
        }

        public override Error Execute()
        {
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            if (battleProcedure.HasTooManyDefenses(city))
            {
                return Error.TooManyTroops;
            }

            ICity targetCity;
            if (!world.TryGetObjects(targetCityId, out targetCity))
            {
                return Error.ObjectNotFound;
            }

            // Can't defend cities that are being deleted
            if (targetCity.Deleted != City.DeletedState.NotDeleted)
            {
                return Error.ObjectNotFound;
            }

            // Can't send out defense while in battle
            if (city.Battle != null)
            {
                return Error.CityInBattle;
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

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId, troopObject.ObjectId, targetCity.PrimaryPosition.X, targetCity.PrimaryPosition.Y, false, false);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
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
                    throw new Exception("Cities missing");
                }
                ICity city = cities[cityId];
                ICity targetCity = cities[targetCityId];

                ITroopObject troopObject;
                if (!city.TryGetTroop(troopObjectId, out troopObject))
                {
                    throw new Exception();
                }

                city.References.Remove(troopObject, this);
                city.Notifications.Remove(this);

                procedure.TroopObjectStation(troopObject, targetCity);

                if (targetCity.Battle != null)
                {
                    troopObject.Stub.BeginUpdate();
                    troopObject.Stub.State = TroopState.BattleStationed;
                    troopObject.Stub.EndUpdate();

                    battleProcedure.AddReinforcementToBattle(targetCity.Battle, troopObject.Stub, FormationType.Defense);
                }

                StateChange(ActionState.Completed);
            });
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }
    }
}