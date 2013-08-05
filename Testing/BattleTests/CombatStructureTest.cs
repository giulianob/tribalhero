using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Moq;
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
            Mock<StructureStats> structureStats = new Mock<StructureStats>();
            Mock<IStructure> structure = new Mock<IStructure>();
            Mock<BattleStats> battleStats = new Mock<BattleStats>();
            Mock<Formula> formula = new Mock<Formula>();
            Mock<IBattleFormulas> battleFormula = new Mock<IBattleFormulas>();
            Mock<IActionFactory> actionFactory = new Mock<IActionFactory>();

            structureStats.SetupAllProperties();
            structure.SetupGet(p => p.Stats).Returns(structureStats.Object);

            CombatStructure combatStructure = new CombatStructure(1,
                                                                  1,
                                                                  structure.Object,
                                                                  battleStats.Object,
                                                                  hp,
                                                                  1,
                                                                  1,
                                                                  formula.Object,
                                                                  actionFactory.Object,
                                                                  battleFormula.Object);
            Resource returning;
            int attackPoints;
            combatStructure.TakeDamage(dmg, out returning, out attackPoints);

            returning.Should().BeNull("No resources should be returned by structures");
            attackPoints.Should().Be(0, "No attack points should have been received");

            combatStructure.Hp.Should().Be(expectedHp);
            structureStats.Object.Hp.Should().Be(expectedHp);
            formula.Verify(m => m.GetStructureKilledAttackPoint(It.IsAny<ushort>(), It.IsAny<byte>()), Times.Never());
            structure.Verify(m => m.BeginUpdate(), Times.Once());
            structure.Verify(m => m.EndUpdate(), Times.Once());
        }

        [Theory, PropertyData("TakeDamageWhenDiedData")]
        public void TakeDamageWhenDied(decimal hp, decimal dmg, decimal expectedHp)
        {
            Mock<StructureStats> structureStats = new Mock<StructureStats>();
            Mock<IStructure> structure = new Mock<IStructure>();
            Mock<BattleStats> battleStats = new Mock<BattleStats>();
            Mock<Formula> formula = new Mock<Formula>();
            Mock<IBattleFormulas> battleFormula = new Mock<IBattleFormulas>();
            Mock<IActionFactory> actionFactory = new Mock<IActionFactory>();

            structureStats.SetupAllProperties();
            structure.SetupGet(p => p.Stats).Returns(structureStats.Object);
            formula.Setup(m => m.GetStructureKilledAttackPoint(100, 2)).Returns(10);

            CombatStructure combatStructure = new CombatStructure(1,
                                                                  1,
                                                                  structure.Object,
                                                                  battleStats.Object,
                                                                  hp,
                                                                  100,
                                                                  2,
                                                                  formula.Object,
                                                                  actionFactory.Object,
                                                                  battleFormula.Object);
            Resource returning;
            int attackPoints;
            combatStructure.TakeDamage(dmg, out returning, out attackPoints);

            returning.Should().BeNull("No resources should be returned by structures");
            attackPoints.Should().Be(10);

            combatStructure.Hp.Should().Be(expectedHp);
            structureStats.Object.Hp.Should().Be(expectedHp);
            structure.Verify(m => m.BeginUpdate(), Times.Once());
            structure.Verify(m => m.EndUpdate(), Times.Once());
        }
    }
}