#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Battle;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util.Locking;
using Ninject;

#endregion

namespace ConsoleSimulator
{
    public class FullSimulation
    {
        #region QuantityUnit enum

        public enum QuantityUnit
        {
            Single,
            GroupSize,
            EvenCost
        }

        #endregion

        private readonly ushort count;
        private readonly byte lvl;
        private readonly bool sameLevelOnly;

        private readonly ushort type;
        private readonly QuantityUnit unit;
        private CombatObject deadObject;
        private StreamWriter sw;

        public FullSimulation(ushort type, byte lvl, ushort count, QuantityUnit unit, bool sameLevelOnly)
        {
            this.type = type;
            this.lvl = lvl;
            this.count = count;
            this.unit = unit;
            this.sameLevelOnly = sameLevelOnly;
        }

        public void RunDef(String filename)
        {
            using (sw = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                sw.WriteLine("{0} - Lvl {1} - Cnt {2} - Defending", Ioc.Kernel.Get<UnitFactory>().GetName(type, lvl), lvl, count);
                sw.WriteLine(
                             "name,type,lvl,count,DealtToAtker,RecvFromAtker,HitDealt,HitRecv,MaxDealt,MinDealt,MaxRecv,MinRecv,Self,Enemy");
                foreach (var kvp in Ioc.Kernel.Get<UnitFactory>().GetList())
                {
                    if (sameLevelOnly && kvp.Value.Lvl != lvl)
                        continue;
                    ushort defCount;
                    ushort atkCount;
                    switch(unit)
                    {
                        case QuantityUnit.Single:
                            defCount = atkCount = count;
                            break;
                        case QuantityUnit.GroupSize:
                            defCount = (ushort)(Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, lvl).Battle.GroupSize * count);
                            atkCount =
                                    (ushort)
                                    (Ioc.Kernel.Get<UnitFactory>().GetUnitStats((ushort)(kvp.Key / 100), lvl).Battle.GroupSize * count);
                            break;
                        default:
                            throw new Exception();
                    }
                    var defender = new Group();
                    var attacker = new Group();

                    defender.AddToLocal(type, lvl, defCount, FormationType.Normal);
                    attacker.AddToAttack((ushort)(kvp.Key/100), kvp.Value.Lvl, atkCount, FormationType.Normal);
                    sw.Write("{0},{1},{2},{3},", kvp.Value.Name, kvp.Key/100, kvp.Value.Lvl, atkCount);
                    var bm = Ioc.Kernel.Get<IBattleManagerFactory>().CreateBattleManager(defender.City); 

                    bm.ExitBattle += BmExitBattle;
                    bm.UnitRemoved += BmUnitRemoved;
                    using (Concurrency.Current.Lock(defender.Local))
                    {
                        defender.Local.BeginUpdate();
                        defender.Local.AddFormation(FormationType.InBattle);
                        defender.Local.Template.LoadStats(TroopBattleGroup.Local);
                        bm.AddToLocal(new List<ITroopStub> {defender.Local}, ReportState.Entering);
                        Procedure.Current.MoveUnitFormation(defender.Local, FormationType.Normal, FormationType.InBattle);
                        defender.Local.EndUpdate();
                    }

                    using (Concurrency.Current.Lock(attacker.AttackStub))
                    {
                        attacker.AttackStub.BeginUpdate();
                        attacker.AttackStub.Template.LoadStats(TroopBattleGroup.Attack);
                        attacker.AttackStub.EndUpdate();
                    }
                    bm.AddToAttack(attacker.AttackStub);

                    using (Concurrency.Current.Lock(attacker.AttackStub, defender.Local))
                    {
                        while (bm.ExecuteTurn())
                        {
                        }
                    }
                    bm.ExitBattle -= BmExitBattle;
                    bm.UnitRemoved -= BmUnitRemoved;
                }
                sw.WriteLine();
            }
        }

