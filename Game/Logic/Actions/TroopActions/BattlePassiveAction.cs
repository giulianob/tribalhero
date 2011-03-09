#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class BattlePassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;
        private uint destroyedHp;

        public BattlePassiveAction(uint cityId)
        {
            this.cityId = cityId;

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.UnitRemoved += BattleUnitRemoved;
        }

        public BattlePassiveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, string nlsDescription, IDictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            cityId = uint.Parse(properties["city_id"]);
            destroyedHp = uint.Parse(properties["destroyed_hp"]);

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception();

            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.UnitRemoved += BattleUnitRemoved;
        }



        public override ActionType Type
        {
            get
            {
                return ActionType.BattlePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] { new XmlKvPair("city_id", cityId), new XmlKvPair("destroyed_hp", destroyedHp) });
            }
        }	

        public override void Callback(object custom)
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City is missing");

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var toBeLocked = new List<ILockable>();
                    toBeLocked.AddRange(city.Battle.LockList);
                    toBeLocked.AddRange(city.Troops.StationedHere().Select(stub => stub.City).Cast<ILockable>());
                    return toBeLocked.ToArray();
                };

            using (new CallbackLock(lockHandler, null, city))
            {
                if (!city.Battle.ExecuteTurn())
                {
                    city.Battle.ActionAttacked -= BattleActionAttacked;
                    city.Battle.UnitRemoved -= BattleUnitRemoved;
                    Global.DbManager.Delete(city.Battle);
                    city.Battle = null;

                    city.DefaultTroop.BeginUpdate();
                    city.DefaultTroop.Template.ClearStats();
                    Procedure.MoveUnitFormation(city.DefaultTroop, FormationType.InBattle, FormationType.Normal);
                    city.DefaultTroop.EndUpdate();

                    //Copy troop stubs from city since the foreach loop below will modify it during the loop
                    var stubsCopy = new List<TroopStub>(city.Troops);

                    foreach (var stub in stubsCopy)
                    {
                        //only set back the state to the local troop or the ones stationed in this city
                        if (stub != city.DefaultTroop && stub.StationedCity != city)
                            continue;

                        if (stub.StationedCity == city && stub.TotalCount == 0)
                        {
                            city.Troops.RemoveStationed(stub.StationedTroopId);
                            stub.City.Troops.Remove(stub.TroopId);
                            continue;
                        }

                        stub.BeginUpdate();
                        switch(stub.State)
                        {
                            case TroopState.BattleStationed:
                                stub.State = TroopState.Stationed;
                                break;
                            case TroopState.Battle:
                                stub.State = TroopState.Idle;
                                break;
                        }
                        stub.EndUpdate();
                    }
					
                    if(destroyedHp>0)                    
                        Procedure.SenseOfUrgency(city, destroyedHp);                    
					
                    StateChange(ActionState.Completed);
                }
                else
                {
                    Global.DbManager.Save(city.Battle);
                    endTime = DateTime.UtcNow.AddSeconds(Config.battle_turn_interval);
                    StateChange(ActionState.Fired);
                }
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            Global.DbManager.Save(city.Battle);

            //Add local troop
            Procedure.AddLocalToBattle(city.Battle, city, ReportState.Entering);

            var list = new List<TroopStub>();

            //Add reinforcement
            foreach (var stub in city.Troops)
            {
                if (stub == city.DefaultTroop || stub.State != TroopState.Stationed || stub.StationedCity != city)
                    continue; //skip if troop is the default troop or isn't stationed here

                stub.BeginUpdate();
                stub.State = TroopState.BattleStationed;
                stub.EndUpdate();

                list.Add(stub);
            }

            city.Battle.AddToDefense(list);
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(Config.battle_turn_interval);

            return Error.Ok;
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {
            var cu = target as DefenseCombatUnit;

            if (cu == null)
                return;

            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return;

            if (cu.TroopStub.StationedCity == city && cu.TroopStub.TotalCount == 0)
            {
                //takes care of killing out stationed troops
                var list = new List<TroopStub>(1) {cu.TroopStub};
                city.Battle.RemoveFromDefense(list, ReportState.Dying);
            }
        }

        private void BattleUnitRemoved(CombatObject obj) {
            // Keep track of our buildings destroyed HP
            if (obj.ClassType == BattleClass.Structure && obj.City.Id == cityId) {
                destroyedHp += obj.Stats.MaxHp;
            }
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new Exception("City removed during battle?");
        }

    }
}