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

        private readonly ITroopObjectInitializer troopObjectInitializer;

        private uint troopObjectId;

        private readonly IWorld world;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        public RetreatChainAction(uint cityId,
                                  ITroopObjectInitializer troopObjectInitializer,
                                  IActionFactory actionFactory,
                                  IWorld world,
                                  Procedure procedure,
                                  ILocker locker)
        {
            this.cityId = cityId;
            this.troopObjectInitializer = troopObjectInitializer;
            this.actionFactory = actionFactory;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
        }

        public RetreatChainAction(uint id,
                                  string chainCallback,
                                  PassiveAction current,
                                  ActionState chainState,
                                  bool isVisible,
                                  Dictionary<string, string> properties,
                                  IActionFactory actionFactory,
                                  IWorld world,
                                  Procedure procedure,
                                  ILocker locker)
                : base(id, chainCallback, current, chainState, isVisible)
        {
            this.actionFactory = actionFactory;
            this.world = world;
            this.procedure = procedure;
            this.locker = locker;
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
            if (!world.TryGetObjects(cityId, out city))
            {
                throw new Exception();
            }

            ITroopObject troopObject;
            var troopInitializeResult = troopObjectInitializer.GetTroopObject(out troopObject);
            if (troopInitializeResult != Error.Ok)
            {
                return troopInitializeResult;
            }

            troopObjectId = troopObject.ObjectId;

            var tma = actionFactory.CreateTroopMovePassiveAction(cityId, troopObject.ObjectId, troopObject.Stub.City.X, troopObject.Stub.City.Y, true, false);

            ExecuteChainAndWait(tma, AfterTroopMoved);

            troopObject.Stub.City.References.Add(troopObject, this);
            troopObject.Stub.City.Notifications.Add(troopObject, this);

            return Error.Ok;
        }

        private void AfterTroopMoved(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                using (locker.Lock(cityId, out city))
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
                        procedure.TroopObjectDelete(troopObject, true);
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
                using (locker.Lock(cityId, out city))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception();
                    }

                    procedure.TroopObjectStation(troopObject, city);
                }
            }
        }

        private void AfterEngageDefense(ActionState state)
        {
            if (state == ActionState.Completed)
            {
                ICity city;
                using (locker.Lock(cityId, out city))
                {
                    ITroopObject troopObject;
                    if (!city.TryGetTroop(troopObjectId, out troopObject))
                    {
                        throw new Exception();
                    }

                    city.References.Remove(troopObject, this);
                    procedure.TroopObjectDelete(troopObject, troopObject.Stub.TotalCount != 0);
                    StateChange(ActionState.Completed);
                }
            }
        }
    }
}