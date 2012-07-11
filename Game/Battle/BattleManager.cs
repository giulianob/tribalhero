#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Battle
{
    public class BattleManager : IBattleManager
    {
        public const string DB_TABLE = "battle_managers";

        private readonly object battleLock = new object();

        private readonly ICombatUnitFactory combatUnitFactory;
        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly BattleFormulas battleFormulas;

        private readonly LargeIdGenerator groupIdGen;
        private readonly LargeIdGenerator idGen;

        private readonly IRewardStrategy rewardStrategy;

        private readonly IDbManager dbManager;

        private BattleOrder battleOrder = new BattleOrder(0);

        public BattleManager(uint battleId, BattleLocation location, BattleOwner owner, IRewardStrategy rewardStrategy, IDbManager dbManager, IBattleReport battleReport, ICombatListFactory combatListFactory, ICombatUnitFactory combatUnitFactory, ObjectTypeFactory objectTypeFactory, BattleFormulas battleFormulas)
        {
            groupIdGen = new LargeIdGenerator(ushort.MaxValue);
            idGen = new LargeIdGenerator(ushort.MaxValue);
            // Group id 1 is always reserved for local troop
            groupIdGen.Set(1);

            BattleId = battleId;
            Location = location;
            Owner = owner;
            BattleReport = battleReport;
            Attackers = combatListFactory.CreateCombatList();
            Defender = combatListFactory.CreateCombatList();

            this.rewardStrategy = rewardStrategy;
            this.dbManager = dbManager;
            this.combatUnitFactory = combatUnitFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.battleFormulas = battleFormulas;
        }

        public uint BattleId { get; private set; }

        public BattleLocation Location { get; private set; }

        public BattleOwner Owner { get; private set; }

        public bool BattleStarted { get; set; }

        public uint Round { get; set; }

        public uint Turn { get; set; }

        public ICombatList Attackers { get; private set; }

        public ICombatList Defender { get; private set; }

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

                foreach (var co in Defender)
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
                return new[] { new DbDependency("BattleReport", true, true) };
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("battle_started", BattleStarted, DbType.Boolean),
                               new DbColumn("round", Round, DbType.UInt32), 
                               new DbColumn("turn", Turn, DbType.UInt32),
                               new DbColumn("report_flag", BattleReport.ReportFlag, DbType.Boolean), 
                               new DbColumn("report_started", BattleReport.ReportStarted, DbType.Boolean),
                               new DbColumn("report_id", BattleReport.ReportId, DbType.UInt32),
                               new DbColumn("owner_type", Owner.Type.ToString(), DbType.String, 15),
                               new DbColumn("owner_id", Owner.Id, DbType.UInt32),
                               new DbColumn("location_type", Location.Type.ToString(), DbType.String, 15),
                               new DbColumn("location_id", Location.Id, DbType.UInt32)
                       };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public CombatObject GetCombatObject(uint id)
        {
            return Attackers.FirstOrDefault(co => co.Id == id) ?? Defender.FirstOrDefault(co => co.Id == id);
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
                var playersDefenders = Defender.Where(co => co.BelongsTo(player)).ToList();
                if (playersDefenders.Any()) {
                    defendersRoundsLeft = playersDefenders.Min(co => Config.battle_min_rounds - co.RoundsParticipated);
                    if (defendersRoundsLeft < 0)
                    {
                        return true;
                    }
                }

                // Check if player has an attacker that is over the minimum battle rounds
                var playersAttackers = Attackers.Where(co => co.BelongsTo(player)).ToList();
                if (playersAttackers.Any())
                {
                    attackersRoundsLeft = playersAttackers.Min(co => Config.battle_min_rounds - co.RoundsParticipated);
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

        public void DbLoaderAddToLocal(CombatStructure structure, uint id)
        {
            structure.Id = id;

            if (structure.IsDead)
                return;

            Defender.Add(structure, false);

            idGen.Set((int)structure.Id);
        }

        public void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal)
        {
            obj.Id = id;

            if (obj.IsDead)
                return;

            if (isLocal)
                Defender.Add(obj, false);
            else
                Attackers.Add(obj, false);

            idGen.Set((int)obj.Id);
            groupIdGen.Set((int)obj.GroupId);
        }

        #endregion

        #region Adding/Removing from Battle

        public void AddToLocal(IEnumerable<ITroopStub> objects, ReportState state)
        {
            AddToCombatList(objects, Defender, true, state);
        }

        public void AddToLocal(IEnumerable<IStructure> objects)
        {
            lock (battleLock)
            {
                var list = new List<CombatObject>();

                bool added = false;

                foreach (var structure in objects)
                {
                    if (structure.Stats.Hp == 0 || Defender.Any(co => co.CompareTo(structure) == 0) || objectTypeFactory.IsStructureType("Unattackable", structure) || structure.IsBlocked)
                    {
                        continue;
                    }

                    // Don't add main building if lvl 1 or if a building is lvl 0
                    if ((objectTypeFactory.IsStructureType("Undestroyable", structure) && structure.Lvl <= 1) || (structure.Lvl == 0) ||
                        objectTypeFactory.IsStructureType("Noncombatstructure", structure))
                    {
                        continue;
                    }

                    added = true;

                    structure.BeginUpdate();
                    structure.State = GameObjectState.BattleState(structure.City.Id);
                    structure.EndUpdate();

                    CombatObject combatObject = combatUnitFactory.CreateStructureCombatUnit(this, structure);
                    combatObject.Id = (uint)idGen.GetNext();
                    combatObject.GroupId = 1;
                    combatObject.LastRound = Round;
                
                    Defender.Add(combatObject);
                    list.Add(combatObject);
                }

                if (BattleStarted && added)
                {
                    BattleReport.WriteReportObjects(list, false, ReportState.Staying);
                    ReinforceDefender(this, list);
                    RefreshBattleOrder();
                }
            }
        }

        public void AddToAttack(ITroopStub stub)
        {
            var list = new List<ITroopStub> {stub};
            AddToAttack(list);
        }

        public void AddToAttack(IEnumerable<ITroopStub> objects)
        {
            AddToCombatList(objects, Attackers, false, ReportState.Entering);
        }

        public void AddToDefense(IEnumerable<ITroopStub> objects)
        {
            AddToCombatList(objects, Defender, false, ReportState.Entering);
        }

        public void RemoveFromAttack(IEnumerable<ITroopStub> objects, ReportState state)
        {
            RemoveFromCombatList(objects, Attackers, state);
        }

        public void RemoveFromDefense(IEnumerable<ITroopStub> objects, ReportState state)
        {
            RemoveFromCombatList(objects, Defender, state);
        }

        private void AddToCombatList(IEnumerable<ITroopStub> objects, ICombatList combatList, bool isLocal, ReportState state)
        {
            lock (battleLock)
            {
                var list = new List<CombatObject>();

                bool added = false;

                foreach (var obj in objects)
                {
                    uint groupId = isLocal ? 1 : (uint)groupIdGen.GetNext();

                    foreach (var formation in obj)
                    {
                        if (formation.Type == FormationType.Garrison || formation.Type == FormationType.InBattle)
                        {
                            continue;
                        }

                        //if it's our local troop then it should be in the battle formation since 
                        //it will be moved by the battle manager (who is calling this function) to the in battle formation
                        //There should be a better way to do this but I cant figure it out right now
                        FormationType formationType = formation.Type;
                        if (isLocal)
                        {
                            formationType = FormationType.InBattle;
                        }

                        foreach (var kvp in formation)
                        {
                            // ReSharper disable CoVariantArrayConversion
                            CombatObject[] combatObjects;
                            if (combatList == Defender)
                            {
                                combatObjects = combatUnitFactory.CreateDefenseCombatUnit(this, obj, formationType, kvp.Key, kvp.Value);
                            }
                            else
                            {
                                combatObjects = combatUnitFactory.CreateAttackCombatUnit(this, obj.TroopObject, formationType, kvp.Key, kvp.Value);
                            }
                            // ReSharper restore CoVariantArrayConversion

                            foreach (var unit in combatObjects)
                            {
                                added = true;
                                unit.LastRound = Round;
                                unit.Id = (uint)idGen.GetNext();
                                unit.GroupId = groupId;
                                combatList.Add(unit);
                                list.Add(unit);
                            }
                        }
                    }
                }

                if (BattleStarted && added)
                {
                    if (combatList == Attackers)
                    {
                        BattleReport.WriteReportObjects(list, true, state);
                        ReinforceAttacker(this, list);
                    }
                    else if (combatList == Defender)
                    {
                        BattleReport.WriteReportObjects(list, false, state);
                        ReinforceDefender(this, list);
                    }

                    RefreshBattleOrder();
                }
            }
        }

        private void RemoveFromCombatList(IEnumerable<ITroopStub> objects, ICombatList combatList, ReportState state)
        {
            lock (battleLock)
            {
                var list = (from obj in combatList
                            where obj is ICombatUnit && objects.Contains(((ICombatUnit)obj).TroopStub)
                            select obj).ToList();

                combatList.RemoveAll(list.Contains);

                if (!BattleStarted)
                    return;

                // Snap a report of exit
                BattleReport.WriteReportObjects(list, combatList == Attackers, state);

                // Tell objects to exit from battle
                foreach (var co in list.Where(co => !co.IsDead))
                {
                    co.ExitBattle();
                }

                // Send exit events
                if (combatList == Attackers)
                {
                    WithdrawAttacker(this, list);
                }
                else if (combatList == Defender)
                {
                    WithdrawDefender(this, list);
                }

                // Clean up objects
                foreach (var co in list.Where(co => !co.Disposed))
                {
                    co.CleanUp();
                }

                // Refresh battle order
                RefreshBattleOrder();
            }
        }

        #endregion

        #region Battle

        public void RefreshBattleOrder()
        {
            battleOrder = new BattleOrder(Round);

            IEnumerator<CombatObject> defIter = Defender.GetEnumerator();
            IEnumerator<CombatObject> atkIter = Attackers.GetEnumerator();

            bool atkDone = false;
            bool defDone = false;

            while (!atkDone || !defDone)
            {
                if (!defDone && defIter.MoveNext())
                    battleOrder.Add(defIter.Current);
                else
                    defDone = true;

                if (!atkDone && atkIter.MoveNext())
                    battleOrder.Add(atkIter.Current);
                else
                    atkDone = true;
            }
        }

        private bool IsBattleValid()
        {
            if (Attackers.Count == 0 || Defender.Count == 0)
                return false;

            //Check to see if there are still units in the local troop
            bool localUnits = false;
            foreach (var combatObj in Defender.Where(combatObj => !combatObj.IsDead))
            {
                if (combatObj is CombatStructure)
                {
                    localUnits = true;
                    break;
                }

                var cu = combatObj as DefenseCombatUnit;
                if (cu == null)
                    continue;

                localUnits = true;
                break;
            }

            if (!localUnits)
                return false;

            //Make sure units can still see each other
            return Attackers.Any(combatObj => Defender.HasInRange(combatObj));
        }

        private void BattleEnded(bool writeReport)
        {
            if (writeReport)
            {
                BattleReport.CompleteBattle();
            }

            foreach (var combatObj in Defender)
            {
                if (!combatObj.IsDead)
                    combatObj.ExitBattle();
            }

            foreach (var combatObj in Attackers)
            {
                if (!combatObj.IsDead)
                    combatObj.ExitBattle();
            }

            ExitBattle(this, Attackers, Defender);

            //have to call to remove from the database
            Attackers.Clear();
            Defender.Clear();            
        }

        private bool GroupIsDead(CombatObject co, IEnumerable<CombatObject> combatList)
        {
            return combatList.All(combatObject => combatObject.GroupId != co.GroupId || combatObject.IsDead);
        }

        public bool ExecuteTurn()
        {
            lock (battleLock)
            {
                // This will finalize any reports already started.
                BattleReport.CompleteReport(ReportState.Staying);

                #region Battle Start

                bool battleJustStarted = !BattleStarted;
                if (!BattleStarted)
                {
                    RefreshBattleOrder();

                    // Makes sure battle is valid before even starting it
                    if (!IsBattleValid())
                    {
                        BattleEnded(false);
                        return false;
                    }

                    BattleStarted = true;
                    BattleReport.CreateBattleReport();
                    EnterBattle(this, Attackers, Defender);
                }

                #endregion

                #region Targeting

                CombatObject currentAttacker;
                IList<CombatObject> currentDefenders;

                do
                {
                    #region Find Attacker                    

                    do
                    {
                        if (!battleOrder.NextObject(out currentAttacker) && !battleJustStarted)
                        {
                            ++Round;
                            battleOrder.ParticipatedInRound();
                            Turn = 0;
                            EnterRound(this, Attackers, Defender, Round);
                        }

                        if (currentAttacker == null || Defender.Count == 0 || Attackers.Count == 0)
                        {
                            BattleEnded(true);
                            return false;
                        }                        
                    } while (!battleOrder.Contains(currentAttacker)); // Since the EventEnterRound can remove the object from battle, we need to make sure he's still here before we proceed

                    #endregion

                    #region Find Target(s)

                    CombatList.BestTargetResult targetResult;

                    if (currentAttacker.CombatList == Attackers)
                        targetResult = Defender.GetBestTargets(currentAttacker, out currentDefenders, battleFormulas.GetNumberOfHits(currentAttacker));
                    else if (currentAttacker.CombatList == Defender)
                        targetResult = Attackers.GetBestTargets(currentAttacker, out currentDefenders, battleFormulas.GetNumberOfHits(currentAttacker));
                    else
                        throw new Exception("How can this happen");

                    if (currentDefenders.Count == 0 || currentAttacker.Stats.Atk == 0)
                    {
                        currentAttacker.ParticipatedInRound();
                        dbManager.Save(currentAttacker);
                        SkippedAttacker(this, currentAttacker);

                        // If the attacker can't attack because it has no one in range, then we skip him and find another target right away.
                        if (targetResult == CombatList.BestTargetResult.NoneInRange)
                            continue;

                        return true;
                    }

                    #endregion

                    break;
                } while (true);

                #endregion

                bool killedADefender = false;

                int attackIndex = 0;
                foreach (var defender in currentDefenders)
                {
                    // Make sure the target is still in the battle
                    if (currentAttacker.CombatList == Attackers && !Defender.Contains(defender))
                        continue;
                    
                    if (currentAttacker.CombatList == Defender && !Attackers.Contains(defender))
                        continue;
                    
                    // Target is still in battle, attack it
                    if (AttackTarget(currentAttacker, defender, attackIndex))
                        killedADefender = true;
                    attackIndex++;
                }

                if (currentAttacker.Disposed)
                    throw new Exception("Attacker has been improperly disposed");

                currentAttacker.ParticipatedInRound();

                dbManager.Save(currentAttacker);

                ExitTurn(this, Attackers, Defender, (int)Turn++);

                // Send back any attackers that have no targets left
                if (killedADefender)
                {
                    if (currentAttacker.CombatList == Attackers && Defender.Count > 0)
                    {
                        //Since the list of attackers will be changing, we need to keep reiterating through it until there are no more to remove
                        AttackCombatUnit co;
                        do
                        {
                            co = Attackers.OfType<AttackCombatUnit>().FirstOrDefault(x => !Defender.HasInRange(x));
                            if (co != null)
                                RemoveFromAttack(new List<ITroopStub> {(co).TroopStub}, ReportState.Exiting);
                        } while (co != null);
                    }
                }

                if (!IsBattleValid())
                {
                    BattleEnded(true);
                    return false;
                }

                return true;
            }
        }

        private bool AttackTarget(CombatObject attacker, CombatObject defender, int attackIndex)
        {
            #region Damage

            decimal dmg = battleFormulas.GetAttackerDmgToDefender(attacker, defender, attacker.CombatList == Defender);            

            decimal actualDmg;
            defender.CalcActualDmgToBeTaken(attacker.CombatList, defender.CombatList, dmg, attackIndex, out actualDmg);
            actualDmg = Math.Min(defender.Hp, actualDmg);

            Resource defenderDroppedLoot;
            int attackPoints;
            defender.TakeDamage(actualDmg, out defenderDroppedLoot, out attackPoints);

            attacker.DmgDealt += actualDmg;
            attacker.MaxDmgDealt = (ushort)Math.Max(attacker.MaxDmgDealt, actualDmg);
            attacker.MinDmgDealt = (ushort)Math.Min(attacker.MinDmgDealt, actualDmg);
            ++attacker.HitDealt;
            attacker.HitDealtByUnit += attacker.Count;

            defender.DmgRecv += actualDmg;
            defender.MaxDmgRecv = (ushort)Math.Max(defender.MaxDmgRecv, actualDmg);
            defender.MinDmgRecv = (ushort)Math.Min(defender.MinDmgRecv, actualDmg);
            ++defender.HitRecv;
            #endregion

            #region Loot and Attack Points

            // NOTE: In the following rewardStrategy calls we are in passing in whatever the existing implementations
            // of reward startegy need. If some specific implementation needs more info then add more params or change it to
            // take the entire BattleManager as a param.
            if (attacker.CombatList == Attackers)
            {
                // Only give loot if we are attacking the first target in the list
                Resource loot = new Resource();
                if (attackIndex == 0)
                {                    
                    if (Round >= Config.battle_loot_begin_round)
                    {
                        rewardStrategy.RemoveLoot(attacker, defender, out loot);
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
                    rewardStrategy.GiveDefendersRewards(Defender, attackPoints, defenderDroppedLoot ?? new Resource());
                }
            }

            #endregion

            #region Object removal

            bool isDefenderDead = defender.IsDead;
            if (isDefenderDead)
            {                
                battleOrder.Remove(defender);

                if (attacker.CombatList == Attackers)
                {
                    Defender.Remove(defender);
                    BattleReport.WriteReportObject(defender, false, GroupIsDead(defender, Defender) ? ReportState.Dying : ReportState.Staying);
                }
                else if (attacker.CombatList == Defender)
                {
                    Attackers.Remove(defender);
                    BattleReport.WriteReportObject(defender, true, GroupIsDead(defender, Attackers) ? ReportState.Dying : ReportState.Staying);
                }

                ActionAttacked(this, attacker, defender, actualDmg);

                UnitRemoved(this, defender);

                if (!defender.Disposed)
                    defender.CleanUp();
            }
            else
            {
                ActionAttacked(this, attacker, defender, actualDmg);

                if (!defender.Disposed)
                    dbManager.Save(defender);
            }

            #endregion            

            return defender.IsDead;
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