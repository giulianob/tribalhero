using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Game.Data;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util.Locking;
using Moq;
using Xunit.Extensions;

namespace Testing.Actions
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
            Mock<IPlayer> player = new Mock<IPlayer>();

            // TechManager
            Mock<ITechnologyManager> technologies = new Mock<ITechnologyManager>();
            technologies.Setup(p => p.GetEffects(It.IsAny<EffectCode>(), It.IsAny<EffectInheritance>()))
                        .Returns(() => new List<Effect>());

            // City
            Mock<ICity> city = new Mock<ICity>();
            city.SetupProperty(p => p.AlignmentPoint, initialValue);
            // Not necessarily related to this test
            city.SetupGet(p => p.Id).Returns(1);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            city.SetupGet(p => p.Resource).Returns(new LazyResource(1, 1, 1, 1, 1));
            city.Setup(p => p.GetEnumerator()).Returns(() => new IStructure[] {}.AsEnumerable().GetEnumerator());
            city.SetupGet(p => p.Technologies).Returns(technologies.Object);

            // ReSharper disable RedundantAssignment
            ICity cityObj = city.Object;
            // ReSharper restore RedundantAssignment

            // Locker
            Mock<ILocker> locker = new Mock<ILocker>();
            locker.Setup(p => p.Lock(1, out cityObj)).Returns(() => new Mock<IMultiObjectLock>().Object);

            // Formula
            Mock<Formula> formula = new Mock<Formula>();

            // Object Type Factory
            Mock<ObjectTypeFactory> objectTypeFactory = new Mock<ObjectTypeFactory>();

            CityPassiveAction action = new CityPassiveAction(1,
                                                             objectTypeFactory.Object,
                                                             locker.Object,
                                                             formula.Object,
                                                             new Mock<IActionFactory>().Object);
            action.WorkerObject = cityObj;

            for (int i = 0; i < cycles; i++)
            {
                action.Callback(null);
            }

            city.Object.AlignmentPoint.Should().Be(expectedValue);
        }
    }
}