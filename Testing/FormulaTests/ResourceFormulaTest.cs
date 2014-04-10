using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
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

        [Theory, AutoNSubstituteData]
        public void HiddenResourceNoBasementsShouldNotProtectAnyResource(IObjectTypeFactory objectTypeFactory, Formula formula)
        {
            var city = Substitute.For<ICity>();
            city.GetEnumerator()
                .Returns(args => new List<IStructure> { Substitute.For<IStructure>() }.GetEnumerator());

            objectTypeFactory.IsStructureType("Basement", Arg.Any<IStructure>()).Returns(false);

            formula.HiddenResource(city).CompareTo(new Resource(0, 0, 0, 0, 0)).Should().Be(0);
            formula.HiddenResource(city, true).CompareTo(new Resource(0, 0, 0, 0, 0)).Should().Be(0);
        }

        [Theory, PropertyData("HiddenResourceWithBasementsShouldProtectResourcesData")]
        public void HiddenResourceWithBasementsShouldProtectResources(byte[] basementLevels,
                                                                      decimal ap,
                                                                      bool checkApBonus,
                                                                      Resource cityResourceLimit,
                                                                      Resource expectedOutput)
        {
            var fixture = FixtureHelper.Create();

            var city = Substitute.For<ICity>();

            var basements = new List<IStructure>();
            foreach (byte basementLevel in basementLevels)
            {
                var basement = Substitute.For<IStructure>();
                basement.Lvl.Returns(basementLevel);
                basements.Add(basement);
            }

            city.GetEnumerator().Returns(args => basements.GetEnumerator());
            city.Resource.Crop.Limit.Returns(cityResourceLimit.Crop);
            city.Resource.Gold.Limit.Returns(cityResourceLimit.Gold);
            city.Resource.Wood.Limit.Returns(cityResourceLimit.Wood);
            city.Resource.Iron.Limit.Returns(cityResourceLimit.Iron);
            city.Resource.Labor.Limit.Returns(cityResourceLimit.Labor);
            city.AlignmentPoint.Returns(ap);

            var objectTypeFactory = fixture.Freeze<IObjectTypeFactory>();
            objectTypeFactory.IsStructureType("Basement", Arg.Any<IStructure>()).Returns(true);

            var formula = fixture.Create<Formula>();

            var hiddenResources = formula.HiddenResource(city, checkApBonus);

            hiddenResources.CompareTo(expectedOutput)
                           .Should()
                           .Be(0,
                               "Expected {0} but found {1}",
                               expectedOutput.ToNiceString(),
                               hiddenResources.ToNiceString());
        }


        public static IEnumerable<object[]> HiddenResourceWithTemporaryBasementsShouldProtectResourcesData
        {
            get
            {
                // Has 1 temporary basement
                yield return
                        new object[]
                        {
                            new byte[] {10}, 0m, false, new Resource(1000, 1000, 10000, 100000),
                            new Resource(1700, 1000, 1000, 1700)
                        };
                // Has multiple temporary basement
                yield return
                        new object[]
                        {
                            new byte[] {8,9,10}, 0m, false, new Resource(1000, 1000, 10000, 100000),
                            new Resource(4800, 2700, 2250, 4800)
                        };

            }
        }

        [Theory, PropertyData("HiddenResourceWithTemporaryBasementsShouldProtectResourcesData")]
        public void HiddenResourceWithTemporaryBasementsShouldProtectResources(byte[] basementLevels,
                                                                      decimal ap,
                                                                      bool checkApBonus,
                                                                      Resource cityResourceLimit,
                                                                      Resource expectedOutput)
        {
            var fixture = FixtureHelper.Create();

            var city = Substitute.For<ICity>();

            var basements = new List<IStructure>();
            foreach (byte basementLevel in basementLevels)
            {
                var basement = Substitute.For<IStructure>();
                basement.Lvl.Returns(basementLevel);
                basements.Add(basement);
            }

            city.GetEnumerator().Returns(args => basements.GetEnumerator());
            city.Resource.Crop.Limit.Returns(cityResourceLimit.Crop);
            city.Resource.Gold.Limit.Returns(cityResourceLimit.Gold);
            city.Resource.Wood.Limit.Returns(cityResourceLimit.Wood);
            city.Resource.Iron.Limit.Returns(cityResourceLimit.Iron);
            city.Resource.Labor.Limit.Returns(cityResourceLimit.Labor);
            city.AlignmentPoint.Returns(ap);

            var objectTypeFactory = fixture.Freeze<IObjectTypeFactory>();
            objectTypeFactory.IsStructureType("TempBasement", Arg.Any<IStructure>()).Returns(true);

            var formula = fixture.Create<Formula>();

            var hiddenResources = formula.HiddenResource(city, checkApBonus);

            hiddenResources.CompareTo(expectedOutput)
                           .Should()
                           .Be(0,
                               "Expected {0} but found {1}",
                               expectedOutput.ToNiceString(),
                               hiddenResources.ToNiceString());
        }
    }
}