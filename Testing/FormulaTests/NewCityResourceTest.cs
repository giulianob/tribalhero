using FluentAssertions;
using Game.Logic.Formulas;
using Game.Setup;
using Moq;
using Xunit;

namespace Testing.FormulaTests
{
    public class NewCityResourceTest
    {
        [Fact]
        public void TestNewCityResource()
        {
            int influencePoints;
            int wagons;

            var formula = new Formula(new Mock<ObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<StructureCsvFactory>(MockBehavior.Strict).Object);

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