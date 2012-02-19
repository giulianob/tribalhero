using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;
using Game.Logic.Formulas;
using Moq;
using Xunit;

namespace Testing.Formulas
{
    public class WeaponExportTest
    {
        [Fact]
        public void TestNoLaborer()
        {
            Formula formula = new Formula();
            formula.GetWeaponExportLaborProduce(1, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(2, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(3, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(4, 0).Should().Be(0);
            formula.GetWeaponExportLaborProduce(5, 0).Should().Be(0);
        }

        [Fact]
        public void TestHalfLaborers()
        {
            Formula formula = new Formula();
            formula.GetWeaponExportLaborProduce(1, 30).Should().Be(10);
            formula.GetWeaponExportLaborProduce(2, 60).Should().Be(20);
            formula.GetWeaponExportLaborProduce(3, 90).Should().Be(36);
            formula.GetWeaponExportLaborProduce(4, 120).Should().Be(48);
            formula.GetWeaponExportLaborProduce(5, 150).Should().Be(75);
        }

        [Fact]
        public void TestTenLaborers()
        {
            Formula formula = new Formula();
            formula.GetWeaponExportLaborProduce(1, 10).Should().Be(3);
            formula.GetWeaponExportLaborProduce(2, 10).Should().Be(3);
            formula.GetWeaponExportLaborProduce(3, 10).Should().Be(4);
            formula.GetWeaponExportLaborProduce(4, 10).Should().Be(4);
            formula.GetWeaponExportLaborProduce(5, 10).Should().Be(5);
        }

        [Fact]
        public void TestBadLevel()
        {
            Formula formula = new Formula();
            formula.GetWeaponExportLaborProduce(7, 30).Should().Be(0);
        }
    }
}