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

        private readonly uint cityId;

        private readonly AttackMode mode;

        private readonly uint targetCityId;

        private readonly uint troopObjectId;

        public CityDefenseChainAction(uint cityId,
                                      uint troopObjectId,
                                      uint targetCityId,
                                      AttackMode mode,
                                      BattleProcedure battleProcedure)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.targetCityId = targetCityId;
            this.mode = mode;
            this.battleProcedure = battleProcedure;
        }

        public CityDefenseChainAction(uint id,
                                      string chainCallback,
                                      PassiveAction current,
                                      ActionState chainState,
                                      bool isVisible,
                                      Dictionary<string, string> properties,
                                      BattleProcedure battleProcedure)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.battleProcedure = battleProcedure;
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
                                new XmlKvPair("city_id", cityId), new XmlKvPair("target_city_id", targetCityId),
                                new XmlKvPair("troop_object_id", troopObjectId)
                        });
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
            ITroopObject troopObject;
            if (!World.Current.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
            {
                return Error.ObjectNotFound;
            }

            if (battleProcedure.HasTooManyDefenses(city))
            {
                return Error.TooManyTroops;
            }

            ICity targetCity;
            if (!World.Current.TryGetObjects(targetCityId, out targetCity))
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

            //Load the units stats into the stub
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.Template.LoadStats(TroopBattleGroup.Defense);
            troopObject.Stub.InitialCount = troopObject.Stub.TotalCount;
            troopObject.Stub.RetreatCount = (ushort)Formula.Current.GetAttackModeTolerance(troopObject.Stub.InitialCount, mode);
            troopObject.Stub.AttackMode = mode;
            troopObject.Stub.EndUpdate();

            city.References.Add(troopObject, this);
            city.Notifications.Add(troopObject, this, targetCity);

            var tma = new TroopMovePassiveAction(cityId, troopObject.ObjectId, targetCity.X, targetCity.Y, false, false);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                Dictionary<uint, ICity> cities;

                using (Concurrency.Current.Lock(out cities, cityId, targetCityId))
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

                    Procedure.Current.TroopObjectStation(troopObject, targetCity);

                    if (targetCity.Battle != null)
                    {
                        troopObject.Stub.BeginUpdate();
                        troopObject.Stub.State = TroopState.BattleStationed;
                        troopObject.Stub.EndUpdate();

                        battleProcedure.AddReinforcementToBattle(targetCity.Battle, troopObject.Stub, FormationType.Defense);
                    }

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