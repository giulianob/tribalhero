using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Logic.Formulas;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class BattleFormulaTest
    {
        [Theory, AutoNSubstituteData]
        public void TestFewAttackersSingleDefender(Formula formula)
        {
            var defender1 = Substitute.For<ICombatGroup>();
            defender1.GetEnumerator().Returns(new List<ICombatObject> {Substitute.For<ICombatObject>()}.GetEnumerator());

            ICombatList defenders = Substitute.For<ICombatList>();
            defenders.GetEnumerator().Returns(new List<ICombatGroup> {defender1}.GetEnumerator());

            var attacker1 = Substitute.For<ICombatGroup>();
            attacker1.GetEnumerator().Returns(new List<ICombatObject> {Substitute.For<ICombatObject>()}.GetEnumerator());

            var attacker2 = Substitute.For<ICombatGroup>();
            attacker2.GetEnumerator().Returns(new List<ICombatObject> {Substitute.For<ICombatObject>(), Substitute.For<ICombatObject>()}.GetEnumerator());

            ICombatList attackers = Substitute.For<ICombatList>();
            attackers.GetEnumerator().Returns(new List<ICombatGroup> {attacker1, attacker2}.GetEnumerator());

            formula.GetBattleInterval(defenders, attackers).Should().BeInRange(19.22, 19.24);
        }

        [Theory]
        [InlineAutoNSubstituteData(100, 300)]
        [InlineAutoNSubstituteData(0, 400)]
        [InlineAutoNSubstituteData(200, 300)]
        public void TestAtMaxOrOverMax(int defenderCnt, int attackerCnt, Formula formula)
        {
            var defendersCombatObjects = new List<ICombatObject>();            
            for (var i = 0; i < defenderCnt; i++)
            {
                defendersCombatObjects.Add(Substitute.For<ICombatObject>());
            }

            var defender1 = Substitute.For<ICombatGroup>();
            defender1.GetEnumerator().Returns(defendersCombatObjects.GetEnumerator());

            ICombatList defenders = Substitute.For<ICombatList>();
            defenders.GetEnumerator().Returns(new List<ICombatGroup> {defender1}.GetEnumerator());

            var attacker1 = Substitute.For<ICombatGroup>();

            var attackersCombatObjects = new List<ICombatObject>();            
            for (var i = 0; i < attackerCnt; i++)
            {
                attackersCombatObjects.Add(Substitute.For<ICombatObject>());
            }

            attacker1.GetEnumerator().Returns(attackersCombatObjects.GetEnumerator());

            ICombatList attackers = Substitute.For<ICombatList>();
            attackers.GetEnumerator().Returns(new List<ICombatGroup> {attacker1}.GetEnumerator());

            formula.GetBattleInterval(defenders, attackers).Should().Be(4);
        }
    }
}