using System;
using System.Collections.Generic;
using System.Linq;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class BattleOrderTest
    {
        public static IEnumerable<object[]> TestRandomness
        {
            get
            {
                yield return new object[] { 0, 10, 10, BattleManager.BattleSide.Attack};
                yield return new object[] { 9, 10, 30, BattleManager.BattleSide.Attack };
                yield return new object[] { 10, 10, 5, BattleManager.BattleSide.Defense };
                yield return new object[] { 11, 10, 10, BattleManager.BattleSide.Defense };
                
                yield return new object[] { 9, 10, 20, BattleManager.BattleSide.Attack };
                yield return new object[] { 10, 10, 20, BattleManager.BattleSide.Defense };
                yield return new object[] { 19, 10, 20, BattleManager.BattleSide.Defense };

                yield return new object[] { 20, 20, 10, BattleManager.BattleSide.Defense };
                yield return new object[] { 19, 20, 10, BattleManager.BattleSide.Attack };
            }
        }

        [Theory, PropertyData("TestRandomness")]
        public void NextObject_WhenCalled_CorrectAttackerIsReturned(int randomness, int attackerUpkeep, int defenderUpkeep, BattleManager.BattleSide side)
        {
            var random = Substitute.For<IBattleRandom>();
            random.Next(Arg.Any<int>()).Returns(randomness);
            BattleOrder battleOrder = new BattleOrder(random);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(0, attackerUpkeep);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(attackerUpkeep, attackerGroup);

            var defenderObject = CreateCombatObject(0, defenderUpkeep);
            var defenderGroup = CreateGroup(defenderObject);
            var defenderList = CreateList(defenderUpkeep, defenderGroup);

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().Be(side == BattleManager.BattleSide.Attack ? attackerObject : defenderObject);
            foundInGroup.Should().Be(side);
            random.Received(1).Next(defenderUpkeep + attackerUpkeep);
        }

        [Theory, AutoNSubstituteData]
        public void TestNoMoreAttacker(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var attackerObject = CreateCombatObject(1, 1);
            var attackerList = CreateList(0, CreateGroup(attackerObject));
            attackerList.UpkeepNotParticipated(1).Returns(1);
            var defenderList = CreateList(0, CreateGroup(CreateCombatObject(1, 1)));
            defenderList.UpkeepNotParticipated(1).Returns(0);

            battleOrder.NextObject(0,
                                   attackerList,
                                   defenderList,
                                   out outCombatObject,
                                   out outCombatGroup,
                                   out foundInGroup).Should().Be(false);
            foundInGroup.Should().Be(BattleManager.BattleSide.Attack);
            outCombatObject.Should().Be(attackerObject);

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
            combatList.GetEnumerator().Returns(args => combatGroups.ToList().GetEnumerator());
            combatList.UpkeepNotParticipated(0).Returns(upkeepNotParticipated);
            return combatList;
        }


    }
}