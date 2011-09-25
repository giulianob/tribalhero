#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Comm;
using Game.Comm.Channel;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Ninject;
using Ninject.Parameters;
using Persistance;

#endregion

namespace Game.Battle
{
    public interface IBattleManager : IPersistableObject
    {
        uint BattleId { get; set; }
        bool BattleStarted { get; set; }
        uint Round { get; set; }
        uint Turn { get; set; }
        City City { get; set; }
        CombatList Attacker { get; }
        CombatList Defender { get; }
        IBattleReport BattleReport { get; }
        ReportedObjects ReportedObjects { get; }
        ReportedTroops ReportedTroops { get; }
        City[] LockList { get; }
        void Subscribe(Session session);
        void Unsubscribe(Session session);
        CombatObject GetCombatObject(uint id);
        bool CanWatchBattle(Player player, out int roundsLeft);
        void DbLoaderAddToLocal(CombatStructure structure, uint id);
        void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal);
        void AddToLocal(IEnumerable<TroopStub> objects, ReportState state);
        void AddToLocal(IEnumerable<Structure> objects);
        void AddToAttack(TroopStub stub);
        void AddToAttack(IEnumerable<TroopStub> objects);
        void AddToDefense(IEnumerable<TroopStub> objects);
        void RemoveFromAttack(IEnumerable<TroopStub> objects, ReportState state);
        void RemoveFromLocal(IEnumerable<Structure> objects, ReportState state);
        void RemoveFromDefense(IEnumerable<TroopStub> objects, ReportState state);
        void RefreshBattleOrder();
        bool GroupIsDead(CombatObject co, CombatList combatList);
        bool ExecuteTurn();
        event BattleManager.OnBattle EnterBattle;
        event BattleManager.OnBattle ExitBattle;
        event BattleManager.OnRound EnterRound;
        event BattleManager.OnTurn EnterTurn;
        event BattleManager.OnTurn ExitTurn;
        event BattleManager.OnReinforce ReinforceAttacker;
        event BattleManager.OnReinforce ReinforceDefender;
        event BattleManager.OnReinforce WithdrawAttacker;
        event BattleManager.OnReinforce WithdrawDefender;
        event BattleManager.OnUnitUpdate UnitAdded;
        event BattleManager.OnUnitUpdate UnitRemoved;
        event BattleManager.OnUnitUpdate UnitUpdated;
        event BattleManager.OnUnitUpdate SkippedAttacker;
        event BattleManager.OnAttack ActionAttacked;
    }

    public class BattleManager : IBattleManager
    {
        public delegate IBattleManager Factory(City owner);

        public const string DB_TABLE = "battle_managers";
        private readonly CombatList attackers;
        private readonly object battleLock = new object();
        private readonly CombatList defenders;
        private readonly LargeIdGenerator groupIdGen;
        private readonly LargeIdGenerator idGen;

        private readonly IDbManager dbManager;
        private readonly IBattleReport report;

        private uint battleId;
        private BattleOrder battleOrder = new BattleOrder(0);
        private bool battleStarted;
        private City city;
        private uint round;
        private uint turn;

        public BattleManager(ICity owner, IDbManager dbManager, IBattleChannel battleChannel, IBattleReport battleReport)
        {
            attackers = Ioc.Kernel.Get<CombatList>();
            defenders = Ioc.Kernel.Get<CombatList>();

            groupIdGen = new LargeIdGenerator(ushort.MaxValue);
            idGen = new LargeIdGenerator(ushort.MaxValue);

            city = (City)owner;
            this.dbManager = dbManager;
            report = battleReport;  
            channel = battleChannel;
            groupIdGen.Set(1);

            ActionAttacked += battleChannel.BattleActionAttacked;
            SkippedAttacker += battleChannel.BattleSkippedAttacker;
            ReinforceAttacker += battleChannel.BattleReinforceAttacker;
            ReinforceDefender += battleChannel.BattleReinforceDefender;
            ExitBattle += battleChannel.BattleExitBattle;
            EnterRound += battleChannel.BattleEnterRound;
        }

        private readonly IBattleChannel channel;

        public uint BattleId
        {
            get
            {
                return battleId;
            }
            set
            {
                battleId = value;
            }
        }

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

        public uint Round
        {
            get
            {
                return round;
            }
            set
            {
                round = value;
            }
        }

        public uint Turn
        {
            get
            {
                return turn;
            }
            set
            {
                turn = value;
            }
        }

        public City City
        {
            get
            {
                return city;
            }
            set
            {
                city = value;
            }
        }

        public CombatList Attacker
        {
            get
            {
                return attackers;
            }
        }

        public CombatList Defender
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

        public ReportedObjects ReportedObjects
        {
            get
            {
                return report.ReportedObjects;
            }
        }

        public ReportedTroops ReportedTroops
        {
            get
            {
                return report.ReportedTroops;
            }
        }

        public City[] LockList
        {
            get
            {
                var cities = new Dictionary<uint, City> {{city.Id, city}};

                foreach (var co in attackers)
                    cities[co.City.Id] = co.City;

                foreach (var co in defenders)
                    cities[co.City.Id] = co.City;

                return cities.Values.ToArray();
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
                return new[] {new DbColumn("city_id", city.Id, DbType.UInt32)};
            }
        }

        public DbDependency[] DbDependencies
        {
            get
            {
                return new[] {new DbDependency("ReportedObjects", true, true), new DbDependency("ReportedTroops", true, true)};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("battle_id", battleId, DbType.UInt32), new DbColumn("battle_started", battleStarted, DbType.Boolean),
                               new DbColumn("round", round, DbType.UInt32), new DbColumn("turn", turn, DbType.UInt32),
                               new DbColumn("report_flag", report.ReportFlag, DbType.Boolean), new DbColumn("report_started", report.ReportStarted, DbType.Boolean),
                               new DbColumn("report_id", report.ReportId, DbType.UInt32)
                       };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public void Subscribe(Session session)
        {
            try
            {
                Global.Channel.Subscribe(session, "/BATTLE/" + city.Id);
            }
            catch(DuplicateSubscriptionException)
            {
            }
        }

        public void Unsubscribe(Session session)
        {
            Global.Channel.Unsubscribe(session, "/BATTLE/" + city.Id);
        }

        public CombatObject GetCombatObject(uint id)
        {
            return attackers.FirstOrDefault(co => co.Id == id) ?? defenders.FirstOrDefault(co => co.Id == id);
        }

        public bool CanWatchBattle(Player player, out int roundsLeft)
        {
            roundsLeft = 0;

            lock (battleLock)
            {
                if (player == city.Owner)
                    return true;

                int defendersRoundsLeft = int.MaxValue;
                int attackersRoundsLeft = int.MaxValue;

                if (defenders.Count > 0)
                {
                    defendersRoundsLeft =
                            defenders.Min(co => co.City.Owner == player ? Math.Max(0, Config.battle_min_rounds - co.RoundsParticipated) : int.MaxValue);
                    if (defendersRoundsLeft == 0)
                        return true;
                }

                if (attackers.Count > 0)
                {
                    attackersRoundsLeft =
                            attackers.Min(co => co.City.Owner == player ? Math.Max(0, Config.battle_min_rounds - co.RoundsParticipated) : int.MaxValue);
                    if (attackersRoundsLeft == 0)
                        return true;
                }

                roundsLeft = Math.Min(attackersRoundsLeft, defendersRoundsLeft);
                if (roundsLeft == int.MaxValue)
                    roundsLeft = 0;

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

        public void AddToLocal(IEnumerable<TroopStub> objects, ReportState state)
        {
            AddToCombatList(objects, defenders, true, state);
        }

        public void AddToLocal(IEnumerable<Structure> objects)
        {
            lock (battleLock)
            {
                var list = new List<CombatObject>();

                bool added = false;

                foreach (var obj in objects)
                {
                    if (obj.Stats.Hp == 0 || defenders.Contains(obj) || Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Unattackable", obj) || obj.IsBlocked)
                        continue;

                    // Don't add main building if lvl 1 or if a building is lvl 0
                    if ((Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Undestroyable", obj) && obj.Lvl <= 1) || (obj.Lvl == 0) ||
                        Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Noncombatstructure", obj))
                        continue;

                    added = true;

                    obj.BeginUpdate();
                    obj.State = GameObjectState.BattleState(obj.City.Id);
                    obj.EndUpdate();

                    CombatObject combatObject = new CombatStructure(this, obj, BattleFormulas.LoadStats(obj)) {Id = (uint)idGen.GetNext(), GroupId = 1};
                    defenders.Add(combatObject);
                    list.Add(combatObject);
                }

                if (battleStarted && added)
                {
                    report.WriteReportObjects(list, false, ReportState.Staying);
                    EventReinforceDefender(list);
                    RefreshBattleOrder();
                }
            }
        }

        public void AddToAttack(TroopStub stub)
        {
            var list = new List<TroopStub> {stub};
            AddToAttack(list);
        }

        public void AddToAttack(IEnumerable<TroopStub> objects)
        {
            AddToCombatList(objects, attackers, false, ReportState.Entering);
        }

        public void AddToDefense(IEnumerable<TroopStub> objects)
        {
            AddToCombatList(objects, defenders, false, ReportState.Entering);
        }

        public void RemoveFromAttack(IEnumerable<TroopStub> objects, ReportState state)
        {
            RemoveFromCombatList(objects, attackers, state);
        }

        public void RemoveFromLocal(IEnumerable<Structure> objects, ReportState state)
        {
            lock (battleLock)
            {
                var list = (from obj in defenders                            
                            where obj is CombatStructure && objects.Contains(((CombatStructure)obj).Structure)
                            select obj).ToList();

                defenders.RemoveAll(list.Contains);

                if (!battleStarted)
                    return;

                report.WriteReportObjects(list, false, state);
                EventWithdrawDefender(list);
                RefreshBattleOrder();
            }
        }

        public void RemoveFromDefense(IEnumerable<TroopStub> objects, ReportState state)
        {
            RemoveFromCombatList(objects, defenders, state);
        }

        private void AddToCombatList(IEnumerable<TroopStub> objects, CombatList combatList, bool isLocal, ReportState state)
        {
            lock (battleLock)
            {
                var list = new List<CombatObject>();

                bool added = false;

                foreach (var obj in objects)
                {
                    uint id = 1;

                    if (!isLocal)
                        id = (uint)idGen.GetNext();

                    foreach (var formation in obj)
                    {
                        if (formation.Type == FormationType.Garrison || formation.Type == FormationType.InBattle)
                            continue;

                        //if it's our local troop then it should be in the battle formation since 
                        //it will be moved by the battle manager (who is calling this function) to the in battle formation
                        //There should be a better way to do this but I cant figure it out right now
                        FormationType formationType = formation.Type;
                        if (isLocal)
                            formationType = FormationType.InBattle;

                        foreach (var kvp in formation)
                        {
                            CombatObject[] combatObjects;
                            if (combatList == defenders)
                                combatObjects = Ioc.Kernel.Get<CombatUnitFactory>().CreateDefenseCombatUnit(this, obj, formationType, kvp.Key, kvp.Value);
                            else
                                combatObjects = Ioc.Kernel.Get<CombatUnitFactory>().CreateAttackCombatUnit(this, obj.TroopObject, formationType, kvp.Key, kvp.Value);

                            foreach (var unit in combatObjects)
                            {
                                added = true;
                                unit.LastRound = round;
                                unit.Id = (uint)idGen.GetNext();
                                unit.GroupId = id;
                                combatList.Add(unit);
                                list.Add(unit);
                            }
                        }
                    }
                }

                if (battleStarted && added)
                {
                    if (combatList == Attacker)
                    {
                        report.WriteReportObjects(list, true, state);
                        EventReinforceAttacker(list);
                    }
                    else if (combatList == Defender)
                    {
                        report.WriteReportObjects(list, false, state);
                        EventReinforceDefender(list);
                    }

                    RefreshBattleOrder();
                }
            }
        }

        private void RemoveFromCombatList(IEnumerable<TroopStub> objects, CombatList combatList, ReportState state)
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
                report.WriteReportObjects(list, combatList == Attacker, state);

                // Tell objects to exit from battle
                foreach (var co in list.Where(co => !co.IsDead))
                    co.ExitBattle();

                // Send exit events
                if (combatList == Attacker)
                    EventWithdrawAttacker(list);
                else if (combatList == Defender)
                    EventWithdrawDefender(list);

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
            battleOrder = new BattleOrder(round);

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
                report.WriteBeginReport();
                report.CompleteReport(ReportState.Exiting);
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

            EventExitBattle(attackers, defenders);

            //have to call to remove from the database
            attackers.Clear();
            defenders.Clear();

            //unsubscribe all
            Global.Channel.Unsubscribe("/BATTLE/" + city.Id);
        }

        public bool GroupIsDead(CombatObject co, CombatList combatList)
        {
            return combatList.All(combatObject => combatObject.GroupId != co.GroupId || combatObject.IsDead);
        }

        public bool ExecuteTurn()
        {
            lock (battleLock)
            {
                // This will finalize any reports already started.
                report.WriteReport(ReportState.Staying);

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
                    EventEnterBattle(attackers, defenders);
                }

                #endregion

                #region Targeting

                CombatObject currentAttacker;
                List<CombatObject> currentDefenders;

                do
                {
                    #region Find Attacker                    

                    do
                    {
                        if (!battleOrder.NextObject(out currentAttacker) && !battleJustStarted)
                        {
                            ++round;
                            battleOrder.ParticipatedInRound();
                            turn = 0;
                            EventEnterRound(Attacker, Defender, round);
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
                        targetResult = defenders.GetBestTargets(currentAttacker, out currentDefenders, BattleFormulas.GetNumberOfHits(currentAttacker));
                    else if (currentAttacker.CombatList == defenders)
                        targetResult = attackers.GetBestTargets(currentAttacker, out currentDefenders, BattleFormulas.GetNumberOfHits(currentAttacker));
                    else
                        throw new Exception("How can this happen");

                    if (currentDefenders == null || currentDefenders.Count == 0 || currentAttacker.Stats.Atk == 0)
                    {
                        currentAttacker.ParticipatedInRound();
                        dbManager.Save(currentAttacker);
                        EventSkippedAttacker(currentAttacker);

                        // If the attacker can't attack because it has no one in range, then we skip him and find another target right away.
                        if (targetResult == CombatList.BestTargetResult.NoneInRange)
                            continue;

                        return true;
                    }

                    #endregion

                    break;
                } while (true);

                #endregion

                #region Miss Chance

                int missChance = BattleFormulas.MissChance(currentAttacker.CombatList == attackers, defenders, attackers);
                if (missChance > 0)
                {
                    var rand = (int)(Config.Random.NextDouble()*100);

                    if (rand <= missChance)
                    {
                        currentAttacker.ParticipatedInRound();
                        dbManager.Save(currentAttacker);
                        EventSkippedAttacker(currentAttacker);
                        return true;
                    }
                }

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

                EventExitTurn(Attacker, Defender, (int)turn++);

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
                                RemoveFromAttack(new List<TroopStub> {(co).TroopStub}, ReportState.Exiting);
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

            ushort dmg = BattleFormulas.GetDamage(attacker, defender, attacker.CombatList == defenders);
            ushort actualDmg;
            Resource lostResource;
            int attackPoints;

            defender.CalculateDamage(dmg, out actualDmg);
            defender.TakeDamage(actualDmg, out lostResource, out attackPoints);

            attacker.DmgDealt += actualDmg;
            attacker.MaxDmgDealt = Math.Max(attacker.MaxDmgDealt, actualDmg);
            attacker.MinDmgDealt = Math.Min(attacker.MinDmgDealt, actualDmg);
            ++attacker.HitDealt;
            attacker.HitDealtByUnit += attacker.Count;

            defender.DmgRecv += actualDmg;
            defender.MaxDmgRecv = Math.Max(defender.MaxDmgRecv, actualDmg);
            defender.MinDmgRecv = Math.Min(defender.MinDmgRecv, actualDmg);
            ++defender.HitRecv;

            #endregion

            #region Loot and Attack Points

            if (attacker.CombatList == Attacker)
            {
                // Only give loot if we are attacking the first target in the list
                if (attackIndex == 0)
                {
                    Resource loot = BattleFormulas.GetRewardResource(attacker, defender, actualDmg);
                    city.BeginUpdate();
                    city.Resource.Subtract(loot, Formula.HiddenResource(city), out loot);
                    attacker.ReceiveReward(attackPoints, loot);
                    city.EndUpdate();
                }
                else
                {
                    city.BeginUpdate();
                    attacker.ReceiveReward(attackPoints, new Resource());
                    city.EndUpdate();
                }
            }
            else
            {
                // Give back any lost resources if the attacker dropped them
                if (lostResource != null && !lostResource.Empty)
                {
                    city.BeginUpdate();
                    city.Resource.Add(lostResource);
                }

                // If the defender killed someone then give the city defense points
                if (attackPoints > 0)
                {
                    if (!city.IsUpdating)
                        city.BeginUpdate();

                    city.DefensePoint += attackPoints;

                    // Give anyone stationed defense points as well
                    // DONT convert this to LINQ because I'm not sure how it might affect the list inside of the loop that keeps changing
                    var uniqueCities = new List<City>();
                    foreach (var co in defenders)
                    {
                        if (co.City == city || uniqueCities.Contains(co.City))
                            continue;
                        co.City.BeginUpdate();
                        co.City.DefensePoint += attackPoints;
                        co.City.EndUpdate();

                        uniqueCities.Add(co.City);
                    }
                }

                if (city.IsUpdating)
                    city.EndUpdate();
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

                EventActionAttacked(attacker, defender, actualDmg);

                EventUnitRemoved(defender);

                if (!defender.Disposed)
                    defender.CleanUp();
            }
            else
            {
                EventActionAttacked(attacker, defender, actualDmg);

                if (!defender.Disposed)
                    dbManager.Save(defender);
            }

            #endregion            

            return defender.IsDead;
        }

        #endregion

        #region Events
        public delegate void OnAttack(CombatObject source, CombatObject target, ushort damage);
        public delegate void OnBattle(CombatList atk, CombatList def);
        public delegate void OnReinforce(IEnumerable<CombatObject> list);
        public delegate void OnRound(CombatList atk, CombatList def, uint round);
        public delegate void OnTurn(CombatList atk, CombatList def, int turn);
        public delegate void OnUnitUpdate(CombatObject obj);

        public event OnBattle EnterBattle;
        public event OnBattle ExitBattle;
        public event OnRound EnterRound;
        public event OnTurn EnterTurn;
        public event OnTurn ExitTurn;
        public event OnReinforce ReinforceAttacker;
        public event OnReinforce ReinforceDefender;
        public event OnReinforce WithdrawAttacker;
        public event OnReinforce WithdrawDefender;
        public event OnUnitUpdate UnitAdded;
        public event OnUnitUpdate UnitRemoved;
        public event OnUnitUpdate UnitUpdated;
        public event OnUnitUpdate SkippedAttacker;
        public event OnAttack ActionAttacked;

        private void EventEnterBattle(CombatList atk, CombatList def)
        {
            if (EnterBattle != null)
                EnterBattle(atk, def);
        }

        private void EventExitBattle(CombatList atk, CombatList def)
        {
            if (ExitBattle != null)
                ExitBattle(atk, def);
        }

        private void EventEnterRound(CombatList atk, CombatList def, uint round)
        {
            if (EnterRound != null)
                EnterRound(atk, def, round);
        }

        private void EventEnterTurn(CombatList atk, CombatList def, int turn)
        {
            if (EnterTurn != null)
                EnterTurn(atk, def, turn);
        }

        private void EventExitTurn(CombatList atk, CombatList def, int turn)
        {
            if (ExitTurn != null)
                ExitTurn(atk, def, turn);
        }

        private void EventReinforceAttacker(IEnumerable<CombatObject> list)
        {
            if (ReinforceAttacker != null)
                ReinforceAttacker(list);
        }

        private void EventReinforceDefender(IEnumerable<CombatObject> list)
        {
            if (ReinforceDefender != null)
                ReinforceDefender(list);
        }

        private void EventWithdrawAttacker(IEnumerable<CombatObject> list)
        {
            if (WithdrawAttacker != null)
                WithdrawAttacker(list);
        }

        private void EventWithdrawDefender(IEnumerable<CombatObject> list)
        {
            if (WithdrawDefender != null)
                WithdrawDefender(list);
        }

        private void EventUnitRemoved(CombatObject obj)
        {
            if (UnitRemoved != null)
                UnitRemoved(obj);
        }

        private void EventUnitAdded(CombatObject obj)
        {
            if (UnitAdded != null)
                UnitAdded(obj);
        }

        private void EventUnitUpdated(CombatObject obj)
        {
            if (UnitUpdated != null)
                UnitUpdated(obj);
        }

        private void EventActionAttacked(CombatObject source, CombatObject target, ushort dmg)
        {
            if (ActionAttacked != null)
                ActionAttacked(source, target, dmg);
        }

        private void EventSkippedAttacker(CombatObject source)
        {
            if (SkippedAttacker != null)
                SkippedAttacker(source);
        }
        #endregion
    }
}