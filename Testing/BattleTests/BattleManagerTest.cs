using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Battle.RewardStrategies;
using Moq;
using Persistance;
using Xunit;

namespace Testing.BattleTests
{
    public class BattleManagerTest
    {
        #region Helper Classes/Methods

        private static ICombatGroup CreateGroup(params ICombatObject[] combatObjects)
        {
            Mock<ICombatGroup> combatGroup = new Mock<ICombatGroup>();
            combatGroup.SetupAllProperties();
            combatGroup.Setup(m => m.GetEnumerator()).Returns(() => combatObjects.ToList().GetEnumerator());
            return combatGroup.Object;
        }

        private class BattleManagerSut
        {
            public readonly StubCombatList MockAttackers = new StubCombatList();

            public readonly Mock<BattleFormulas> MockBattleFormulas = new Mock<BattleFormulas>();

            public readonly Mock<IBattleReport> MockBattleReport = new Mock<IBattleReport>();

            public readonly Mock<ICombatListFactory> MockCombatListFactory = new Mock<ICombatListFactory>();

            public readonly Mock<IDbManager> MockDbManager = new Mock<IDbManager>();

            public readonly StubCombatList MockDefenders = new StubCombatList();

            public readonly Mock<IRewardStrategy> MockRewardStrategy = new Mock<IRewardStrategy>();

            public BattleManagerSut()
            {
                var combatLists = new Queue<ICombatList>();
                combatLists.Enqueue(MockAttackers);
                combatLists.Enqueue(MockDefenders);

                MockCombatListFactory.Setup(p => p.GetCombatList()).Returns(combatLists.Dequeue);
            }

