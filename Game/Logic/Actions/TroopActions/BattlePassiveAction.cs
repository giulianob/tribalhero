#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class BattlePassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;

        private readonly IActionFactory actionFactory;

        private readonly Procedure procedure;

        private readonly ILocker locker;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private uint destroyedHp;

        public BattlePassiveAction(uint cityId, IActionFactory actionFactory, Procedure procedure, ILocker locker, IGameObjectLocator gameObjectLocator, IDbManager dbManager, Formula formula)
        {
            this.cityId = cityId;
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception("Did not find city that was supposed to be having a battle");
            }

            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.UnitRemoved += BattleUnitRemoved;
        }

        public BattlePassiveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, string nlsDescription, IDictionary<string, string> properties, IActionFactory actionFactory, Procedure procedure, ILocker locker, IGameObjectLocator gameObjectLocator, IDbManager dbManager, Formula formula)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.actionFactory = actionFactory;
            this.procedure = procedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;

            cityId = uint.Parse(properties["city_id"]);
            destroyedHp = uint.Parse(properties["destroyed_hp"]);

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception();
            }

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
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var toBeLocked = new List<ILockable>();
                    toBeLocked.AddRange(city.Battle.LockList);
                    toBeLocked.AddRange(city.Troops.StationedHere().Select(stub => stub.City));
                    return toBeLocked.ToArray();
                };

            using (locker.Lock(lockHandler, null, city))
            {
                if (city.Battle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(city.Battle);
                    endTime = SystemClock.Now.AddSeconds(formula.GetBattleInterval(city.Battle.Defender.Count + city.Battle.Attacker.Count));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                city.Battle.ActionAttacked -= BattleActionAttacked;
                city.Battle.UnitRemoved -= BattleUnitRemoved;
                dbManager.Delete(city.Battle);
                city.Battle = null;

                // Move the default troop back into normal and clear its temporary battle stats
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.Template.ClearStats();
                city.DefaultTroop.State = TroopState.Idle;
                procedure.MoveUnitFormation(city.DefaultTroop, FormationType.InBattle, FormationType.Normal);
                city.DefaultTroop.EndUpdate();

                // Get a COPY of the stubs that are stationed in the town since the loop below will modify city.Troops
                var stationedTroops = city.Troops.StationedHere().Where(stub => stub.State == TroopState.BattleStationed).ToList();

                // Go through each stationed troop and either remove them from the city if they died
                // or set their states back to normal
                foreach (var stub in stationedTroops)
                {
                    // Check if stub has died, if so, remove it completely from both cities
                    if (stub.TotalCount == 0)
                    {
                        city.Troops.RemoveStationed(stub.StationedTroopId);
                        stub.City.Troops.Remove(stub.TroopId);
                        continue;
                    }

                    // Set the stationed stub back to just TroopState.Stationed
                    stub.BeginUpdate();
                    stub.State = TroopState.Stationed;
                    stub.EndUpdate();
                }

                // Handle SenseOfUrgency technology
                if (destroyedHp > 0)
                {
                    procedure.SenseOfUrgency(city, destroyedHp);
                }

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            dbManager.Save(city.Battle);

            //Add local troop
            procedure.AddLocalToBattle(city.Battle, city, ReportState.Entering);

            var list = new List<ITroopStub>();

            //Add reinforcement
            foreach (var stub in city.Troops.Where(stub => stub != city.DefaultTroop && stub.State == TroopState.Stationed && stub.StationedCity == city))
            {
                stub.BeginUpdate();
                stub.State = TroopState.BattleStationed;
                stub.EndUpdate();

                list.Add(stub);
            }

            city.Battle.AddToDefense(list);
            beginTime = SystemClock.Now;
            endTime = SystemClock.Now;

            return Error.Ok;
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, decimal damage)
        {
            var combatUnit = target as ICombatUnit;

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                return;
            }

            // Handle sending stationed troops back if they are below the set threshold
            if (combatUnit != null && !combatUnit.IsAttacker && target.TroopStub.StationedCity == city && target.TroopStub.TotalCount > 0 && target.TroopStub.TotalCount <= target.TroopStub.StationedRetreatCount)
            {
                ITroopStub stub = combatUnit.TroopStub;

                // Remove the object from the battle
                city.Battle.RemoveFromDefense(new List<ITroopStub> {combatUnit.TroopStub}, ReportState.Retreating);                
                stub.BeginUpdate();
                stub.State = TroopState.Stationed;
                stub.EndUpdate();

                // Send the defender back to their city but restation them if there's a problem
                var retreatChainAction = actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId);
                stub.City.Worker.DoPassive(stub.City, retreatChainAction, true);
            }
        }

        private void BattleUnitRemoved(CombatObject obj) {
            // Keep track of our buildings destroyed HP
            if (obj.ClassType == BattleClass.Structure && obj.City.Id == cityId) {
                destroyedHp += (uint)obj.Stats.MaxHp;
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