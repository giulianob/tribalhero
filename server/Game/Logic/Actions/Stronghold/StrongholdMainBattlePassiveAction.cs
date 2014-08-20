#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Data.Stronghold;
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
    public class StrongholdMainBattlePassiveAction : ScheduledPassiveAction
    {
        private readonly IActionFactory actionFactory;

        private readonly BattleProcedure battleProcedure;

        private readonly StrongholdBattleProcedure strongholdBattleProcedure;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private uint strongholdId;

        private readonly IStrongholdManager strongholdManager;

        private readonly IWorld world;

        private uint npcGroupId;

        private bool npcGroupKilled;

        private readonly ITroopObjectInitializerFactory troopInitializerFactory;

        public StrongholdMainBattlePassiveAction(BattleProcedure battleProcedure,
                                                 StrongholdBattleProcedure strongholdBattleProcedure,
                                                 ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 IWorld world,
                                                 IStrongholdManager strongholdManager,
                                                 IActionFactory actionFactory,
                                                 ITroopObjectInitializerFactory troopInitializerFactory)
        {
            this.battleProcedure = battleProcedure;
            this.strongholdBattleProcedure = strongholdBattleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.world = world;
            this.strongholdManager = strongholdManager;
            this.actionFactory = actionFactory;
            this.troopInitializerFactory = troopInitializerFactory;
        }

        public StrongholdMainBattlePassiveAction(uint strongholdId,
                                                 BattleProcedure battleProcedure,
                                                 StrongholdBattleProcedure strongholdBattleProcedure,
                                                 ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 IWorld world,
                                                 IStrongholdManager strongholdManager,
                                                 IActionFactory actionFactory,
                                                 ITroopObjectInitializerFactory troopInitializerFactory) 
            : this(battleProcedure, strongholdBattleProcedure, locker, gameObjectLocator, dbManager, formula, world, strongholdManager, actionFactory, troopInitializerFactory)
        {
            this.strongholdId = strongholdId;

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Did not find stronghold that was supposed to be having a battle");
            }

            RegisterBattleListeners(stronghold);
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            strongholdId = uint.Parse(properties["stronghold_id"]);
            npcGroupId = uint.Parse(properties["npc_group_id"]);
            npcGroupKilled = bool.Parse(properties["npc_group_killed"]);

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception();
            }

            RegisterBattleListeners(stronghold);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StrongholdMainBattlePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                            new XmlKvPair("stronghold_id", strongholdId), new XmlKvPair("npc_group_id", npcGroupId),
                            new XmlKvPair("npc_group_killed", npcGroupKilled)
                        });
            }
        }

        public override Error SystemCancelable
        {
            get
            {
                return Error.UncancelableStrongholdMainBattle;
            }
        }

        private void RegisterBattleListeners(IStronghold stronghold)
        {
            stronghold.MainBattle.UnitCountDecreased += MainBattleOnUnitKilled;
            stronghold.MainBattle.GroupKilled += MainBattleOnGroupKilled;
            stronghold.MainBattle.AboutToExitBattle += MainBattleOnAboutToExitBattle;
            stronghold.MainBattle.ActionAttacked += MainBattleOnActionAttacked;
            stronghold.MainBattle.ExitTurn += MainBattleOnExitTurn;
            stronghold.MainBattle.EnterBattle += MainBattleOnEnterBattle;
        }

        private void MainBattleOnEnterBattle(IBattleManager battle, ICombatList attackers, ICombatList defenders)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            battle.BattleReport.AddAccess(new BattleOwner(BattleOwnerType.Tribe, stronghold.GateOpenTo.Id),
                                          BattleManager.BattleSide.Attack);
        }

        private void MainBattleOnExitTurn(IBattleManager battle, ICombatList attackers, ICombatList defenders, uint turn)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            var defensiveMeter = battle.GetProperty<decimal>("defense_stronghold_meter");
            var offensiveMeter = battle.GetProperty<decimal>("offense_stronghold_meter");

            // Make copy since defenders may change
            var defendersLoopCopy = defenders.ToList();
            // Remove defenders if:
            //  defensive meter is 0
            //  stronghold is still neutral (no tribe)
            //  the defender isnt part of the tribe that owns the stronghold
            foreach (var defender in defendersLoopCopy)
            {
                var cityCombatGroup = defender as CityDefensiveCombatGroup;

                // If cityCombatGroup is null then we're dealing w/ a NPC unit
                if (cityCombatGroup == null)
                {
                    if (defensiveMeter > 0)
                    {
                        continue;
                    }

                    battle.Remove(defender, BattleManager.BattleSide.Defense, ReportState.OutOfStamina);
                }
                        // Else we're dealing w/ a player unit
                else
                {
                    if (defensiveMeter > 0 && stronghold.Tribe != null && defender.Tribe == stronghold.Tribe)
                    {
                        continue;
                    }

                    battle.Remove(cityCombatGroup, BattleManager.BattleSide.Defense, ReportState.OutOfStamina);

                    // Dead troops should just be removed immediately
                    if (cityCombatGroup.TroopStub.TotalCount == 0)
                    {
                        stronghold.Troops.RemoveStationed(cityCombatGroup.TroopStub.StationTroopId);
                        cityCombatGroup.TroopStub.City.Troops.Remove(cityCombatGroup.TroopStub.TroopId);
                        continue;
                    }

                    // Defenders need to manually be sent back
                    cityCombatGroup.TroopStub.BeginUpdate();
                    cityCombatGroup.TroopStub.State = TroopState.Stationed;
                    cityCombatGroup.TroopStub.EndUpdate();

                    var troopInitializer = troopInitializerFactory.CreateStationedTroopObjectInitializer(cityCombatGroup.TroopStub);
                    var retreatChainAction = actionFactory.CreateRetreatChainAction(cityCombatGroup.TroopStub.City.Id, troopInitializer);
                    var result = cityCombatGroup.TroopStub.City.Worker.DoPassive(cityCombatGroup.TroopStub.City, retreatChainAction, true);
                    if (result != Error.Ok)
                    {
                        throw new Exception("Unexpected failure when retreating a unit from stronghold");
                    }
                }
            }

            // Remove attackers that have quit the tribe or have low meter
            // Make copy since attackers will be changing
            var attackerLoopCopy = attackers.ToList();
            foreach (var attacker in attackerLoopCopy.Where(attacker => offensiveMeter <= 0 || attacker.Tribe != stronghold.GateOpenTo))
            {
                // Remove from battle, no need to send them back since attacking troops have actions to handle that
                battle.Remove(attacker, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
            }
        }

        private void MainBattleOnActionAttacked(IBattleManager battle,
                                                BattleManager.BattleSide attackingSide,
                                                ICombatGroup attackerGroup,
                                                ICombatObject attacker,
                                                ICombatGroup targetGroup,
                                                ICombatObject target,
                                                decimal damage,
                                                int attackerCount,
                                                int targetCount)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            var cityCombatTarget = target as CityCombatObject;

            // Check if we should retreat a unit
            if (cityCombatTarget == null || attackingSide == BattleManager.BattleSide.Defense ||
                cityCombatTarget.TroopStub.Station != stronghold || cityCombatTarget.TroopStub.TotalCount == 0 ||
                cityCombatTarget.TroopStub.TotalCount > cityCombatTarget.TroopStub.RetreatCount)
            {
                return;
            }

            ITroopStub stub = cityCombatTarget.TroopStub;

            // Remove the object from the battle
            stronghold.MainBattle.Remove(targetGroup, BattleManager.BattleSide.Defense, ReportState.Retreating);
            stub.BeginUpdate();
            stub.State = TroopState.Stationed;
            stub.EndUpdate();

            // Send the defender back to their city
            var troopInitializer = troopInitializerFactory.CreateStationedTroopObjectInitializer(stub);
            var retreatChainAction = actionFactory.CreateRetreatChainAction(stub.City.Id, troopInitializer);
            var result = stub.City.Worker.DoPassive(stub.City, retreatChainAction, true);
            if (result != Error.Ok)
            {
                throw new Exception("Unexpected failure when retreating a unit from stronghold");
            }
        }

        private void MainBattleOnAboutToExitBattle(IBattleManager battle, ICombatList attackers, ICombatList defenders)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            var defensiveMeter = battle.GetProperty<decimal>("defense_stronghold_meter");
            // Transfer stronghold if 
            // - defensive meter is 0
            // - occupied state and there is no one left defending it
            // - neutral state and the attacker killed the main group

            var hasDefendingUnitsLeft = stronghold.Troops.StationedHere().Any(p => p.TotalCount > 0);

            if (defensiveMeter <= 0 ||
                (stronghold.StrongholdState == StrongholdState.Occupied && !hasDefendingUnitsLeft) ||
                (stronghold.StrongholdState == StrongholdState.Neutral && npcGroupKilled))
            {
                strongholdManager.TransferTo(stronghold, stronghold.GateOpenTo);
            }
            else
            {
                strongholdManager.TribeFailedToTakeStronghold(stronghold, stronghold.GateOpenTo);
            }
        }

        private void MainBattleOnGroupKilled(IBattleManager battle, ICombatGroup group)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            if (stronghold.StrongholdState == StrongholdState.Neutral && group.Id == npcGroupId)
            {
                npcGroupKilled = true;
            }

            // Take care of clearing any dead stationed troops while in the middle of the battle
            foreach (var stub in stronghold.Troops.StationedHere().Where(stub => stub.TotalCount == 0).ToList())
            {
                stronghold.Troops.RemoveStationed(stub.StationTroopId);
                stub.City.Troops.Remove(stub.TroopId);
            }
        }

        private void MainBattleOnUnitKilled(IBattleManager battle,
                                            BattleManager.BattleSide combatObjectSide,
                                            ICombatGroup combatGroup,
                                            ICombatObject combatObject,
                                            int count)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            var defensiveMeter = battle.GetProperty<decimal>("defense_stronghold_meter");
            var offensiveMeter = battle.GetProperty<decimal>("offense_stronghold_meter");

            if (combatObjectSide == BattleManager.BattleSide.Attack)
            {
                offensiveMeter -= combatObject.Stats.NormalizedCost * count;
            }
            else
            {
                defensiveMeter -= combatObject.Stats.NormalizedCost * count;
            }

            battle.SetProperty("defense_stronghold_meter", defensiveMeter);
            battle.SetProperty("offense_stronghold_meter", offensiveMeter);
        }

        public override void Callback(object custom)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold is missing");
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate { return stronghold.LockList().ToArray(); };

            locker.Lock(lockHandler, null, stronghold).Do(() =>
            {
                if (stronghold.MainBattle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(stronghold.MainBattle);
                    endTime = SystemClock.Now.AddSeconds(formula.GetBattleInterval(stronghold.MainBattle.Defenders, stronghold.MainBattle.Attackers));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                stronghold.MainBattle.UnitCountDecreased -= MainBattleOnUnitKilled;
                stronghold.MainBattle.GroupKilled -= MainBattleOnGroupKilled;
                stronghold.MainBattle.ActionAttacked -= MainBattleOnActionAttacked;
                stronghold.MainBattle.ExitTurn -= MainBattleOnExitTurn;
                stronghold.MainBattle.EnterBattle -= MainBattleOnEnterBattle;

                // Set troop states to stationed and 
                // send back anyone stationed here that doesn't belong
                // Make copy because it may change
                var stationedHere = stronghold.Troops.StationedHere().ToList();
                foreach (var stub in stationedHere)
                {
                    stub.BeginUpdate();
                    stub.State = TroopState.Stationed;
                    stub.EndUpdate();

                    if (stub.City.Owner.IsInTribe && stub.City.Owner.Tribesman.Tribe == stronghold.Tribe)
                    {
                        continue;
                    }

                    var troopInitializer = troopInitializerFactory.CreateStationedTroopObjectInitializer(stub);
                    var retreatChainAction = actionFactory.CreateRetreatChainAction(stub.City.Id, troopInitializer);
                    var result = stub.City.Worker.DoPassive(stub.City, retreatChainAction, true);
                    if (result != Error.Ok)
                    {
                        throw new Exception("Unexpected failure when retreating a unit from stronghold");
                    }
                }

                world.Remove(stronghold.MainBattle);
                dbManager.Delete(stronghold.MainBattle);
                stronghold.BeginUpdate();
                stronghold.GateOpenTo = null;
                stronghold.MainBattle = null;
                stronghold.GateMax = (int)formula.StrongholdGateLimit(stronghold.Lvl);
                stronghold.Gate = Math.Max(Math.Min(stronghold.GateMax, stronghold.Gate), formula.StrongholdGateHealHp(stronghold.StrongholdState, stronghold.Lvl));
                stronghold.State = GameObjectStateFactory.NormalState();
                stronghold.EndUpdate();

                StateChange(ActionState.Completed);
            });
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                return Error.ObjectNotFound;
            }

            world.Add(stronghold.MainBattle);
            dbManager.Save(stronghold.MainBattle);

            if (stronghold.StrongholdState == StrongholdState.Occupied)
            {
                // Add stationed to battle
                foreach (var stub in stronghold.Troops.StationedHere())
                {
                    stub.BeginUpdate();
                    stub.State = TroopState.BattleStationed;
                    stub.EndUpdate();

                    battleProcedure.AddReinforcementToBattle(stronghold.MainBattle, stub, FormationType.Defense);
                }
            }
            else
            {
                var strongholdGroup = strongholdBattleProcedure.AddStrongholdUnitsToBattle(stronghold.MainBattle,
                                                                                           stronghold,
                                                                                           strongholdManager.GenerateNeutralStub(stronghold));

                npcGroupId = strongholdGroup.Id;
            }

            stronghold.BeginUpdate();
            stronghold.State = GameObjectStateFactory.BattleState(stronghold.MainBattle.BattleId);
            stronghold.EndUpdate();

            beginTime = SystemClock.Now;
            endTime = SystemClock.Now.Add(formula.GetBattleDelayStartInterval());

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new Exception("Stronghold removed during battle?");
        }
    }
}