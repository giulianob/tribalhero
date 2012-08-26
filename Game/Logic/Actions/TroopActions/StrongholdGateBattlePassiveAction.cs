#region

using System;
using System.Collections.Generic;
using System.Globalization;
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
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class StrongholdGateBattlePassiveAction : ScheduledPassiveAction
    {
        private readonly uint strongholdId;

        private uint localGroupId;

        private readonly BattleProcedure battleProcedure;

        private readonly ILocker locker;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly Dictionary<uint, decimal> tribeDamageDealt = new Dictionary<uint, decimal>();

        public StrongholdGateBattlePassiveAction(uint strongholdId,
                                   BattleProcedure battleProcedure,
                                   ILocker locker,
                                   IGameObjectLocator gameObjectLocator,
                                   IDbManager dbManager,
                                   Formula formula)
        {
            this.strongholdId = strongholdId;
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Did not find stronghold that was supposed to be having a battle");
            }

            stronghold.Battle.GroupKilled += BattleOnGroupKilled;
            stronghold.Battle.ActionAttacked += BattleOnActionAttacked;
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
                                   Formula formula)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;

            localGroupId = uint.Parse(properties["local_group_id"]);

            foreach (var parts in properties["tribe_damage_dealt"].Split(',').Select(damageDealtItem => damageDealtItem.Split('=')))
            {
                tribeDamageDealt[Convert.ToUInt32(parts[0], CultureInfo.InvariantCulture)] = Convert.ToDecimal(parts[1], CultureInfo.InvariantCulture);
            }

            strongholdId = uint.Parse(properties["stronghold_id"]);

            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception();
            }

            stronghold.Battle.GroupKilled += BattleOnGroupKilled;
            stronghold.Battle.ActionAttacked += BattleOnActionAttacked;
        }

        private void BattleOnActionAttacked(IBattleManager battle, BattleManager.BattleSide attackingSide, ICombatGroup attackerGroup, ICombatObject attacker, ICombatGroup targetGroup, ICombatObject target, decimal damage)
        {
            if (attackingSide == BattleManager.BattleSide.Attack && attackerGroup.Owner.Type == BattleOwnerType.City)
            {
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
                                      combatGroup.Owner.Type == BattleOwnerType.City && gameObjectLocator.TryGetObjects(combatGroup.Owner.Id, out loopCity) &&
                                      loopCity.Owner.IsInTribe
                              select loopCity.Owner.Tribesman.Tribe).Distinct().ToDictionary(k => k.Id, v => v);

                var winningTribe = (from kv in tribeDamageDealt
                                    orderby kv.Value descending
                                    select new
                                    {
                                            TribeId = kv.Key,
                                            Damage = kv.Value
                                    }).FirstOrDefault(x => attackerTribes.ContainsKey(x.TribeId));

                if (winningTribe != null)
                {
                    stronghold.BeginUpdate();
                    stronghold.GateOpenTo = attackerTribes[winningTribe.TribeId];
                    stronghold.EndUpdate();
                    dbManager.Save(stronghold);
                }
            }
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
                return XmlSerializer.Serialize(new[]
                {
                        new XmlKvPair("stronghold_id", strongholdId),
                        new XmlKvPair("tribe_damage_dealt", String.Join(",", tribeDamageDealt.Select(item => item.Key.ToString(CultureInfo.InvariantCulture) + "=" + item.Value.ToString(CultureInfo.InvariantCulture))))
                });
            }
        }

        public override void Callback(object custom)
        {
            IStronghold stronghold;
            if (!gameObjectLocator.TryGetObjects(strongholdId, out stronghold))
            {
                throw new Exception("Stronghold is missing");
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    return stronghold.LockList.ToArray();
                };

            using (locker.Lock(lockHandler, null, stronghold))
            {
                if (stronghold.Battle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(stronghold.Battle);
                    endTime = SystemClock.Now.AddSeconds(formula.GetBattleInterval(stronghold.Battle.Defenders.Count + stronghold.Battle.Attackers.Count));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                stronghold.Battle.GroupKilled -= BattleOnGroupKilled;
                stronghold.Battle.ActionAttacked -= BattleOnActionAttacked;

                World.Current.Remove(stronghold.Battle);
                dbManager.Delete(stronghold.Battle);
                stronghold.Battle = null;

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

            World.Current.Add(stronghold.Battle);
            dbManager.Save(stronghold.Battle);

            //Add gate to battle
            var combatGroup = battleProcedure.AddGateToBattle(stronghold.Battle, stronghold);            
            localGroupId = combatGroup.Id;

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