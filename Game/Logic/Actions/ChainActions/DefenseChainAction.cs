#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class DefenseChainAction : ChainAction
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
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                return Error.ObjectNotFound;

            if (city.Troops.Size > 12)
                return Error.TooManyTroops;

            City targetCity;
            if (!Global.World.TryGetObjects(targetCityId, out targetCity))
                return Error.ObjectNotFound;

            if (city.Battle != null)
                return Error.CityInBattle;

            //Load the units stats into the stub
            stub.BeginUpdate();
            stub.Template.LoadStats(TroopBattleGroup.Defense);
            stub.EndUpdate();

            city.Worker.References.Add(stub.TroopObject, this);
            city.Worker.Notifications.Add(stub.TroopObject, this, targetCity);

            var tma = new TroopMovePassiveAction(cityId, stub.TroopObject.ObjectId, targetCity.MainBuilding.X, targetCity.MainBuilding.Y, false, false);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                Dictionary<uint, City> cities;

                using (new MultiObjectLock(out cities, cityId, targetCityId))
                {
                    City city = cities[cityId];
                    City targetCity = cities[targetCityId];

                    TroopStub stub;
                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();

                    city.Worker.References.Remove(stub.TroopObject, this);
                    city.Worker.Notifications.Remove(this);

                    Procedure.TroopObjectStation(stub.TroopObject, targetCity);

                    if (targetCity.Battle != null)
                    {
                        var list = new List<TroopStub>();

                        stub.BeginUpdate();
                        stub.State = TroopState.BattleStationed;
                        stub.EndUpdate();

                        list.Add(stub);

                        targetCity.Battle.AddToDefense(list);
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