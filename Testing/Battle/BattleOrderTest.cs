using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Moq;
using Ploeh.AutoFixture;
using Xunit.Extensions;

namespace Testing.Battle
{
    public class BattleOrderTest
    {
        public static IEnumerable<object[]> TestNextObjectData
        {
            get
            {
                // offensive side has obj and sideAttacking = Attack
                ICombatObject expectedCombatObject = CreateCombatObject(1);
                yield return
                        new object[]
                        {
                                1,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(2),
                                                    CreateCombatObject(0),
                                                    expectedCombatObject,
                                                    CreateCombatObject(1)),
                                        CreateGroup(CreateCombatObject(1))
                                },
                                new List<ICombatGroup> {CreateGroup(CreateCombatObject(1), CreateCombatObject(2))},
                                BattleManager.BattleSide.Attack, BattleManager.BattleSide.Attack, expectedCombatObject,
                                true
                        };

                // defensive side has obj and sideAttacking = Attack
                expectedCombatObject = CreateCombatObject(3);
                yield return
                        new object[]
                        {
                                3,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(4), CreateCombatObject(4), CreateCombatObject(4)),
                                        CreateGroup(CreateCombatObject(5))
                                },
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(4), expectedCombatObject, CreateCombatObject(4))
                                }
                                , BattleManager.BattleSide.Attack, BattleManager.BattleSide.Defense,
                                expectedCombatObject, true
                        };

                // offensive side has obj and sideAttacking = Defense
                expectedCombatObject = CreateCombatObject(2);
                yield return
                        new object[]
                        {
                                2,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(4), CreateCombatObject(4), CreateCombatObject(4)),
                                        CreateGroup(expectedCombatObject)
                                },
                                new List<ICombatGroup> {CreateGroup(CreateCombatObject(4), CreateCombatObject(4))},
                                BattleManager.BattleSide.Defense, BattleManager.BattleSide.Attack, expectedCombatObject,
                                true
                        };

                // defensive side has obj and sideAttacking = Defense
                expectedCombatObject = CreateCombatObject(5);
                yield return
                        new object[]
                        {
                                5,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6), CreateCombatObject(6))
                                },
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6)),
                                        CreateGroup(expectedCombatObject),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6))
                                },
                                BattleManager.BattleSide.Defense, BattleManager.BattleSide.Defense, expectedCombatObject
                                , true
                        };

                // defensive side attacking and everyone is on next round
                expectedCombatObject = CreateCombatObject(6);
                yield return
                        new object[]
                        {
                                5,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6), CreateCombatObject(6))
                                },
                                new List<ICombatGroup>
                                {
                                        CreateGroup(expectedCombatObject),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6)),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6))
                                },
                                BattleManager.BattleSide.Defense, BattleManager.BattleSide.Defense, expectedCombatObject
                                , false
                        };

                // offensive side attacking and everyone is on next round
                expectedCombatObject = CreateCombatObject(6);
                yield return
                        new object[]
                        {
                                5,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(expectedCombatObject,
                                                    CreateCombatObject(6),
                                                    CreateCombatObject(6),
                                                    CreateCombatObject(6))
                                },
                                new List<ICombatGroup>
                                {
                                        CreateGroup(),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6)),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6))
                                },
                                BattleManager.BattleSide.Attack, BattleManager.BattleSide.Attack, expectedCombatObject,
                                false
                        };

                // Invalid order
                yield return
                        new object[]
                        {
                                4,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6), CreateCombatObject(6))
                                },
                                new List<ICombatGroup>
                                {
                                        CreateGroup(),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6)),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6))
                                },
                                BattleManager.BattleSide.Attack, BattleManager.BattleSide.Attack, null, true
                        };

                yield return
                        new object[]
                        {
                                4,
                                new List<ICombatGroup>
                                {
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6), CreateCombatObject(6))
                                },
                                new List<ICombatGroup>
                                {
                                        CreateGroup(),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6)),
                                        CreateGroup(CreateCombatObject(6), CreateCombatObject(6))
                                },
                                BattleManager.BattleSide.Defense, BattleManager.BattleSide.Defense, null, true
                        };
            }
        }

        [Theory, PropertyData("TestNextObjectData")]
        public void TestNextObjectWhenOffensiveHasObjectAndSideIsOffense(int round,
                                                                         List<ICombatGroup> attackers,
                                                                         List<ICombatGroup> defenders,
                                                                         BattleManager.BattleSide sideAttacking,
                                                                         BattleManager.BattleSide expectedFoundInGroup,
                                                                         ICombatObject expectedCombatObject,
                                                                         bool expectedResult)
        {
            var fixture = new Fixture();
            var battleOrder = fixture.Create<BattleOrder>();

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var nextObject = battleOrder.NextObject(round: (uint)round,
                                                    attacker: attackers,
                                                    defender: defenders,
                                                    sideAttacking: sideAttacking,
                                                    outCombatObject: out outCombatObject,
                                                    outCombatGroup: out outCombatGroup,
                                                    foundInGroup: out foundInGroup);

            foundInGroup.Should().Be(expectedFoundInGroup);
            if (expectedCombatObject == null)
            {
                outCombatGroup.Should().BeNull();
            }
            else if (expectedFoundInGroup == BattleManager.BattleSide.Defense)
            {
                defenders.Should().Contain(outCombatGroup);
            }
            else
            {
                attackers.Should().Contain(outCombatGroup);
            }

            outCombatObject.Should().Be(expectedCombatObject);
            nextObject.Should().Be(expectedResult);
        }

        private static ICombatObject CreateCombatObject(uint round)
        {
            Mock<ICombatObject> combatObject = new Mock<ICombatObject>();
            combatObject.SetupProperty(p => p.LastRound, round);
            return combatObject.Object;
        }

        private static ICombatGroup CreateGroup(params ICombatObject[] combatObjects)
        {
            Mock<ICombatGroup> combatGroup = new Mock<ICombatGroup>();
            combatGroup.Setup(m => m.GetEnumerator()).Returns(() => combatObjects.ToList().GetEnumerator());
            return combatGroup.Object;
        }
    }
}