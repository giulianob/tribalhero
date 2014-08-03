#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using JsonFx.Json;
using Persistance;

#endregion

namespace Game.Battle
{
    public class BattleManager : IBattleManager
    {
        public enum BattleSide
        {
            Defense = 0,

            Attack = 1
        }

        public const string DB_TABLE = "battle_managers";

        private readonly IBattleFormulas battleFormulas;

        private readonly IBattleRandom battleRandom;

        private readonly IBattleOrder battleOrder;

        private readonly object battleLock = new object();

        private readonly IDbManager dbManager;

        private readonly LargeIdGenerator groupIdGen;

        private readonly LargeIdGenerator idGen;

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        private readonly IRewardStrategy rewardStrategy;

        public BattleManager(uint battleId,
                             BattleLocation location,
                             BattleOwner owner,
                             IRewardStrategy rewardStrategy,
                             IDbManager dbManager,
                             IBattleReport battleReport,
                             ICombatListFactory combatListFactory,
                             IBattleFormulas battleFormulas,
                             IBattleOrder battleOrder,
                             IBattleRandom battleRandom)
        {

            groupIdGen = new LargeIdGenerator(uint.MaxValue);
            idGen = new LargeIdGenerator(uint.MaxValue);
            // Group id 1 is always reserved for local troop
            groupIdGen.Set(1);

            BattleId = battleId;
            Location = location;
            Owner = owner;
            BattleReport = battleReport;
            Attackers = combatListFactory.GetAttackerCombatList();
            Defenders = combatListFactory.GetDefenderCombatList();

            this.rewardStrategy = rewardStrategy;
            this.dbManager = dbManager;
            this.battleOrder = battleOrder;
            this.battleFormulas = battleFormulas;
            this.battleRandom = battleRandom;
        }

        public uint BattleId { get; private set; }

        public BattleLocation Location { get; private set; }

        public BattleOwner Owner { get; private set; }

        public bool BattleStarted { get; set; }

        public uint Round { get; set; }

        public uint Turn { get; set; }

        public ICombatList Attackers { get; private set; }

        public ICombatList Defenders { get; private set; }

        public IBattleReport BattleReport { get; private set; }

        public IEnumerable<ILockable> LockList
        {
            get
            {
                var locks = new HashSet<ILockable>();

                foreach (var co in Attackers)
                {
                    locks.Add(co);
                }

                foreach (var co in Defenders)
                {
                    locks.Add(co);
                }

                return locks;
            }
        }

        public ICombatObject GetCombatObject(uint id)
        {
            return Attackers.AllCombatObjects().FirstOrDefault(co => co.Id == id) ??
                   Defenders.AllCombatObjects().FirstOrDefault(co => co.Id == id);
        }

        public virtual Error CanWatchBattle(IPlayer player, out IEnumerable<string> errorParams)
        {
            errorParams = new string[0];

            lock (battleLock)
            {
                if (Owner.IsOwner(player))
                {
                    return Error.Ok;
                }

                int attackersUpkeepTotal = Attackers.UpkeepTotal;
                int defendersRoundsLeft = int.MaxValue;
                int attackersRoundsLeft = int.MaxValue;

                // Check if player has a defender that is over the minimum battle rounds
                var playersDefenders = Defenders.Where(combatGroup => combatGroup.BelongsTo(player)).ToList();
                if (playersDefenders.Any())
                {
                    if (attackersUpkeepTotal > 1000)
                    {
                        return Error.Ok;
                    }

                    defendersRoundsLeft = playersDefenders.Min(combatGroup =>
                                                               combatGroup.Min(combatObject => Config.battle_min_rounds - combatObject.RoundsParticipated));
                    if (defendersRoundsLeft < 0)
                    {
                        return Error.Ok;
                    }
                }

                // Check if player has an attacker that is over the minimum battle rounds
                var playersAttackers = Attackers.Where(co => co.BelongsTo(player)).ToList();
                if (playersAttackers.Any())
                {
                    if (attackersUpkeepTotal > 1000)
                    {
                        return Error.Ok;
                    }

                    attackersRoundsLeft = playersAttackers.Min(combatGroup =>
                                                               combatGroup.Min(combatObject => Config.battle_min_rounds - combatObject.RoundsParticipated));
                    if (attackersRoundsLeft < 0)
                    {
                        return Error.Ok;
                    }
                }

                // Calculate how many rounds until player can see the battle
                var roundsLeft = Math.Min(attackersRoundsLeft, defendersRoundsLeft);
                if (roundsLeft == int.MaxValue)
                {
                    return Error.BattleViewableNoTroopsInBattle;
                }

                roundsLeft = Math.Max(roundsLeft, 1);
                errorParams = new[] {roundsLeft.ToString(CultureInfo.InvariantCulture)};
                return Error.BattleViewableInRounds;
            }
        }

