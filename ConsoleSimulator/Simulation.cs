#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Battle;
using Game.Comm.Channel;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace ConsoleSimulator
{
    public class Simulation
    {
        public Group Attacker{ get; private set; }
        public Group Defender{ get; private set; }
        public uint CurrentRound { get; private set; }
        private IBattleManager bm;
        private BattleViewer bv;

        public Simulation(Group attack, Group defense)
        {
            Attacker = attack;
            Defender = defense;
            CurrentRound = 0;
            TurnIntervalInSecond = 0;

            bm = Ioc.Kernel.Get<IBattleManagerFactory>().CreateBattleManager(Defender.City);
            bm.BattleReport.Battle = bm;
            bv = new BattleViewer(bm);
            using (Concurrency.Current.Lock(Defender.Local)) {
                Defender.Local.BeginUpdate();
                Defender.Local.AddFormation(FormationType.InBattle);
                Defender.Local.Template.LoadStats(TroopBattleGroup.Local);
                bm.AddToLocal(new List<ITroopStub> { Defender.Local }, ReportState.Entering);
                bm.AddToLocal(Defender.City);
                Procedure.Current.MoveUnitFormation(Defender.Local, FormationType.Normal, FormationType.InBattle);
                Defender.Local.EndUpdate();
            }

            using (Concurrency.Current.Lock(Attacker.AttackStub))
            {
                Attacker.AttackStub.BeginUpdate();
                Attacker.AttackStub.Template.LoadStats(TroopBattleGroup.Attack);
                Attacker.AttackStub.EndUpdate();
                bm.AddToAttack(Attacker.AttackStub);
            }
        }

        public int TurnIntervalInSecond { get; set; }

        public void Run()
        {
            RunTill();
        }

        public void RunTill(int round = int.MaxValue)
        {
            using (Concurrency.Current.Lock(Attacker.AttackStub, Defender.Local))
            {
                while (bm.ExecuteTurn()) {
                    CurrentRound = bm.Round;
                    if ((CurrentRound = bm.Round) >= round) return;
                    System.Threading.Thread.Sleep(new TimeSpan(0, 0, 0, TurnIntervalInSecond));
                }
            }
        }
    }
}