using System.Collections.Generic;
using System.Linq;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class BattleOrderTest
    {
        public static IEnumerable<object[]> TestSequence5V1Data
        {
            get
            {
                //  Attacker 5 vs Defender 1
                yield return
                        new object[]
                        {
                            CreateList(5,CreateGroup(CreateCombatObject(0,5))),
                            CreateList(1,CreateGroup(CreateCombatObject(0,1))),
                            new []
                            {
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                            }
                        };
                //  Defender 5 vs Attacker 1
                yield return
                        new object[]
                        {
                            CreateList(1,CreateGroup(CreateCombatObject(0,1))),
                            CreateList(5,CreateGroup(CreateCombatObject(0,5))),
                            new []
                            {
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                            }
                        };
            }
        }
        public static IEnumerable<object[]> TestSequence15V4Data
        {
            get
            {
                //  Attacker 15 vs Defender 4
                yield return
                        new object[]
                        {
                            CreateList(15,CreateGroup(CreateCombatObject(0,15))),
                            CreateList(4,CreateGroup(CreateCombatObject(0,4))),
                            new []
                            {
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                            }
                        };
            }
        }

        public static IEnumerable<object[]> TestSequence1V1Data
        {
            get
            {
                //  Attacker 1 vs Defender 1
                yield return
                        new object[]
                        {
                            CreateList(1,CreateGroup(CreateCombatObject(0,1))),
                            CreateList(1,CreateGroup(CreateCombatObject(0,1))),
                            new []
                            {
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                            }
                        };
            }
        }  

        public static IEnumerable<object[]> TestSequenceMultiGroupData
        {
            get
            {
                //  Attacker 15 vs Defender 4
                yield return
                        new object[]
                        {
                            CreateList(15,CreateGroup(CreateCombatObject(0,1),CreateCombatObject(0,2)),
                                       CreateGroup(CreateCombatObject(0,3),CreateCombatObject(0,4),CreateCombatObject(0,5))),
                            CreateList(4,CreateGroup(CreateCombatObject(0,1),CreateCombatObject(0,1))),
                            new []
                            {
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Attack,
                                BattleManager.BattleSide.Defense,
                            }
                        };
            }
        }
        [Theory, PropertyData("TestSequenceMultiGroupData")]
        public void TestSequenceMultiGroup(ICombatList attackers, ICombatList defenders, BattleManager.BattleSide[] patterns)
        {
            TestSequence(attackers, defenders, patterns);
        }

        [Theory, PropertyData("TestSequence5V1Data")]
        public void TestSequence5V1(ICombatList attackers, ICombatList defenders, BattleManager.BattleSide[] patterns)
        {
            TestSequence(attackers, defenders, patterns);
        }

        [Theory, PropertyData("TestSequence15V4Data")]
        public void TestSequence15V4(ICombatList attackers, ICombatList defenders, BattleManager.BattleSide[] patterns)
        {
            TestSequence(attackers, defenders, patterns);
        }

        /* when it's 1v1, defender should hit first */
        [Theory, PropertyData("TestSequence1V1Data")]
        public void TestSequence1V1(ICombatList attackers, ICombatList defenders, BattleManager.BattleSide[] patterns)
        {
            TestSequence(attackers, defenders, patterns);
        }
        
        private void TestSequence(ICombatList attackers, ICombatList defenders, IEnumerable<BattleManager.BattleSide> patterns)
        {
            var fixture = new Fixture();
            var battleOrder = fixture.Create<BattleOrder>();

            uint round = 0;
            uint turn = 0;
            foreach (BattleManager.BattleSide side in patterns)
            {
                ICombatObject outCombatObject;
                ICombatGroup outCombatGroup;
                BattleManager.BattleSide foundInGroup;
                if (battleOrder.NextObject(round, turn++, attackers, defenders, out outCombatObject, out outCombatGroup, out foundInGroup))
                {
                    foundInGroup.Should().Be(side);
                }
                else
                {
                    round++;
                    turn = 0;
                }
            }
        }

        [Theory, AutoNSubstituteData]
        public void TestNoMoreAttacker(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            battleOrder.NextObject(0,
                                   2,
                                   CreateList(0, CreateGroup(CreateCombatObject(1, 1))),
                                   CreateList(0, CreateGroup(CreateCombatObject(1, 1))),
                                   out outCombatObject,
                                   out outCombatGroup,
                                   out foundInGroup);
            outCombatObject.Should().BeNull();
            outCombatGroup.Should().BeNull();
        }

        /* Hardcoded ratio to 1:1
         * next attackside should be defender at turn 0 
         * but still there's no defender left, it will choose the next attacker       
         */
        [Theory, AutoNSubstituteData]
        public void TestOnlyAttackerLeft(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var expectedObject = CreateCombatObject(0, 3);
            var result = battleOrder.NextObject(0,
                                   0,
                                   CreateList(1, CreateGroup(CreateCombatObject(1, 1), expectedObject, CreateCombatObject(0, 2))),
                                   CreateList(1, CreateGroup(CreateCombatObject(1, 1))),
                                   out outCombatObject,
                                   out outCombatGroup,
                                   out foundInGroup);
            
            result.Should().Be(true);
            outCombatObject.Should().Be(expectedObject);
            foundInGroup.Should().Be(BattleManager.BattleSide.Attack);
        }

        /* Hardcoded ratio to 1:1 and turn 1
         * next attackside should be attacker at turn 0 
         * but still there's no attacker left, it will choose the next defender       
         */
        [Theory, AutoNSubstituteData]
        public void TestOnlyDefenderLeft(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var expectedObject = CreateCombatObject(0, 3);
            var expectedGroup = CreateGroup(CreateCombatObject(1, 1), expectedObject, CreateCombatObject(0, 2));
            battleOrder.NextObject(0,
                                   1,
                                   CreateList(1, CreateGroup(CreateCombatObject(1, 1))),
                                   CreateList(1, expectedGroup),
                                   out outCombatObject,
                                   out outCombatGroup,
                                   out foundInGroup).Should().Be(true);
            outCombatObject.Should().Be(expectedObject);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);

        }

        private static ICombatObject CreateCombatObject(uint round, int upkeep)
        {
            var combatObject = Substitute.For<ICombatObject>();
            combatObject.LastRound.Returns(round);
            combatObject.Upkeep.Returns(upkeep);
            return combatObject;
        }

        private static ICombatGroup CreateGroup(params ICombatObject[] combatObjects)
        {
            var combatGroup = Substitute.For<ICombatGroup>();
            combatGroup.GetEnumerator().Returns(args => combatObjects.ToList().GetEnumerator());
            return combatGroup;
        }

        private static ICombatList CreateList(int upkeepNotParticipated, params ICombatGroup[] combatGroups)
        {
            var combatList = Substitute.For<ICombatList>();
            combatList.GetEnumerator().Returns(combatGroups.ToList().GetEnumerator());
            combatList.UpkeepNotParticipated(0).ReturnsForAnyArgs(upkeepNotParticipated);
            return combatList;
        }


    }
}