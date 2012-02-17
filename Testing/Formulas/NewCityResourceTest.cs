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
    public class NewCityResourceTest
    {
        [Fact]
        public void TestNewCityResource()
        {
            int influencePoints;
            int wagons;
            Formula formula = new Formula();
            formula.GetNewCityCost(1, out influencePoints, out wagons);
            influencePoints.Should().Be(100);
            wagons.Should().Be(50);
            formula.GetNewCityCost(10, out influencePoints, out wagons);
            influencePoints.Should().Be(2800);
            wagons.Should().Be(500);
            formula.GetNewCityCost(30, out influencePoints, out wagons);
            influencePoints.Should().Be(20400);
            wagons.Should().Be(1500);
        }
    }
}