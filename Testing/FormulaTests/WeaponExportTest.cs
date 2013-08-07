using FluentAssertions;
using Game.Logic.Formulas;
using Game.Setup;
using Moq;
using Xunit;

namespace Testing.FormulaTests
{
    public class WeaponExportTest
    {
        [Fact]
        public void TestNoLaborer()
        {
            var formula = new Formula(new Mock<IObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            formula.GetWeaponExportLaborProduce(1, 0, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(2, 0, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(3, 0, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(4, 0, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(5, 0, 0).Should().Be(0);
        }

        [Fact]
        public void TestHalfLaborers()
        {
            var formula = new Formula(new Mock<IObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            formula.GetWeaponExportLaborProduce(1, 30, 0).Should().Be(10);
            formula.GetWeaponExportLaborProduce(2, 60, 0).Should().Be(20);
            formula.GetWeaponExportLaborProduce(3, 90, 0).Should().Be(36);
            formula.GetWeaponExportLaborProduce(4, 120, 0).Should().Be(48);
            formula.GetWeaponExportLaborProduce(5, 150, 0).Should().Be(75);
        }

        [Fact]
        public void TestTenLaborers()
        {
            var formula = new Formula(new Mock<IObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            formula.GetWeaponExportLaborProduce(1, 10, 0).Should().Be(3);
            formula.GetWeaponExportLaborProduce(2, 10, 0).Should().Be(3);
            formula.GetWeaponExportLaborProduce(3, 10, 0).Should().Be(4);
            formula.GetWeaponExportLaborProduce(4, 10, 0).Should().Be(4);
            formula.GetWeaponExportLaborProduce(5, 10, 0).Should().Be(5);
        }

        [Fact]
        public void TestBadLevel()
        {
            var formula = new Formula(new Mock<IObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            formula.GetWeaponExportLaborProduce(7, 30, 0).Should().Be(0);
        }

        [Fact]
        public void TestHalfLaborersOverLimit()
        {
            var formula = new Formula(new Mock<IObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            formula.GetWeaponExportLaborProduce(1, 30, 2001).Should().Be(2);
            formula.GetWeaponExportLaborProduce(2, 60, 4001).Should().Be(4);
            formula.GetWeaponExportLaborProduce(3, 90, 6001).Should().Be(7);
            formula.GetWeaponExportLaborProduce(4, 120, 8001).Should().Be(9);
            formula.GetWeaponExportLaborProduce(5, 150, 10001).Should().Be(15);
        }

        [Fact]
        public void TestWeirdNumber()
        {
            var formula = new Formula(new Mock<IObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            formula.GetWeaponExportLaborProduce(3, 3, 0).Should().Be(1);
            formula.GetWeaponExportLaborProduce(3, 15, 6001).Should().Be(1);
            formula.GetWeaponExportLaborProduce(4, 3, 0).Should().Be(1);
            formula.GetWeaponExportLaborProduce(4, 15, 8001).Should().Be(1);
        }

    }
}