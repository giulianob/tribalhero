#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Battle;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace ConsoleSimulator
{
    public class Simulation
    {
        public Group Attacker{ get; private set; }
        public Group Defender{ get; private set; }
        public uint CurrentRound { get; private set; }
        private BattleManager bm;
        private BattleViewer bv;

        public Simulation(Group attack, Group defense)
        {
            Attacker = attack;
            Defender = defense;
            CurrentRound = 0;
            TurnIntervalInSecond = 0;

            bm = new BattleManager(Defender.City);
            bv = new BattleViewer(bm);
            using (new MultiObjectLock(Defender.Local)) {
                Defender.Local.BeginUpdate();
                Defender.Local.AddFormation(FormationType.InBattle);
                Defender.Local.Template.LoadStats(TroopBattleGroup.Local);
                bm.AddToLocal(new List<TroopStub> { Defender.Local }, ReportState.Entering);
                bm.AddToLocal(Defender.City);
                Procedure.MoveUnitFormation(Defender.Local, FormationType.Normal, FormationType.InBattle);
                Defender.Local.EndUpdate();
            }

            using (new MultiObjectLock(Attacker.AttackStub)) {
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
            using (new MultiObjectLock(Attacker.AttackStub, Defender.Local)) {
                while (bm.ExecuteTurn()) {
                    CurrentRound = bm.Round;
                    if ((CurrentRound = bm.Round) >= round) return;
                    System.Threading.Thread.Sleep(new TimeSpan(0, 0, 0, TurnIntervalInSecond));
                }
            }
        }
    }
}