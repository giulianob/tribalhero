#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    public class CityEngageDefensePassiveAction : PassiveAction
    {
        private readonly BattleProcedure battleProcedure;

        private readonly CityBattleProcedure cityBattleProcedure;

        private readonly uint cityId;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly uint troopObjectId;

        private readonly FormationType formationType;

        private uint groupId;

        public CityEngageDefensePassiveAction(uint cityId,
                                              uint troopObjectId,
                                              FormationType formationType,
                                              BattleProcedure battleProcedure,
                                              CityBattleProcedure cityBattleProcedure,
                                              IGameObjectLocator gameObjectLocator)
        {
            this.cityId = cityId;
            this.troopObjectId = troopObjectId;
            this.formationType = formationType;
            this.battleProcedure = battleProcedure;
            this.cityBattleProcedure = cityBattleProcedure;
            this.gameObjectLocator = gameObjectLocator;
        }

        public CityEngageDefensePassiveAction(uint id,
                                              bool isVisible,
                                              IDictionary<string, string> properties,
                                              BattleProcedure battleProcedure,
                                              CityBattleProcedure cityBattleProcedure,
                                              IGameObjectLocator gameObjectLocator)
                : base(id, isVisible)
        {
            this.battleProcedure = battleProcedure;
            this.cityBattleProcedure = cityBattleProcedure;
            this.gameObjectLocator = gameObjectLocator;
            cityId = uint.Parse(properties["troop_city_id"]);
            groupId = uint.Parse(properties["group_id"]);
            troopObjectId = uint.Parse(properties["troop_object_id"]);

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception();
            }

            RegisterBattleListeners(city);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.CityEngageDefensePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("troop_city_id", cityId), new XmlKvPair("troop_object_id", troopObjectId),
                                new XmlKvPair("group_id", groupId)
                        });
            }
        }

        private void RegisterBattleListeners(ICity city)
        {
            city.Battle.GroupKilled += BattleGroupKilled;
            city.Battle.ExitBattle += BattleExitBattle;
        }

        private void DeregisterBattleListeners(ICity city)
        {
            city.Battle.GroupKilled -= BattleGroupKilled;
            city.Battle.ExitBattle -= BattleExitBattle;
        }

        /// <summary>
        ///     Handles ending this action if our group has been killed
        /// </summary>
        private void BattleGroupKilled(IBattleManager battle,
                                       ICombatGroup combatGroup)
        {
            if (combatGroup.Id != groupId)
            {
                return;
            }

            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
            {
                throw new Exception();
            }

            DeregisterBattleListeners(city);

            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.NormalState();
            troopObject.EndUpdate();

            troopObject.Stub.BeginUpdate();            
            troopObject.Stub.State = TroopState.Idle;
            troopObject.Stub.EndUpdate();
            
            StateChange(ActionState.Completed);
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;

            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
            {
                return Error.ObjectNotFound;
            }

            if (city.Battle == null)
            {
                StateChange(ActionState.Completed);
                return Error.Ok;
            }

            RegisterBattleListeners(city);

            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.BattleState(city.Battle.BattleId);
            troopObject.EndUpdate();
            troopObject.Stub.BeginUpdate();
            troopObject.Stub.State = TroopState.Battle;
            troopObject.Stub.EndUpdate();

            // Add units to battle
            groupId = battleProcedure.AddReinforcementToBattle(city.Battle, troopObject.Stub, formationType);
            cityBattleProcedure.AddLocalUnitsToBattle(city.Battle, city);

            return Error.Ok;
        }

        private void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            ICity city;
            ITroopObject troopObject;
            if (!gameObjectLocator.TryGetObjects(cityId, troopObjectId, out city, out troopObject))
            {
                throw new Exception();
            }

            DeregisterBattleListeners(city);

            troopObject.BeginUpdate();
            troopObject.State = GameObjectState.NormalState();
            troopObject.EndUpdate();

            troopObject.Stub.BeginUpdate();            
            troopObject.Stub.State = TroopState.Idle;
            troopObject.Stub.EndUpdate();

            StateChange(ActionState.Completed);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }

        public override void UserCancelled()
        {
        }
    }
}