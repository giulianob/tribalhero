#region

using System;
using System.Collections.Generic;
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
    public class RetreatChainAction : ChainAction
    {
        private readonly IActionFactory actionFactory;

        private readonly uint cityId;

        private readonly ushort stubId;

        private uint troopObjectId;

        public RetreatChainAction(uint cityId, ushort stubId, IActionFactory actionFactory)
        {
            this.cityId = cityId;
            this.stubId = stubId;
            this.actionFactory = actionFactory;
        }

        public RetreatChainAction(uint id,
                                  string chainCallback,
                                  PassiveAction current,
                                  ActionState chainState,
                                  bool isVisible,
                                  Dictionary<string, string> properties,
                                  IActionFactory actionFactory)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.actionFactory = actionFactory;
            cityId = uint.Parse(properties["city_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.RetreatChain;
            }
        }

        public override ActionCategory Category
        {
            get
            {
                return ActionCategory.Defense;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[]
                {
                        new XmlKvPair("city_id", cityId), 
                        new XmlKvPair("troop_object_id", troopObjectId)
                });
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            ITroopStub stub;
            if (!World.Current.TryGetObjects(cityId, stubId, out city, out stub))
            {
                throw new Exception();
            }

            if (stub.State != TroopState.Stationed)
            {
                return Error.TroopInBattle;
            }

            ITroopObject troopObject;
            if (!Procedure.Current.TroopObjectCreateFromStation(stub, out troopObject))
            {
                return Error.Unexpected;
            }

            troopObjectId = troopObject.ObjectId;

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId, troopObject.ObjectId, stub.City.PrimaryPosition.X, stub.City.PrimaryPosition.Y, true, false);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            stub.City.References.Add(troopObject, this);
            stub.City.Notifications.Add(troopObject, this);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                using (Concurrency.Current.Lock(cityId, out city))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception();
                    }

                    city.Notifications.Remove(this);

                    if (city.Battle == null)
                    {
                        city.References.Remove(troopObject, this);
                        Procedure.Current.TroopObjectDelete(troopObject, true);
                        StateChange(ActionState.Completed);
                    }
                    else
                    {
                        var eda = actionFactory.CreateCityEngageDefensePassiveAction(cityId, troopObjectId, FormationType.Defense);
                        ExecuteChainAndWait(eda, AfterEngageDefense);
                    }
                }
            }
            else if (state == ActionState.Failed)
            {
                ICity city;
                using (Concurrency.Current.Lock(cityId, out city))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception();
                    }

                    Procedure.Current.TroopObjectStation(troopObject, city);
                }
            }
        }

        private void AfterEngageDefense(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                using (Concurrency.Current.Lock(cityId, out city))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception();
                    }

                    city.References.Remove(troopObject, this);
                    Procedure.Current.TroopObjectDelete(troopObject, troopObject.Stub.TotalCount != 0);
                    StateChange(ActionState.Completed);
                }
            }
        }
    }
}