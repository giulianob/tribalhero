#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Formulas;
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
        private readonly ICombatList attackers;
        private readonly object battleLock = new object();
        private readonly ICombatList defenders;
        private readonly ICombatUnitFactory combatUnitFactory;
        private readonly ObjectTypeFactory objectTypeFactory;
        private readonly LargeIdGenerator groupIdGen;
        private readonly LargeIdGenerator idGen;

        private readonly IDbManager dbManager;
        private readonly IBattleReport report;

        private BattleOrder battleOrder = new BattleOrder(0);
        private bool battleStarted;

        public BattleManager(uint battleId, ICity owner, IDbManager dbManager, IBattleReport battleReport, ICombatListFactory combatListFactory, ICombatUnitFactory combatUnitFactory, ObjectTypeFactory objectTypeFactory)
        {
            attackers = combatListFactory.CreateCombatList();
            defenders = combatListFactory.CreateCombatList();

            groupIdGen = new LargeIdGenerator(ushort.MaxValue);
            idGen = new LargeIdGenerator(ushort.MaxValue);

            BattleId = battleId;
            this.dbManager = dbManager;            
            this.combatUnitFactory = combatUnitFactory;
            this.objectTypeFactory = objectTypeFactory;
            City = owner;
            report = battleReport;

            // Group id 1 is reserved for local troop
            groupIdGen.Set(1);
        }

        public uint BattleId { get; private set; }

        public bool BattleStarted
        {
            get
            {
                return battleStarted;
            }
            set
            {
                battleStarted = value;
            }
        }

        public uint Round { get; set; }

        public uint Turn { get; set; }

        public ICity City { get; set; }

        public ICombatList Attackers
        {
            get
            {
                return attackers;
            }
        }

        public ICombatList Defender
        {
            get
            {
                return defenders;
            }
        }

        public IBattleReport BattleReport
        {
            get
            {
                return report;
            }
        }

        public IEnumerable<ILockable> LockList
        {
            get
            {
                var locks = new HashSet<ILockable> {City};

                foreach (var co in attackers)
                {
                    locks.Add(co);
                }

                foreach (var co in defenders)
                {
                    locks.Add(co);
                }

                return locks.ToArray();
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
                return new[] {new DbColumn("city_id", City.Id, DbType.UInt32)};
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
                               new DbColumn("battle_id", BattleId, DbType.UInt32), new DbColumn("battle_started", battleStarted, DbType.Boolean),
                               new DbColumn("round", Round, DbType.UInt32), new DbColumn("turn", Turn, DbType.UInt32),
                               new DbColumn("report_flag", report.ReportFlag, DbType.Boolean), new DbColumn("report_started", report.ReportStarted, DbType.Boolean),
                               new DbColumn("report_id", report.ReportId, DbType.UInt32)
                       };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public CombatObject GetCombatObject(uint id)
        {
            return attackers.FirstOrDefault(co => co.Id == id) ?? defenders.FirstOrDefault(co => co.Id == id);
        }

        public bool CanWatchBattle(IPlayer player, out int roundsLeft)
        {
            roundsLeft = 0;

            lock (battleLock)
            {
                if (player == City.Owner)
                    return true;

                int defendersRoundsLeft = int.MaxValue;
                int attackersRoundsLeft = int.MaxValue;

                var playersDefenders = defenders.Where(co => co.City.Owner == player).ToList();
                if (playersDefenders.Any()) {
                    defendersRoundsLeft = playersDefenders.Min(co => Config.battle_min_rounds - co.RoundsParticipated);
                    if (defendersRoundsLeft < 0)
                        return true;
                }

                var playersAttackers = attackers.Where(co => co.City.Owner == player).ToList();
                if (playersAttackers.Any())
                {
                    attackersRoundsLeft = playersAttackers.Min(co => Config.battle_min_rounds - co.RoundsParticipated);
                    if (attackersRoundsLeft < 0)
                        return true;
                }

                roundsLeft = Math.Min(attackersRoundsLeft, defendersRoundsLeft);
                if (roundsLeft == int.MaxValue)
                    roundsLeft = 0;
                else
                    roundsLeft++;

                return false;
            }
        }

        #region Database Loader

        public void DbLoaderAddToLocal(CombatStructure structure, uint id)
        {
            structure.Id = id;

            if (structure.IsDead)
                return;

            defenders.Add(structure, false);

            idGen.Set((int)structure.Id);
        }

        public void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal)
        {
            obj.Id = id;

            if (obj.IsDead)
                return;

            if (isLocal)
                defenders.Add(obj, false);
            else
                attackers.Add(obj, false);

            idGen.Set((int)obj.Id);
            groupIdGen.Set((int)obj.GroupId);
        }

        #endregion

        #region Adding/Removing from Battle

        public void AddToLocal(IEnumerable<ITroopStub> objects, ReportState state)
        {
            AddToCombatList(objects, defenders, true, state);
        }

        public void AddToLocal(IEnumerable<IStructure> objects)
        {
            lock (battleLock)
            {
                var list = new List<CombatObject>();

                bool added = false;

                foreach (var structure in objects)
                {
                    if (structure.Stats.Hp == 0 || defenders.Any(co => co.CompareTo(structure) == 0) || objectTypeFactory.IsStructureType("Unattackable", structure) || structure.IsBlocked)
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
                
                    defenders.Add(combatObject);
                    list.Add(combatObject);
                }

                if (battleStarted && added)
                {
                    report.WriteReportObjects(list, false, ReportState.Staying);
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
            AddToCombatList(objects, attackers, false, ReportState.Entering);
        }

        public void AddToDefense(IEnumerable<ITroopStub> objects)
        {
            AddToCombatList(objects, defenders, false, ReportState.Entering);
        }

        public void RemoveFromAttack(IEnumerable<ITroopStub> objects, ReportState state)
        {
            RemoveFromCombatList(objects, attackers, state);
        }

        public void RemoveFromDefense(IEnumerable<ITroopStub> objects, ReportState state)
        {
            RemoveFromCombatList(objects, defenders, state);
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
                            if (combatList == defenders)
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

                if (battleStarted && added)
                {
                    if (combatList == Attackers)
                    {
                        report.WriteReportObjects(list, true, state);
                        ReinforceAttacker(this, list);
                    }
                    else if (combatList == Defender)
                    {
                        report.WriteReportObjects(list, false, state);
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

                if (!battleStarted)
                    return;

                // Snap a report of exit
                report.WriteReportObjects(list, combatList == Attackers, state);

                // Tell objects to exit from battle
                foreach (var co in list.Where(co => !co.IsDead))
                    co.ExitBattle();

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
                foreach (var co in list)
                {
                    if (!co.Disposed)
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

            IEnumerator<CombatObject> defIter = defenders.GetEnumerator();
            IEnumerator<CombatObject> atkIter = attackers.GetEnumerator();

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
            if (attackers.Count == 0 || defenders.Count == 0)
                return false;

            //Check to see if there are still units in the local troop
            bool localUnits = false;
            foreach (var combatObj in defenders.Where(combatObj => !combatObj.IsDead))
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
            return attackers.Any(combatObj => defenders.HasInRange(combatObj));
        }

        private void BattleEnded(bool writeReport)
        {
            if (writeReport)
            {
                report.CompleteBattle();
            }

            foreach (var combatObj in defenders)
            {
                if (!combatObj.IsDead)
                    combatObj.ExitBattle();
            }

            foreach (var combatObj in attackers)
            {
                if (!combatObj.IsDead)
                    combatObj.ExitBattle();
            }

            ExitBattle(this, attackers, defenders);

            //have to call to remove from the database
            attackers.Clear();
            defenders.Clear();            
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
                report.CompleteReport(ReportState.Staying);

                #region Battle Start

                bool battleJustStarted = !battleStarted;
                if (!battleStarted)
                {
                    RefreshBattleOrder();

                    // Makes sure battle is valid before even starting it
                    if (!IsBattleValid())
                    {
                        BattleEnded(false);
                        return false;
                    }

                    battleStarted = true;
                    report.CreateBattleReport();
                    EnterBattle(this, attackers, defenders);
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

                        if (currentAttacker == null || defenders.Count == 0 || attackers.Count == 0)
                        {
                            BattleEnded(true);
                            return false;
                        }                        
                    } while (!battleOrder.Contains(currentAttacker)); // Since the EventEnterRound can remove the object from battle, we need to make sure he's still here before we proceed

                    #endregion

                    #region Find Target(s)

                    CombatList.BestTargetResult targetResult;

                    if (currentAttacker.CombatList == attackers)
                        targetResult = defenders.GetBestTargets(currentAttacker, out currentDefenders, BattleFormulas.Current.GetNumberOfHits(currentAttacker));
                    else if (currentAttacker.CombatList == defenders)
                        targetResult = attackers.GetBestTargets(currentAttacker, out currentDefenders, BattleFormulas.Current.GetNumberOfHits(currentAttacker));
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
                    if (currentAttacker.CombatList == attackers && !defenders.Contains(defender))
                        continue;
                    
                    if (currentAttacker.CombatList == defenders && !attackers.Contains(defender))
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
                    if (currentAttacker.CombatList == attackers && defenders.Count > 0)
                    {
                        //Since the list of attackers will be changing, we need to keep reiterating through it until there are no more to remove
                        AttackCombatUnit co;
                        do
                        {
                            co = attackers.OfType<AttackCombatUnit>().FirstOrDefault(x => !defenders.HasInRange(x));
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

            decimal dmg = BattleFormulas.Current.GetDamage(attacker, defender, attacker.CombatList == defenders);
            decimal actualDmg;
            Resource lostResource;
            int attackPoints;

            #region Miss Chance
            var missChance = BattleFormulas.Current.MissChance(attacker.CombatList.Sum(x => x.Upkeep), defender.CombatList.Sum(x => x.Upkeep));
            if (missChance > 0) {
                var rand = (int)(Config.Random.NextDouble() * 100);
                if (rand <= missChance)
                    dmg /= 2;
            }
            #endregion

            #region Splash Damage Reduction
            if (attackIndex > 0)
            {
                decimal reduction = defender.City.Technologies.GetEffects(EffectCode.SplashReduction, EffectInheritance.SelfAll).Where(effect => BattleFormulas.Current.UnitStatModCheck(defender.Stats.Base, TroopBattleGroup.Defense, (string)effect.Value[1])).DefaultIfEmpty().Max(x => x == null ? 0 : (int)x.Value[0]);
                reduction = (100 - reduction)/100;
                dmg = reduction*dmg;
            }
            #endregion

            defender.CalculateDamage(dmg, out actualDmg);
            defender.TakeDamage(actualDmg, out lostResource, out attackPoints);

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

            if (attacker.CombatList == Attackers)
            {
                // Only give loot if we are attacking the first target in the list
                if (attackIndex == 0)
                {
                    Resource loot=null;
                    City.BeginUpdate();
                    if (Round >= Config.battle_loot_begin_round)
                    {
                        loot = BattleFormulas.Current.GetRewardResource(attacker, defender);
                        City.Resource.Subtract(loot, Formula.Current.HiddenResource(City,true), out loot);
                    } 
                    attacker.ReceiveReward(attackPoints, loot ?? new Resource() );
                    City.EndUpdate();
                }
                else
                {
                    City.BeginUpdate();
                    attacker.ReceiveReward(attackPoints, new Resource());
                    City.EndUpdate();
                }
            }
            else
            {
                // Give back any lost resources if the attacker dropped them
                if (lostResource != null && !lostResource.Empty)
                {
                    City.BeginUpdate();
                    City.Resource.Add(lostResource);
                }

                // If the defender killed someone then give the city defense points
                if (attackPoints > 0)
                {
                    // Give anyone stationed defense points as well
                    // DONT convert this to LINQ because I'm not sure how it might affect the list inside of the loop that keeps changing
                    var uniqueCities = new HashSet<ICity>();

                    foreach (var co in defenders)
                    {
                        if (!uniqueCities.Add(co.City))
                        {
                            continue;
                        }

                        if (!co.City.IsUpdating)
                        {
                            co.City.BeginUpdate();
                        }

                        co.City.DefensePoint += attackPoints;
                        co.City.EndUpdate();                        
                    }

                    var tribes = new List<ITribe>(uniqueCities.Where(w=>w.Owner.Tribesman!=null).Select(s => s.Owner.Tribesman.Tribe).Distinct());
                    ThreadPool.QueueUserWorkItem(delegate {
                        foreach (var tribe in tribes) {
                            using (Concurrency.Current.Lock(tribe)) {
                                tribe.DefensePoint += attackPoints;
                            }
                        }
                    });
                }

                if (City.IsUpdating)
                    City.EndUpdate();
            }

            #endregion

            #region Object removal

            bool isDefenderDead = defender.IsDead;
            if (isDefenderDead)
            {                
                battleOrder.Remove(defender);

                if (attacker.CombatList == attackers)
                {
                    defenders.Remove(defender);
                    report.WriteReportObject(defender, false, GroupIsDead(defender, defenders) ? ReportState.Dying : ReportState.Staying);
                }
                else if (attacker.CombatList == defenders)
                {
                    attackers.Remove(defender);
                    report.WriteReportObject(defender, true, GroupIsDead(defender, attackers) ? ReportState.Dying : ReportState.Staying);
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