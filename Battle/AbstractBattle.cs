using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Game.Fighting {
    public class BattleCalulator {
        public static Random random = new Random();

        public static Unit getBestTarget(Troop atk, Troop def, Unit unit) {
            List<Unit> list = new List<Unit>(def.vlist.getList(1, atk.Vision));
            if (list.Count == 0) return null;
            return list[random.Next(list.Count - 1)];
        }

        public static ushort getDamage(Unit attacker, Unit target) {
           
            if (attacker.stats.Atk > target.stats.Def) { 
                if (attacker.count > target.count) { // good fire power, good quantity
                    return (ushort)(attacker.count / 3);
                } else {                            // good fire power, bad quantity
                    return (ushort)(attacker.count / 5);
                }
            } else {
                if (attacker.count > target.count) { // bad fire power,good quantity
                    return (ushort)(attacker.count / 4);
                } else {                            // bad fire power, bad quantity
                    return (ushort)(attacker.count / 6);
                }
            } 
           
        }

        internal static void updateVision(Troop force) {
            force.Vision += 15;
        }
    }
    public class Participant : IEnumerator<Unit> {
        List<BattleFormation> formations;
        int f_index;
        IEnumerator<Unit> itr;
        public Participant(List<BattleFormation> formations) {
            this.formations = formations;
            Reset();
        }

        public bool remove(Unit unit) {
            foreach (BattleFormation formation in formations) {
                if (formation.remove(unit)) {
                    return true;
                }
            }
            return false;
        }

        #region IEnumerator<Unit> Members

        public Unit Current {
            get { return itr.Current; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current {
            get { return itr.Current; }
        }

        public bool MoveNext() {
            if (itr == null) return false;
            if (itr.MoveNext()) {
                return true;
            }
            if( f_index+1>=formations.Count ) {
                return false;
            }
            itr = formations[++f_index].GetEnumerator();
            itr.Reset();
            return MoveNext();
        }

        public void Reset() {
            f_index = 0;
            if( formations.Count>0 ) {
                itr = formations[f_index].GetEnumerator();
                itr.Reset();
            } else {
                itr = null;
            }
        }

        #endregion
    }
    public class StandardBattle {
        object next_mutex = new object();
        uint next_id = 0;
        public uint NextId() {
            lock (next_mutex) {
                return ++next_id;
            }
        }

        TimeSpan delay = TimeSpan.FromSeconds(1);
        public TimeSpan Delay {
            get { return this.delay; }
            set { this.delay = value; }
        }
        public delegate void OnBattle(Troop atk, Troop def);
        public event OnBattle EnterBattle;
        public event OnBattle ExitBattle;

        public delegate void OnRound(Troop atk, Troop def, int round);
        public event OnRound EnterRound;
        public event OnRound ExitRound;

        public delegate void OnPhase(Troop atk, Troop def, string phase);
        public event OnPhase EnterPhase;
        public event OnPhase ExitPhase;

        public delegate void OnVisionChanged(Troop troop, int old_vsn, int new_vsn);
        public event OnVisionChanged VisionChanged;

        public delegate void OnUnitUpdate(Troop troop, FormationType type, Unit unit);
        public event OnUnitUpdate UnitAdded;
        public event OnUnitUpdate UnitRemoved;
        public event OnUnitUpdate UnitUpdated;

        public delegate void OnAttack(Unit source, Unit target, ushort damage);
        public event OnAttack ActionAttacked;

        public void EventEnterBattle(Troop atk, Troop def) {
            if (this.EnterBattle != null) this.EnterBattle(atk, def);
        }
        public void EventExitBattle(Troop atk, Troop def) {
            if (this.ExitBattle != null) this.ExitBattle(atk, def);
        }
        public void EventEnterRound(Troop atk, Troop def,int round) {
            if (this.EnterRound != null) this.EnterRound(atk, def, round);
        }
        public void EventExitRound(Troop atk, Troop def,int round) {
            if (this.ExitRound != null) this.ExitRound(atk, def, round);
        }
        public void EventEnterPhase(Troop atk, Troop def, String phase) {
            if (this.EnterPhase != null) this.EnterPhase(atk, def, phase);
        }
        public void EventExitPhase(Troop atk, Troop def, String phase) {
            if (this.ExitPhase != null) this.ExitPhase(atk, def, phase);
        }
        public void EventVisionChanged(Troop troop, int old_vsn, int new_vsn) {
            if (this.VisionChanged != null) this.VisionChanged(troop, old_vsn, new_vsn);
        }
        public void EventUnitRemoved(Troop troop, FormationType type, Unit unit) {
            if (this.UnitRemoved != null) this.UnitRemoved(troop, type, unit);
        }
        public void EventUnitAdded(Troop troop, FormationType type, Unit unit) {
            if (this.UnitAdded != null) this.UnitAdded(troop, type, unit);
        }
        public void EventUnitUpdated(Troop troop, FormationType type, Unit unit) {
            if (this.UnitUpdated != null) this.UnitUpdated(troop, type, unit);
        }
        public void EventActionAttacked(Unit source, Unit target, ushort dmg) {
            if (this.ActionAttacked != null) this.ActionAttacked(source, target, dmg);
        }
    }

    public class Battle : StandardBattle {
        public void attack_phase(Troop atk_troop,List<BattleFormation> atk_formations,Troop def_troop, List<BattleFormation> def_formations) {
            Participant atk_p = new Participant(atk_formations);
            Participant def_p = new Participant(def_formations);

            
            bool atk_done = false, def_done = false;
            EventEnterPhase(atk_troop, def_troop, "Attack");
            IEnumerator<Unit> atk = (IEnumerator<Unit>)atk_p;
            IEnumerator<Unit> def = (IEnumerator<Unit>)def_p;
            atk.Reset();
            def.Reset(); 
            
            while (!atk_done || !def_done) {
                if (atk.MoveNext()) {
                    Unit target = BattleCalulator.getBestTarget(atk_troop, def_troop, atk.Current);
                    if (target != null) {
                        ushort dmg = BattleCalulator.getDamage(atk.Current, target);
                        Console.Out.WriteLine("Attacker[{0}] attacks unit[{1}][{2}] dmg[{3}]",
                                atk.Current.type,
                                target.type,
                                target.count,
                                dmg);

                        if (target.count <= dmg) {
                            Console.Out.WriteLine("unit[{0}] died", target.type);
                            target.count = 0;
                            if (!def_p.remove(target)) { // use the enumerator to remove object to prevent breaking enumaration
                                def_troop.remove(target);
                            }
                            def_troop.vlist.remove(target);
                            EventUnitRemoved(def_troop, target.formation_type, target);
                        } else {
                            target.count -= dmg;
                            if (dmg != 0) EventUnitUpdated(def_troop, target.formation_type, target);
                        }
                        EventActionAttacked(atk.Current, target, dmg); 
                        System.Threading.Thread.Sleep(Delay);
                    } else {
                        Console.Out.WriteLine("Attacker[{0}] found nothing to attack.", atk.Current.type);
                    }
                } else {
                    atk_done = true;
                }

                if (def.MoveNext()) {
                    Unit target = BattleCalulator.getBestTarget(def_troop, atk_troop, def.Current);
                    if (target != null) {
                        ushort dmg = BattleCalulator.getDamage(def.Current, target);
                        Console.Out.WriteLine("Defender[{0}] attacks unit[{1}][{2}] dmg[{3}]",
                                def.Current.type,
                                target.type,
                                target.count,
                                dmg);
                        if (target.count <= dmg) {
                            Console.Out.WriteLine("unit[{0}] died", target.type);

                            if (!atk_p.remove(target)) {
                                atk_troop.remove(target);
                            }
                            atk_troop.vlist.remove(target);
                            if (atk_troop.Size != atk_troop.vlist.Size) {
                                Console.Out.WriteLine("something is fishy");
                            }
                            target.count = 0;
                            EventUnitRemoved(atk_troop, target.formation_type, target);
                        } else {
                            target.count -= dmg;
                            if (dmg != 0) EventUnitUpdated(atk_troop, target.formation_type, target);
                        }
                        EventActionAttacked(def.Current, target, dmg);
                        System.Threading.Thread.Sleep(Delay);
                    } else {
                        Console.Out.WriteLine("Defender[{0}] found nothing to attack.", def.Current.type);
                    }
                } else {
                    def_done = true;
                }
            }
            EventExitPhase(atk_troop, def_troop, "Attack");
        }



        public void prepare(Troop troop) {
            troop.id = NextId();
            foreach( KeyValuePair<FormationType,BattleFormation> kvp in troop ) {
                kvp.Value.shuffle();
                foreach(Unit unit in kvp.Value) {
                    unit.id = NextId();
                    unit.formation_type = kvp.Key;
                }
            }
        }

        Troop atk, def;
        public void start(Troop atk , Troop def) {
            this.atk = atk;
            this.def = def;
            Thread newthread = new Thread(new ThreadStart(this.exec));
            newthread.Start();
          }
        private void exec() {
            prepare(atk);
            prepare(def);
            EventEnterBattle(atk, def);

            int atk_count = atk.Size;
            int def_count = def.Size;
            int round = 1;
            while (atk.Size > 0 && def.Size > 0) {
                EventEnterRound(atk, def, round);
                attack_phase(atk, atk.getList(new FormationType[] { FormationType.Attack }), def, def.getList(new FormationType[] { FormationType.Attack }));
                Console.Out.WriteLine("Special phase");
                attack_phase(atk,
                              atk.getList(new FormationType[] { FormationType.Defense }),
                              def,
                              def.getList(new FormationType[] { FormationType.Defense }));
                attack_phase(atk,
                              atk.getList(new FormationType[] { FormationType.Scout }),
                              def,
                              def.getList(new FormationType[] { FormationType.Scout }));
                ushort old_vsn = atk.Vision;
                BattleCalulator.updateVision(atk);
                EventVisionChanged(atk, old_vsn, atk.Vision);
                old_vsn = def.Vision;
                BattleCalulator.updateVision(def);
                EventVisionChanged(def, old_vsn, def.Vision);
                EventEnterRound(atk, def, round++);
            }
             //Console.ReadKey();
            EventExitBattle(atk, def);
        }
    }
}
