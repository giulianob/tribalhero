using System.Collections.Generic;
using System.Linq;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
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

            var attackerObject = CreateCombatObject(0);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(attackerUpkeep, attackerGroup);

            var defenderObject = CreateCombatObject(0);
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
            var attackerObject = CreateCombatObject(1);
            var attackerList = CreateList(0, CreateGroup(attackerObject));
            attackerList.UpkeepNotParticipatedInRound(1).Returns(1);
            var defenderList = CreateList(0, CreateGroup(CreateCombatObject(1)));
            defenderList.UpkeepNotParticipatedInRound(1).Returns(0);

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
        public void NextObject_WhenNextObjectShouldBeDefenderAndNoDefendersAreLeftInTheRound_ShouldChooseAttacker(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var expectedObject = CreateCombatObject(0);
            var result = battleOrder.NextObject(0,
                                   CreateList(1, CreateGroup(CreateCombatObject(1), expectedObject, CreateCombatObject(0))),
                                   CreateList(1, CreateGroup(CreateCombatObject(1))),
                                   out outCombatObject,
                                   out outCombatGroup,
                                   out foundInGroup);
            
            result.Should().Be(true);
            outCombatObject.Should().Be(expectedObject);
            foundInGroup.Should().Be(BattleManager.BattleSide.Attack);
        }

        [Theory, AutoNSubstituteData]
        public void NextObject_WhenNextObjectIsWaitingToJoinBattleIsTrue_ShouldNotChooseThatObject(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var expectedObject = CreateCombatObject(0);

            var isStillWaitingToJoinbattleCombatObject = CreateCombatObject(0);
            isStillWaitingToJoinbattleCombatObject.IsWaitingToJoinBattle.Returns(true);

            var result = battleOrder.NextObject(0,
                                   CreateList(1, CreateGroup(isStillWaitingToJoinbattleCombatObject, expectedObject)),
                                   CreateList(1, CreateGroup(CreateCombatObject(1))),
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
        public void NextObject_WhenNextObjectShouldBeAttackerAndNotAttackersLeft_ShouldChooseDefense(BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;
            var expectedObject = CreateCombatObject(0);
            var expectedGroup = CreateGroup(CreateCombatObject(1), expectedObject, CreateCombatObject(0));
            battleOrder.NextObject(0,
                                   CreateList(1, CreateGroup(CreateCombatObject(1))),
                                   CreateList(1, expectedGroup),
                                   out outCombatObject,
                                   out outCombatGroup,
                                   out foundInGroup).Should().Be(true);
            outCombatObject.Should().Be(expectedObject);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);

        }

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenHasObjectsInCurrentRoundAndGroupRandomReturns0AndObjectRandomReturns1_ShouldReturnSecondObjectFromFirstGroup(
            [Frozen] IBattleRandom battleRandom,
            BattleOrder battleOrder)
        {
            // 999 controls order to be defense
            // 0 is the first group idx
            // 1 is the second combat obj idx
            battleRandom.Next(Arg.Any<int>()).Returns(999, 0, 1);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(round: 0);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(upkeepNotParticipated: 1, combatGroups: attackerGroup);

            var defenderObject1 = CreateCombatObject(round: 0);
            var defenderObject2 = CreateCombatObject(round: 0);
            var defenderGroup = CreateGroup(defenderObject1, defenderObject2);
            var defenderList = CreateList(upkeepNotParticipated: 999, combatGroups: defenderGroup);

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().Be(defenderObject2);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);
        }

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenGroupRandomReturns0AndObjectRandomReturns1AndObject1IsNotInRound_ShouldReturnFirstObjectFromFirstGroup(
            [Frozen] IBattleRandom battleRandom,
            BattleOrder battleOrder)
        {
            // 999 controls order to be defense
            // 0 is the first group idx
            // 1 is the second combat obj idx
            battleRandom.Next(Arg.Any<int>()).Returns(999, 0, 1);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(round: 0);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(upkeepNotParticipated: 1, combatGroups: attackerGroup);

            var defenderObject1 = CreateCombatObject(round: 0);
            var defenderObject2 = CreateCombatObject(round: 1);
            var defenderGroup = CreateGroup(defenderObject1, defenderObject2);
            var defenderList = CreateList(upkeepNotParticipated: 999, combatGroups: defenderGroup);

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().Be(defenderObject1);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);
        }

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenGroupRandomReturns1AndGroupHasObjectsLeft_ShouldReturnFirstObjectFromSecondGroup(
            [Frozen] IBattleRandom battleRandom,
            BattleOrder battleOrder)
        {
            // 999 controls order to be defense
            // 1 is the second group idx
            // 0 is the first combat obj idx
            battleRandom.Next(Arg.Any<int>()).Returns(999, 1, 0);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(round: 0);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(upkeepNotParticipated: 1, combatGroups: attackerGroup);

            var defenderObject1 = CreateCombatObject(round: 0);
            var defenderGroup1 = CreateGroup(defenderObject1);
            var defenderObject2 = CreateCombatObject(round: 0);
            var defenderGroup2 = CreateGroup(defenderObject2);            
            var defenderList = CreateList(upkeepNotParticipated: 999, combatGroups: new [] { defenderGroup1, defenderGroup2 });

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().Be(defenderObject2);
            ((object)outCombatGroup).Should().Be(defenderGroup2);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);
        }

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenGroupRandomReturns1AndGroupHasNoObjectsLeft_ShouldReturnFirstObjectFromFirstGroup(
            [Frozen] IBattleRandom battleRandom,
            BattleOrder battleOrder)
        {
            // 999 controls order to be defense
            // 1 is the second group idx
            // 0 is the first combat obj idx
            battleRandom.Next(Arg.Any<int>()).Returns(999, 1, 0);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(round: 0);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(upkeepNotParticipated: 1, combatGroups: attackerGroup);

            var defenderObject1 = CreateCombatObject(round: 0);
            var defenderGroup1 = CreateGroup(defenderObject1);
            var defenderObject2 = CreateCombatObject(round: 1);
            var defenderGroup2 = CreateGroup(defenderObject2);
            var defenderList = CreateList(upkeepNotParticipated: 999, combatGroups: new [] { defenderGroup1, defenderGroup2 });

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().Be(defenderObject1);
            ((object)outCombatGroup).Should().Be(defenderGroup1);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);
        }
        
        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenThereAreNoGroups_ShouldReturnTrueAndNullObjAndNullGroup(
            BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerList = CreateList(upkeepNotParticipated: 0);

            var defenderList = CreateList(upkeepNotParticipated: 0);

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().BeNull();
            ((object)outCombatGroup).Should().BeNull();
        }    

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenGroupsAreEmpty_ShouldReturnTrueAndNullObjAndNullGroup(
            BattleOrder battleOrder)
        {
            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerList = CreateList(upkeepNotParticipated: 0, combatGroups: CreateGroup());

            var defenderList = CreateList(upkeepNotParticipated: 0, combatGroups: CreateGroup());

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(true);
            outCombatObject.Should().BeNull();
            ((object)outCombatGroup).Should().BeNull();
        }    

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenDoesNotHaveObjectsInCurrentRoundAndGroupRandomReturns0AndObjectRandomReturns1_ShouldReturnSecondObjectFromFirstGroup(
            [Frozen] IBattleRandom battleRandom,
            BattleOrder battleOrder)
        {
            // 999 controls order to be defense
            // 0 is the first group idx
            // 1 is the second combat obj idx
            // 1 is the random obj after we've found no objects are in the current round
            // 0 is going to be when looking into the attacker list (for group and obj) after it didnt find a defense
            battleRandom.Next(Arg.Any<int>()).Returns(999, 0, 1, 1, 0);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(round: 1);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(upkeepNotParticipated: 1, combatGroups: attackerGroup);

            var defenderObject1 = CreateCombatObject(round: 1);
            var defenderObject2 = CreateCombatObject(round: 1);
            var defenderGroup = CreateGroup(defenderObject1, defenderObject2);
            var defenderList = CreateList(upkeepNotParticipated: 999, combatGroups: defenderGroup);

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(false);
            outCombatObject.Should().Be(defenderObject2);
            ((object)outCombatGroup).Should().Be(defenderGroup);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);
        }       

        [Theory, AutoNSubstituteData]
        public void ExecuteTurn_WhenDoesNotHaveObjectsInCurrentRoundAndGroupRandomReturns0AndFirstGroupIsEmpty_ShouldReturnObjectFromSecondGroup(
            [Frozen] IBattleRandom battleRandom,
            BattleOrder battleOrder)
        {
            // 999 controls order to be defense
            // 0 is the to always return first obj and first group
            battleRandom.Next(Arg.Any<int>()).Returns(999, 0);

            ICombatObject outCombatObject;
            ICombatGroup outCombatGroup;
            BattleManager.BattleSide foundInGroup;

            var attackerObject = CreateCombatObject(round: 1);
            var attackerGroup = CreateGroup(attackerObject);
            var attackerList = CreateList(upkeepNotParticipated: 1, combatGroups: attackerGroup);

            var defenderGroup1 = CreateGroup();
            var defenderObject2 = CreateCombatObject(round: 1);
            var defenderGroup2 = CreateGroup(defenderObject2);
            var defenderList = CreateList(upkeepNotParticipated: 999, combatGroups: new [] { defenderGroup1, defenderGroup2 });

            var result = battleOrder.NextObject(0, attackerList, defenderList, out outCombatObject, out outCombatGroup, out foundInGroup);

            result.Should().Be(false);
            outCombatObject.Should().Be(defenderObject2);
            ((object)outCombatGroup).Should().Be(defenderGroup2);
            foundInGroup.Should().Be(BattleManager.BattleSide.Defense);
        }   

        private static ICombatObject CreateCombatObject(uint round)
        {
            var combatObject = Substitute.For<ICombatObject>();
            combatObject.HasAttacked(Arg.Any<uint>()).Returns(true);
            combatObject.HasAttacked(round).Returns(false);
            return combatObject;
        }

        private static ICombatGroup CreateGroup(params ICombatObject[] combatObjects)
        {
            var combatGroup = Substitute.For<ICombatGroup>();
            combatGroup.Count.Returns(combatObjects.Count());
            combatGroup.GetEnumerator().Returns(args => combatObjects.ToList().GetEnumerator());
            combatGroup[Arg.Any<int>()].Returns(args => combatObjects[(int)args[0]]);
            return combatGroup;
        }

        private static ICombatList CreateList(int upkeepNotParticipated, params ICombatGroup[] combatGroups)
        {
            var combatList = Substitute.For<ICombatList>();                        
            combatList.Count.Returns(combatGroups.Count());
            combatList.GetEnumerator().Returns(args => combatGroups.ToList().GetEnumerator());
            combatList.UpkeepNotParticipatedInRound(0).ReturnsForAnyArgs(upkeepNotParticipated);
            combatList[Arg.Any<int>()].Returns(args => combatGroups[(int)args[0]]);

            return combatList;
        }


    }
}