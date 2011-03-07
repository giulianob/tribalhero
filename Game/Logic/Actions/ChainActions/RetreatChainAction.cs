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
    public class RetreatChainAction : ChainAction
    {
        private readonly uint cityId;
        private readonly byte stubId;

        public RetreatChainAction(uint cityId, byte stubId)
        {
            this.cityId = cityId;
            this.stubId = stubId;
        }

        public RetreatChainAction(uint id, string chainCallback, PassiveAction current, ActionState chainState, bool isVisible, Dictionary<string, string> properties)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            cityId = uint.Parse(properties["city_id"]);
            stubId = byte.Parse(properties["troop_stub_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.RetreatChain;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId), new XmlKvPair("troop_stub_id", stubId)});
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                throw new Exception();

            var tma = new TroopMovePassiveAction(cityId, stub.TroopObject.ObjectId, stub.City.MainBuilding.X, stub.City.MainBuilding.Y, true, false);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            stub.City.Worker.References.Add(stub.TroopObject, this);
            stub.City.Worker.Notifications.Add(stub.TroopObject, this);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                City city;
                using (new MultiObjectLock(cityId, out city))
                {
                    TroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();

                    if (stub.City.Battle == null)
                    {
                        stub.City.Worker.Notifications.Remove(this);
                        stub.City.Worker.References.Remove(stub.TroopObject, this);
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                        StateChange(ActionState.Completed);
                    }
                    else
                    {
                        var eda = new EngageDefensePassiveAction(cityId, stubId);
                        ExecuteChainAndWait(eda, AfterEngageDefense);
                    }
                }
            }
        }

        private void AfterEngageDefense(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                City city;
                using (new MultiObjectLock(cityId, out city))
                {
                    TroopStub stub;

                    if (!city.Troops.TryGetStub(stubId, out stub))
                        throw new Exception();

                    stub.City.Worker.References.Remove(stub.TroopObject, this);
                    stub.City.Worker.Notifications.Remove(this);
                    if (stub.TotalCount == 0)
                        Procedure.TroopObjectDelete(stub.TroopObject, false);
                    else
                        Procedure.TroopObjectDelete(stub.TroopObject, true);
                    StateChange(ActionState.Completed);
                }
            }
        }
    }
}