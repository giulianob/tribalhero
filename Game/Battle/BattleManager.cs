using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;
using Game.Util;
using Game.Comm;
using Game.Setup;
using Game.Database;
using Game.Data.Troop;

namespace Game.Battle {
    public class BattleChannel {
        string channelName;
        BattleBase battle;
        public BattleChannel(BattleBase battle, string channelName) {
            this.battle = battle;
            this.channelName = channelName;
            battle.ActionAttacked += new BattleBase.OnAttack(battle_ActionAttacked);
            battle.ReinforceAttacker += new BattleBase.OnReinforce(battle_ReinforceAttacker);
            battle.ReinforceDefender += new BattleBase.OnReinforce(battle_ReinforceDefender);
            battle.ExitBattle += new BattleBase.OnBattle(battle_ExitBattle);
        }

        void battle_ExitBattle(CombatList atk, CombatList def) {
            Packet packet = new Packet(Command.BATTLE_ENDED);
            Global.Channel.Post(channelName, packet);
        }

        void battle_ReinforceDefender(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_DEFENDER);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        void battle_ReinforceAttacker(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_ATTACKER);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        void battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            Packet packet = new Packet(Command.BATTLE_ATTACK);
            packet.addUInt32(source.Id);
            packet.addUInt32(target.Id);
            packet.addUInt16(damage);
            Global.Channel.Post(channelName, packet);
        }
    }

    public class BattleBase {
        public delegate void OnBattle(CombatList atk, CombatList def);
        public event OnBattle EnterBattle;
        public event OnBattle ExitBattle;

        public delegate void OnRound(CombatList atk, CombatList def, uint round, int stamina);
        public event OnRound EnterRound;

        public delegate void OnTurn(CombatList atk, CombatList def, int turn);
        public event OnTurn EnterTurn;
        public event OnTurn ExitTurn;
        
        public delegate void OnReinforce(IEnumerable<CombatObject> list);
        public event OnReinforce ReinforceAttacker;
        public event OnReinforce ReinforceDefender;
        public event OnReinforce WithdrawAttacker;
        public event OnReinforce WithdrawDefender;

        public delegate void OnUnitUpdate(CombatObject obj);
        public event OnUnitUpdate UnitAdded;
        public event OnUnitUpdate UnitRemoved;
        public event OnUnitUpdate UnitUpdated;

        public delegate void OnAttack(CombatObject source, CombatObject target, ushort damage);
        public event OnAttack ActionAttacked;

        public void EventEnterBattle(CombatList atk, CombatList def) {
            if (this.EnterBattle != null) this.EnterBattle(atk, def);
        }
        public void EventExitBattle(CombatList atk, CombatList def) {
            if (this.ExitBattle != null) this.ExitBattle(atk, def);
        }
        public void EventEnterRound(CombatList atk, CombatList def, uint round, int stamina) {
            if (this.EnterRound != null) this.EnterRound(atk, def, round, stamina);
        }

        public void EventEnterTurn(CombatList atk, CombatList def, int turn) {
            if (this.EnterTurn != null) this.EnterTurn(atk, def, turn);
        }
        public void EventExitTurn(CombatList atk, CombatList def, int turn) {
            if (this.ExitTurn != null) this.ExitTurn(atk, def, turn);
        }
        public void EventReinforceAttacker(IEnumerable<CombatObject> list) {
            if (this.ReinforceAttacker != null) this.ReinforceAttacker(list);
        }
        public void EventReinforceDefender(IEnumerable<CombatObject> list) {
            if (this.ReinforceDefender != null) this.ReinforceDefender(list);
        }
        public void EventWithdrawAttacker(IEnumerable<CombatObject> list) {
            if (this.WithdrawAttacker != null) this.WithdrawAttacker(list);
        }
        public void EventWithdrawDefender(IEnumerable<CombatObject> list) {
            if (this.WithdrawDefender != null) this.WithdrawDefender(list);
        }
        public void EventUnitRemoved(CombatObject obj) {
            if (this.UnitRemoved != null) this.UnitRemoved(obj);
        }
        public void EventUnitAdded(CombatObject obj) {
            if (this.UnitAdded != null) this.UnitAdded(obj);
        }
        public void EventUnitUpdated(CombatObject obj) {
            if (this.UnitUpdated != null) this.UnitUpdated(obj);
        }
        public void EventActionAttacked(CombatObject source, CombatObject target, ushort dmg) {
            if (this.ActionAttacked != null) this.ActionAttacked(source, target, dmg);
        }
    }

    public class BattleManager : BattleBase, IPersistableObject {     
        LargeIdGenerator idGen = new LargeIdGenerator(ushort.MaxValue);
        LargeIdGenerator groupIdGen = new LargeIdGenerator(ushort.MaxValue);

        CombatList attackers = new CombatList();
        CombatList defenders = new CombatList();

        BattleOrder battleOrder = new BattleOrder(0);
        BattleChannel channel;
        BattleReport report;        
        
        #region Properties Members

        uint battleId;        
        public uint BattleId {
            get { return battleId; }
            set { battleId = value; }
        }

        bool battleStarted = false;
        public bool BattleStarted {
            get { return battleStarted; }
            set { battleStarted = value; }
        }

        uint round = 0;
        public uint Round {
            get { return round; }
            set { round = value; }
        }

        uint turn = 0;
        public uint Turn {
            get { return turn; }
            set { turn = value; }
        }

        ushort stamina;
        public ushort Stamina
        {
            get { return stamina; }
            set { stamina = value; }
        }

        City city;

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
                Dictionary<uint, City> cities = new Dictionary<uint, City>();
                cities.Add(city.CityId, city);

                foreach (CombatObject co in attackers)
                    cities[co.City.CityId] = co.City;

                foreach (CombatObject co in defenders)
                    cities[co.City.CityId] = co.City;                

                City[] citiesArr = new City[cities.Values.Count];
                cities.Values.CopyTo(citiesArr, 0);
                return citiesArr;
            }
        }
        #endregion

        public BattleManager(City owner) {            
            this.city = owner;
            this.stamina = BattleFormulas.getStamina(city);
            channel = new BattleChannel(this, "/BATTLE/" + owner.CityId);
            report = new BattleReport(this);
            groupIdGen.set(1);
        }

        public void subscribe(Session session) {
            Global.Channel.Subscribe(session, "/BATTLE/" + city.CityId);
        }

        public void unsubscribe(Session session) {
            Global.Channel.Unsubscribe(session, "/BATTLE/" + city.CityId);
        }

        public CombatObject getCombatObject(uint id) {
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

        #region Database Loader 
        public void dbLoaderAddToLocal(CombatStructure structure, uint id) {
            structure.Id = id;

            if (structure.IsDead) return;

            defenders.Add(structure, false);

            idGen.set((int)structure.Id);            
        }

        public void dbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal) {
            obj.Id = id;

            if (obj.IsDead) return;

            if (isLocal)
                defenders.Add(obj, false);
            else
                attackers.Add(obj, false);

            idGen.set((int)obj.Id);
            groupIdGen.set((int)obj.GroupId);
        }
        #endregion

        #region Adding/Removing from Battle
        public void AddToLocal(TroopStub stub, ReportState state) {
            List<TroopStub> list = new List<TroopStub>();
            list.Add(stub);
            AddToLocal(list, state);
        }

        public void AddToLocal(List<TroopStub> objects, ReportState state) {
            addToCombatList(objects, defenders, true, state);
        }

        public void AddToLocal(List<Structure> objects) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (Structure obj in objects) {
                    if (obj.Stats.Hp == 0 || defenders.Contains(obj))
                        continue;
                    
                    if (obj == obj.City.MainBuilding && obj.Lvl <= 1)
                        continue;

                    added = true;

                    obj.BeginUpdate();
                    obj.State = GameObjectState.BattleState(obj.City.CityId);
                    obj.EndUpdate();

                    CombatObject combatObject = null;

                    combatObject = new CombatStructure(this, obj, BattleFormulas.LoadStats(obj));
                    combatObject.Id = (uint)idGen.getNext();
                    combatObject.GroupId = 1;
                    defenders.Add(combatObject);
                    list.Add(combatObject);
                }

                if (battleStarted && added) {
                    stamina = BattleFormulas.getStaminaReinforced(city, stamina, round);
                    report.WriteReportObjects(list, false, ReportState.Staying);
                    this.EventReinforceDefender(list);
                    refreshBattleOrder();
                }                
            }
        }

        public void addToAttack(TroopStub stub) {
            List<TroopStub> list = new List<TroopStub>();
            list.Add(stub);
            addToAttack(list);
        }

        public void addToAttack(List<TroopStub> objects) {
            addToCombatList(objects, attackers, false, ReportState.Entering);
        }
        
        public void addToDefense(List<TroopStub> objects) {
            addToCombatList(objects, defenders, false, ReportState.Entering);
        }    

        public void removeFromAttack(List<TroopStub> objects, ReportState state) {
            removeFromCombatList(objects, attackers, state);
        }

        public void removeFromLocal(List<Structure> objects) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();
                foreach (CombatObject obj in defenders) {
                    if (!(obj is CombatStructure))
                        continue;

                    CombatStructure combatStructure = obj as CombatStructure;
                    if (objects.Contains(combatStructure.Structure)) {
                        list.Add(obj);
                    }
                }
                
                defenders.RemoveAll(delegate(CombatObject obj) {
                    return list.Contains(obj);
                });

                if (battleStarted) {
                    report.WriteReportObjects(list, false, ReportState.Exiting);
                    this.EventWithdrawDefender(list);
                    refreshBattleOrder();
                }
            }
        }

        public void removeFromDefense(List<TroopStub> objects, ReportState state) {
            removeFromCombatList(objects, defenders, state);
        }

        void addToCombatList(List<TroopStub> objects, CombatList combatList, bool isLocal, ReportState state) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (TroopStub obj in objects) {                    

                    uint id = 1;

                    if (!isLocal)
                        id = (uint)idGen.getNext();

                    obj.BeginUpdate();
                    obj.Template.LoadStats();
                    obj.EndUpdate();

                    foreach (KeyValuePair<FormationType, Formation> formation in obj as IEnumerable<KeyValuePair<FormationType, Formation>>) {
                        if (formation.Key == FormationType.Garrison || formation.Key == FormationType.InBattle) continue;

                        FormationType formationType = formation.Key;
                        //if it's our local troop then it should be in the battle formation since 
                        //it will be moved by the battle manager (who is calling this function) to the in battle formation
                        //There should be a better way to do this but I cant figure it out right now
                        if (isLocal && formationType == FormationType.Normal) 
                            formationType = FormationType.InBattle;

                        foreach (KeyValuePair<ushort, ushort> kvp in formation.Value) {
                            CombatObject[] combatObjects;
                            if (combatList == defenders)
                                combatObjects = CombatUnitFactory.CreateDefenseCombatUnit(this, obj, formationType, kvp.Key, kvp.Value);
                            else
                                combatObjects = CombatUnitFactory.CreateAttackCombatUnit(this, obj.TroopObject, formationType, kvp.Key, kvp.Value);

                            foreach (CombatObject unit in combatObjects) {
                                added = true;
                                unit.LastRound = round;
                                unit.Id = (uint)idGen.getNext();
                                unit.GroupId = id;
                                combatList.Add(unit);
                                list.Add(unit);
                            }
                        }
                    }
                }

                if (battleStarted && added)
                {
                    stamina = BattleFormulas.getStaminaReinforced(city,stamina,round);
                    if (combatList == this.Attacker)
                    {
                        report.WriteReportObjects(list, true, state);
                        this.EventReinforceAttacker(list);
                    }
                    else if (combatList == this.Defender)
                    {
                        report.WriteReportObjects(list, false, state);
                        this.EventReinforceDefender(list);
                    }

                    refreshBattleOrder();
                }
            }
        }

        void removeFromCombatList(List<TroopStub> objects, CombatList combatList, ReportState state) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();
                foreach (CombatObject obj in combatList) {
                    if (!(obj is ICombatUnit))
                        continue;

                    ICombatUnit unit = obj as ICombatUnit;
                    if (objects.Contains(unit.TroopStub)) {
                        list.Add(obj);
                    }
                }

                combatList.RemoveAll(delegate(CombatObject obj) {
                    return list.Contains(obj);
                });

                if (battleStarted) {
                    if (combatList == this.Attacker) {
                        report.WriteReportObjects(list, true, state);
                        this.EventWithdrawAttacker(list);
                    }
                    else if (combatList == this.Defender) {
                        report.WriteReportObjects(list, false, state);
                        this.EventWithdrawDefender(list);
                    }

                    refreshBattleOrder();
                }
            }
        }
        #endregion

        #region Battle 
        public void refreshBattleOrder() {
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

        bool battleIsValid() {
            if (attackers.Count == 0 || defenders.Count == 0) 
                return false;

            //Check to see if there are still units in the local troop
            bool localUnits = false;
            foreach (CombatObject combatObj in defenders) {
                if (combatObj.IsDead) continue;

                if (combatObj is CombatStructure) {
                    localUnits = true;
                    break;
                }

                DefenseCombatUnit cu = combatObj as DefenseCombatUnit;
                if (cu != null) {
                    localUnits = true;
                    break;
                }
            }

            if (!localUnits) return false;

            //Make sure units can still see each other
            foreach (CombatObject combatObj in attackers) {
                CombatObject target;
                CombatList.BestTargetResult result = defenders.GetBestTarget(combatObj, out target);

                if (result == CombatList.BestTargetResult.Ok || result == CombatList.BestTargetResult.NoneVisible)
                    return true;
            }
            
            return false;
        }

        void battleEnded() {
            report.WriteBeginReport();
            report.CompleteReport(ReportState.Exiting);
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
        }

        public bool groupIsDead(CombatObject co, CombatList combatList) {
            foreach (CombatObject combatObject in combatList) {
                if (combatObject.GroupId == co.GroupId && !combatObject.IsDead)
                    return false;
            }

            return true;
        }

        public bool executeTurn() {
            lock (this) {                
                report.WriteReport(ReportState.Staying);

                if (!battleStarted) {

                    refreshBattleOrder();
                    battleStarted = true;
                    report.CreateBattleReport();                    
                    EventEnterBattle(attackers, defenders);
                }

                while (true) {

                    CombatObject attacker = null;

                    if (!battleOrder.NextObject(out attacker)) {
                        ++round;
                        stamina = BattleFormulas.getStaminaRoundEnded(city, stamina, turn);
                        battleOrder.ParticipatedInRound();
                        turn = 0;
                        EventEnterRound(Attacker, Defender, round, stamina);
                    }

                    if (attacker == null || defenders.Count == 0 || attackers.Count == 0 || stamina <= 0) {
                        battleEnded();
                        return false;
                    }

                    CombatObject defender = null;

                    if (attacker.CombatList == attackers)
                        defenders.GetBestTarget(attacker, out defender);
                    else if (attacker.CombatList == defenders)
                        attackers.GetBestTarget(attacker, out defender);
                    else
                        throw new Exception("How can this happen");

                    if (defender == null || attacker.Stats.Atk == 0) {
                        attacker.ParticipatedInRound();
                        continue;
                    }

                    ushort dmg = BattleFormulas.getDamage(attacker, defender, attacker.CombatList == defenders);
                    int actualDmg;

                    defender.CalculateDamage(dmg, out actualDmg);

                    attacker.DmgDealt += actualDmg;
                    attacker.MaxDmgDealt = Math.Max(attacker.MaxDmgDealt, actualDmg);
                    attacker.MinDmgDealt = Math.Min(attacker.MinDmgDealt, actualDmg);
                    ++attacker.HitDealt;

                    defender.DmgRecv += actualDmg;
                    defender.MaxDmgRecv = Math.Max(defender.MaxDmgRecv, actualDmg);
                    defender.MinDmgRecv = Math.Min(defender.MinDmgRecv, actualDmg);
                    ++defender.HitRecv;

                    defender.TakeDamage(actualDmg);

                    if (defender.IsDead) {
                        EventUnitRemoved(defender);
                        battleOrder.Remove(defender);

                        if (attacker.CombatList == attackers) {
                            if (defender.ClassType == BattleClass.Structure) {
                                stamina = BattleFormulas.getStaminaStructureDestroyed(city, stamina, round);
                            }
                            defenders.Remove(defender);
                            report.WriteReportObject(defender, false, groupIsDead(defender, defenders) ? ReportState.Dying : ReportState.Staying);
                        }
                        else if (attacker.CombatList == defenders) {
                            attackers.Remove(defender);
                            report.WriteReportObject(defender, true, groupIsDead(defender, attackers) ? ReportState.Dying : ReportState.Staying);
                        }

                        defender.CleanUp();
                    }

                    if (attacker.CombatList == Attacker) {
                        Resource loot = BattleFormulas.getRewardResource(attacker, defender, actualDmg);
                        city.BeginUpdate();
                        city.Resource.Subtract(loot, out loot);
                        attacker.ReceiveReward(defender.Stats.Reward * actualDmg, loot);
                        city.EndUpdate();
                    }

                    EventActionAttacked(attacker, defender, (ushort)actualDmg);

                    attacker.ParticipatedInRound();

                    EventExitTurn(Attacker, Defender, (int)turn++);

                    if (!battleIsValid()) {
                        battleEnded();
                        return false;
                    }

                    return true;
                }
            }
        }
        #endregion

        #region IPersistable Members

        bool dbPersisted = false;

        public const string DB_TABLE = "battle_managers";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] { 
                    new DbColumn("city_id", city.CityId, System.Data.DbType.UInt32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { 
                    new DbDependency("ReportedObjects", true, true),
                    new DbDependency("ReportedTroops", true, true)
                    };
            }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("battle_id", battleId, System.Data.DbType.UInt32),
                    new DbColumn("battle_started", battleStarted, System.Data.DbType.Boolean),
                    new DbColumn("stamina", stamina, System.Data.DbType.UInt16),
                    new DbColumn("round", round, System.Data.DbType.UInt32),
                    new DbColumn("turn", turn, System.Data.DbType.UInt32),
                    new DbColumn("report_flag", report.ReportFlag, System.Data.DbType.Boolean),
                    new DbColumn("report_started", report.ReportStarted, System.Data.DbType.Boolean),
                    new DbColumn("report_id", report.ReportId, System.Data.DbType.UInt32)
                };
            }
        }

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion
    }
}