            public StubbedAttackTargetBattleManager CreateStubbedAttackBattleManager(uint battleId,
                                                                                     BattleLocation location,
                                                                                     BattleOwner owner,
                                                                                     IBattleOrder battleOrder,
                                                                                     Action
                                                                                             <ICombatList, ICombatList,
                                                                                             ICombatObject,
                                                                                             CombatList.Target, int>
                                                                                             attackTargetCallback)
            {
                return new StubbedAttackTargetBattleManager(battleId,
                                                            location,
                                                            owner,
                                                            MockRewardStrategy.Object,
                                                            MockDbManager.Object,
                                                            MockBattleReport.Object,
                                                            MockCombatListFactory.Object,
                                                            MockBattleFormulas.Object,
                                                            battleOrder,
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
            public int Upkeep { get; private set; }

            public int UpkeepNotParticipated(uint round)
            {
                return MockInRange.Where(x => x.LastRound <= round).Sum(x => x.Upkeep);
            }

            public bool HasInRange(ICombatObject attacker)
            {
                return MockInRange.Contains(attacker);
            }

            public CombatList.BestTargetResult GetBestTargets(uint battleId, ICombatObject attacker, out List<CombatList.Target> result, int maxCount)
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
                                                    BattleFormulas battleFormulas,
                                                    IBattleOrder battleOrder,
                                                    Action
                                                            <ICombatList, ICombatList, ICombatObject, CombatList.Target,
                                                            int> attackTargetCallback)
                    : base(
                            battleId,
                            location,
                            owner,
                            rewardStrategy,
                            dbManager,
                            battleReport,
                            combatListFactory,
                            battleFormulas,
                            battleOrder)
            {
                this.attackTargetCallback = attackTargetCallback;
            }

            protected override void AttackTarget(ICombatList offensiveCombatList,
                                                 ICombatList defensiveCombatList,
                                                 ICombatGroup attackerGroup,
                                                 ICombatObject attacker,
                                                 BattleSide sideAttacking,
                                                 CombatList.Target target,
                                                 int attackIndex)
            {
                attackTargetCallback(offensiveCombatList, defensiveCombatList, attacker, target, attackIndex);
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            var defenderGroup = CreateGroup(defender.Object);

            sut.MockDefenders.Add(defenderGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            defender.Verify(m => m.ExitBattle(), Times.Once());
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
            sut.MockBattleReport.Verify(m => m.CompleteReport(ReportState.Staying), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var attacker = new Mock<ICombatObject>();
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockAttackers.Add(attackerGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            attacker.Verify(m => m.ExitBattle(), Times.Once());
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
            sut.MockBattleReport.Verify(m => m.CompleteReport(ReportState.Staying), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            var defenderGroup = CreateGroup(defender.Object);

            sut.MockDefenders.Add(defenderGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };
            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            defender.Verify(m => m.ExitBattle(), Times.Once());
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
            sut.MockBattleReport.Verify(m => m.CompleteReport(ReportState.Staying), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };
            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
            sut.MockBattleReport.Verify(m => m.CompleteReport(ReportState.Staying), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var attacker = new Mock<ICombatObject>();
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockAttackers.Add(attackerGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };
            battle.BattleStarted = true;

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            attacker.Verify(m => m.ExitBattle(), Times.Once());
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
            sut.MockBattleReport.Verify(m => m.CompleteReport(ReportState.Staying), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            var attacker = new Mock<ICombatObject>();
            var defenderGroup = CreateGroup(defender.Object);
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };
            var exitBattleRaised = false;
            battle.ExitBattle += delegate { exitBattleRaised = true; };

            battle.ExecuteTurn().Should().BeFalse();

            enterBattleRaised.Should().BeFalse();
            exitBattleRaised.Should().BeTrue();
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
            sut.MockBattleReport.Verify(m => m.CompleteReport(ReportState.Staying), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            var attacker = new Mock<ICombatObject>();
            var defenderGroup = CreateGroup(defender.Object);
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker.Object};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender.Object};

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets = new List<CombatList.Target>()
            });

            var enterBattleRaised = false;
            battle.EnterBattle += delegate { enterBattleRaised = true; };

            var enterRoundRaised = false;
            battle.EnterRound += delegate { enterRoundRaised = true; };

            battle.ExecuteTurn().Should().BeTrue();
            enterBattleRaised.Should().BeTrue();
            enterRoundRaised.Should().BeFalse();
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            defender.SetupProperty(p => p.LastRound, (uint)1);
            var attacker = new Mock<ICombatObject>();
            attacker.SetupProperty(p => p.LastRound, (uint)1);
            var defenderGroup = CreateGroup(defender.Object);
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker.Object};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender.Object};

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets = new List<CombatList.Target>()
            });

            var enterRoundRaised = false;
            battle.EnterRound += (manager, attackers, defenders, round) =>
                {
                    round.Should().Be(1);
                    enterRoundRaised = true;
                };

            battle.BattleStarted = true;
            battle.Turn = 100;
            battle.ExecuteTurn().Should().BeTrue();
            battle.Round.Should().Be(1);
            battle.Turn.Should().Be(0);
            enterRoundRaised.Should().BeTrue();
            sut.MockBattleReport.Verify(m => m.CreateBattleReport(), Times.Never());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            defender.SetupProperty(p => p.Stats.Atk, 0m);
            var attacker = new Mock<ICombatObject>();

