#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class CityBattlePassiveAction : ScheduledPassiveAction
    {
        private readonly IActionFactory actionFactory;

        private readonly BattleProcedure battleProcedure;

        private readonly uint cityId;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly CityBattleProcedure cityBattleProcedure;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly IWorld world;

        private uint destroyedHp;

        public CityBattlePassiveAction(uint cityId,
                                       IActionFactory actionFactory,
                                       BattleProcedure battleProcedure,
                                       ILocker locker,
                                       IGameObjectLocator gameObjectLocator,
                                       IDbManager dbManager,
                                       Formula formula,
                                       CityBattleProcedure cityBattleProcedure,
                                       IWorld world)
        {
            this.cityId = cityId;
            this.actionFactory = actionFactory;
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.cityBattleProcedure = cityBattleProcedure;
            this.world = world;

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception("Did not find city that was supposed to be having a battle");
            }

            city.Battle.EnterRound += BattleEnterRound;
            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.UnitKilled += BattleUnitKilled;
        }

        public CityBattlePassiveAction(uint id,
                                       DateTime beginTime,
                                       DateTime nextTime,
                                       DateTime endTime,
                                       bool isVisible,
                                       string nlsDescription,
                                       IDictionary<string, string> properties,
                                       IActionFactory actionFactory,
                                       BattleProcedure battleProcedure,
                                       ILocker locker,
                                       IGameObjectLocator gameObjectLocator,
                                       IDbManager dbManager,
                                       Formula formula,
                                       CityBattleProcedure cityBattleProcedure,
                                       IWorld world)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.actionFactory = actionFactory;
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.cityBattleProcedure = cityBattleProcedure;
            this.world = world;

            cityId = uint.Parse(properties["city_id"]);
            destroyedHp = uint.Parse(properties["destroyed_hp"]);

            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception();
            }

            city.Battle.EnterRound += BattleEnterRound;
            city.Battle.ActionAttacked += BattleActionAttacked;
            city.Battle.UnitKilled += BattleUnitKilled;
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.CityBattlePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {new XmlKvPair("city_id", cityId), new XmlKvPair("destroyed_hp", destroyedHp)});
            }
        }

        private void AddAlignmentPoint(ICombatList attackers, ICombatList defenders, uint numberOfRounds)
        {
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            // Subtract the "In Battle" formation of the local troop since that's already
            // included in our defenders
            decimal defUpkeep = defenders.Upkeep + city.Troops.Upkeep -
                                city.DefaultTroop.UpkeepForFormation(FormationType.InBattle);
            decimal atkUpkeep = attackers.Upkeep;

            if (atkUpkeep == 0 || atkUpkeep <= defUpkeep)
            {
                return;
            }

            decimal points =
                    Math.Min(defUpkeep == 0 ? Config.ap_max_per_battle : (atkUpkeep / defUpkeep - 1),
                             Config.ap_max_per_battle) * numberOfRounds / 20m;

            foreach (ITroopStub stub in
                    attackers.Where(p => p is CityOffensiveCombatGroup)
                             .Select(offensiveCombatGroup => ((CityOffensiveCombatGroup)offensiveCombatGroup).TroopObject.Stub))
            {
                stub.City.BeginUpdate();
                stub.City.AlignmentPoint -= stub.Upkeep / atkUpkeep * points;
                stub.City.EndUpdate();
            }

            city.BeginUpdate();
            city.AlignmentPoint += points;
            city.EndUpdate();
        }

        public void BattleEnterRound(IBattleManager battle, ICombatList attackers, ICombatList defenders, uint round)
        {
            AddAlignmentPoint(attackers, defenders, 1);
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
                    toBeLocked.Add(city);
                    toBeLocked.AddRange(city.Troops.StationedHere().Select(stub => stub.City).Distinct());
                    return toBeLocked.ToArray();
                };

            using (locker.Lock(lockHandler, null, city))
            {
                if (city.Battle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(city.Battle);
                    endTime =
                            SystemClock.Now.AddSeconds(
                                                       formula.GetBattleInterval(city.Battle.Defenders.Count +
                                                                                 city.Battle.Attackers.Count));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                city.Battle.ActionAttacked -= BattleActionAttacked;
                city.Battle.UnitKilled -= BattleUnitKilled;
                city.Battle.EnterRound -= BattleEnterRound;
                world.Remove(city.Battle);
                dbManager.Delete(city.Battle);
                city.Battle = null;

                // Move the default troop back into normal and clear its temporary battle stats
                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.Template.ClearStats();
                city.DefaultTroop.State = TroopState.Idle;
                cityBattleProcedure.MoveUnitFormation(city.DefaultTroop, FormationType.InBattle, FormationType.Normal);
                city.DefaultTroop.EndUpdate();

                // Get a COPY of the stubs that are stationed in the town since the loop below will modify city.Troops
                var stationedTroops =
                        city.Troops.StationedHere().Where(stub => stub.State == TroopState.BattleStationed).ToList();

                // Go through each stationed troop and either remove them from the city if they died
                // or set their states back to normal
                foreach (var stub in stationedTroops)
                {
                    // Check if stub has died, if so, remove it completely from both cities
                    if (stub.TotalCount == 0)
                    {
                        city.Troops.RemoveStationed(stub.StationTroopId);
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
                    battleProcedure.SenseOfUrgency(city, destroyedHp);
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

            world.Add(city.Battle);
            dbManager.Save(city.Battle);

            //Add local troop
            cityBattleProcedure.AddLocalUnitsToBattle(city.Battle, city);

            //Add reinforcement
            foreach (
                    var stub in
                            city.Troops.Where(
                                              stub =>
                                              stub != city.DefaultTroop && stub.State == TroopState.Stationed &&
                                              stub.Station == city))
            {
                stub.BeginUpdate();
                stub.State = TroopState.BattleStationed;
                stub.EndUpdate();

                battleProcedure.AddReinforcementToBattle(city.Battle, stub, FormationType.Defense);
            }

            beginTime = SystemClock.Now;
            endTime = SystemClock.Now;

            return Error.Ok;
        }

        private void BattleActionAttacked(IBattleManager battleManager,
                                          BattleManager.BattleSide attackingSide,
                                          ICombatGroup attackerGroup,
                                          ICombatObject source,
                                          ICombatGroup targetGroup,
                                          ICombatObject target,
                                          decimal damage)
        {
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                return;
            }

            var cityCombatTarget = target as CityCombatObject;

            // Check if we should retreat a unit
            if (cityCombatTarget == null || attackingSide == BattleManager.BattleSide.Defense ||
                cityCombatTarget.TroopStub.Station != city || cityCombatTarget.TroopStub.TotalCount <= 0 ||
                cityCombatTarget.TroopStub.TotalCount > cityCombatTarget.TroopStub.RetreatCount)
            {
                return;
            }

            ITroopStub stub = cityCombatTarget.TroopStub;

            // Remove the object from the battle
            city.Battle.Remove(targetGroup, BattleManager.BattleSide.Defense, ReportState.Retreating);
            stub.BeginUpdate();
            stub.State = TroopState.Stationed;
            stub.EndUpdate();

            // Send the defender back to their city
            var retreatChainAction = actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId);
            var result = stub.City.Worker.DoPassive(stub.City, retreatChainAction, true);
            if (result != Error.Ok)
            {
                throw new Exception("Unexpected failure when retreating a unit from city");
            }
        }

        /// <summary>
        ///     Gives alignment points when a structure is destroyed.
        /// </summary>
        private void BattleUnitKilled(IBattleManager battle,
                                      BattleManager.BattleSide objSide,
                                      ICombatGroup combatGroup,
                                      ICombatObject obj)
        {
            // Keep track of our buildings destroyed HP
            if (objSide != BattleManager.BattleSide.Defense || obj.ClassType != BattleClass.Structure)
            {
                return;
            }

            destroyedHp += (uint)obj.Stats.MaxHp;
            AddAlignmentPoint(battle.Attackers, battle.Defenders, Config.battle_stamina_destroyed_deduction);
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