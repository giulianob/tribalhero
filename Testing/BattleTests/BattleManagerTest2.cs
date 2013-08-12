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
            // 16% should be carried over
            battleFormulas.GetAttackerDmgToDefender(attacker1, defender1).Returns(300);
            decimal outActualDmg;
            defender1.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg))
                     .Do(args => { args[5] = 150m; });
            defender1.Hp.Returns(100m);

            battleFormulas.GetAttackerDmgToDefender(attacker1, defender2).Returns(200);

            battleManager.Attackers.Count.Returns(1);
            battleManager.Attackers.AllAliveCombatObjects().Returns(new[] {attacker1});

            battleManager.Defenders.Count.Returns(2);
            battleManager.Defenders.HasInRange(null).ReturnsForAnyArgs(true);
            battleManager.Defenders.AllCombatObjects().Returns(new[] {defender1, defender2});

            List<CombatList.Target> outCarryOverDefender;
            battleManager.Defenders.GetBestTargets(0, null, out outCarryOverDefender, 0)
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
                                                         Arg.Is<decimal>(dmg => dmg > 33 && dmg < 34),
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

            battleFormulas.GetAttackerDmgToDefender(null, null).ReturnsForAnyArgs(100m);
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
            battleManager.Defenders.GetBestTargets(0, null, out outCarryOverDefender, 0)
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
        
        [Theory, AutoNSubstituteData]
        public void Execute_WhenHittingSplashesAndKillsTargetWithSecondaryHit_ShouldCarryOverToNextTarget(
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

            decimal outActualDmg;
            defender1.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg))
                     .Do(args => { args[5] = 10m; });
            defender1.Hp.Returns(100);

            // Defender2 takes 130 dmg but only has 100 hp so 30 dmg should be carried over            
            battleFormulas.GetAttackerDmgToDefender(attacker1, defender2).Returns(1);
            defender2.WhenForAnyArgs(p => p.CalcActualDmgToBeTaken(null, null, null, 0, 0, out outActualDmg))
                     .Do(args => { args[5] = 130m; });
            defender2.Hp.Returns(100m);

            battleManager.Attackers.Count.Returns(1);
            battleManager.Attackers.AllAliveCombatObjects().Returns(new[] {attacker1});

            battleManager.Defenders.Count.Returns(3);
            battleManager.Defenders.HasInRange(null).ReturnsForAnyArgs(true);
            battleManager.Defenders.AllCombatObjects().Returns(new[] {defender1, defender2, defender3});

            List<CombatList.Target> outCarryOverDefender;
            battleManager.Defenders.GetBestTargets(0, null, out outCarryOverDefender, 0)
                         .ReturnsForAnyArgs(args =>
                             {
                                 args[2] = new List<CombatList.Target>
                                 {
                                         new CombatList.Target {CombatObject = defender1, Group = defenderGroup},
                                         new CombatList.Target {CombatObject = defender2, Group = defenderGroup}
                                 };
                                 return CombatList.BestTargetResult.Ok;
                             }, args =>
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

            defender3.ReceivedWithAnyArgs(1).CalcActualDmgToBeTaken(null,
                                                                    null,
                                                                    null,
                                                                    0,
                                                                    0,
                                                                    out outActualDmg);
        }
    }
}