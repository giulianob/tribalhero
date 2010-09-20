#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Fighting;
using Game.Setup;
using Game.Util;
using Game.Logic;

#endregion

namespace Game.Battle {
    public class BattleManager : BattleBase, IPersistableObject {
        private readonly LargeIdGenerator idGen = new LargeIdGenerator(ushort.MaxValue);
        private readonly LargeIdGenerator groupIdGen = new LargeIdGenerator(ushort.MaxValue);

        private readonly CombatList attackers = new CombatList();
        private readonly CombatList defenders = new CombatList();

        private BattleOrder battleOrder = new BattleOrder(0);
        private readonly BattleReport report;

        private readonly object battleLock = new object();

        #region Properties Members

        private uint battleId;

        public BattleChannel Channel { get; private set; }

        public uint BattleId {
            get { return battleId; }
            set { battleId = value; }
        }

        private bool battleStarted;

        public bool BattleStarted {
            get { return battleStarted; }
            set { battleStarted = value; }
        }

        private uint round;

        public uint Round {
            get { return round; }
            set { round = value; }
        }

        private uint turn;

        public uint Turn {
            get { return turn; }
            set { turn = value; }
        }

        private ushort stamina;

        public ushort Stamina {
            get { return stamina; }
            set { stamina = value; }
        }

        private City city;

        public City City {
            get { return city; }
            set { city = value; }
        }

        public CombatList Attacker {
            get { return attackers; }
        }

        public CombatList Defender {
            get { return defenders; }
        }

        public BattleReport BattleReport {
            get { return report; }
        }

        public ReportedObjects ReportedObjects {
            get { return report.ReportedObjects; }
        }

        public ReportedTroops ReportedTroops {
            get { return report.ReportedTroops; }
        }

        public City[] LockList {
            get {
                Dictionary<uint, City> cities = new Dictionary<uint, City> { { city.Id, city } };

                foreach (CombatObject co in attackers)
                    cities[co.City.Id] = co.City;

                foreach (CombatObject co in defenders)
                    cities[co.City.Id] = co.City;

                City[] citiesArr = new City[cities.Values.Count];
                cities.Values.CopyTo(citiesArr, 0);
                return citiesArr;
            }
        }

        #endregion

        public BattleManager(City owner) {
            city = owner;
            stamina = BattleFormulas.GetStamina(city);
            report = new BattleReport(this);
            Channel = new BattleChannel(this, "/BATTLE/" + city.Id);
            groupIdGen.Set(1);
        }

        public void Subscribe(Session session) {
            try {
                Global.Channel.Subscribe(session, "/BATTLE/" + city.Id);
            } catch (DuplicateSubscriptionException) { }
        }

        public void Unsubscribe(Session session) {
            Global.Channel.Unsubscribe(session, "/BATTLE/" + city.Id);
        }

        public CombatObject GetCombatObject(uint id) {
            foreach (CombatObject co in attackers) {
                if (co.Id == id)
                    return co;
            }

            foreach (CombatObject co in defenders) {
                if (co.Id == id)
                    return co;
            }

            return null;
        }

        public bool CanWatchBattle(Player player) {
            if (player == city.Owner) return true;
        
            return defenders.Any(co => co.City.Owner == player && co.RoundsParticipated >= Config.battle_min_rounds) || attackers.Any(co => co.City.Owner == player && co.RoundsParticipated >= Config.battle_min_rounds);
        }

        #region Database Loader

        public void DbLoaderAddToLocal(CombatStructure structure, uint id) {
            structure.Id = id;

            if (structure.IsDead)
                return;

            defenders.Add(structure, false);

            idGen.Set((int)structure.Id);
        }

