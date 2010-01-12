#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Fighting;
using Game.Util;

#endregion

namespace Game.Battle {
    public class BattleChannel {
        private string channelName;
        private BattleManager battle;

        public BattleChannel(BattleManager battle, string channelName) {
            this.battle = battle;
            this.channelName = channelName;
            battle.ActionAttacked += battle_ActionAttacked;
            battle.ReinforceAttacker += battle_ReinforceAttacker;
            battle.ReinforceDefender += battle_ReinforceDefender;
            battle.ExitBattle += battle_ExitBattle;
        }

        private void battle_ExitBattle(CombatList atk, CombatList def) {
            Packet packet = new Packet(Command.BATTLE_ENDED);
            Global.Channel.Post(channelName, packet);
        }

        private void battle_ReinforceDefender(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_DEFENDER);
            packet.addUInt16(battle.Stamina);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void battle_ReinforceAttacker(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_ATTACKER);
            packet.addUInt16(battle.Stamina);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            Packet packet = new Packet(Command.BATTLE_ATTACK);
            packet.addUInt16(battle.Stamina);
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
            if (EnterBattle != null)
                EnterBattle(atk, def);
        }

        public void EventExitBattle(CombatList atk, CombatList def) {
            if (ExitBattle != null)
                ExitBattle(atk, def);
        }

        public void EventEnterRound(CombatList atk, CombatList def, uint round, int stamina) {
            if (EnterRound != null)
                EnterRound(atk, def, round, stamina);
        }

        public void EventEnterTurn(CombatList atk, CombatList def, int turn) {
            if (EnterTurn != null)
                EnterTurn(atk, def, turn);
        }

        public void EventExitTurn(CombatList atk, CombatList def, int turn) {
            if (ExitTurn != null)
                ExitTurn(atk, def, turn);
        }

        public void EventReinforceAttacker(IEnumerable<CombatObject> list) {
            if (ReinforceAttacker != null)
                ReinforceAttacker(list);
        }

        public void EventReinforceDefender(IEnumerable<CombatObject> list) {
            if (ReinforceDefender != null)
                ReinforceDefender(list);
        }

        public void EventWithdrawAttacker(IEnumerable<CombatObject> list) {
            if (WithdrawAttacker != null)
                WithdrawAttacker(list);
        }

        public void EventWithdrawDefender(IEnumerable<CombatObject> list) {
            if (WithdrawDefender != null)
                WithdrawDefender(list);
        }

        public void EventUnitRemoved(CombatObject obj) {
            if (UnitRemoved != null)
                UnitRemoved(obj);
        }

        public void EventUnitAdded(CombatObject obj) {
            if (UnitAdded != null)
                UnitAdded(obj);
        }

        public void EventUnitUpdated(CombatObject obj) {
            if (UnitUpdated != null)
                UnitUpdated(obj);
        }

