#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Battle
{
    public class BattleManager : IBattleManager
    {
        public enum BattleSide
        {
            Defense,

            Attack
        }

        public const string DB_TABLE = "battle_managers";

        private readonly object battleLock = new object();

        private readonly BattleFormulas battleFormulas;

        private readonly LargeIdGenerator groupIdGen;

        private readonly LargeIdGenerator idGen;

        private readonly IRewardStrategy rewardStrategy;

        private readonly IDbManager dbManager;

        private readonly BattleOrder battleOrder = new BattleOrder();

        public BattleManager(uint battleId,
                             BattleLocation location,
                             BattleOwner owner,
                             IRewardStrategy rewardStrategy,
                             IDbManager dbManager,
                             IBattleReport battleReport,
                             ICombatListFactory combatListFactory,
                             BattleFormulas battleFormulas)
        {
            NextToAttack = BattleSide.Defense;

            groupIdGen = new LargeIdGenerator(ushort.MaxValue);
            idGen = new LargeIdGenerator(ushort.MaxValue);
            // Group id 1 is always reserved for local troop
            groupIdGen.Set(1);

            BattleId = battleId;
            Location = location;
            Owner = owner;
            BattleReport = battleReport;
            Attackers = combatListFactory.CreateCombatList();
            Defenders = combatListFactory.CreateCombatList();

            this.rewardStrategy = rewardStrategy;
            this.dbManager = dbManager;
            this.battleFormulas = battleFormulas;
        }

        public uint BattleId { get; private set; }

        public BattleLocation Location { get; private set; }

        public BattleOwner Owner { get; private set; }

        public bool BattleStarted { get; set; }

        public uint Round { get; set; }

        public uint Turn { get; set; }

        public BattleSide NextToAttack { private get; set; }

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
                        new DbColumn("battle_started", BattleStarted, DbType.Boolean), new DbColumn("round", Round, DbType.UInt32),
                        new DbColumn("turn", Turn, DbType.UInt32),
                        new DbColumn("report_started", BattleReport.ReportStarted, DbType.Boolean), new DbColumn("report_id", BattleReport.ReportId, DbType.UInt32),
                        new DbColumn("owner_type", Owner.Type.ToString(), DbType.String, 15), new DbColumn("owner_id", Owner.Id, DbType.UInt32),
                        new DbColumn("location_type", Location.Type.ToString(), DbType.String, 15), new DbColumn("location_id", Location.Id, DbType.UInt32),
                        new DbColumn("next_to_attack", (byte)NextToAttack, DbType.Byte)
                };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public CombatObject GetCombatObject(uint id)
        {
            return Attackers.AllCombatObjects().FirstOrDefault(co => co.Id == id) ?? Defenders.AllCombatObjects().FirstOrDefault(co => co.Id == id);
        }

        public bool CanWatchBattle(IPlayer player, out int roundsLeft)
        {
            roundsLeft = 0;

            lock (battleLock)
            {
                if (Owner.IsOwner(player))
                {
                    return true;
                }

                int defendersRoundsLeft = int.MaxValue;
                int attackersRoundsLeft = int.MaxValue;

                // Check if player has a defender that is over the minimum battle rounds
                var playersDefenders = Defenders.Where(combatGroup => combatGroup.BelongsTo(player)).ToList();
                if (playersDefenders.Any())
                {
                    defendersRoundsLeft = playersDefenders.Min(combatGroup => combatGroup.Min(combatObject => Config.battle_min_rounds - combatObject.RoundsParticipated));
                    if (defendersRoundsLeft < 0)
                    {
                        return true;
                    }
                }

                // Check if player has an attacker that is over the minimum battle rounds
                var playersAttackers = Attackers.Where(co => co.BelongsTo(player)).ToList();
                if (playersAttackers.Any())
                {
                    attackersRoundsLeft = playersAttackers.Min(combatGroup => combatGroup.Min(combatObject => Config.battle_min_rounds - combatObject.RoundsParticipated));
                    if (attackersRoundsLeft < 0)
                    {
                        return true;
                    }
                }

                // Calculate how many rounds until player can see the battle
                roundsLeft = Math.Min(attackersRoundsLeft, defendersRoundsLeft);
                if (roundsLeft == int.MaxValue)
                {
                    roundsLeft = 0;
                }
                else
                {
                    // Increase by 1 since here 0 roundsLeft actually means 1 round left in normal terms
                    roundsLeft++;
                }

                return false;
            }
        }

        #region Database Loader

        public void DbLoaderAddToCombatList(CombatGroup group, BattleSide side)
        {
            if (side == BattleSide.Defense)
            {
                Defenders.Add(group, false);
            }
            else
            {
                Attackers.Add(group, false);
            }
            
            idGen.Set((int)group.Max(combatObject => combatObject.Id));
            groupIdGen.Set((int)group.Id);
        }

        #endregion

        #region Adding/Removing Groups

        public uint GetNextGroupId()
        {
            return (uint)groupIdGen.GetNext();
        }

        public void Add(CombatGroup combatGroup, BattleSide battleSide)
        {
            AddToCombatList(combatGroup, battleSide == BattleSide.Attack, battleSide == BattleSide.Attack ? Attackers : Defenders, ReportState.Entering);
        }

        private void AddToCombatList(CombatGroup group, bool isAttacker, ICombatList combatList, ReportState state)
        {
            lock (battleLock)
            {
                if (!BattleStarted)
                {
                    return;
                }

                combatList.Add(group);

                BattleReport.WriteReportGroup(group, isAttacker, state);

                if (isAttacker) {
                    ReinforceAttacker(this, group);
                }
                else
                {
                    ReinforceDefender(this, group);
                }
            }
        }

        public void Remove(CombatGroup group, BattleSide side, ReportState state)
        {
            lock (battleLock)
            {
                // Remove from appropriate combat list
                if (side == BattleSide.Attack)
                {
                    Attackers.Remove(group);    
                }
                else
                {
                    Defenders.Remove(group);
                }

                // If battle hasnt started then dont worry about cleaning anything up since nothing has happened to these objects
                if (!BattleStarted)
                {
                    return;
                }

                // Snap a report of exit
                BattleReport.WriteReportGroup(group, side == BattleSide.Attack, state);

                // Tell objects to exit from battle
                foreach (var co in group.Where(co => !co.IsDead))
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

                // Clean up objects
                foreach (var co in group.Where(co => !co.Disposed))
                {
                    co.CleanUp();
                }
            }
        }

        #endregion

        #region Battle

        private bool IsBattleValid()
        {
            if (Attackers.Count == 0 || Defenders.Count == 0)
            {
                return false;
            }

            //Check to see if there are still units in the local troop
            if (Defenders.AllCombatObjects().Any(combatObj => !combatObj.IsDead))
            {
                return false;
            }

            //Make sure units can still see each other
            return Attackers.AllCombatObjects().Any(combatObj => Defenders.HasInRange(combatObj));
        }

        private void BattleEnded(bool writeReport)
        {
            if (writeReport)
            {
                BattleReport.CompleteBattle();
            }

            foreach (var combatObj in Defenders.AllCombatObjects())
            {
                if (!combatObj.IsDead)
                {
                    combatObj.ExitBattle();
                }
            }

            foreach (var combatObj in Attackers.AllCombatObjects())
            {
                if (!combatObj.IsDead)
                {
                    combatObj.ExitBattle();
                }
            }

            ExitBattle(this, Attackers, Defenders);

            //have to call to remove from the database
            Attackers.Clear();
            Defenders.Clear();
        }
        
        public bool ExecuteTurn()
        {
            lock (battleLock)
            {
                ICombatList offensiveCombatList;
                ICombatList defensiveCombatList;

                // This will finalize any reports already started.
                BattleReport.CompleteReport(ReportState.Staying);

                #region Battle Start

                bool battleJustStarted = !BattleStarted;
                if (!BattleStarted)
                {
                    // Makes sure battle is valid before even starting it
                    if (!IsBattleValid())
                    {
                        BattleEnded(false);
                        return false;
                    }

                    BattleStarted = true;
                    BattleReport.CreateBattleReport();
                    EnterBattle(this, Attackers, Defenders);
                }

                #endregion

                #region Targeting

                IList<CombatList.Target> currentDefenders;
                CombatObject attackerObject;                
                CombatGroup attackerGroup;

                do
                {
                    #region Find Attacker                    

                    BattleSide sideAttacking;

                    do
                    {
                        if (
                                !battleOrder.NextObject(Round,
                                                        NextToAttack == BattleSide.Defense ? Defenders.ToList() : Attackers.ToList(),
                                                        NextToAttack == BattleSide.Defense ? Attackers.ToList() : Defenders.ToList(),
                                                        out attackerObject,
                                                        out attackerGroup,
                                                        out sideAttacking) && !battleJustStarted)
                        {
                            ++Round;
                            Turn = 0;
                            EnterRound(this, Attackers, Defenders, Round);
                        }

                        if (attackerObject == null || Defenders.Count == 0 || Attackers.Count == 0)
                        {
                            BattleEnded(true);
                            return false;
                        }                        
                    }
                    // Since the EventEnterRound can remove the object from battle, we need to make sure he's still here before we proceed
                    while (!Attackers.AllCombatObjects().Contains(attackerObject) && !Defenders.AllCombatObjects().Contains(attackerObject));

                    // Save the offensive/defensive side relative to the attacker to make it easier in this function to know
                    // which side is attacking
                    offensiveCombatList = sideAttacking == BattleSide.Attack ? Attackers : Defenders;
                    defensiveCombatList = sideAttacking == BattleSide.Attack ? Defenders : Attackers;

                    // Modify the attacker so we know who is the side currently attacking and next round we pick the other side
                    NextToAttack = sideAttacking;

                    #endregion

                    #region Find Target(s)

                    CombatList.BestTargetResult targetResult = offensiveCombatList.GetBestTargets(attackerObject, out currentDefenders, battleFormulas.GetNumberOfHits(attackerObject));

                    if (currentDefenders.Count == 0 || attackerObject.Stats.Atk == 0)
                    {
                        attackerObject.ParticipatedInRound();
                        dbManager.Save(attackerObject);
                        SkippedAttacker(this, attackerObject);

                        // If the attacker can't attack because it has no one in range, then we skip him and find another target right away.
                        if (targetResult == CombatList.BestTargetResult.NoneInRange)
                        {
                            continue;
                        }

                        FlipNextToAttack();
                        return true;
                    }

                    #endregion

                    break;
                }
                while (true);

                #endregion

                bool killedAnyTargets = false;

                int attackIndex = 0;
                foreach (var defender in currentDefenders)
                {
                    // Make sure the target is still in the battle
                    // We just skip incase they left while we were attacking
                    // which means if the attacker is dealing splash they may deal less hits in this fringe case
                    if (!defensiveCombatList.AllCombatObjects().Contains(defender.CombatObject))
                    {
                        continue;
                    }                    

                    // Target is still in battle, attack it
                    if (AttackTarget(offensiveCombatList, defensiveCombatList, attackerObject, defender, attackIndex))
                    {
                        killedAnyTargets = true;
                    }

                    attackIndex++;
                }

                if (attackerObject.Disposed)
                {
                    throw new Exception("Attacker has been improperly disposed");
                }

                attackerObject.ParticipatedInRound();

                dbManager.Save(attackerObject);

                ExitTurn(this, Attackers, Defenders, (int)Turn++);

                // Send back any attackers that have no targets left if the attackers killed a defender
                if (killedAnyTargets && NextToAttack == BattleSide.Attack && Defenders.Count > 0)
                {
                    foreach (var group in Attackers.Where(group => group.All(combatObjects => !Defenders.HasInRange(combatObjects))).ToList())
                    {
                        Remove(group, BattleSide.Attack, ReportState.Exiting);
                    }                                                        
                }

                if (!IsBattleValid())
                {
                    BattleEnded(true);
                    return false;
                }

                FlipNextToAttack();
                return true;
            }
        }

        public CombatGroup GetCombatGroup(uint id)
        {
            return Attackers.FirstOrDefault(group => group.Id == id) ?? Defenders.FirstOrDefault(group => group.Id == id);
        }

        private void FlipNextToAttack()
        {
            NextToAttack = NextToAttack == BattleSide.Attack ? BattleSide.Defense : BattleSide.Attack;
        }

        private bool AttackTarget(ICombatList offensiveCombatList, ICombatList defensiveCombatList, CombatObject attacker, CombatList.Target target, int attackIndex)
        {
            #region Damage

            decimal dmg = battleFormulas.GetAttackerDmgToDefender(attacker, target.CombatObject, NextToAttack == BattleSide.Defense);

            decimal actualDmg;
            target.CombatObject.CalcActualDmgToBeTaken(offensiveCombatList,
                                            defensiveCombatList,
                                            dmg,
                                            attackIndex,
                                            out actualDmg);
            actualDmg = Math.Min(target.CombatObject.Hp, actualDmg);

            Resource defenderDroppedLoot;
            int attackPoints;
            target.CombatObject.TakeDamage(actualDmg, out defenderDroppedLoot, out attackPoints);

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
            if (NextToAttack == BattleSide.Attack)
            {
                // Only give loot if we are attacking the first target in the list
                Resource loot = new Resource();
                if (attackIndex == 0)
                {
                    if (Round >= Config.battle_loot_begin_round)
                    {
                        rewardStrategy.RemoveLoot(attacker, target.CombatObject, out loot);
                    }
                }

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
                    rewardStrategy.GiveDefendersRewards(Defenders.AllCombatObjects(), attackPoints, defenderDroppedLoot ?? new Resource());
                }
            }

            #endregion

            #region Object removal

            bool isDefenderDead = target.CombatObject.IsDead;
            if (isDefenderDead)
            {                
                if (target.Group.IsDead())
                {
                    BattleReport.WriteReportGroup(target.Group, NextToAttack == BattleSide.Attack, ReportState.Dying);
                    defensiveCombatList.Remove(target.Group);
                }
                else
                {
                    BattleReport.WriteReportGroup(target.Group, NextToAttack == BattleSide.Attack, ReportState.Staying);
                    target.Group.Remove(target.CombatObject);
                }                                
                
                ActionAttacked(this, attacker, target.CombatObject, actualDmg);

                UnitRemoved(this, target.CombatObject);

                if (!target.CombatObject.Disposed)
                {
                    target.CombatObject.CleanUp();
                }
            }
            else
            {
                ActionAttacked(this, attacker, target.CombatObject, actualDmg);

                if (!target.CombatObject.Disposed)
                {
                    dbManager.Save(target.CombatObject);
                }
            }

            #endregion

            return target.CombatObject.IsDead;
        }

        #endregion

        #region Events

        public delegate void OnAttack(IBattleManager battle, CombatObject source, CombatObject target, decimal damage);

        public delegate void OnBattle(IBattleManager battle, ICombatList atk, ICombatList def);

        public delegate void OnReinforce(IBattleManager battle, IEnumerable<CombatObject> list);

        public delegate void OnRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round);

        public delegate void OnTurn(IBattleManager battle, ICombatList atk, ICombatList def, int turn);

        public delegate void OnUnitUpdate(IBattleManager battle, CombatObject obj);

        public event OnBattle EnterBattle = delegate { };

        public event OnBattle ExitBattle = delegate { };

        public event OnRound EnterRound = delegate { };

        public event OnTurn EnterTurn = delegate { };

        public event OnTurn ExitTurn = delegate { };

        public event OnReinforce ReinforceAttacker = delegate { };

        public event OnReinforce ReinforceDefender = delegate { };

        public event OnReinforce WithdrawAttacker = delegate { };

        public event OnReinforce WithdrawDefender = delegate { };

        public event OnUnitUpdate UnitAdded = delegate { };

        public event OnUnitUpdate UnitRemoved = delegate { };

        public event OnUnitUpdate UnitUpdated = delegate { };

        public event OnUnitUpdate SkippedAttacker = delegate { };

        public event OnAttack ActionAttacked = delegate { };

        #endregion
    }
}