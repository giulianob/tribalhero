#region

using System;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Fighting;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Game.Logic;

#endregion

namespace Game.Battle {
    public class BattleChannel {
        private readonly string channelName;
        private readonly BattleManager battle;

        public BattleChannel(BattleManager battle, string channelName) {
            this.battle = battle;
            this.channelName = channelName;
            battle.ActionAttacked += BattleActionAttacked;
            battle.SkippedAttacker += BattleSkippedAttacker;
            battle.ReinforceAttacker += BattleReinforceAttacker;
            battle.ReinforceDefender += BattleReinforceDefender;
            battle.ExitBattle += BattleExitBattle;
        }

        private void BattleExitBattle(CombatList atk, CombatList def) {
            Packet packet = new Packet(Command.BATTLE_ENDED);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceDefender(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_DEFENDER);
            packet.AddUInt16(battle.Stamina);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceAttacker(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_ATTACKER);
            packet.AddUInt16(battle.Stamina);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleSkippedAttacker(CombatObject source) {
            Packet packet = new Packet(Command.BATTLE_SKIPPED);
            packet.AddUInt16(battle.Stamina);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            Packet packet = new Packet(Command.BATTLE_ATTACK);
            packet.AddUInt16(battle.Stamina);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddUInt16(damage);
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

        public event OnUnitUpdate SkippedAttacker;

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

        public void EventSkippedAttacker(CombatObject source) {
            if (SkippedAttacker != null)
                SkippedAttacker(source);
        }
    }

    public class BattleManager : BattleBase, IPersistableObject {
        private readonly LargeIdGenerator idGen = new LargeIdGenerator(ushort.MaxValue);
        private readonly LargeIdGenerator groupIdGen = new LargeIdGenerator(ushort.MaxValue);

        private readonly CombatList attackers = new CombatList();
        private readonly CombatList defenders = new CombatList();

        private BattleOrder battleOrder = new BattleOrder(0);
        private readonly BattleReport report;

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
            groupIdGen.set(1);
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
        
            foreach (CombatObject co in defenders) {
                if (co.City.Owner == player && co.RoundsParticipated >= Config.battle_min_rounds)
                    return true;
            }

            foreach (CombatObject co in attackers) {
                if (co.City.Owner == player && co.RoundsParticipated >= Config.battle_min_rounds)
                    return true;
            }

            return false;
        }

        #region Database Loader

        public void DbLoaderAddToLocal(CombatStructure structure, uint id) {
            structure.Id = id;

            if (structure.IsDead)
                return;

            defenders.Add(structure, false);

            idGen.set((int)structure.Id);
        }

        public void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal) {
            obj.Id = id;

            if (obj.IsDead)
                return;

            if (isLocal)
                defenders.Add(obj, false);
            else
                attackers.Add(obj, false);

            idGen.set((int)obj.Id);
            groupIdGen.set((int)obj.GroupId);
        }

        #endregion

        #region Adding/Removing from Battle


        public void AddToLocal(List<TroopStub> objects, ReportState state) {
            AddToCombatList(objects, defenders, true, state);
        }

        public void AddToLocal(List<Structure> objects) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (Structure obj in objects) {
                    if (obj.Stats.Hp == 0 || defenders.Contains(obj))
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
                        Id = (uint)idGen.getNext(),
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
            lock (this) {
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
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();

                bool added = false;

                foreach (TroopStub obj in objects) {
                    uint id = 1;

                    if (!isLocal)
                        id = (uint) idGen.getNext();

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

                    RefreshBattleOrder();
                }
            }
        }

        private void RemoveFromCombatList(List<TroopStub> objects, CombatList combatList, ReportState state) {
            lock (this) {
                List<CombatObject> list = new List<CombatObject>();
                foreach (CombatObject obj in combatList) {
                    if (!(obj is ICombatUnit))
                        continue;

                    ICombatUnit unit = obj as ICombatUnit;
                    if (objects.Contains(unit.TroopStub))
                        list.Add(obj);
                }

                combatList.RemoveAll(list.Contains);

                if (!battleStarted)
                    return;

                if (combatList == Attacker) {
                    report.WriteReportObjects(list, true, state);
                    EventWithdrawAttacker(list);
                } else if (combatList == Defender) {
                    report.WriteReportObjects(list, false, state);
                    EventWithdrawDefender(list);
                }

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

                if (result == CombatList.BestTargetResult.OK || result == CombatList.BestTargetResult.NONE_VISIBLE)
                    return true;
            }

            return false;
        }

        private void BattleEnded() {
            report.WriteBeginReport();
            report.CompleteReport(ReportState.EXITING);
            report.CompleteBattle();

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

        public bool GroupIsDead(CombatObject co, CombatList combatList) {
            foreach (CombatObject combatObject in combatList) {
                if (combatObject.GroupId == co.GroupId && !combatObject.IsDead)
                    return false;
            }

            return true;
        }

        public bool ExecuteTurn() {
            lock (this) {
                report.WriteReport(ReportState.STAYING);

                if (!battleStarted) {
                    RefreshBattleOrder();
                    battleStarted = true;
                    report.CreateBattleReport();
                    EventEnterBattle(attackers, defenders);
                }

                CombatObject attacker;

                if (!battleOrder.NextObject(out attacker)) {
                    ++round;
                    stamina = BattleFormulas.GetStaminaRoundEnded(city, stamina, turn);
                    battleOrder.ParticipatedInRound();
                    turn = 0;
                    EventEnterRound(Attacker, Defender, round, stamina);
                }

                if (attacker == null || defenders.Count == 0 || attackers.Count == 0 || stamina <= 0) {
                    BattleEnded();
                    return false;
                }

                #region Find Target

                CombatObject defender;

                if (attacker.CombatList == attackers)
                    defenders.GetBestTarget(attacker, out defender);
                else if (attacker.CombatList == defenders)
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

                defender.DmgRecv += actualDmg;
                defender.MaxDmgRecv = Math.Max(defender.MaxDmgRecv, actualDmg);
                defender.MinDmgRecv = Math.Min(defender.MinDmgRecv, actualDmg);
                ++defender.HitRecv;

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
                    }

                    if (city.IsUpdating)
                        city.EndUpdate();
                }

                #endregion

                #region Object removal

                if (defender.IsDead) {
                    EventUnitRemoved(defender);
                    battleOrder.Remove(defender);

                    if (attacker.CombatList == attackers) {
                        if (defender.ClassType == BattleClass.STRUCTURE)
                            stamina = BattleFormulas.GetStaminaStructureDestroyed(city, stamina, round);
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

                EventActionAttacked(attacker, defender, actualDmg);

                attacker.ParticipatedInRound();                

                EventExitTurn(Attacker, Defender, (int) turn++);

                Global.DbManager.Save(attacker);

                if (!BattleIsValid()) {
                    BattleEnded();
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