using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Moq;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class CityValueTest
    {
        public static IEnumerable<object[]> WithDifferentCities
        {
            get
            {
                // Single structure
                yield return new object[] {new[] {CreateMockStructure(1, 1)}, 1};

                // Multiple structures w/ same id
                yield return new object[] {new[] {CreateMockStructure(1, 1), CreateMockStructure(1, 10)}, 11};

                // Structure that shouldnt count towards city value
                yield return new object[] {new[] {CreateMockStructure(100, 1)}, 0};

                // Multiple structures
                yield return
                        new object[]
                        {
                                new[]
                                {
                                        CreateMockStructure(1, 1), CreateMockStructure(1, 10), CreateMockStructure(3, 5),
                                        CreateMockStructure(101, 10)
                                },
                                16
                        };
            }
        }

        [Theory, PropertyData("WithDifferentCities")]
        public void CityValueShouldReturnProperValue(IEnumerable<IStructure> structures, int expected)
        {
            Mock<ObjectTypeFactory> objectTypeFactory = new Mock<ObjectTypeFactory>(MockBehavior.Strict);
            // Structures with id less than 100 count towards Influence, others dont
            objectTypeFactory.Setup(m => m.IsStructureType("NoInfluencePoint", It.IsAny<IStructure>()))
                             .Returns((string type, IStructure s) => s.Type >= 100);

            var formula = new Formula(objectTypeFactory.Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<IStructureCsvFactory>(MockBehavior.Strict).Object);

            var city = new Mock<ICity>();
            city.Setup(m => m.GetEnumerator()).Returns(structures.GetEnumerator());

            formula.CalculateCityValue(city.Object).Should().Be((ushort)expected);
        }

        private static IStructure CreateMockStructure(ushort type, byte level)
        {
            var structure = new Mock<IStructure>();
            structure.SetupGet(p => p.Type).Returns(type);
            structure.SetupGet(p => p.Lvl).Returns(level);

            return structure.Object;
        }
    }
}