using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Battle;
using Game.Fighting;
using Game.Setup;
using System.IO;
using Game.Data.Stats;
using Game.Data.Troop;

namespace ConsoleSimulator {
    public class Simulation {
        public enum QuantityUnit {
            Single,
            GroupSize,
            EvenCost
        }
        ushort type, count;
        byte lvl;
        QuantityUnit unit;
        bool sameLevelOnly;


        StreamWriter sw;
        public Simulation(ushort type, byte lvl, ushort count, QuantityUnit unit, bool sameLevelOnly) {
            this.type = type;
            this.lvl = lvl;
            this.count = count;
            this.unit = unit;
            this.sameLevelOnly = sameLevelOnly;
        }

        public void RunDef(String filename) {            
            ushort defCount, atkCount;

            using (sw = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))) {
                sw.WriteLine("{0} - Lvl {1} - Cnt {2} - Defending", UnitFactory.GetName(type, lvl), lvl, count);
                sw.WriteLine("name,type,lvl,count,DealtToAtker,RecvFromAtker,HitDealt,HitRecv,MaxDealt,MinDealt,MaxRecv,MinRecv");
                foreach (KeyValuePair<int, BaseUnitStats> kvp in UnitFactory.dict) {
                    if (sameLevelOnly && kvp.Value.Lvl != lvl) continue;
                    switch (unit) {
                        case QuantityUnit.Single:
                            defCount = atkCount = count;
                            break;
                        case QuantityUnit.GroupSize:
                            defCount = (ushort)(UnitFactory.GetUnitStats(type, lvl).Battle.GroupSize * count);
                            atkCount = (ushort)(UnitFactory.GetUnitStats((ushort)(kvp.Key / 100), lvl).Battle.GroupSize * count);
                            break;
                        default:
                            throw new Exception();
                    }
                    Group defender = new Group();
                    Group attacker = new Group();


                    defender.AddToLocal(type, lvl, defCount, FormationType.NORMAL);
                    attacker.AddToAttack((ushort)(kvp.Key / 100), kvp.Value.Lvl, atkCount, FormationType.NORMAL);
                    sw.Write("{0},{1},{2},{3},", kvp.Value.Name, kvp.Key / 100, kvp.Value.Lvl, atkCount);
                    BattleManager bm = new BattleManager(defender.City);

                    bm.ExitBattle += new BattleBase.OnBattle(bm_ExitBattle);
                    bm.UnitRemoved += new BattleBase.OnUnitUpdate(bm_UnitRemoved);
                    bm.AddToLocal(new List<TroopStub> {defender.Local}, ReportState.ENTERING);
                    bm.AddToAttack(attacker.AttackStub);
                    while (bm.ExecuteTurn()) ;
                    bm.ExitBattle -= new BattleBase.OnBattle(bm_ExitBattle);
                    bm.UnitRemoved -= new BattleBase.OnUnitUpdate(bm_UnitRemoved);
                }
                sw.WriteLine();
            }
        }

        public void RunAtk(String filename) {
            ushort defCount, atkCount;

            using (sw = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))) {
                sw.WriteLine("{0} - Lvl {1} - Cnt {2} - Attacking", UnitFactory.GetName(type, lvl), lvl, count);
                sw.WriteLine("name,type,lvl,count,DealtToDefender,RecvFromDefender,HitDealt,HitRecv,MaxDealt,MinDealt,MaxRecv,MinRecv");
                foreach (KeyValuePair<int, BaseUnitStats> kvp in UnitFactory.dict) {
                    if (sameLevelOnly && kvp.Value.Lvl != lvl) continue;
                    switch (unit) {
                        case QuantityUnit.Single:
                            defCount = atkCount = count;
                            break;
                        case QuantityUnit.GroupSize:
                            atkCount = (ushort)(UnitFactory.GetUnitStats(type, lvl).Battle.GroupSize * count);
                            defCount = (ushort)(UnitFactory.GetUnitStats((ushort)(kvp.Key / 100), lvl).Battle.GroupSize * count);
                            break;
                        default:
                            throw new Exception();
                    }
                    Group defender = new Group();
                    Group attacker = new Group();

                    defender.AddToLocal((ushort)(kvp.Key / 100), kvp.Value.Lvl, defCount, FormationType.NORMAL);
                    attacker.AddToAttack(type, lvl, atkCount, FormationType.NORMAL);
                    sw.Write("{0},{1},{2},{3},", kvp.Value.Name, kvp.Key / 100, kvp.Value.Lvl, defCount);
                    BattleManager bm = new BattleManager(defender.City);

                    bm.ExitBattle +=new BattleBase.OnBattle(bm_ExitBattle2);
                    bm.UnitRemoved += new BattleBase.OnUnitUpdate(bm_UnitRemoved2);

                    bm.AddToLocal(new List<TroopStub> {defender.Local}, ReportState.ENTERING);
                    bm.AddToAttack(attacker.AttackStub);
                    while (bm.ExecuteTurn()) ;
                    bm.ExitBattle -= new BattleBase.OnBattle(bm_ExitBattle2);
                    bm.UnitRemoved -= new BattleBase.OnUnitUpdate(bm_UnitRemoved2);
                }
                sw.WriteLine();
            }
        }

        void writeResult(CombatObject obj) {
            sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", 
                obj.DmgDealt,
                obj.DmgRecv,
                obj.HitDealt, 
                obj.HitRecv,
                obj.MaxDmgDealt,
                obj.MinDmgDealt==int.MaxValue?0:obj.MinDmgDealt, 
                obj.MaxDmgRecv,
                obj.MinDmgRecv==int.MaxValue?0:obj.MinDmgRecv);
        }
        void bm_UnitRemoved(CombatObject obj) {
            if (obj is DefenseCombatUnit) {
                if (sw!=null) {
                    writeResult(obj);
                }
            }
        }

        void bm_ExitBattle(CombatList atk, CombatList def) {
            if (def.Count > 0) {
                if (sw != null) {
                    writeResult(def[0]);
                }
            }
        }

        void bm_UnitRemoved2(CombatObject obj) {
            if (obj is AttackCombatUnit) {
                if (sw != null) {
                    writeResult(obj);
                }
            }
        }

        void bm_ExitBattle2(CombatList atk, CombatList def) {
            if (atk.Count > 0) {
                if (sw != null) {
                    writeResult(atk[0]);
                }
            }
        }

    }

}