        public void EventActionAttacked(CombatObject source, CombatObject target, ushort dmg) {
            if (ActionAttacked != null)
                ActionAttacked(source, target, dmg);
        }
    }

    public class BattleManager : BattleBase, IPersistableObject {
        private LargeIdGenerator idGen = new LargeIdGenerator(ushort.MaxValue);
        private LargeIdGenerator groupIdGen = new LargeIdGenerator(ushort.MaxValue);

        private CombatList attackers = new CombatList();
        private CombatList defenders = new CombatList();

        private BattleOrder battleOrder = new BattleOrder(0);
        private BattleReport report;

        private BattleChannel channel;

        #region Properties Members

        private uint battleId;

        public uint BattleId {
            get { return battleId; }
            set { battleId = value; }
        }

        private bool battleStarted = false;

        public bool BattleStarted {
            get { return battleStarted; }
            set { battleStarted = value; }
        }

        private uint round = 0;

        public uint Round {
            get { return round; }
            set { round = value; }
        }

        private uint turn = 0;

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
            city = owner;
            stamina = BattleFormulas.GetStamina(city);
            report = new BattleReport(this);
            channel = new BattleChannel(this, "/BATTLE/" + city.CityId);
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

            if (structure.IsDead)
                return;

            defenders.Add(structure, false);

            idGen.set((int) structure.Id);
        }

        public void dbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal) {
            obj.Id = id;

            if (obj.IsDead)
                return;

            if (isLocal)
                defenders.Add(obj, false);
            else
                attackers.Add(obj, false);

            idGen.set((int) obj.Id);
            groupIdGen.set((int) obj.GroupId);
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
                    combatObject.Id = (uint) idGen.getNext();
                    combatObject.GroupId = 1;
                    defenders.Add(combatObject);
                    list.Add(combatObject);
                }

                if (battleStarted && added) {
                    stamina = BattleFormulas.GetStaminaReinforced(city, stamina, round);
                    report.WriteReportObjects(list, false, ReportState.Staying);
                    EventReinforceDefender(list);
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
                    if (objects.Contains(combatStructure.Structure))
                        list.Add(obj);
                }

                defenders.RemoveAll(delegate(CombatObject obj) { return list.Contains(obj); });

                if (battleStarted) {
                    report.WriteReportObjects(list, false, ReportState.Exiting);
                    EventWithdrawDefender(list);
                    refreshBattleOrder();
                }
            }
        }

        public void removeFromDefense(List<TroopStub> objects, ReportState state) {
            removeFromCombatList(objects, defenders, state);
        }

        private void addToCombatList(List<TroopStub> objects, CombatList combatList, bool isLocal, ReportState state) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (TroopStub obj in objects) {
                    uint id = 1;

                    if (!isLocal)
                        id = (uint) idGen.getNext();

                    foreach (
                        KeyValuePair<FormationType, Formation> formation in
                            obj as IEnumerable<KeyValuePair<FormationType, Formation>>) {
                        if (formation.Key == FormationType.Garrison || formation.Key == FormationType.InBattle)
                            continue;

                        FormationType formationType = formation.Key;
                        //if it's our local troop then it should be in the battle formation since 
                        //it will be moved by the battle manager (who is calling this function) to the in battle formation
                        //There should be a better way to do this but I cant figure it out right now
                        if (isLocal && formationType == FormationType.Normal)
                            formationType = FormationType.InBattle;

                        foreach (KeyValuePair<ushort, ushort> kvp in formation.Value) {
                            CombatObject[] combatObjects;
                            if (combatList == defenders)
                                combatObjects = CombatUnitFactory.CreateDefenseCombatUnit(this, obj, formationType,
                                                                                          kvp.Key, kvp.Value);
                            else
                                combatObjects = CombatUnitFactory.CreateAttackCombatUnit(this, obj.TroopObject,
                                                                                         formationType, kvp.Key,
                                                                                         kvp.Value);

                            foreach (CombatObject unit in combatObjects) {
                                added = true;
                                unit.LastRound = round;
                                unit.Id = (uint) idGen.getNext();
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

                    refreshBattleOrder();
                }
            }
        }

        private void removeFromCombatList(List<TroopStub> objects, CombatList combatList, ReportState state) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();
                foreach (CombatObject obj in combatList) {
                    if (!(obj is ICombatUnit))
                        continue;

                    ICombatUnit unit = obj as ICombatUnit;
                    if (objects.Contains(unit.TroopStub))
                        list.Add(obj);
                }

                combatList.RemoveAll(delegate(CombatObject obj) { return list.Contains(obj); });

                if (battleStarted) {
                    if (combatList == Attacker) {
                        report.WriteReportObjects(list, true, state);
                        EventWithdrawAttacker(list);
                    } else if (combatList == Defender) {
                        report.WriteReportObjects(list, false, state);
                        EventWithdrawDefender(list);
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

        private bool battleIsValid() {
            if (attackers.Count == 0 || defenders.Count == 0)
                return false;

            //Check to see if there are still units in the local troop
            bool localUnits = false;
            foreach (CombatObject combatObj in defenders) {
                if (combatObj.IsDead)
                    continue;

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

            if (!localUnits)
                return false;

            //Make sure units can still see each other
            foreach (CombatObject combatObj in attackers) {
                CombatObject target;
                CombatList.BestTargetResult result = defenders.GetBestTarget(combatObj, out target);

                if (result == CombatList.BestTargetResult.Ok || result == CombatList.BestTargetResult.NoneVisible)
                    return true;
            }

            return false;
        }

        private void battleEnded() {
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
                        stamina = BattleFormulas.GetStaminaRoundEnded(city, stamina, turn);
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

                    ushort dmg = BattleFormulas.GetDamage(attacker, defender, attacker.CombatList == defenders);
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
                            if (defender.ClassType == BattleClass.Structure)
                                stamina = BattleFormulas.GetStaminaStructureDestroyed(city, stamina, round);
                            defenders.Remove(defender);
                            report.WriteReportObject(defender, false,
                                                     groupIsDead(defender, defenders)
                                                         ? ReportState.Dying
                                                         : ReportState.Staying);
                        } else if (attacker.CombatList == defenders) {
                            attackers.Remove(defender);
                            report.WriteReportObject(defender, true,
                                                     groupIsDead(defender, attackers)
                                                         ? ReportState.Dying
                                                         : ReportState.Staying);
                        }

                        defender.CleanUp();
                    }

                    if (attacker.CombatList == Attacker) {
                        Resource loot = BattleFormulas.GetRewardResource(attacker, defender, actualDmg);
                        city.BeginUpdate();
                        city.Resource.Subtract(loot, out loot);
                        attacker.ReceiveReward(defender.Stats.Base.Reward*actualDmg, loot);
                        city.EndUpdate();
                    }

                    EventActionAttacked(attacker, defender, (ushort) actualDmg);

                    attacker.ParticipatedInRound();

                    EventExitTurn(Attacker, Defender, (int) turn++);

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

        private bool dbPersisted = false;

        public const string DB_TABLE = "battle_managers";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new DbColumn[] {new DbColumn("city_id", city.CityId, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[]
                       {new DbDependency("ReportedObjects", true, true), new DbDependency("ReportedTroops", true, true)};
            }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
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

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

        #endregion
    }
}