        public void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal) {
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

        public void AddToLocal(List<TroopStub> objects, ReportState state) {
            AddToCombatList(objects, defenders, true, state);
        }

        public void AddToLocal(List<Structure> objects) {
            lock (battleLock) {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (Structure obj in objects) {
                    if (obj.Stats.Hp == 0 || defenders.Contains(obj) || ObjectTypeFactory.IsStructureType("Unattackable", obj) || obj.IsBlocked)
                        continue;

                    // Don't add main building if lvl 1 or if a building is lvl 0
                    if ((ObjectTypeFactory.IsStructureType("Undestroyable",obj) && obj.Lvl <= 1) || 
                        (obj.Lvl == 0) || 
                        ObjectTypeFactory.IsStructureType("Noncombatstructure",obj))
                        continue;

                    added = true;

                    obj.BeginUpdate();
                    obj.State = GameObjectState.BattleState(obj.City.Id);
                    obj.EndUpdate();

                    CombatObject combatObject = new CombatStructure(this, obj, BattleFormulas.LoadStats(obj)) {
                        Id = (uint)idGen.GetNext(),
                        GroupId = 1
                    };
                    defenders.Add(combatObject);
                    list.Add(combatObject);
                }

                if (battleStarted && added) {
                    stamina = BattleFormulas.GetStaminaReinforced(city, stamina, round);
                    report.WriteReportObjects(list, false, ReportState.STAYING);
                    EventReinforceDefender(list);
                    RefreshBattleOrder();
                }
            }
        }

        public void AddToAttack(TroopStub stub) {
            List<TroopStub> list = new List<TroopStub> { stub };
            AddToAttack(list);
        }

        public void AddToAttack(List<TroopStub> objects) {
            AddToCombatList(objects, attackers, false, ReportState.ENTERING);
        }

        public void AddToDefense(List<TroopStub> objects) {
            AddToCombatList(objects, defenders, false, ReportState.ENTERING);
        }

        public void RemoveFromAttack(List<TroopStub> objects, ReportState state) {
            RemoveFromCombatList(objects, attackers, state);
        }

        public void RemoveFromLocal(List<Structure> objects) {
            lock (battleLock)
            {
                List<CombatObject> list = new List<CombatObject>();
                foreach (CombatObject obj in defenders) {
                    if (!(obj is CombatStructure))
                        continue;

                    CombatStructure combatStructure = obj as CombatStructure;
                    if (objects.Contains(combatStructure.Structure))
                        list.Add(obj);
                }

                defenders.RemoveAll(list.Contains);

                if (battleStarted) {
                    report.WriteReportObjects(list, false, ReportState.EXITING);
                    EventWithdrawDefender(list);
                    RefreshBattleOrder();
                }
            }
        }

        public void RemoveFromDefense(List<TroopStub> objects, ReportState state) {
            RemoveFromCombatList(objects, defenders, state);
        }

        private void AddToCombatList(IEnumerable<TroopStub> objects, CombatList combatList, bool isLocal, ReportState state) {
            lock (battleLock)
            {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (TroopStub obj in objects) {
                    uint id = 1;

                    if (!isLocal)
                        id = (uint) idGen.GetNext();

                    foreach (Formation formation in obj) {
                        if (formation.Type == FormationType.GARRISON || formation.Type == FormationType.IN_BATTLE)
                            continue;

                        //if it's our local troop then it should be in the battle formation since 
                        //it will be moved by the battle manager (who is calling this function) to the in battle formation
                        //There should be a better way to do this but I cant figure it out right now
                        FormationType formationType = formation.Type;
                        if (isLocal)
                            formationType = FormationType.IN_BATTLE;

                        foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                            CombatObject[] combatObjects;
                            if (combatList == defenders)
                                combatObjects = CombatUnitFactory.CreateDefenseCombatUnit(this, obj, formationType, kvp.Key, kvp.Value);
                            else
                                combatObjects = CombatUnitFactory.CreateAttackCombatUnit(this, obj.TroopObject, formationType, kvp.Key, kvp.Value);

                            foreach (CombatObject unit in combatObjects) {
                                added = true;
                                unit.LastRound = round;
                                unit.Id = (uint) idGen.GetNext();
                                unit.GroupId = id;
                                combatList.Add(unit);
                                list.Add(unit);
                            }
                        }
                    }
                }

                if (battleStarted && added) {
                    stamina = BattleFormulas.GetStaminaReinforced(city, stamina, round);
                    if (combatList == Attacker) {
                        report.WriteReportObjects(list, true, state);
                        EventReinforceAttacker(list);
                    } else if (combatList == Defender) {
                        report.WriteReportObjects(list, false, state);
                        EventReinforceDefender(list);
                    }

                    RefreshBattleOrder();
                }
            }
        }

