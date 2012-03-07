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
    public class DefenseChainAction : ChainAction
    {
        private readonly uint cityId;
        private readonly byte stubId;
        private readonly uint targetCityId;

        public DefenseChainAction(uint cityId, byte stubId, uint targetCityId)
        {
            this.cityId = cityId;
            this.stubId = stubId;
            this.targetCityId = targetCityId;
        }

        public DefenseChainAction(uint id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible, Dictionary<string, string> properties)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["troop_stub_id"]);
            targetCityId = uint.Parse(properties["target_city_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.DefenseChain;
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
                                                        new XmlKvPair("troop_stub_id", stubId)
                                                });
            }
        }

        public override Error Execute()
        {
            ICity city;
            ITroopStub stub;
            if (!World.Current.TryGetObjects(cityId, stubId, out city, out stub))
                return Error.ObjectNotFound;

            int currentReinforcements = city.Worker.PassiveActions.Values.Count(action => action is DefenseChainAction);
            if (currentReinforcements > 20)
            {
                return Error.TooManyTroops;
            }

            ICity targetCity;
            if (!World.Current.TryGetObjects(targetCityId, out targetCity))
                return Error.ObjectNotFound;

            // Can't defend cities that are being deleted
            if (targetCity.Deleted != City.DeletedState.NotDeleted)
                return Error.ObjectNotFound;

            // Can't send out defense while in battle
            if (city.Battle != null)
                return Error.CityInBattle;

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats(TroopBattleGroup.Defense);
            stub.EndUpdate();

            city.Worker.References.Add(stub.TroopObject, this);
            city.Worker.Notifications.Add(stub.TroopObject, this, targetCity);

            var tma = new TroopMovePassiveAction(cityId, stub.TroopObject.ObjectId, targetCity.X, targetCity.Y, false, false);

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
                    ICity city = cities[cityId];
                    ICity targetCity = cities[targetCityId];

                    ITroopStub stub;
                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();

                    city.Worker.References.Remove(stub.TroopObject, this);
                    city.Worker.Notifications.Remove(this);

                    Procedure.Current.TroopObjectStation(stub.TroopObject, targetCity);

                    if (targetCity.Battle != null)
                    {
                        stub.BeginUpdate();
                        stub.State = TroopState.BattleStationed;
                        stub.EndUpdate();

                        targetCity.Battle.AddToDefense(new List<ITroopStub> { stub });
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