        #region Database Loader

        public void DbLoaderAddToCombatList(ICombatGroup group, BattleSide side)
        {
            if (side == BattleSide.Defense)
            {
                Defenders.Add(group, false);
            }
            else
            {
                Attackers.Add(group, false);
            }

            groupIdGen.Set(group.Id);
        }

        #endregion

        #region Adding/Removing Groups

        public uint GetNextGroupId()
        {
            return groupIdGen.GetNext();
        }

        public uint GetNextCombatObjectId()
        {
            return idGen.GetNext();
        }

        public void DbFinishedLoading()
        {
            uint maxId =
                    Math.Max(
                             Attackers.SelectMany(group => group)
                                      .DefaultIfEmpty()
                                      .Max(combatObject => combatObject == null ? 0 : combatObject.Id),
                             Defenders.SelectMany(group => group)
                                      .DefaultIfEmpty()
                                      .Max(combatObject => combatObject == null ? 0 : combatObject.Id));
            idGen.Set(maxId);
        }

        public T GetProperty<T>(string name)
        {
            return (T)Convert.ChangeType(properties[name], typeof(T), CultureInfo.InvariantCulture);
        }

        public void SetProperty(string name, object value)
        {
            properties[name] = value;
        }

        public void DbLoadProperties(Dictionary<string, object> dbProperties)
        {
            properties.Clear();
            foreach (var property in dbProperties)
            {
                properties.Add(property.Key, property.Value);
            }
        }

        public IDictionary<string, object> ListProperties()
        {
            return new Dictionary<string, object>(properties);
        }

        public void Add(ICombatGroup combatGroup, BattleSide battleSide, bool allowReportAccess)
        {
            bool isAttacker = battleSide == BattleSide.Attack;

            lock (battleLock)
            {
                if (GetCombatGroup(combatGroup.Id) != null)
                {
                    throw new Exception(
                            string.Format("Trying to add a group to battle {0} with id {1} that already exists",
                                          BattleId,
                                          combatGroup.Id));
                }

                (battleSide == BattleSide.Attack ? Attackers : Defenders).Add(combatGroup);

                if (isAttacker)
                {
                    combatGroup.CombatObjectAdded += AttackerGroupOnCombatObjectAdded;
                    combatGroup.CombatObjectRemoved += AttackerGroupOnCombatObjectRemoved;
                }
                else
                {
                    combatGroup.CombatObjectAdded += DefenderGroupOnCombatObjectAdded;
                    combatGroup.CombatObjectRemoved += DefenderGroupOnCombatObjectRemoved;
                }

                if (BattleStarted)
                {
                    BattleReport.WriteReportGroup(combatGroup, isAttacker, ReportState.Entering);

                    if (isAttacker)
                    {
                        ReinforceAttacker(this, combatGroup);
                    }
                    else
                    {
                        ReinforceDefender(this, combatGroup);
                    }
                }

                if (allowReportAccess)
                {
                    BattleReport.AddAccess(combatGroup, battleSide);
                }

                if (combatGroup.Tribe != null)
                {
                    BattleReport.AddTribeToBattle(combatGroup.Tribe, battleSide);
                }
            }
        }

        public void Remove(ICombatGroup group, BattleSide side, ReportState state)
        {
            lock (battleLock)
            {
                // Remove from appropriate combat list
                if (side == BattleSide.Attack)
                {
                    if (!Attackers.Remove(group))
                    {
                        return;
                    }

                    group.CombatObjectAdded -= AttackerGroupOnCombatObjectAdded;
                    group.CombatObjectRemoved -= AttackerGroupOnCombatObjectRemoved;
                }
                else
                {
                    if (!Defenders.Remove(group))
                    {
                        return;
                    }

                    group.CombatObjectAdded -= DefenderGroupOnCombatObjectAdded;
                    group.CombatObjectRemoved -= DefenderGroupOnCombatObjectRemoved;
                }

                // If battle hasnt started then dont worry about cleaning anything up since nothing has happened to these objects
                if (!BattleStarted)
                {
                    return;
                }

                // Snap a report of exit
                BattleReport.WriteReportGroup(group, side == BattleSide.Attack, state);

                // Tell objects to exit from battle
                foreach (var co in group.Where(co => !co.Disposed))
                {
                    co.ExitBattle();
                }

                // Send exit events
                if (side == BattleSide.Attack)
                {
                    WithdrawAttacker(this, group);
                }
                else
                {
                    WithdrawDefender(this, group);
                }
            }
        }

