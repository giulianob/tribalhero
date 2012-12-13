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

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly uint strongholdId;

        private readonly IStrongholdManager strongholdManager;

        private readonly IWorld world;

        private uint npcGroupId;

        private bool npcGroupKilled;

        public StrongholdMainBattlePassiveAction(uint strongholdId,
                                                 BattleProcedure battleProcedure,
                                                 ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 IWorld world,
                                                 IStrongholdManager strongholdManager,
                                                 IActionFactory actionFactory)
        {
            this.strongholdId = strongholdId;
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.world = world;
            this.strongholdManager = strongholdManager;
            this.actionFactory = actionFactory;

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Did not find stronghold that was supposed to be having a battle");
            }

            RegisterBattleListeners(stronghold);
        }

        public StrongholdMainBattlePassiveAction(uint id,
                                                 DateTime beginTime,
                                                 DateTime nextTime,
                                                 DateTime endTime,
                                                 bool isVisible,
                                                 string nlsDescription,
                                                 IDictionary<string, string> properties,
                                                 BattleProcedure battleProcedure,
                                                 ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 IWorld world,
                                                 IStrongholdManager strongholdManager,
                                                 IActionFactory actionFactory)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.world = world;
            this.strongholdManager = strongholdManager;
            this.actionFactory = actionFactory;

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

        private void MainBattleOnExitTurn(IBattleManager battle, ICombatList attackers, ICombatList defenders, int turn)
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

                if (cityCombatGroup == null)
                {
                    if (defensiveMeter > 0)
                    {
                        continue;
                    }

                    battle.Remove(defender, BattleManager.BattleSide.Defense, ReportState.OutOfStamina);
                }
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

                    // Send the defender back to their city
                    var retreatChainAction = actionFactory.CreateRetreatChainAction(cityCombatGroup.TroopStub.City.Id,
                                                                                    cityCombatGroup.TroopStub.TroopId);
                    if (
                            cityCombatGroup.TroopStub.City.Worker.DoPassive(cityCombatGroup.TroopStub.City,
                                                                            retreatChainAction,
                                                                            true) != Error.Ok)
                    {
                        throw new Exception("Should always be able to retreat troops from stronghold to main city");
                    }
                }
            }

            if (offensiveMeter <= 0)
            {
                // Make copy since attackers will be changing
                var attackerLoopCopy = attackers.ToList();
                foreach (var attacker in attackerLoopCopy)
                {
                    // Remove from battle, no need to send them back since attacking troops have actions to handle that
                    battle.Remove(attacker, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);
                }
            }
        }

        private void MainBattleOnActionAttacked(IBattleManager battle,
                                                BattleManager.BattleSide attackingSide,
                                                ICombatGroup attackerGroup,
                                                ICombatObject attacker,
                                                ICombatGroup targetGroup,
                                                ICombatObject target,
                                                decimal damage)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold not found");
            }

            var cityCombatTarget = target as CityCombatObject;

            // Check if we should retreat a unit
            if (cityCombatTarget == null || attackingSide == BattleManager.BattleSide.Defense ||
                cityCombatTarget.TroopStub.Station != stronghold || cityCombatTarget.TroopStub.TotalCount <= 0 ||
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
            var retreatChainAction = actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId);
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
            // If stronghold has no one left then it means the attacker took over
            // This same action removes all defenders if the defensive meter gets to 0
            if ((stronghold.StrongholdState == StrongholdState.Occupied && !stronghold.Troops.StationedHere().Any()) ||
                (stronghold.StrongholdState == StrongholdState.Neutral && (npcGroupKilled || defensiveMeter <= 0)))
            {
                strongholdManager.TransferTo(stronghold, stronghold.GateOpenTo);
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

            CallbackLock.CallbackLockHandler lockHandler = delegate { return stronghold.LockList.ToArray(); };

            using (locker.Lock(lockHandler, null, stronghold))
            {
                if (stronghold.MainBattle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(stronghold.MainBattle);
                    endTime =
                            SystemClock.Now.AddSeconds(
                                                       formula.GetBattleInterval(stronghold.MainBattle.Defenders.Count +
                                                                                 stronghold.MainBattle.Attackers.Count));
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

                foreach (var stub in stronghold.Troops.StationedHere())
                {
                    stub.BeginUpdate();
                    stub.State = TroopState.Stationed;
                    stub.EndUpdate();
                }

                world.Remove(stronghold.MainBattle);
                dbManager.Delete(stronghold.MainBattle);
                stronghold.BeginUpdate();
                stronghold.GateOpenTo = null;
                stronghold.MainBattle = null;
                stronghold.Gate = formula.GetGateLimit(stronghold.Lvl);
                if (stronghold.StrongholdState != StrongholdState.Neutral)
                {
                    stronghold.Gate /= 2m;
                }
                stronghold.State = GameObjectState.NormalState();
                stronghold.EndUpdate();

                StateChange(ActionState.Completed);
            }
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
                var strongholdGroup = battleProcedure.AddStrongholdUnitsToBattle(stronghold.MainBattle,
                                                                                 stronghold,
                                                                                 strongholdManager.GenerateNeutralStub(
                                                                                                                       stronghold));

                npcGroupId = strongholdGroup.Id;
            }
            stronghold.BeginUpdate();
            stronghold.State = GameObjectState.BattleState(stronghold.MainBattle.BattleId);
            stronghold.EndUpdate();

            beginTime = SystemClock.Now;
            endTime = SystemClock.Now;

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