            var defenderGroup = CreateGroup(defender.Object);
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker.Object};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender.Object};

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets =
                            new List<CombatList.Target>
                            {
                                    new CombatList.Target {CombatObject = attacker.Object, Group = attackerGroup}
                            }
            });

            var skippedAttackerCalled = false;
            battle.SkippedAttacker += (manager, side, group, o) =>
                {
                    side.Should().Be(BattleManager.BattleSide.Defense);
                    o.Should().Be(defender.Object);
                    skippedAttackerCalled = true;
                };

            battle.BattleStarted = true;
            battle.ExecuteTurn().Should().BeTrue();
            skippedAttackerCalled.Should().BeTrue();
            sut.MockDbManager.Verify(m => m.Save(defender.Object), Times.Once());
            defender.Verify(m => m.ParticipatedInRound(0), Times.Once());
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
                                                              new BattleOrder(new Random()),
                                                              delegate { });

            var defender = new Mock<ICombatObject>();
            defender.SetupProperty(p => p.Stats.Atk, 0m);
            defender.SetupProperty(p => p.LastRound);
            defender.Setup(m => m.ParticipatedInRound(0)).Callback(() => { defender.Object.LastRound++; });
            var attacker = new Mock<ICombatObject>();

            var defenderGroup = CreateGroup(defender.Object);
            var attackerGroup = CreateGroup(attacker.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker.Object};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender.Object};

            // Have it skip both objects but w/ different code path
            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.NoneInRange,
                    Targets =
                            new List<CombatList.Target>
                            {
                                    new CombatList.Target {CombatObject = attacker.Object, Group = attackerGroup}
                            }
            });

            sut.MockDefenders.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets = new List<CombatList.Target>()
            });

            int skippedCallCount = 0;
            battle.SkippedAttacker += (manager, side, group, o) =>
                {
                    skippedCallCount++;

                    if (skippedCallCount == 1)
                    {
                        side.Should().Be(BattleManager.BattleSide.Defense);
                        o.Should().Be(defender.Object);
                    }
                    else if (skippedCallCount == 2)
                    {
                        side.Should().Be(BattleManager.BattleSide.Attack);
                        o.Should().Be(attacker.Object);
                    }
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

            var attacker = new Mock<ICombatObject>();
            attacker.SetupProperty(p => p.Stats.Atk, 10m);

            var attackerWithoutInRange = new Mock<ICombatObject>();
            var defender1 = new Mock<ICombatObject>();
            var defender2 = new Mock<ICombatObject>();

            var attacker1Group = CreateGroup(attacker.Object);
            var attackerWithoutInRangeGroup = CreateGroup(attackerWithoutInRange.Object);
            var defenderGroup = CreateGroup(defender1.Object, defender2.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attacker1Group);
            sut.MockAttackers.Add(attackerWithoutInRangeGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker.Object};

            // ReSharper disable RedundantAssignment
            var attackerBattleSide = BattleManager.BattleSide.Attack;
            var attackerObject = attacker.Object;
            // ReSharper restore RedundantAssignment
            var battleOrder = new Mock<IBattleOrder>();
            battleOrder.Setup(
                                       m =>
                                       m.NextObject(It.IsAny<uint>(),
                                                    It.IsAny<ICombatList>(),
                                                    It.IsAny<ICombatList>(),
                                                    out attackerObject,
                                                    out attacker1Group,
                                                    out attackerBattleSide)).Returns(true);

            sut.MockDefenders.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets =
                            new List<CombatList.Target>
                            {
                                    new CombatList.Target {CombatObject = defender1.Object, Group = attacker1Group},
                                    new CombatList.Target {CombatObject = defender2.Object, Group = attacker1Group}
                            }
            });

            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              battleOrder.Object,
                                                              delegate { });

            var withdrawCalled = false;
            battle.WithdrawAttacker += (manager, @group) =>
                {
                    withdrawCalled = true;
                    Assert.Same(@group, attackerWithoutInRangeGroup);
                };

            battle.BattleStarted = true;
            
          //  //battle.NextToAttack = BattleManager.BattleSide.Attack;
            battle.ExecuteTurn().Should().BeTrue();

            withdrawCalled.Should().BeTrue();
            attacker.Verify(m => m.ParticipatedInRound(0), Times.Once());
            attackerWithoutInRange.Verify(m => m.ExitBattle(), Times.Once());
            attacker.Verify(m => m.ExitBattle(), Times.Never());
        }

        /// <summary>
        ///     When an object has targets
        ///     And NextToAttack is Defense
        ///     And ExecuteTurn is called
        ///     Then he should attack all the targets
        ///     And ExitTurn should be called
        ///     And ParticipatedInRound should be called on the attacker
        /// </summary>
        [Fact]
        public void TestExecuteWhenHasTargetsAndOffensiveSideAttackingButDoesntKillShouldAttack()
        {
            var sut = new BattleManagerSut();

            var attacker = new Mock<ICombatObject>();
            attacker.SetupProperty(p => p.Stats.Atk, 10m);
            attacker.SetupProperty(p => p.LastRound);
            attacker.Setup(m => m.ParticipatedInRound(0)).Callback(() => { attacker.Object.LastRound++; });
            var defender1 = new Mock<ICombatObject>();
            var defender2 = new Mock<ICombatObject>();

            var attackerGroup = CreateGroup(attacker.Object);
            var defenderGroup = CreateGroup(defender1.Object, defender2.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker.Object};

            sut.MockDefenders.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets =
                            new List<CombatList.Target>
                            {
                                    new CombatList.Target {CombatObject = defender1.Object, Group = attackerGroup},
                                    new CombatList.Target {CombatObject = defender2.Object, Group = attackerGroup}
                            }
            });

            // ReSharper disable RedundantAssignment
            var attackerBattleSide = BattleManager.BattleSide.Defense;
            var attackerOutObject = attacker.Object;
            // ReSharper restore RedundantAssignment
            var battleOrder = new Mock<IBattleOrder>();
            battleOrder.Setup(
                              m =>
                              m.NextObject(It.IsAny<uint>(),
                                           It.IsAny<ICombatList>(),
                                           It.IsAny<ICombatList>(),
                                           out attackerOutObject,
                                           out attackerGroup,
                                           out attackerBattleSide)).Returns(true);

            var attackTargetCallCount = 0;

            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              new BattleOrder(new Random()),
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
                                                                              attackerObject.Should()
                                                                                            .Be(defender1.Object);
                                                                              break;
                                                                          case 1:
                                                                              attackerObject.Should()
                                                                                            .Be(defender2.Object);
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
           // battle.NextToAttack = BattleManager.BattleSide.Attack;
            battle.ExecuteTurn().Should().BeTrue();

            battle.Turn.Should().Be(1);
            attackTargetCallCount.Should().Be(2);
            exitTurnCalled.Should().BeTrue();
            attacker.Verify(m => m.ParticipatedInRound(0), Times.Once());
            sut.MockDbManager.Verify(m => m.Save(attacker.Object), Times.Once());
        }

        /// <summary>
        ///     When an object has targets
        ///     And NextToAttack is Defense
        ///     And ExecuteTurn is called
        ///     Then he should attack all the targets
        ///     And ExitTurn should be called
        ///     And ParticipatedInRound should be called on the attacker
        /// </summary>
        [Fact]
        public void TestExecuteWhenHasTargetsButDoesntKillShouldAttack()
        {
            var sut = new BattleManagerSut();

            var defender = new Mock<ICombatObject>();
            defender.SetupProperty(p => p.Stats.Atk, 10m);
            defender.SetupProperty(p => p.LastRound);
            defender.Setup(m => m.ParticipatedInRound(0)).Callback(() => { defender.Object.LastRound++; });
            var attacker1 = new Mock<ICombatObject>();
            var attacker2 = new Mock<ICombatObject>();

            var defenderGroup = CreateGroup(defender.Object);
            var attackerGroup = CreateGroup(attacker1.Object, attacker2.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker1.Object, attacker2.Object};
            sut.MockAttackers.MockInRange = new List<ICombatObject> {defender.Object};

            sut.MockAttackers.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets =
                            new List<CombatList.Target>
                            {
                                    new CombatList.Target {CombatObject = attacker1.Object, Group = attackerGroup},
                                    new CombatList.Target {CombatObject = attacker2.Object, Group = attackerGroup}
                            }
            });

            // ReSharper disable RedundantAssignment
            var defenderBattleSide = BattleManager.BattleSide.Defense;
            var defenderOutObject = defender.Object;
            // ReSharper restore RedundantAssignment
            var battleOrder = new Mock<IBattleOrder>();
            battleOrder.Setup(
                              m =>
                              m.NextObject(It.IsAny<uint>(),
                                           It.IsAny<ICombatList>(),
                                           It.IsAny<ICombatList>(),
                                           out defenderOutObject,
                                           out defenderGroup,
                                           out defenderBattleSide)).Returns(true);
            var attackTargetCallCount = 0;

            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              battleOrder.Object,
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
                                                                              attackerObject.Should()
                                                                                            .Be(attacker1.Object);
                                                                              break;
                                                                          case 1:
                                                                              attackerObject.Should()
                                                                                            .Be(attacker2.Object);
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
           // battle.NextToAttack = BattleManager.BattleSide.Defense;
            battle.ExecuteTurn().Should().BeTrue();

            battle.Turn.Should().Be(1);
            attackTargetCallCount.Should().Be(2);
            exitTurnCalled.Should().BeTrue();
            defender.Verify(m => m.ParticipatedInRound(0), Times.Once());
            sut.MockDbManager.Verify(m => m.Save(defender.Object), Times.Once());
        }

        /// <summary>
        ///     When a attacker has entered the battle late
        ///     And NextToAttack is Defense
        ///     And ExecuteTurn is called
        ///     Then attacker should attack
        ///     And ExitTurn should be called
        ///     And ParticipatedInRound should be called on the attacker
        /// </summary>
        [Fact]
        public void TestExecuteWhenObjectJoinedLateShouldAttack()
        {
            var sut = new BattleManagerSut();

            var attacker1 = new Mock<ICombatObject>();
            attacker1.SetupProperty(p => p.Stats.Atk, 10m);
            attacker1.SetupProperty(p => p.LastRound, (uint)3);
            attacker1.SetupGet(p => p.Upkeep).Returns(1);

            var attacker2 = new Mock<ICombatObject>();
            attacker2.SetupProperty(p => p.Stats.Atk, 10m);
            attacker2.SetupProperty(p => p.LastRound, (uint)0);
            attacker2.SetupGet(p => p.Upkeep).Returns(1);
            
            var defender1 = new Mock<ICombatObject>();
            defender1.SetupGet(p => p.Upkeep).Returns(1);

            var attackerGroup = CreateGroup(attacker1.Object, attacker2.Object);
            var defenderGroup = CreateGroup(defender1.Object);

            sut.MockDefenders.Add(defenderGroup);
            sut.MockAttackers.Add(attackerGroup);
            sut.MockDefenders.MockInRange = new List<ICombatObject> {attacker1.Object, attacker2.Object};

            sut.MockDefenders.MockGetBestTargets.Enqueue(new StubCombatList.GetBestTargetQueueItem
            {
                    BestTargetResult = CombatList.BestTargetResult.Ok,
                    Targets =
                            new List<CombatList.Target>
                            {
                                    new CombatList.Target {CombatObject = defender1.Object, Group = defenderGroup}                                    
                            }
            });



            var battle = sut.CreateStubbedAttackBattleManager(1,
                                                              new BattleLocation(BattleLocationType.City, 100),
                                                              new BattleOwner(BattleOwnerType.City, 200),
                                                              new BattleOrder(new Random()),
                                                              delegate { });
            battle.Round = 2;
            battle.Turn = 2;

            var exitTurnCalled = false;
            battle.ExitTurn += (manager, attackers, defenders, turn) =>
                {
                    turn.Should().Be(2);
                    exitTurnCalled = true;
                };

            battle.BattleStarted = true;
            //battle.NextToAttack = BattleManager.BattleSide.Attack;
            battle.ExecuteTurn().Should().BeTrue();

            battle.Turn.Should().Be(3);
            attacker2.Verify(p => p.ParticipatedInRound(2), Times.Once());
            exitTurnCalled.Should().BeTrue();            
            sut.MockDbManager.Verify(m => m.Save(attacker2.Object), Times.Once());
        }
    }
}