        private void DefenderGroupOnCombatObjectAdded(ICombatGroup group, ICombatObject combatObject)
        {
            GroupUnitAdded(this, BattleSide.Defense, group, combatObject);
        }

        private void DefenderGroupOnCombatObjectRemoved(ICombatGroup group, ICombatObject combatObject)
        {
            GroupUnitRemoved(this, BattleSide.Defense, group, combatObject);
        }

        private void AttackerGroupOnCombatObjectAdded(ICombatGroup group, ICombatObject combatObject)
        {
            GroupUnitAdded(this, BattleSide.Attack, group, combatObject);
        }

        private void AttackerGroupOnCombatObjectRemoved(ICombatGroup group, ICombatObject combatObject)
        {
            GroupUnitRemoved(this, BattleSide.Attack, group, combatObject);
        }

        #endregion

        #region Battle

        public bool ExecuteTurn()
        {
            lock (battleLock)
            {
                battleRandom.UpdateSeed(Round, Turn);

                ICombatList offensiveCombatList;
                ICombatList defensiveCombatList;
                BattleSide sideAttacking;


                // This will finalize any reports already started.
                BattleReport.CompleteReport(ReportState.Staying);
                 
                #region Battle Start

                if (!BattleStarted)
                {
                    // Makes sure battle is valid before even starting it
                    // This shouldnt really happen but I remember it happening in the past
                    // so until we can make sure that we dont even start invalid battles, it's better to leave it
                    if (!IsBattleValid())
                    {
                        BattleEnded(false);
                        return false;
                    }

                    BringWaitingTroopsIntoBattle();
                    
                    BattleStarted = true;
                    BattleReport.CreateBattleReport();
                    EnterBattle(this, Attackers, Defenders);
                }

                #endregion

                #region Targeting

                List<CombatList.Target> currentDefenders;
                ICombatGroup attackerGroup;
                ICombatObject attackerObject;

                do
                {
                    #region Find Attacker

                    var newRound =
                            !battleOrder.NextObject(Round,
                                                    Attackers,
                                                    Defenders,
                                                    out attackerObject,
                                                    out attackerGroup,
                                                    out sideAttacking);

                    // Save the offensive/defensive side relative to the attacker to make it easier in this function to know
                    // which side is attacking.
                    defensiveCombatList = sideAttacking == BattleSide.Attack ? Defenders : Attackers;
                    offensiveCombatList = sideAttacking == BattleSide.Attack ? Attackers : Defenders;

                    if (newRound)
                    {
                        ++Round;
                        Turn = 0;

                        BringWaitingTroopsIntoBattle();

                        EnterRound(this, Attackers, Defenders, Round);

                        // Since the EventEnterRound can remove the object from battle, we need to make sure he's still in the battle
                        // If they aren't, then we just find a new attacker
                        if (attackerObject != null && !offensiveCombatList.AllCombatObjects().Contains(attackerObject))
                        {
                            continue;
                        }
                    }

                    // Verify battle is still good to go
                    if (attackerObject == null || !IsBattleValid())
                    {
                        BattleEnded(true);
                        return false;
                    }

                    #endregion

                    #region Find Target(s)

                    var targetResult = defensiveCombatList.GetBestTargets(BattleId,
                                                                          attackerObject,
                                                                          out currentDefenders,
                                                                          battleFormulas.GetNumberOfHits(attackerObject, defensiveCombatList),
                                                                          Round);

                    if (currentDefenders.Count == 0 || attackerObject.Stats.Atk == 0)
                    {
                        attackerObject.ParticipatedInRound(Round);
                        dbManager.Save(attackerObject);
                        SkippedAttacker(this, sideAttacking, attackerGroup, attackerObject);

                        // If the attacker can't attack because it has no one in range, then we skip him and find another target right away.
                        if (targetResult == CombatList.BestTargetResult.NoneInRange)
                        {
                            continue;
                        }

                        return true;
                    }

                    #endregion

                    break;
                }
                while (true);

                #endregion

                for (int attackIndex = 0; attackIndex < currentDefenders.Count; attackIndex++)
                {
                    var defender = currentDefenders[attackIndex];

                    // Make sure the target is still in the battle (they can leave if someone else in the group took dmg and caused the entire group to leave)
                    // We just skip incase they left while we were attacking
                    // which means if the attacker is dealing splash they may deal less hits in this fringe case
                    if (!defensiveCombatList.AllCombatObjects().Contains(defender.CombatObject))
                    {
                        continue;
                    }

                    // Target is still in battle, attack it
                    decimal carryOverDmg;
                    AttackTarget(offensiveCombatList,
                                 defensiveCombatList,
                                 attackerGroup,
                                 attackerObject,
                                 sideAttacking,
                                 defender,
                                 attackIndex,
                                 Round,
                                 out carryOverDmg);

                    // Add another target if we have to carry over some dmg and our current hit isnt from a carry over already
                    if (carryOverDmg > 0m && !defender.DamageCarryOverPercentage.HasValue)
                    {
                        List<CombatList.Target> carryOverDefender;
                        defensiveCombatList.GetBestTargets(BattleId,
                                                           attackerObject,
                                                           out carryOverDefender,
                                                           1,
                                                           Round);

                        if (carryOverDefender.Count > 0)
                        {
                            var target = carryOverDefender.First();
                            target.DamageCarryOverPercentage = carryOverDmg;
                            currentDefenders.Insert(attackIndex + 1, target);
                        }
                    }
                }

                // Just a safety check
                if (attackerObject.Disposed)
                {
                    throw new Exception("Attacker has been improperly disposed");
                }

                attackerObject.ParticipatedInRound(Round);

                dbManager.Save(attackerObject);

                ExitTurn(this, Attackers, Defenders, Turn++);

                if (!IsBattleValid())
                {
                    BattleEnded(true);
                    return false;
                }

                // Remove any attackers that don't have anyone in range
                var groupsWithoutAnyoneInRange = Attackers.Where(attacker => attacker.All(combatObjects => !Defenders.HasInRange(combatObjects))).ToList();
                foreach (var group in groupsWithoutAnyoneInRange)
                {
                    Remove(group, BattleSide.Attack, ReportState.Exiting);
                }

                return true;
            }
        }

