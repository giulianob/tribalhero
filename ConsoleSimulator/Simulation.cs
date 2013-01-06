#region

using System;
using System.Linq;
using System.Threading;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

#endregion

namespace ConsoleSimulator
{
    public class Simulation
    {
        private readonly IBattleManager battleManager;

        private BattleViewer bv;

        public Simulation(Group attack, Group defense)
        {
            Attacker = attack;
            Defender = defense;
            CurrentRound = 0;
            TurnIntervalInSecond = 0;

            battleManager =
                    Ioc.Kernel.Get<IBattleManagerFactory>()
                       .CreateBattleManager(new BattleLocation(BattleLocationType.City, Defender.City.Id),
                                            new BattleOwner(BattleOwnerType.City, Defender.City.Id),
                                            Defender.City);
            battleManager.BattleReport.Battle = battleManager;
            bv = new BattleViewer(battleManager);

            // Add local to battle
            using (Concurrency.Current.Lock(Defender.Local))
            {
                Defender.Local.BeginUpdate();
                Defender.Local.AddFormation(FormationType.InBattle);
                Defender.Local.Template.LoadStats(TroopBattleGroup.Local);
                var localGroup = new CityDefensiveCombatGroup(battleManager.BattleId,
                                                              1,
                                                              Defender.Local,
                                                              Ioc.Kernel.Get<IDbManager>());
                var combatUnitFactory = Ioc.Kernel.Get<ICombatUnitFactory>();
                foreach (var kvp in Defender.Local[FormationType.Normal])
                {
                    combatUnitFactory.CreateDefenseCombatUnit(battleManager,
                                                              Defender.Local,
                                                              FormationType.InBattle,
                                                              kvp.Key,
                                                              kvp.Value).ToList().ForEach(localGroup.Add);
                }

                foreach (IStructure structure in Defender.City)
                {
                    localGroup.Add(combatUnitFactory.CreateStructureCombatUnit(battleManager, structure));
                }
                battleManager.Add(localGroup, BattleManager.BattleSide.Defense, false);
                Ioc.Kernel.Get<BattleProcedure>()
                   .MoveUnitFormation(Defender.Local, FormationType.Normal, FormationType.InBattle);
                Defender.Local.EndUpdate();
            }

            // Add attack stub to battle
            using (Concurrency.Current.Lock(Attacker.AttackStub))
            {
                Attacker.AttackStub.BeginUpdate();
                Attacker.AttackStub.Template.LoadStats(TroopBattleGroup.Attack);
                Attacker.AttackStub.EndUpdate();
                var attackGroup = new CityDefensiveCombatGroup(battleManager.BattleId,
                                                               2,
                                                               Attacker.AttackStub,
                                                               Ioc.Kernel.Get<IDbManager>());
                var combatUnitFactory = Ioc.Kernel.Get<ICombatUnitFactory>();
                foreach (var kvp in Attacker.AttackStub[FormationType.Normal])
                {
                    combatUnitFactory.CreateAttackCombatUnit(battleManager,
                                                             Attacker.TroopObject,
                                                             FormationType.InBattle,
                                                             kvp.Key,
                                                             kvp.Value).ToList().ForEach(attackGroup.Add);
                }
                battleManager.Add(attackGroup, BattleManager.BattleSide.Attack, false);
            }
        }

        public Group Attacker { get; private set; }

        public Group Defender { get; private set; }

        public uint CurrentRound { get; private set; }

        public int TurnIntervalInSecond { get; set; }

        public void Run()
        {
            RunTill();
        }

        public void RunTill(int round = int.MaxValue)
        {
            using (Concurrency.Current.Lock(Attacker.AttackStub, Defender.Local))
            {
                while (battleManager.ExecuteTurn())
                {
                    CurrentRound = battleManager.Round;
                    if ((CurrentRound = battleManager.Round) >= round)
                    {
                        return;
                    }
                    Thread.Sleep(new TimeSpan(0, 0, 0, TurnIntervalInSecond));
                }
            }
        }
    }
}