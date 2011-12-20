#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class EngageDefensePassiveAction : PassiveAction
    {
        private readonly uint cityId;
        private readonly byte stubId;
        private decimal originalHp;
        private decimal remainingHp;

        public EngageDefensePassiveAction(uint cityId, byte stubId)
        {
            this.cityId = cityId;
            this.stubId = stubId;
        }

        public EngageDefensePassiveAction(uint id, bool isVisible, IDictionary<string, string> properties) : base(id, isVisible)
        {
            cityId = uint.Parse(properties["troop_city_id"]);
            stubId = byte.Parse(properties["troop_id"]);
            originalHp = decimal.Parse(properties["original_hp"]);
            remainingHp = decimal.Parse(properties["remaining_hp"]);

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
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
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
                return Error.ObjectNotFound;

            if (city.Battle == null)
            {
                StateChange(ActionState.Completed);
                return Error.Ok;
            }

            var list = new List<TroopStub> {stub};
            originalHp = remainingHp = stub.TotalHp;

            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.ExitBattle += BattleExitBattle;
            city.Battle.AddToDefense(list);

            stub.TroopObject.BeginUpdate();
            stub.TroopObject.State = GameObjectState.BattleState(cityId);
            stub.TroopObject.EndUpdate();
            stub.TroopObject.Stub.BeginUpdate();
            stub.TroopObject.Stub.State = TroopState.Battle;
            stub.TroopObject.Stub.EndUpdate();

            // Add any units in local troop to battle
            Procedure.AddLocalToBattle(city.Battle, city, ReportState.Reinforced);

            return Error.Ok;
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, decimal damage)
        {
            if (target.City.Id != cityId)
                return;

            var unit = target as AttackCombatUnit;
            if (unit == null)
                return;

            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
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

        private void BattleExitBattle(CombatList atk, CombatList def)
        {
            City city;
            TroopStub stub;
            if (!Global.World.TryGetObjects(cityId, stubId, out city, out stub))
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