        private void BringWaitingTroopsIntoBattle()
        {
            foreach (var defender in Defenders.AllCombatObjects().Where(co => co.IsWaitingToJoinBattle))
            {
                defender.JoinBattle(Round);
                dbManager.Save(defender);
            }

            foreach (var attacker in Attackers.AllCombatObjects().Where(co => co.IsWaitingToJoinBattle))
            {
                attacker.JoinBattle(Round);
                dbManager.Save(attacker);
            }
        }

        public ICombatGroup GetCombatGroup(uint id)
        {
            return Attackers.FirstOrDefault(group => group.Id == id) ??
                   Defenders.FirstOrDefault(group => group.Id == id);
        }

        private bool IsBattleValid()
        {
            if (Attackers.Count == 0 || Defenders.Count == 0)
            {
                return false;
            }

            // Check to see if all units in the defense is dead
            // and make sure units can still see each other
            return Attackers.AllAliveCombatObjects().Any(combatObj => Defenders.HasInRange(combatObj));
        }

        private void BattleEnded(bool writeReport)
        {
            if (writeReport)
            {
                BattleReport.CompleteBattle();
            }

            AboutToExitBattle(this, Attackers, Defenders);
            ExitBattle(this, Attackers, Defenders);

            foreach (var combatObj in Defenders.AllCombatObjects().Where(combatObj => !combatObj.IsDead))
            {
                combatObj.ExitBattle();
            }

            foreach (var combatObj in Attackers.AllCombatObjects().Where(combatObj => !combatObj.IsDead))
            {
                combatObj.ExitBattle();
            }

            foreach (var group in Attackers)
            {
                WithdrawAttacker(this, group);
            }

            foreach (var group in Defenders)
            {
                WithdrawDefender(this, group);
            }

            // Delete all groups
            Attackers.Clear();
            Defenders.Clear();
        }

