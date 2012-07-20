#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    public class EngageDefensePassiveAction : PassiveAction
    {
        private readonly uint cityId;
        private readonly byte stubId;

        private readonly BattleProcedure battleProcedure;

        private decimal originalHp;
        private decimal remainingHp;

        public EngageDefensePassiveAction(uint cityId, byte stubId, BattleProcedure battleProcedure)
        {
            this.cityId = cityId;
            this.stubId = stubId;
            this.battleProcedure = battleProcedure;
        }

        public EngageDefensePassiveAction(uint id, bool isVisible, IDictionary<string, string> properties, BattleProcedure battleProcedure) : base(id, isVisible)
        {
            this.battleProcedure = battleProcedure;
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);
            originalHp = decimal.Parse(properties["original_hp"]);
            remainingHp = decimal.Parse(properties["remaining_hp"]);

            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.ExitBattle += BattleExitBattle;
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.EngageDefensePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                                                {
                                                        new XmlKvPair("troop_city_id", cityId), new XmlKvPair("troop_id", stubId),
                                                        new XmlKvPair("original_hp", originalHp), new XmlKvPair("remaining_hp", remainingHp)
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
                return Error.ObjectNotFound;

            if (city.Battle == null)
            {
                StateChange(ActionState.Completed);
                return Error.Ok;
            }

            originalHp = remainingHp = stub.TotalHp;

            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.ExitBattle += BattleExitBattle;            

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(cityId);
            stub.TroopObject.EndUpdate();
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopState.Battle;
            stub.TroopObject.Stub.EndUpdate();

            // Add units to battle
            battleProcedure.AddReinforcementToBattle(city.Battle, stub);
            battleProcedure.AddLocalUnitsToBattle(city.Battle, city);

            return Error.Ok;
        }

        private void BattleActionAttacked(IBattleManager battle, CombatObject source, CombatObject target, decimal damage)
        {
            var unit = target as AttackCombatUnit;

            if (unit == null || unit.City.Id != cityId)
            {
                return;
            }

            ICity city;
            ITroopStub stub;
            if (!World.Current.TryGetObjects(cityId, stubId, out city, out stub))
                throw new Exception();

            if (unit.TroopStub != stub)
                return;

            remainingHp -= damage;
            if (remainingHp > 0)
                return;

            city.Battle.ActionAttacked -= BattleActionAttacked;
            city.Battle.ExitBattle -= BattleExitBattle;

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.TroopObject.Stub.State = TroopState.Idle;
            stub.TroopObject.EndUpdate();
            StateChange(ActionState.Completed);
        }

        private void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            ICity city;
            ITroopStub stub;
            if (!World.Current.TryGetObjects(cityId, stubId, out city, out stub))
                throw new Exception();

            city.Battle.ActionAttacked -= BattleActionAttacked;
            city.Battle.ExitBattle -= BattleExitBattle;

            stub.TroopObject.BeginUpdate();
            stub.BeginUpdate();
            stub.TroopObject.State = GameObjectState.NormalState();
            stub.State = TroopState.Idle;
            stub.EndUpdate();
            stub.TroopObject.EndUpdate();

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