using System.Collections.Generic;
using Common.Testing;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class BattleManagerTest2
    {
        [Theory, AutoNSubstituteData]
        public void Execute_WhenHittingUnitKillsTarget_ShouldCarryOverToNextTarget(
                [Frozen] IBattleFormulas battleFormulas,
                [Frozen] IBattleOrder battleOrder,
                ICombatObject attacker1,
                ICombatGroup attackerGroup,
                ICombatObject defender1,
                ICombatObject defender2,
                ICombatGroup defenderGroup,
                BattleManager battleManager
                )
        {
            attacker1.Stats.Atk.Returns(10);

            // Attacker has 300 raw damage
            // Defender takes 150 dmg but only has 100 hp so 50 dmg should be carried over
            // 33% should be carried over
            battleFormulas.GetAttackerDmgToDefender(attacker1, defender1, Arg.Any<uint>()).Returns(300);
            decimal outActualDmg;
            defender1.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg))
                     .Do(args => { args[5] = 150m; });
            defender1.Hp.Returns(100m);

            battleFormulas.GetAttackerDmgToDefender(attacker1, defender2, Arg.Any<uint>()).Returns(200);

            battleManager.Attackers.Count.Returns(1);
            battleManager.Attackers.AllAliveCombatObjects().Returns(new[] {attacker1});

            battleManager.Defenders.Count.Returns(2);
            battleManager.Defenders.HasInRange(null).ReturnsForAnyArgs(true);
            battleManager.Defenders.AllCombatObjects().Returns(new[] {defender1, defender2});

            List<CombatList.Target> outCarryOverDefender;
            battleManager.Defenders.GetBestTargets(0, null, out outCarryOverDefender, 0, 0)
                         .ReturnsForAnyArgs(args =>
                             {
                                 args[2] = new List<CombatList.Target>
                                 {
                                         new CombatList.Target {CombatObject = defender1, Group = defenderGroup}
                                 };
                                 return CombatList.BestTargetResult.Ok;
                             }, args =>
                                 {
                                     args[2] = new List<CombatList.Target>
                                     {
                                             new CombatList.Target {CombatObject = defender2, Group = defenderGroup}
                                     };
                                     return CombatList.BestTargetResult.Ok;
                                 });

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide outFoundInGroup;
            battleOrder.NextObject(0, null, null, out outCombatObject, out outCombatGroup, out outFoundInGroup)
                       .ReturnsForAnyArgs(args =>
                           {
                               args[3] = attacker1;
                               args[4] = attackerGroup;
                               args[5] = BattleManager.BattleSide.Attack;
                               return true;
                           });

            battleManager.ExecuteTurn();

            defender2.Received(1).CalcActualDmgToBeTaken(Arg.Any<ICombatList>(),
                                                         Arg.Any<ICombatList>(),
                                                         Arg.Any<IBattleRandom>(),
                                                         Arg.Is<decimal>(dmg => dmg > 66.6m && dmg < 66.67m),
                                                         1,
                                                         out outActualDmg);
        }        

        [Theory, AutoNSubstituteData]
        public void Execute_WhenHittingUnitKillsTarget_ShouldCarryOverToNextTargetOnlyOnce(
                [Frozen] IBattleFormulas battleFormulas,
                [Frozen] IBattleOrder battleOrder,
                ICombatObject attacker1,
                ICombatGroup attackerGroup,
                ICombatObject defender1,
                ICombatObject defender2,
                ICombatObject defender3,
                ICombatGroup defenderGroup,
                BattleManager battleManager
                )
        {
            attacker1.Stats.Atk.Returns(10);

            battleFormulas.GetAttackerDmgToDefender(null, null, 0).ReturnsForAnyArgs(100m);
            decimal outActualDmg;
            defender1.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg))
                     .Do(args => { args[5] = 100m; });
            defender1.Hp.Returns(10m);

            defender2.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg))
                     .Do(args => { args[5] = 100m; });
            defender2.Hp.Returns(10m);

            battleManager.Attackers.Count.Returns(1);
            battleManager.Attackers.AllAliveCombatObjects().Returns(new[] {attacker1});

            battleManager.Defenders.Count.Returns(2);
            battleManager.Defenders.HasInRange(null).ReturnsForAnyArgs(true);
            battleManager.Defenders.AllCombatObjects().Returns(new[] {defender1, defender2, defender3});

            List<CombatList.Target> outCarryOverDefender;
            battleManager.Defenders.GetBestTargets(0, null, out outCarryOverDefender, 0, 0)
                         .ReturnsForAnyArgs(
                                            args =>
                                                {
                                                    args[2] = new List<CombatList.Target>
                                                    {
                                                            new CombatList.Target {CombatObject = defender1, Group = defenderGroup}
                                                    };
                                                    return CombatList.BestTargetResult.Ok;
                                                },
                                            args =>
                                                {
                                                    args[2] = new List<CombatList.Target>
                                                    {
                                                            new CombatList.Target {CombatObject = defender2, Group = defenderGroup}
                                                    };
                                                    return CombatList.BestTargetResult.Ok;
                                                },
                                            args =>
                                                {
                                                    args[2] = new List<CombatList.Target>
                                                    {
                                                            new CombatList.Target {CombatObject = defender3, Group = defenderGroup}
                                                    };
                                                    return CombatList.BestTargetResult.Ok;
                                                });

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide outFoundInGroup;
            battleOrder.NextObject(0, null, null, out outCombatObject, out outCombatGroup, out outFoundInGroup)
                       .ReturnsForAnyArgs(args =>
                           {
                               args[3] = attacker1;
                               args[4] = attackerGroup;
                               args[5] = BattleManager.BattleSide.Attack;
                               return true;
                           });

            battleManager.ExecuteTurn();

            defender3.DidNotReceiveWithAnyArgs().CalcActualDmgToBeTaken(null,
                                                                        null,
                                                                        null,
                                                                        0,
                                                                        0,
                                                                        out outActualDmg);
        }    
        
        /// <summary>
        /// Scenario:
        /// Attacker always tries to deal 100 damage but actual is 40 dmg and splashes 2.
        /// Defender1 has 10 HP
        /// Defender2 has 5 HP
        /// Defender3 has 20 HP
        /// Defender4 has 300 HP
        /// Defender1 and Defender2 are hit by splash.
        /// Defender1 dies and damage is carried over to Defender3
        /// Defender2 dies and 35 damage is carried over to Defender4
        /// </summary>
        [Theory, AutoNSubstituteData]
        public void Execute_WhenHittingSplashesAndKillsTargetWithSecondaryHit_ShouldCarryOverToNextTarget(
                [Frozen] IBattleFormulas battleFormulas,
                [Frozen] IBattleOrder battleOrder,
                ICombatObject attacker1,
                ICombatGroup attackerGroup,
                ICombatObject defender1,
                ICombatObject defender2,
                ICombatObject defender3,
                ICombatObject defender4,
                ICombatGroup defenderGroup,
                BattleManager battleManager)
        {
            decimal outActualDmg;            
            
            attacker1.Stats.Atk.Returns(1);
            battleFormulas.GetAttackerDmgToDefender(attacker1, Arg.Any<ICombatObject>(), Arg.Any<uint>()).Returns(100m);

            defender1.Hp.Returns(10);            
            defender1.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg)).Do(args => { args[5] = 40m; });

            defender2.Hp.Returns(5);
            defender2.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg)).Do(args => { args[5] = 40m; });            

            defender3.Hp.Returns(20);
            defender3.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg)).Do(args => { args[5] = 40m; });            

            defender4.Hp.Returns(300);
            defender4.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg)).Do(args => { args[5] = 40m; });            
            
            battleManager.Attackers.Count.Returns(1);
            battleManager.Attackers.AllAliveCombatObjects().Returns(new[] {attacker1});

            battleManager.Defenders.Count.Returns(4);
            battleManager.Defenders.HasInRange(null).ReturnsForAnyArgs(true);
            battleManager.Defenders.AllCombatObjects().Returns(new[] {defender1, defender2, defender3, defender4});

            List<CombatList.Target> outCarryOverDefender;
            battleManager.Defenders.GetBestTargets(0, null, out outCarryOverDefender, 0, 0).ReturnsForAnyArgs(args =>
            {
                args[2] = new List<CombatList.Target>
                {
                    new CombatList.Target {CombatObject = defender1, Group = defenderGroup},
                    new CombatList.Target {CombatObject = defender2, Group = defenderGroup}
                };
                return CombatList.BestTargetResult.Ok;
            }, args =>
            {
                args[2] = new List<CombatList.Target> {new CombatList.Target {CombatObject = defender3, Group = defenderGroup}};
                return CombatList.BestTargetResult.Ok;
            }, args =>
            {
                args[2] = new List<CombatList.Target> {new CombatList.Target {CombatObject = defender4, Group = defenderGroup}};
                return CombatList.BestTargetResult.Ok;
            });

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide outFoundInGroup;
            battleOrder.NextObject(0, null, null, out outCombatObject, out outCombatGroup, out outFoundInGroup).ReturnsForAnyArgs(args =>
            {
                args[3] = attacker1;
                args[4] = attackerGroup;
                args[5] = BattleManager.BattleSide.Attack;
                return true;
            });


            battleManager.ExecuteTurn();

            outActualDmg = 40;
            defender3.Received(1).CalcActualDmgToBeTaken(Arg.Any<ICombatList>(), Arg.Any<ICombatList>(), Arg.Any<IBattleRandom>(), 75m, 1, out outActualDmg);
            defender4.Received(1).CalcActualDmgToBeTaken(Arg.Any<ICombatList>(), Arg.Any<ICombatList>(), Arg.Any<IBattleRandom>(), 87.5m, 3, out outActualDmg);
        }
    }
}