        protected virtual void AttackTarget(ICombatList offensiveCombatList,
                                            ICombatList defensiveCombatList,
                                            ICombatGroup attackerGroup,
                                            ICombatObject attacker,
                                            BattleSide sideAttacking,
                                            CombatList.Target target,
                                            int attackIndex,
                                            uint round,
                                            out decimal carryOverDmg)
        {
            var attackerCount = attacker.Count;
            var targetCount = target.CombatObject.Count;

            #region Damage            

            decimal dmg = battleFormulas.GetAttackerDmgToDefender(attacker, target.CombatObject,round);
            
            if (target.DamageCarryOverPercentage.HasValue)
            {
                dmg *= target.DamageCarryOverPercentage.Value;
            }

            decimal actualDmg;
            target.CombatObject.CalcActualDmgToBeTaken(offensiveCombatList,
                                                       defensiveCombatList,
                                                       battleRandom,
                                                       dmg,
                                                       attackIndex,
                                                       out actualDmg);
            
            Resource defenderDroppedLoot;
            int attackPoints;
            int initialCount = target.CombatObject.Count;

            carryOverDmg = 0;
            if (dmg > 0 && target.CombatObject.Hp - actualDmg < 0m)
            {
                carryOverDmg = (actualDmg - target.CombatObject.Hp) / actualDmg;
            }

            actualDmg = Math.Min(target.CombatObject.Hp, actualDmg);

            target.CombatObject.TakeDamage(actualDmg, out defenderDroppedLoot, out attackPoints);
            if (initialCount > target.CombatObject.Count)
            {
                UnitCountDecreased(this,
                                   sideAttacking == BattleSide.Attack ? BattleSide.Defense : BattleSide.Attack,
                                   target.Group,
                                   target.CombatObject,
                                   initialCount - target.CombatObject.Count);
            }

            attacker.DmgDealt += actualDmg;
            attacker.MaxDmgDealt = (ushort)Math.Max(attacker.MaxDmgDealt, actualDmg);
            attacker.MinDmgDealt = (ushort)Math.Min(attacker.MinDmgDealt, actualDmg);
            ++attacker.HitDealt;
            attacker.HitDealtByUnit += attacker.Count;

            target.CombatObject.DmgRecv += actualDmg;
            target.CombatObject.MaxDmgRecv = (ushort)Math.Max(target.CombatObject.MaxDmgRecv, actualDmg);
            target.CombatObject.MinDmgRecv = (ushort)Math.Min(target.CombatObject.MinDmgRecv, actualDmg);
            ++target.CombatObject.HitRecv;

            #endregion

            #region Loot and Attack Points

            // NOTE: In the following rewardStrategy calls we are in passing in whatever the existing implementations
            // of reward startegy need. If some specific implementation needs more info then add more params or change it to
            // take the entire BattleManager as a param.
            if (sideAttacking == BattleSide.Attack)
            {
                // Only give loot if we are attacking the first target in the list
                Resource loot;
                rewardStrategy.RemoveLoot(this, attackIndex, attacker, target.CombatObject, out loot);

                if (attackPoints > 0 || (loot != null && !loot.Empty))
                {
                    rewardStrategy.GiveAttackerRewards(attacker, attackPoints, loot ?? new Resource());
                }
            }
            else
            {
                // Give defender rewards if there are any
                if (attackPoints > 0 || (defenderDroppedLoot != null && !defenderDroppedLoot.Empty))
                {
                    rewardStrategy.GiveDefendersRewards(attacker,
                                                        attackPoints,
                                                        defenderDroppedLoot ?? new Resource());
                }
            }

            #endregion

            #region Object removal

            bool isDefenderDead = target.CombatObject.IsDead;

            ActionAttacked(this, sideAttacking, attackerGroup, attacker, target.Group, target.CombatObject, actualDmg, attackerCount, targetCount);

            if (isDefenderDead)
            {
                bool isGroupDead = target.Group.IsDead();
                if (isGroupDead)
                {
                    // Remove the entire group
                    Remove(target.Group,
                           sideAttacking == BattleSide.Attack ? BattleSide.Defense : BattleSide.Attack,
                           ReportState.Dying);
                }
                else if (!target.CombatObject.Disposed)
                {
                    // Only remove the single object
                    BattleReport.WriteExitingObject(target.Group, sideAttacking != BattleSide.Attack, target.CombatObject);
                    target.Group.Remove(target.CombatObject);
                }

                UnitKilled(this,
                           sideAttacking == BattleSide.Attack ? BattleSide.Defense : BattleSide.Attack,
                           target.Group,
                           target.CombatObject);

                if (!target.CombatObject.Disposed)
                {
                    target.CombatObject.ExitBattle();
                }

                if (isGroupDead)
                {
                    GroupKilled(this, target.Group);
                }
            }
            else
            {
                if (!target.CombatObject.Disposed)
                {
                    dbManager.Save(target.CombatObject);
                }
            }

            #endregion
        }

        #endregion

        #region Events

