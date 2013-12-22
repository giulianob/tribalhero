using Common.Testing;
using FluentAssertions;
using Game.Logic.Formulas;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class NewCityResourceTest
    {
        [Theory]
        [InlineAutoNSubstituteData(1, 130, 50)]
        [InlineAutoNSubstituteData(10, 5800, 500)]
        [InlineAutoNSubstituteData(30, 47400, 1500)]
        public void TestNewCityResource(int cityCount, int expectedIp, int expectedWagons, Formula formula)
        {
            int influencePoints;
            int wagons;

            formula.GetNewCityCost(cityCount, out influencePoints, out wagons);
            influencePoints.Should().Be(expectedIp);
            wagons.Should().Be(expectedWagons);
        }
    }
}