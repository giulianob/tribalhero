using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Data;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.ActionsTests
{
    public class CityPassiveActionTest
    {
        public static IEnumerable<object[]> TestAlignmentPointsData
        {
            get
            {
                // When neutral value
                yield return new object[] {50m, 1, 50m};
                yield return new object[] {50m, 2, 50m};

                // When close to neutral
                yield return new object[] {49.9m, 1, 50m};
                yield return new object[] {49.9m, 2, 50m};
                yield return new object[] {50.1m, 1, 50m};

                // When below
                yield return new object[] {20m, 1, 20.125m};
                yield return new object[] {20m, 2, 20.250m};

                // When above
                yield return new object[] {60.250m, 2, 60m};
            }
        }

        [Theory, PropertyData("TestAlignmentPointsData")]
        public void TestAlignmentPoints(decimal initialValue, int cycles, decimal expectedValue)
        {
            // Config vars
            Config.actions_skip_city_actions = false;
            Config.troop_starve = false;

            // Player
            var player = Substitute.For<IPlayer>();

            // TechManager
            var technologies = Substitute.For<ITechnologyManager>();
            technologies.GetEffects(Arg.Any<EffectCode>()).ReturnsForAnyArgs(x => new List<Effect>());

            // City
            var city = Substitute.For<ICity>();
            city.AlignmentPoint = initialValue;
            // Not necessarily related to this test
            city.Id.Returns((uint)1);
            city.Owner.Returns(player);
            city.Resource.Returns(new LazyResource(1, 1, 1, 1, 1));
            city.GetEnumerator().Returns(x => new List<IStructure>().GetEnumerator());
            city.Technologies.Returns(technologies);

            ICity cityObj;
            // Locker
            var locker = Substitute.For<ILocker>();
            locker.Lock(1, out cityObj).Returns(x =>
                {
                    x[1] = city;
                    return Substitute.For<IMultiObjectLock>();
                });

            CityPassiveAction action = new CityPassiveAction(1,
                                                             Substitute.For<ObjectTypeFactory>(),
                                                             locker,
                                                             Substitute.For<Formula>(),
                                                             Substitute.For<IActionFactory>(),
                                                             Substitute.For<Procedure>(),
                                                             Substitute.For<IGameObjectLocator>(),
                                                             Substitute.For<IBattleFormulas>())
            {
                WorkerObject = city
            };

            for (int i = 0; i < cycles; i++)
            {
                action.Callback(null);
            }

            city.AlignmentPoint.Should().Be(expectedValue);
        }
    }
}