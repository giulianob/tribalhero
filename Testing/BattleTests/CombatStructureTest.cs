using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class CombatStructureTest
    {
        public static IEnumerable<object[]> TakeDamageWhenNotDiedData
        {
            get
            {
                yield return new object[] {10.5m, 0.5m, 10m};
                yield return new object[] {10.4m, 0.3m, 10.1m};
                yield return new object[] {10.5m, 0.25m, 10.25m};
                yield return new object[] {10.4m, 10m, 0.4m};
            }
        }

        public static IEnumerable<object[]> TakeDamageWhenDiedData
        {
            get
            {
                yield return new object[] {10.5m, 10.5m, 0m};
                yield return new object[] {0.3m, 30.4m, 0m};
                yield return new object[] {300.3m, 1000.4m, 0m};
            }
        }

        [Theory, PropertyData("TakeDamageWhenNotDiedData")]
        public void TakeDamageWhenNotDied(decimal hp, decimal dmg, decimal expectedHp)
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            
            var structure = Substitute.For<IStructure>();
            var formula = Substitute.For<Formula>();

            structure.Stats.Hp.Returns(hp);
            fixture.Register(() => structure);
            
            var combatStructure = fixture.Create<CombatStructure>();
            
            Resource returning;
            int attackPoints;
            combatStructure.TakeDamage(dmg, out returning, out attackPoints);

            returning.Should().BeNull("No resources should be returned by structures");
            attackPoints.Should().Be(0, "No attack points should have been received");

            combatStructure.Hp.Should().Be(expectedHp);
            structure.Stats.Hp.Should().Be(expectedHp);
            formula.DidNotReceiveWithAnyArgs().GetStructureKilledAttackPoint(0, 0);
            structure.Received(1).BeginUpdate();
            structure.Received(1).EndUpdate();
        }
        
        [Theory, PropertyData("TakeDamageWhenDiedData")]
        public void TakeDamageWhenDied(decimal hp, decimal dmg, decimal expectedHp)
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            
            var structure = Substitute.For<IStructure>();

            structure.Stats.Hp.Returns(hp);
            structure.Type.Returns<ushort>(100);
            structure.Lvl.Returns<byte>(2);
            fixture.Register(() => structure);

            var formula = Substitute.For<Formula>();
            fixture.Register(() => formula);
            formula.GetStructureKilledAttackPoint(100, 2).Returns(10);

            var combatStructure = fixture.Create<CombatStructure>();

            Resource returning;
            int attackPoints;
            combatStructure.TakeDamage(dmg, out returning, out attackPoints);

            returning.Should().BeNull("No resources should be returned by structures");
            attackPoints.Should().Be(10);

            combatStructure.Hp.Should().Be(expectedHp);
            structure.Stats.Hp.Should().Be(expectedHp);
            structure.Received(1).BeginUpdate();
            structure.Received(1).EndUpdate();
        }         
    }
}