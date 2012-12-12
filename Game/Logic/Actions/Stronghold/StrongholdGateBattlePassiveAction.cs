#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stronghold;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using JsonFx.Json;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class StrongholdGateBattlePassiveAction : ScheduledPassiveAction
    {
        private readonly BattleProcedure battleProcedure;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly uint strongholdId;

        private readonly Dictionary<uint, decimal> tribeDamageDealt = new Dictionary<uint, decimal>();

        private readonly IWorld world;

        private uint localGroupId;

        public StrongholdGateBattlePassiveAction(uint strongholdId,
                                                 BattleProcedure battleProcedure,
                                                 ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 IWorld world)
        {
            this.strongholdId = strongholdId;
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.world = world;

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Did not find stronghold that was supposed to be having a battle");
            }

            stronghold.GateBattle.GroupKilled += BattleOnGroupKilled;
            stronghold.GateBattle.ActionAttacked += BattleOnActionAttacked;
        }

        public StrongholdGateBattlePassiveAction(uint id,
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
                                                 IWorld world)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.world = world;

            localGroupId = uint.Parse(properties["local_group_id"]);

            tribeDamageDealt =
                    new JsonReader().Read<Dictionary<string, decimal>>(properties["tribe_damage_dealt"])
                                    .ToDictionary(k => uint.Parse(k.Key), v => v.Value);

            strongholdId = uint.Parse(properties["stronghold_id"]);

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception();
            }

            stronghold.GateBattle.GroupKilled += BattleOnGroupKilled;
            stronghold.GateBattle.ActionAttacked += BattleOnActionAttacked;
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StrongholdGateBattlePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("stronghold_id", strongholdId), new XmlKvPair("local_group_id", localGroupId),
                                new XmlKvPair("tribe_damage_dealt", new JsonWriter().Write(tribeDamageDealt))
                        });
            }
        }

        private void BattleOnActionAttacked(IBattleManager battle,
                                            BattleManager.BattleSide attackingSide,
                                            ICombatGroup attackerGroup,
                                            ICombatObject attacker,
                                            ICombatGroup targetGroup,
                                            ICombatObject target,
                                            decimal damage)
        {
            if (attackingSide != BattleManager.BattleSide.Attack || attackerGroup.Owner.Type != BattleOwnerType.City)
            {
                return;
            }

            ICity attackingCity;
            if (!gameObjectLocator.TryGetObjects(attackerGroup.Owner.Id, out attackingCity))
            {
                throw new Exception("Attacker city not found");
            }

            if (!attackingCity.Owner.IsInTribe)
            {
                return;
            }

            var tribeId = attackingCity.Owner.Tribesman.Tribe.Id;

            if (tribeDamageDealt.ContainsKey(tribeId))
            {
                tribeDamageDealt[tribeId] += damage;
            }
            else
            {
                tribeDamageDealt[tribeId] = damage;
            }
        }

        private void BattleOnGroupKilled(IBattleManager battle, ICombatGroup group)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception();
            }

            if (group.Id == localGroupId)
            {
                ICity loopCity = null;
                var attackerTribes = (from combatGroup in battle.Attackers
                                      where
                                              combatGroup.Owner.Type == BattleOwnerType.City &&
                                              gameObjectLocator.TryGetObjects(combatGroup.Owner.Id, out loopCity) &&
                                              loopCity.Owner.IsInTribe
                                      select loopCity.Owner.Tribesman.Tribe).Distinct().ToDictionary(k => k.Id, v => v);

                var winningTribe =
                        (from kv in tribeDamageDealt
                         orderby kv.Value descending
                         select new {TribeId = kv.Key, Damage = kv.Value}).FirstOrDefault(
                                                                                          x =>
                                                                                          attackerTribes.ContainsKey(
                                                                                                                     x
                                                                                                                             .TribeId));

                if (winningTribe != null)
                {
                    stronghold.BeginUpdate();
                    stronghold.GateOpenTo = attackerTribes[winningTribe.TribeId];
                    stronghold.EndUpdate();
                }
            }
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
                if (stronghold.GateBattle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(stronghold.GateBattle);
                    endTime =
                            SystemClock.Now.AddSeconds(
                                                       formula.GetBattleInterval(stronghold.GateBattle.Defenders.Count +
                                                                                 stronghold.GateBattle.Attackers.Count));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                stronghold.GateBattle.GroupKilled -= BattleOnGroupKilled;
                stronghold.GateBattle.ActionAttacked -= BattleOnActionAttacked;

                world.Remove(stronghold.GateBattle);
                dbManager.Delete(stronghold.GateBattle);
                stronghold.BeginUpdate();
                stronghold.GateBattle = null;
                stronghold.State = GameObjectState.NormalState();
                if (stronghold.GateOpenTo == null && stronghold.StrongholdState == StrongholdState.Neutral)
                {
                    stronghold.Gate = formula.GetGateLimit(stronghold.Lvl);
                }
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

            world.Add(stronghold.GateBattle);
            dbManager.Save(stronghold.GateBattle);

            //Add gate to battle
            var combatGroup = battleProcedure.AddStrongholdGateToBattle(stronghold.GateBattle, stronghold);
            localGroupId = combatGroup.Id;

            stronghold.BeginUpdate();
            stronghold.State = GameObjectState.BattleState(stronghold.GateBattle.BattleId);
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
            throw new Exception("City removed during battle?");
        }
    }
}