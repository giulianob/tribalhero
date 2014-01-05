using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture;
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
                yield return new object[] {new[] {CreateMockStructure(1, 1, 1)}, 1};

                // Multiple structures w/ same type
                yield return new object[] {new[] {CreateMockStructure(1, 1, 1), CreateMockStructure(1, 10, 1)}, 11};

                // Structure that shouldnt count towards city value
                yield return new object[] {new[] {CreateMockStructure(100, 1, 1)}, 0};

                // Multiple structures w/ diff sizes
                yield return
                        new object[]
                        {
                                new[]
                                {
                                        CreateMockStructure(1, 1, 1), 
                                        CreateMockStructure(1, 10, 3), 
                                        CreateMockStructure(3, 5, 3),
                                        CreateMockStructure(101, 10, 2)
                                }, 136
                        };
            }
        }

        [Theory, PropertyData("WithDifferentCities")]
        public void CityValueShouldReturnProperValue(IEnumerable<IStructure> structures, int expected)
        {
            var fixture = FixtureHelper.Create();

            var objectTypeFactory = fixture.Freeze<IObjectTypeFactory>();
            // Structures with id less than 100 count towards Influence, others dont
            objectTypeFactory.IsStructureType("NoInfluencePoint", Arg.Any<IStructure>())
                             .Returns(args => ((IStructure)args[1]).Type >= 100);

            var formula = fixture.Create<Formula>();

            var city = Substitute.For<ICity>();
            city.GetEnumerator().Returns(structures.GetEnumerator());

            formula.CalculateCityValue(city).Should().Be((ushort)expected);
        }

        private static IStructure CreateMockStructure(ushort type, byte level, byte size)
        {
            var structure = Substitute.For<IStructure>();
            structure.Type.Returns(type);
            structure.Lvl.Returns(level);
            structure.Size.Returns(size);

            return structure;
        }
    }
}