        public void RunAtk(String filename)
        {
            using (sw = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                sw.WriteLine("{0} - Lvl {1} - Cnt {2} - Attacking", Ioc.Kernel.Get<UnitFactory>().GetName(type, lvl), lvl, count);
                sw.WriteLine(
                             "name,type,lvl,count,DealtToDefender,RecvFromDefender,HitDealt,HitRecv,MaxDealt,MinDealt,MaxRecv,MinRecv");
                foreach (var kvp in Ioc.Kernel.Get<UnitFactory>().GetList())
                {
                    if (sameLevelOnly && kvp.Value.Lvl != lvl)
                        continue;
                    ushort defCount;
                    ushort atkCount;
                    switch(unit)
                    {
                        case QuantityUnit.Single:
                            defCount = atkCount = count;
                            break;
                        case QuantityUnit.GroupSize:
                            atkCount = (ushort)(Ioc.Kernel.Get<UnitFactory>().GetUnitStats(type, lvl).Battle.GroupSize * count);
                            defCount =
                                    (ushort)
                                    (Ioc.Kernel.Get<UnitFactory>().GetUnitStats((ushort)(kvp.Key / 100), lvl).Battle.GroupSize * count);
                            break;
                        default:
                            throw new Exception();
                    }
                    var defender = new Group();
                    var attacker = new Group();

                    defender.AddToLocal((ushort)(kvp.Key/100), kvp.Value.Lvl, defCount, FormationType.Normal);
                    attacker.AddToAttack(type, lvl, atkCount, FormationType.Normal);
                    sw.Write("{0},{1},{2},{3},", kvp.Value.Name, kvp.Key/100, kvp.Value.Lvl, defCount);
                    var bm = Ioc.Kernel.Get<IBattleManagerFactory>().CreateBattleManager(defender.City);

                    bm.ExitBattle += BmExitBattle2;
                    bm.UnitRemoved += BmUnitRemoved2;

                    using (Concurrency.Current.Lock(defender.Local))
                    {
                        defender.Local.BeginUpdate();
                        defender.Local.AddFormation(FormationType.InBattle);
                        defender.Local.Template.LoadStats(TroopBattleGroup.Local);
                        bm.AddToLocal(new List<ITroopStub> {defender.Local}, ReportState.Entering);
                        Procedure.Current.MoveUnitFormation(defender.Local, FormationType.Normal, FormationType.InBattle);
                        defender.Local.EndUpdate();
                    }

                    using (Concurrency.Current.Lock(attacker.AttackStub))
                    {
                        attacker.AttackStub.BeginUpdate();
                        attacker.AttackStub.Template.LoadStats(TroopBattleGroup.Attack);
                        attacker.AttackStub.EndUpdate();
                    }
                    bm.AddToAttack(attacker.AttackStub);

                    using (Concurrency.Current.Lock(attacker.AttackStub, defender.Local))
                    {
                        while (bm.ExecuteTurn())
                        {
                        }
                    }

                    bm.ExitBattle -= BmExitBattle2;
                    bm.UnitRemoved -= BmUnitRemoved2;
                }
                sw.WriteLine();
            }
        }

        private void WriteResult(CombatObject obj)
        {
            sw.Write("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                     obj.DmgDealt,
                     obj.DmgRecv,
                     obj.HitDealt,
                     obj.HitRecv,
                     obj.MaxDmgDealt,
                     obj.MinDmgDealt == ushort.MaxValue ? 0 : obj.MinDmgDealt,
                     obj.MaxDmgRecv,
                     obj.MinDmgRecv == ushort.MaxValue ? 0 : obj.MinDmgRecv,
                     obj.Count);
        }

        private void WriteResultEnd(int enemyCount)
        {
            sw.WriteLine(",{0}", enemyCount);
        }

        private void BmUnitRemoved(IBattleManager battle, CombatObject obj)
        {
            deadObject = obj;
            if (obj is DefenseCombatUnit)
            {
                if (sw != null)
                    WriteResult(obj);
            }
        }

        private void BmExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            if (def.Count > 0)
            {
                if (sw != null)
                {
                    WriteResult(def[0]);
                    WriteResultEnd(deadObject.Count);
                }
            }
            else
                WriteResultEnd(atk[0].Count);
        }

        private void BmUnitRemoved2(IBattleManager battle, CombatObject obj)
        {
            deadObject = obj;
            if (obj is AttackCombatUnit)
            {
                if (sw != null)
                    WriteResult(obj);
            }
        }

        private void BmExitBattle2(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            if (atk.Count > 0)
            {
                if (sw != null)
                {
                    WriteResult(atk[0]);
                    WriteResultEnd(deadObject.Count);
                }
            }
            else
                WriteResultEnd(def[0].Count);
        }
    }
}