        private void RemoveFromCombatList(List<TroopStub> objects, CombatList combatList, ReportState state) {
            lock (battleLock)
            {
                List<CombatObject> list = new List<CombatObject>();
                foreach (CombatObject obj in combatList) {
                    if (!(obj is ICombatUnit))
                        continue;

                    ICombatUnit unit = obj as ICombatUnit;
                    if (objects.Contains(unit.TroopStub)) {
                        list.Add(obj);
                    }
                }

                combatList.RemoveAll(list.Contains);

                if (!battleStarted)
                    return;

                // Snap a report of exit
                report.WriteReportObjects(list, combatList == Attacker, state);

                // Clean up object
                foreach (CombatObject co in list.Where(co => !co.IsDead)) {
                    co.ExitBattle();
                }

                // Send exit events
                if (combatList == Attacker)              
                    EventWithdrawAttacker(list);
                else if (combatList == Defender)
                    EventWithdrawDefender(list);                

                // Refresh battle order
                RefreshBattleOrder();
            }
        }

        #endregion

        #region Battle

        public void RefreshBattleOrder() {
            battleOrder = new BattleOrder(round);

            IEnumerator<CombatObject> defIter = defenders.GetEnumerator();
            IEnumerator<CombatObject> atkIter = attackers.GetEnumerator();

            bool atkDone = false;
            bool defDone = false;

            while (!atkDone || !defDone) {
                if (!defDone) {
                    while (true) {
                        if (defIter.MoveNext()) {
                            if (defIter.Current.IsDead)
                                continue;
                            battleOrder.Add(defIter.Current);
                        } else {
                            defDone = true;
                            break;
                        }
                    }
                }

                if (!atkDone) {
                    while (true) {
                        if (atkIter.MoveNext()) {
                            if (atkIter.Current.IsDead)
                                continue;
                            battleOrder.Add(atkIter.Current);
                        } else {
                            atkDone = true;
                            break;
                        }
                    }
                }
            }
        }

