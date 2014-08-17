using System;
using System.Collections.Generic;
using System.Linq;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using NSubstitute;
using Persistance;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class BattleManagerTest
    {
        #region Helper Classes/Methods

        private static ICombatGroup CreateGroup(params ICombatObject[] combatObjects)
        {
            var combatGroup = Substitute.For<ICombatGroup>();
            combatGroup.GetEnumerator().Returns(c => combatObjects.ToList().GetEnumerator());
            return combatGroup;
        }

        private class BattleManagerSut
        {
            public readonly StubCombatList MockAttackers = new StubCombatList();

            public readonly IBattleFormulas MockBattleFormulas = Substitute.For<IBattleFormulas>();

            public readonly IBattleReport MockBattleReport = Substitute.For<IBattleReport>();

            public readonly IBattleOrder MockBattleOrder = Substitute.For<IBattleOrder>();

            public readonly IBattleRandom MockBattleRandom = Substitute.For<IBattleRandom>();

            public readonly ICombatListFactory MockCombatListFactory = Substitute.For<ICombatListFactory>();

            public readonly IDbManager MockDbManager = Substitute.For<IDbManager>();

            public readonly StubCombatList MockDefenders = new StubCombatList();

            public readonly IRewardStrategy MockRewardStrategy = Substitute.For<IRewardStrategy>();

            public BattleManagerSut()
            {
                MockCombatListFactory.GetAttackerCombatList().Returns(MockAttackers);
                MockCombatListFactory.GetDefenderCombatList().Returns(MockDefenders);
            }

            public StubbedAttackTargetBattleManager CreateStubbedAttackBattleManager(uint battleId,
                                                                                     BattleLocation location,
                                                                                     BattleOwner owner,
                                                                                     Action
                                                                                             <ICombatList, ICombatList,
                                                                                             ICombatObject,
                                                                                             CombatList.Target, int>
                                                                                             attackTargetCallback)
            {
                return new StubbedAttackTargetBattleManager(battleId,
                                                            location,
                                                            owner,
                                                            MockRewardStrategy,
                                                            MockDbManager,
                                                            MockBattleReport,
                                                            MockCombatListFactory,
                                                            MockBattleFormulas,
                                                            MockBattleOrder,
                                                            MockBattleRandom,
                                                            attackTargetCallback);
            }
        }

        private class StubCombatList : List<ICombatGroup>, ICombatList
        {
            public StubCombatList()
            {
                MockGetBestTargets = new Queue<GetBestTargetQueueItem>();
                MockInRange = new List<ICombatObject>();
            }

            /// <summary>
            ///     Set this to a list of objects that should be in range
            /// </summary>
            public IEnumerable<ICombatObject> MockInRange { get; set; }

            /// <summary>
            ///     Queue items for return from GetBestTargets call
            /// </summary>
            public Queue<GetBestTargetQueueItem> MockGetBestTargets { get; set; }

            /// <summary>
            ///     Sets to true if Clear() was called
            /// </summary>
            public bool ClearCalled { get; set; }

            /// <summary>
            ///     Set the upkeep manually in tests
            /// </summary>
            public int UpkeepExcludingWaitingToJoinBattle { get; set; }

            public int UpkeepTotal { get; set; }

            public int UpkeepNotParticipatedInRound(uint round)
            {
                return MockInRange.Where(x => x.LastRound <= round).Sum(x => x.Upkeep);
            }

            public bool HasInRange(ICombatObject attacker)
            {
                return MockInRange.Contains(attacker);
            }

            public CombatList.BestTargetResult GetBestTargets(uint battleId, ICombatObject attacker, out List<CombatList.Target> result, int maxCount, uint round)
            {
                var queueItem = MockGetBestTargets.Dequeue();
                result = queueItem.Targets;
                return queueItem.BestTargetResult;
            }

            public IEnumerable<ICombatObject> AllCombatObjects()
            {
                return this.SelectMany(group => group.Select(combatObject => combatObject));
            }

            public IEnumerable<ICombatObject> AllAliveCombatObjects()
            {
                return
                        this.SelectMany(
                                        group =>
                                        group.Where(combatObject => !combatObject.IsDead)
                                             .Select(combatObject => combatObject));
            }

            public void Add(ICombatGroup item, bool save)
            {
                Add(item);
            }

            public bool Remove(ICombatGroup item, bool save)
            {
                return Remove(item);
            }

            public new void Clear()
            {
                ClearCalled = true;
            }

            public class GetBestTargetQueueItem
            {
                public CombatList.BestTargetResult BestTargetResult { get; set; }

                public List<CombatList.Target> Targets { get; set; }
            }
        }

        private class StubbedAttackTargetBattleManager : BattleManager
        {
            private readonly Action<ICombatList, ICombatList, ICombatObject, CombatList.Target, int>
                    attackTargetCallback;

            public StubbedAttackTargetBattleManager(uint battleId,
                                                    BattleLocation location,
                                                    BattleOwner owner,
                                                    IRewardStrategy rewardStrategy,
                                                    IDbManager dbManager,
                                                    IBattleReport battleReport,
                                                    ICombatListFactory combatListFactory,
                                                    IBattleFormulas battleFormulas,
                                                    IBattleOrder battleOrder,
                                                    IBattleRandom battleRandom,
                                                    Action<ICombatList, ICombatList, ICombatObject, CombatList.Target, int> attackTargetCallback)
                    : base(
                            battleId,
                            location,
                            owner,
                            rewardStrategy,
                            dbManager,
                            battleReport,
                            combatListFactory,
                            battleFormulas,
                            battleOrder,
                            battleRandom)
            {
                this.attackTargetCallback = attackTargetCallback;
            }

            protected override void AttackTarget(ICombatList offensiveCombatList,
                                                 ICombatList defensiveCombatList,
                                                 ICombatGroup attackerGroup,
                                                 ICombatObject attacker,
                                                 BattleSide sideAttacking,
                                                 CombatList.Target target,
                                                 int attackIndex,
                                                 uint round,
                                                 out decimal carryOverDmg)
            {
                attackTargetCallback(offensiveCombatList, defensiveCombatList, attacker, target, attackIndex);
                carryOverDmg = 0;
            }
        }

        #endregion

        /// <summary>
        ///     When battle has not yet begun
        ///     And ExecuteTurn is called with an invalid battle because there are no attackers
        ///     Then no battle report should be created and battle ended should be fired
        /// </summary>
        [Fact]
        public void TestExecuteInvalidBattleNoAttackersWhenBattleJustStarted()
        {
            var attackResults = new Queue<bool>();
            attackResults.Enqueue(false);
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            var defenderGroup = CreateGroup(defender);

            sut.MockDefenders.Add(defenderGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            defender.Received(1).ExitBattle();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
            sut.MockBattleReport.Received().CompleteReport(ReportState.Staying);
        }

        /// <summary>
        ///     When battle has not yet begun
        ///     And ExecuteTurn is called with an invalid battle because there are no defenders
        ///     Then no battle report should be created,
        ///     ExitBattle should be fired,
        ///     CombatObject.Exit should be called on all objects in battle
        ///     Attacker/Defender combat list should be cleared
        /// </summary>
        [Fact]
        public void TestExecuteInvalidBattleNoDefendersWhenBattleJustStarted()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var attacker = Substitute.For<ICombatObject>();
            var attackerGroup = CreateGroup(attacker);

            sut.MockAttackers.Add(attackerGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            attacker.Received().ExitBattle();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
            sut.MockBattleReport.Received(1).CompleteReport(ReportState.Staying);
            sut.MockAttackers.ClearCalled.Should().BeTrue();
            sut.MockDefenders.ClearCalled.Should().BeTrue();
        }

        /// <summary>
        ///     When battle has already begun
        ///     And ExecuteTurn is called with an invalid battle because there are no attackers
        ///     Then no battle report should be created and battle ended should be fired
        /// </summary>
        [Fact]
        public void TestExecuteInvalidBattleNoAttackersWhenBattleAlreadyStarted()
        {
            var attackResults = new Queue<bool>();
            attackResults.Enqueue(false);
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            var defenderGroup = CreateGroup(defender);

            sut.MockDefenders.Add(defenderGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };
            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            defender.Received(1).ExitBattle();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
            sut.MockBattleReport.Received(1).CompleteReport(ReportState.Staying);
        }

        /// <summary>
        ///     When battle has already begun
        ///     And ExecuteTurn is called with an invalid battle because there is no one in the battle
        ///     Then no battle report should be created and battle ended should be fired
        /// </summary>
        [Fact]
        public void TestExecuteInvalidBattleNoAttackersWhenCompletelyInvalid()
        {
            var attackResults = new Queue<bool>();
            attackResults.Enqueue(false);
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };
            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
            sut.MockBattleReport.Received().CompleteReport(ReportState.Staying);
        }

        /// <summary>
        ///     When battle has already begun
        ///     And ExecuteTurn is called with an invalid battle because there are no defenders
        ///     Then no battle report should be created,
        ///     ExitBattle should be fired,
        ///     CombatObject.Exit should be called on all objects in battle
        ///     Attacker/Defender combat list should be cleared
        /// </summary>
        [Fact]
        public void TestExecuteInvalidBattleNoDefendersWhenBattleHasStarted()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var attacker = Substitute.For<ICombatObject>();
            var attackerGroup = CreateGroup(attacker);

            sut.MockAttackers.Add(attackerGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };
            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            attacker.Received().ExitBattle();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
            sut.MockBattleReport.Received(1).CompleteReport(ReportState.Staying);
            sut.MockAttackers.ClearCalled.Should().BeTrue();
            sut.MockDefenders.ClearCalled.Should().BeTrue();
        }

        /// <summary>
        ///     When battle has not yet begun
        ///     And ExecuteTurn is called with an invalid battle because no one is in range
        ///     Then no battle report should be created and battle ended should be fired
        /// </summary>
        [Fact]
        public void TestExecuteInvalidBattleNoneInRangeWhenBattleJustStarted()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            var attacker = Substitute.For<ICombatObject>();
            var defenderGroup = CreateGroup(defender);
            var attackerGroup = CreateGroup(attacker);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
            sut.MockBattleReport.Received().CompleteReport(ReportState.Staying);
            sut.MockAttackers.ClearCalled.Should().BeTrue();
            sut.MockDefenders.ClearCalled.Should().BeTrue();
        }

        /// <summary>
        ///     When battle has not yet begun
        ///     And ExecuteTurn is called with a valid battle
        ///     Then a battle report should be created and enterbattle event fired
        ///     NOTE: This scenario will exit out before attacking since GetBestTargets wont return anything
        /// </summary>
        [Fact]
        public void TestExecuteWhenBattleJustStarted()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            var attacker = Substitute.For<ICombatObject>();
            var defenderGroup = CreateGroup(defender);
            var attackerGroup = CreateGroup(attacker);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender};

            BattleManager.BattleSide attackerBattleSide;
            ICombatGroup outAttackerGroup;
            ICombatObject outAttackerObject;
            sut.MockBattleOrder.NextObject(Arg.Any<uint>(),
                                           Arg.Any<ICombatList>(),
                                           Arg.Any<ICombatList>(),
                                           out outAttackerObject,
                                           out outAttackerGroup,
                                           out attackerBattleSide).Returns(args =>
                                           {
                                               args[3] = null;
                                               args[4] = null;
                                               args[5] = BattleManager.BattleSide.Attack;
                                               return true;
                                           });

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };

            var enterRoundRaised = false;
            battle.EnterRound += delegate { enterRoundRaised = true; };

            battle.ExecuteTurn();
            enterBattleRaised.Should().BeTrue();
            enterRoundRaised.Should().BeFalse();
            sut.MockBattleReport.Received(1).CreateBattleReport();
        }

        /// <summary>
        ///     When all units are in the next round
        ///     And ExecuteTurn is called
        ///     Then the round should be incremented
        ///     and EnterRound should be fired
        ///     NOTE: This scenario will exit out before attacking since GetBestTargets wont return anything
        /// </summary>
        [Fact]
        public void TestExecuteWhenFinishedRoundShouldEnterNewRound()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            var attacker = Substitute.For<ICombatObject>();
            var defenderGroup = CreateGroup(defender);
            var attackerGroup = CreateGroup(attacker);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender};

            // ReSharper disable RedundantAssignment
            var attackerBattleSide = BattleManager.BattleSide.Attack;
            ICombatGroup outAttackerGroup = null;
            ICombatObject outAttackerObject = null;
            // ReSharper restore RedundantAssignment
            sut.MockBattleOrder.NextObject(Arg.Any<uint>(),
                                           Arg.Any<ICombatList>(),
                                           Arg.Any<ICombatList>(),
                                           out outAttackerObject,
                                           out outAttackerGroup,
                                           out attackerBattleSide).Returns(false);

            var enterRoundRaised = false;
            battle.EnterRound += (manager, attackers, defenders, round) =>
            {
                round.Should().Be(1);
                enterRoundRaised = true;
            };

            battle.BattleStarted = true;
            battle.Turn = 100;
            battle.ExecuteTurn();
            battle.Round.Should().Be(1);
            battle.Turn.Should().Be(0);
            enterRoundRaised.Should().BeTrue();
            sut.MockBattleReport.DidNotReceive().CreateBattleReport();
        }

        /// <summary>
        ///     When the next unit to fight has 0 attack
        ///     And ExecuteTurn is called
        ///     Then he should be skipped
        /// </summary>
        [Fact]
        public void TestExecuteWhenAtkIs0ShouldSkip()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            defender.Stats.Atk.Returns(0m);
            var attacker = Substitute.For<ICombatObject>();

            var defenderGroup = CreateGroup(defender);
            var attackerGroup = CreateGroup(attacker);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender};

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                BestTargetResult = CombatList.BestTargetResult.Ok,
                Targets =
                    new List<CombatList.Target>
                    {
                        new CombatList.Target {CombatObject = attacker, Group = attackerGroup}
                    }
            });
            ICombatObject outAttackerObject;
            ICombatGroup outDefenderGroup;
            BattleManager.BattleSide outAttackerBattleSide;
            sut.MockBattleOrder.NextObject(Arg.Any<uint>(),
                                           Arg.Any<ICombatList>(),
                                           Arg.Any<ICombatList>(),
                                           out outAttackerObject,
                                           out outDefenderGroup,
                                           out outAttackerBattleSide)
               .Returns(args =>
               {
                   args[3] = defender;
                   args[4] = defenderGroup;
                   args[5] = BattleManager.BattleSide.Defense;
                   return true;
               });

            var skippedAttackerCalled = false;
            battle.SkippedAttacker += (manager, side, group, o) =>
            {
                side.Should().Be(BattleManager.BattleSide.Defense);
                o.Should().Be(defender);
                skippedAttackerCalled = true;
            };

            battle.BattleStarted = true;
            battle.ExecuteTurn().Should().BeTrue();
            skippedAttackerCalled.Should().BeTrue();
            sut.MockDbManager.Received().Save(defender);
            defender.Received().ParticipatedInRound(0);
        }

        /// <summary>
        ///     When an object has no on in range
        ///     And ExecuteTurn is called
        ///     Then he should be skipped
        ///     And another attacker should be used immediatelly
        /// </summary>
        [Fact]
        public void TestExecuteWhenSkippedDueToNoneInRangeShouldFindAnotherTargetImmediately()
        {
            var sut = new BattleManagerSut();
            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var defender = Substitute.For<ICombatObject>();
            defender.Stats.Atk.Returns(0m);
            var attacker = Substitute.For<ICombatObject>();

            var defenderGroup = CreateGroup(defender);
            var attackerGroup = CreateGroup(attacker);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender};

            // Have it skip both objects but w/ different code path
            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                BestTargetResult = CombatList.BestTargetResult.NoneInRange,
                Targets =
                    new List<CombatList.Target>
                    {
                        new CombatList.Target {CombatObject = attacker, Group = attackerGroup}
                    }
            });

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                BestTargetResult = CombatList.BestTargetResult.Ok,
                Targets = new List<CombatList.Target>()
            });

            // ReSharper disable RedundantAssignment
            BattleManager.BattleSide firstOrderAttackSide;
            ICombatObject firstOrderAttackObject;
            ICombatGroup firstOrderAttackGroup;
            // ReSharper restore RedundantAssignment
            sut.MockBattleOrder
               .NextObject(Arg.Any<uint>(),
                           Arg.Any<ICombatList>(),
                           Arg.Any<ICombatList>(),
                           out firstOrderAttackObject,
                           out firstOrderAttackGroup,
                           out firstOrderAttackSide)
               .Returns(args =>
               {
                   args[3] = defender;
                   args[4] = defenderGroup;
                   args[5] = BattleManager.BattleSide.Defense;
                   return true;
               });

            int skippedCallCount = 0;
            battle.SkippedAttacker += (manager, side, group, o) =>
            {
                skippedCallCount++;

                side.Should().Be(BattleManager.BattleSide.Defense);
                o.Should().Be(defender);
            };

            battle.BattleStarted = true;
            battle.ExecuteTurn().Should().BeTrue();
            skippedCallCount.Should().Be(2);
        }

        /// <summary>
        ///     When an attacker has no one in range
        ///     And ExecuteTurn is called
        ///     Then he should be removed from the battle
        /// </summary>
        [Fact]
        public void TestExecuteWhenAttackerHasNoneInRangeIsRemoved()
        {
            var sut = new BattleManagerSut();

            var attacker = Substitute.For<ICombatObject>();
            attacker.Stats.Atk.Returns(10m);

            var attackerWithoutInRange = Substitute.For<ICombatObject>();
            var defender1 = Substitute.For<ICombatObject>();
            var defender2 = Substitute.For<ICombatObject>();

            var attacker1Group = CreateGroup(attacker);
            var attackerWithoutInRangeGroup = CreateGroup(attackerWithoutInRange);
            var defenderGroup = CreateGroup(defender1, defender2);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attacker1Group);
            sut.MockAttackers.Add(attackerWithoutInRangeGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker};

            // ReSharper disable RedundantAssignment
            BattleManager.BattleSide outAttackerBattleSide;
            ICombatObject outAttackerObject;
            ICombatGroup outCombatGroup;
            // ReSharper restore RedundantAssignment
            sut.MockBattleOrder.NextObject(Arg.Any<uint>(),
                                           Arg.Any<ICombatList>(),
                                           Arg.Any<ICombatList>(),
                                           out outAttackerObject,
                                           out outCombatGroup,
                                           out outAttackerBattleSide)
               .Returns(args =>
               {
                   args[3] = attacker;
                   args[4] = attacker1Group;
                   args[5] = BattleManager.BattleSide.Attack;
                   return true;
               });

            sut.MockDefenders.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                BestTargetResult = CombatList.BestTargetResult.Ok,
                Targets =
                    new List<CombatList.Target>
                    {
                        new CombatList.Target {CombatObject = defender1, Group = attacker1Group},
                        new CombatList.Target {CombatObject = defender2, Group = attacker1Group}
                    }
            });

            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              delegate { });

            var withdrawCalled = false;
            battle.WithdrawAttacker += (manager, @group) =>
            {
                withdrawCalled = true;
                attackerWithoutInRangeGroup.Should().Equal(@group);
            };

            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeTrue();

            withdrawCalled.Should().BeTrue();
            attacker.Received().ParticipatedInRound(0);
            attackerWithoutInRange.Received().ExitBattle();
            attacker.DidNotReceive().ExitBattle();
        }

        /// <summary>
        ///     When an object has targets        
        ///     And ExecuteTurn is called
        ///     Then he should attack all the targets
        ///     And ExitTurn should be called
        ///     And ParticipatedInRound should be called on the attacker
        /// </summary>
        [Fact]
        public void TestExecuteWhenHasTargetsAndOffensiveSideAttackingButDoesntKillShouldAttack()
        {
            var sut = new BattleManagerSut();

            var attacker = Substitute.For<ICombatObject>();
            attacker.Stats.Atk.Returns(10m);
            attacker.When(x => x.ParticipatedInRound(0)).Do(args => { attacker.LastRound++; });
            var defender1 = Substitute.For<ICombatObject>();
            var defender2 = Substitute.For<ICombatObject>();

            var attackerGroup = CreateGroup(attacker);
            var defenderGroup = CreateGroup(defender1, defender2);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker};

            sut.MockDefenders.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                BestTargetResult = CombatList.BestTargetResult.Ok,
                Targets =
                    new List<CombatList.Target>
                    {
                        new CombatList.Target {CombatObject = defender1, Group = attackerGroup},
                        new CombatList.Target {CombatObject = defender2, Group = attackerGroup}
                    }
            });

            // ReSharper disable RedundantAssignment
            BattleManager.BattleSide outAttackerBattleSide;
            ICombatObject outAttackerObject;
            ICombatGroup outAttackerGroup;
            // ReSharper restore RedundantAssignment            
            sut.MockBattleOrder.NextObject(Arg.Any<uint>(),
                                           Arg.Any<ICombatList>(),
                                           Arg.Any<ICombatList>(),
                                           out outAttackerObject,
                                           out outAttackerGroup,
                                           out outAttackerBattleSide)
               .Returns(args =>
               {
                   args[3] = attacker;
                   args[4] = attackerGroup;
                   args[5] = BattleManager.BattleSide.Attack;
                   return true;
               });

            var attackTargetCallCount = 0;

            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              (offensiveCombatList,
                                                               defensiveCombatList,
                                                               attackerObject,
                                                               defenderObject,
                                                               attackIndex) =>
                                                              {
                                                                  attackTargetCallCount++;

                                                                  switch(attackIndex)
                                                                  {
                                                                      case 0:
                                                                          defenderObject.CombatObject.Should().Be(defender1);
                                                                          break;
                                                                      case 1:
                                                                          defenderObject.CombatObject.Should().Be(defender2);
                                                                          break;
                                                                      default:
                                                                          throw new Exception(
                                                                                  "Unexpected attackIndex");
                                                                  }
                                                              });

            var exitTurnCalled = false;
            battle.ExitTurn += (manager, attackers, defenders, turn) =>
            {
                turn.Should().Be(0);
                exitTurnCalled = true;
            };

            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeTrue();

            battle.Turn.Should().Be(1);
            attackTargetCallCount.Should().Be(2);
            exitTurnCalled.Should().BeTrue();
            attacker.Received().ParticipatedInRound(0);
            sut.MockDbManager.Received().Save(attacker);
        }

        /// <summary>
        ///     When defense should be next one to attack
        ///     And ExecuteTurn is called
        ///     Then he should attack all the targets
        ///     And ExitTurn should be called
        ///     And ParticipatedInRound should be called on the attacker
        /// </summary>
        [Fact]
        public void TestExecuteWhenHasTargetsButDoesntKillShouldAttack()
        {
            var sut = new BattleManagerSut();

            var defender = Substitute.For<ICombatObject>();
            defender.Stats.Atk.Returns(10m);
            defender.When(m => m.ParticipatedInRound(0)).Do(args => { defender.LastRound++; });
            var attacker1 = Substitute.For<ICombatObject>();
            var attacker2 = Substitute.For<ICombatObject>();

            var defenderGroup = CreateGroup(defender);
            var attackerGroup = CreateGroup(attacker1, attacker2);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker1, attacker2};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender};

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                BestTargetResult = CombatList.BestTargetResult.Ok,
                Targets =
                    new List<CombatList.Target>
                    {
                        new CombatList.Target {CombatObject = attacker1, Group = attackerGroup},
                        new CombatList.Target {CombatObject = attacker2, Group = attackerGroup}
                    }
            });

            BattleManager.BattleSide outDefenderBattleSide;
            ICombatObject outDefenderOutObject;
            ICombatGroup outDefenderGroup;
            sut.MockBattleOrder.NextObject(Arg.Any<uint>(),
                                           Arg.Any<ICombatList>(),
                                           Arg.Any<ICombatList>(),
                                           out outDefenderOutObject,
                                           out outDefenderGroup,
                                           out outDefenderBattleSide)
               .Returns(args =>
               {
                   args[3] = defender;
                   args[4] = defenderGroup;
                   args[5] = BattleManager.BattleSide.Defense;
                   return true;
               });

            var attackTargetCallCount = 0;

            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              (offensiveCombatList,
                                                               defensiveCombatList,
                                                               attackerObject,
                                                               defenderObject,
                                                               attackIndex) =>
                                                              {
                                                                  attackTargetCallCount++;

                                                                  switch(attackIndex)
                                                                  {
                                                                      case 0:
                                                                          defenderObject.CombatObject.Should().Be(attacker1);
                                                                          break;
                                                                      case 1:
                                                                          defenderObject.CombatObject.Should().Be(attacker2);
                                                                          break;
                                                                      default:
                                                                          throw new Exception(
                                                                                  "Unexpected attackIndex");
                                                                  }
                                                              });

            var exitTurnCalled = false;
            battle.ExitTurn += (manager, attackers, defenders, turn) =>
            {
                turn.Should().Be(0);
                exitTurnCalled = true;
            };

            battle.BattleStarted = true;
            battle.ExecuteTurn().Should().BeTrue();

            battle.Turn.Should().Be(1);
            attackTargetCallCount.Should().Be(2);
            exitTurnCalled.Should().BeTrue();
            defender.Received().ParticipatedInRound(0);
            sut.MockDbManager.Received().Save(defender);
        }

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

        [Theory, AutoNSubstituteData]
        public void Execute_WhenNewRound_ShouldCallJoinBattleOnAllNewTroops(
                [Frozen] IDbManager dbManager,
                [Frozen] IBattleOrder battleOrder,
                [Frozen] ICombatListFactory combatListFactory,
                ICombatObject attacker1,
                ICombatObject attacker2,
                ICombatGroup attackerGroup,
                ICombatObject defender1,
                ICombatObject defender2,
                ICombatGroup defenderGroup,
                BattleManager battleManager)
        {
            defender1.IsWaitingToJoinBattle.Returns(false);
            defender2.IsWaitingToJoinBattle.Returns(true);
            attacker1.IsWaitingToJoinBattle.Returns(false);
            attacker2.IsWaitingToJoinBattle.Returns(true);
            attacker2.Id.Returns((uint)1337);

            battleManager.Attackers.Count.Returns(1);
            battleManager.Attackers.AllAliveCombatObjects().Returns(new[] {attacker1});
            battleManager.Attackers.AllCombatObjects().Returns(c => new[] {attacker1, attacker2});

            battleManager.Defenders.Count.Returns(1);
            battleManager.Defenders.HasInRange(null).ReturnsForAnyArgs(true);
            battleManager.Defenders.AllCombatObjects().Returns(c => new[] {defender1, defender2});

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide outFoundInGroup;
            battleOrder.NextObject(0, null, null, out outCombatObject, out outCombatGroup, out outFoundInGroup).ReturnsForAnyArgs(args =>
            {
                args[3] = null;
                args[4] = attackerGroup;
                args[5] = BattleManager.BattleSide.Attack;
                return false;
            });
            
            battleManager.ExecuteTurn();

            attacker1.DidNotReceive().JoinBattle(1);
            defender1.DidNotReceive().JoinBattle(1);
            defender2.Received().JoinBattle(1);
            attacker2.Received().JoinBattle(1);

            dbManager.Received().Save(defender2);
            dbManager.Received().Save(attacker2);
        }
    }
}