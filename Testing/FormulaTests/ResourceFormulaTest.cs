using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class ResourceFormulaTest
    {
        public static IEnumerable<object[]> HiddenResourceWithBasementsShouldProtectResourcesData
        {
            get
            {
                // No basements but has AP bonus
                yield return
                        new object[]
                        {
                                new byte[] {}, 75m, true, new Resource(100, 1000, 10000, 100000, 0),
                                new Resource(75, 750, 7500, 75000, 0)
                        };

                // Has level 0 basements
                yield return
                        new object[]
                        {
                                new byte[] {0, 0}, 0m, true, new Resource(1000, 1000, 1000, 1000, 0),
                                new Resource(0, 0, 0, 0, 0)
                        };

                // Has 2 basements and AP bonuses
                yield return
                        new object[]
                        {
                                new byte[] {1, 2}, 75m, true, new Resource(1000, 1000, 10000, 100000, 0),
                                new Resource(750, 750, 7500, 75000, 0)
                        };

                // Has 2 basements and no AP bonuses
                yield return
                        new object[]
                        {
                                new byte[] {1, 10}, 0m, true, new Resource(10000, 10000, 10000, 10000, 0),
                                new Resource(1200, 740, 660, 1200, 0)
                        };

                // Has basement higher than AP bonus
                yield return
                        new object[]
                        {
                                new byte[] {1, 10}, 90m, true, new Resource(100, 100, 100, 100, 0),
                                new Resource(1200, 740, 660, 1200, 0)
                        };

                // Has 2 basements and AP bonus but bonus should not be checked
                yield return
                        new object[]
                        {
                                new byte[] {1, 2}, 75m, false, new Resource(1000, 1000, 10000, 100000, 0),
                                new Resource(250, 0, 0, 250, 0)
                        };
            }
        }

        [Fact]
        public void HiddenResourceNoBasementsShouldNotProtectAnyResource()
        {
            var city = new Mock<ICity>();
            city.Setup(m => m.GetEnumerator())
                .Returns(() => new List<IStructure> {new Mock<IStructure>().Object}.GetEnumerator());
            var unitFactory = new Mock<UnitFactory>();
            var structureFactory = new Mock<StructureFactory>();

            var objectTypeFactory = new Mock<ObjectTypeFactory>();
            objectTypeFactory.Setup(m => m.IsStructureType("Basement", It.IsAny<IStructure>())).Returns(false);

            var formula = new Formula(objectTypeFactory.Object, unitFactory.Object, structureFactory.Object);

            formula.HiddenResource(city.Object).CompareTo(new Resource(0, 0, 0, 0, 0)).Should().Be(0);
            formula.HiddenResource(city.Object, true).CompareTo(new Resource(0, 0, 0, 0, 0)).Should().Be(0);
        }

        [Theory, PropertyData("HiddenResourceWithBasementsShouldProtectResourcesData")]
        public void HiddenResourceWithBasementsShouldProtectResources(byte[] basementLevels,
                                                                      decimal ap,
                                                                      bool checkApBonus,
                                                                      Resource cityResourceLimit,
                                                                      Resource expectedOutput)
        {
            var city = new Mock<ICity>();

            var basements = new List<IStructure>();
            foreach (byte basementLevel in basementLevels)
            {
                var basement = new Mock<IStructure>();
                basement.SetupGet(p => p.Lvl).Returns(basementLevel);
                basements.Add(basement.Object);
            }

            city.Setup(m => m.GetEnumerator()).Returns(() => basements.GetEnumerator());
            city.SetupGet(p => p.Resource.Crop.Limit).Returns(cityResourceLimit.Crop);
            city.SetupGet(p => p.Resource.Gold.Limit).Returns(cityResourceLimit.Gold);
            city.SetupGet(p => p.Resource.Wood.Limit).Returns(cityResourceLimit.Wood);
            city.SetupGet(p => p.Resource.Iron.Limit).Returns(cityResourceLimit.Iron);
            city.SetupGet(p => p.Resource.Labor.Limit).Returns(cityResourceLimit.Labor);
            city.SetupGet(p => p.AlignmentPoint).Returns(ap);

            var objectTypeFactory = new Mock<ObjectTypeFactory>();
            objectTypeFactory.Setup(m => m.IsStructureType("Basement", It.IsAny<IStructure>())).Returns(true);

            var unitFactory = new Mock<UnitFactory>();
            var structureFactory = new Mock<StructureFactory>();

            var formula = new Formula(objectTypeFactory.Object, unitFactory.Object, structureFactory.Object);

            var hiddenResources = formula.HiddenResource(city.Object, checkApBonus);

            hiddenResources.CompareTo(expectedOutput)
                           .Should()
                           .Be(0,
                               "Expected {0} but found {1}",
                               expectedOutput.ToNiceString(),
                               hiddenResources.ToNiceString());
        }
    }
}