        private bool BattleIsValid() {
            if (attackers.Count == 0 || defenders.Count == 0 || Stamina == 0)
                return false;

            //Check to see if there are still units in the local troop
            bool localUnits = false;
            foreach (CombatObject combatObj in defenders.Where(combatObj => !combatObj.IsDead)) {
                if (combatObj is CombatStructure) {
                    localUnits = true;
                    break;
                }

                DefenseCombatUnit cu = combatObj as DefenseCombatUnit;
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

        private void BattleEnded(bool writeReport) {
            if (writeReport) {
                report.WriteBeginReport();
                report.CompleteReport(Stamina == 0 ? ReportState.OUT_OF_STAMINA : ReportState.EXITING);
                report.CompleteBattle();
            }

            foreach (CombatObject combatObj in defenders) {
                if (!combatObj.IsDead)
                    combatObj.ExitBattle();
            }

            foreach (CombatObject combatObj in attackers) {
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

        public bool GroupIsDead(CombatObject co, CombatList combatList) {
            return combatList.All(combatObject => combatObject.GroupId != co.GroupId || combatObject.IsDead);
        }

        public bool ExecuteTurn() {
            lock (battleLock)
            {
                // This will finalize any reports already started.
                report.WriteReport(ReportState.STAYING);

                #region Battle Start
                if (!battleStarted) {
                    RefreshBattleOrder();

                    // Makes sure battle is valid before even starting it
                    if (!BattleIsValid()) {
                        BattleEnded(false);
                        return false;
                    }
                    
                    battleStarted = true;
                    report.CreateBattleReport();
                    EventEnterBattle(attackers, defenders);
                }
                #endregion

                #region Find Attacker 
                CombatObject attacker;

                if (!battleOrder.NextObject(out attacker)) {
                    ++round;
                    stamina = BattleFormulas.GetStaminaRoundEnded(city, stamina, turn);
                    battleOrder.ParticipatedInRound();
                    turn = 0;
                    EventEnterRound(Attacker, Defender, round, stamina);
                }

                if (attacker == null || defenders.Count == 0 || attackers.Count == 0 || stamina <= 0) {
                    BattleEnded(true);
                    return false;
                }
                #endregion

                #region Find Target

                CombatObject defender;

                if (attacker.CombatList == attackers) {
                    defenders.GetBestTarget(attacker, out defender);
                } else if (attacker.CombatList == defenders)
                    attackers.GetBestTarget(attacker, out defender);
                else
                    throw new Exception("How can this happen");

                if (defender == null || (attacker.CombatList == attackers && attacker.Stats.Atk == 0) || (attacker.CombatList == defenders && attacker.Stats.Def == 0)) {
                    attacker.ParticipatedInRound();
                    Global.DbManager.Save(attacker);
                    EventSkippedAttacker(attacker);
                    return true;
                }

                #endregion

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

                Global.Logger.Debug(string.Format("{0}[{1}] hit {2}[{3}] for {4} damage", attacker.ClassType, attacker.Type, defender.ClassType, defender.Type, dmg));
                if (lostResource != null && lostResource.Total > 0) Global.Logger.Debug(string.Format("Defender lost {0} resources", lostResource));
                if (attackPoints > 0) Global.Logger.Debug(string.Format("Attacker gained {0} attack points", attackPoints));

                #endregion                

                #region Loot and Attack Points

                if (attacker.CombatList == Attacker) {
                    Resource loot = BattleFormulas.GetRewardResource(attacker, defender, actualDmg);
                    city.BeginUpdate();
                    city.Resource.Subtract(loot, Formula.HiddenResource(city), out loot);
                    attacker.ReceiveReward(attackPoints, loot);
                    city.EndUpdate();
                } else {                    
                    // Give back any lost resources if the attacker dropped them
                    if (lostResource != null && !lostResource.Empty) {
                        city.BeginUpdate();
                        city.Resource.Add(lostResource);
                    }
                    
                    // If the defender killed someone then give the city defense points
                    if (attackPoints > 0) {
                        if (!city.IsUpdating)
                            city.BeginUpdate();                    

                        city.DefensePoint += attackPoints;

                        // Give anyone stationed defense points as well
                        // DONT convert this to LINQ because I'm not sure how it might affect the list inside of the loop that keeps changing
                        List<City> uniqueCities = new List<City>();
                        foreach (CombatObject co in defenders) {
                            if (co.City == city || uniqueCities.Contains(co.City)) continue;
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
                    Global.Logger.Debug("Defender has died");

                    EventUnitRemoved(defender);
                    battleOrder.Remove(defender);

                    if (attacker.CombatList == attackers) {
                        
                        if (defender.ClassType == BattleClass.STRUCTURE) stamina = BattleFormulas.GetStaminaStructureDestroyed(city, stamina, round);

                        defenders.Remove(defender);
                        report.WriteReportObject(defender, false, GroupIsDead(defender, defenders) ? ReportState.DYING : ReportState.STAYING);
                    } else if (attacker.CombatList == defenders) {
                        attackers.Remove(defender);
                        report.WriteReportObject(defender, true, GroupIsDead(defender, attackers) ? ReportState.DYING : ReportState.STAYING);
                    }

                    defender.CleanUp();                    
                }
                else {
                    Global.DbManager.Save(defender);
                }

                #endregion

                attacker.ParticipatedInRound();                

                Global.DbManager.Save(attacker);

                EventActionAttacked(attacker, defender, actualDmg);

                EventExitTurn(Attacker, Defender, (int)turn++);

                // Send back any attackers that have no targets left
                if (isDefenderDead && defender.CombatList == defenders && defenders.Count > 0) {
                    AttackCombatUnit co;
                    do {
                        co = attackers.OfType<AttackCombatUnit>().FirstOrDefault(x => !defenders.HasInRange(x));
                        if (co != null) RemoveFromAttack(new List<TroopStub> {(co).TroopStub}, ReportState.EXITING);                        
                    } while (co != null);
                }

                if (!BattleIsValid()) {
                    BattleEnded(true);
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "battle_managers";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] { new DbColumn("city_id", city.Id, DbType.UInt32) }; }
        }

        public DbDependency[] DbDependencies {
            get {
                return new[] { new DbDependency("ReportedObjects", true, true), new DbDependency("ReportedTroops", true, true) };
            }
        }

        public DbColumn[] DbColumns {
            get {
                return new[] {
                                          new DbColumn("battle_id", battleId, DbType.UInt32),
                                          new DbColumn("battle_started", battleStarted, DbType.Boolean),
                                          new DbColumn("stamina", stamina, DbType.UInt16), new DbColumn("round", round, DbType.UInt32),
                                          new DbColumn("turn", turn, DbType.UInt32),
                                          new DbColumn("report_flag", report.ReportFlag, DbType.Boolean),
                                          new DbColumn("report_started", report.ReportStarted, DbType.Boolean),
                                          new DbColumn("report_id", report.ReportId, DbType.UInt32)
                                      };
            }
        }

        public bool DbPersisted { get; set; }

        #endregion
    }
}