        public delegate void OnAttack(
                IBattleManager battle,
                BattleSide attackingSide,
                ICombatGroup attackerGroup,
                ICombatObject attacker,
                ICombatGroup targetGroup,
                ICombatObject target,
                decimal damage,
                int attackerCount,
                int targetCount);

        public delegate void OnBattle(IBattleManager battle, ICombatList attackers, ICombatList defenders);

        public delegate void OnReinforce(IBattleManager battle, ICombatGroup combatGroup);

        public delegate void OnRound(IBattleManager battle, ICombatList attackers, ICombatList defenders, uint round);

        public delegate void OnTurn(IBattleManager battle, ICombatList attackers, ICombatList defenders, uint turn);

        public delegate void OnUnitCountChange(
                IBattleManager battle,
                BattleSide combatObjectSide,
                ICombatGroup combatGroup,
                ICombatObject combatObject,
                int count);

        public delegate void OnUnitUpdate(
                IBattleManager battle, BattleSide combatObjectSide, ICombatGroup combatGroup, ICombatObject combatObject
                );

        public delegate void OnWithdraw(IBattleManager battle, ICombatGroup group);

        /// <summary>
        ///     Fired once when the battle begins
        /// </summary>
        public event OnBattle EnterBattle = delegate { };

        /// <summary>
        ///     Fired right before the battle is about to end
        /// </summary>
        public event OnBattle AboutToExitBattle = delegate { };

        /// <summary>
        ///     Fired once when the battle ends
        /// </summary>
        public event OnBattle ExitBattle = delegate { };

        /// <summary>
        ///     Fired when a new round starts
        /// </summary>
        public event OnRound EnterRound = delegate { };

        /// <summary>
        ///     Fired everytime a unit exits its turn
        /// </summary>
        public event OnTurn ExitTurn = delegate { };

        /// <summary>
        ///     Fired when a new attacker joins the battle
        /// </summary>
        public event OnReinforce ReinforceAttacker = delegate { };

        /// <summary>
        ///     Fired when a new defender joins the battle
        /// </summary>
        public event OnReinforce ReinforceDefender = delegate { };

        /// <summary>
        ///     Fired when an attacker withdraws from the battle
        /// </summary>
        public event OnWithdraw WithdrawAttacker = delegate { };

        /// <summary>
        ///     Fired when a defender withdraws from the battle
        /// </summary>
        public event OnWithdraw WithdrawDefender = delegate { };

        /// <summary>
        ///     Fired when all of the units in a group are killed
        /// </summary>
        public event OnWithdraw GroupKilled = delegate { };

        /// <summary>
        ///     Fired when one of the groups in battle receives a new unit
        /// </summary>
        public event OnUnitUpdate GroupUnitAdded = delegate { };

        /// <summary>
        ///     Fired when one of the groups in battle loses a unit
        /// </summary>
        public event OnUnitUpdate GroupUnitRemoved = delegate { };

        /// <summary>
        ///     Fired when obecj is killed
        /// </summary>
        public event OnUnitUpdate UnitKilled = delegate { };

        /// <summary>
        ///     Fired when a single unit is killed
        /// </summary>
        public event OnUnitCountChange UnitCountDecreased = delegate { };

        /// <summary>
        ///     Fired when an attacker is unable to take his turn
        /// </summary>
        public event OnUnitUpdate SkippedAttacker = delegate { };

        /// <summary>
        ///     Fired when a unit hits another one
        /// </summary>
        public event OnAttack ActionAttacked = delegate { };

        #endregion

        #region IPersistableObject Members

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("battle_id", BattleId, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new[] {new DbDependency("BattleReport", true, true)};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("battle_started", BattleStarted, DbType.Boolean),
                        new DbColumn("round", Round, DbType.UInt32), new DbColumn("turn", Turn, DbType.UInt32),
                        new DbColumn("report_started", BattleReport.ReportStarted, DbType.Boolean),
                        new DbColumn("report_id", BattleReport.ReportId, DbType.UInt32),
                        new DbColumn("owner_type", Owner.Type.ToString(), DbType.String, 15),
                        new DbColumn("owner_id", Owner.Id, DbType.UInt32),
                        new DbColumn("location_type", Location.Type.ToString(), DbType.String, 15),
                        new DbColumn("location_id", Location.Id, DbType.UInt32),
                        new DbColumn("snapped_important_event", BattleReport.SnappedImportantEvent, DbType.Boolean),
                        new DbColumn("properties", new JsonWriter().Write(properties), DbType.String)
                };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}