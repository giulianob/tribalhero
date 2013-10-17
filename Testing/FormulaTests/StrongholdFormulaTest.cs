using System;
using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data.Stronghold;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Xunit;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class StrongholdFormulaTest : IDisposable
    {
        public StrongholdFormulaTest()
        {
            SystemClock.SetClock(new DateTime(2013, 1, 1, 1, 1, 1, 1));            
        }

        public void Dispose()
        {
            SystemClock.ResyncClock();
        }

        [Theory]
        [InlineData(1, 500)]
        [InlineData(2, 600)]
        [InlineData(3, 700)]
        [InlineData(4, 800)]
        [InlineData(5, 950)]
        [InlineData(6, 1100)]
        [InlineData(7, 1250)]
        [InlineData(8, 1450)]
        [InlineData(9, 1650)]
        [InlineData(10, 1850)]
        [InlineData(11, 2100)]
        [InlineData(12, 2350)]
        [InlineData(13, 2600)]
        [InlineData(14, 2900)]
        [InlineData(15, 3200)]
        [InlineData(16, 3500)]
        [InlineData(17, 3850)]
        [InlineData(18, 4200)]
        [InlineData(19, 4600)]
        [InlineData(20, 5000)]
        public void TestMeterValue(int level, int expectedValue)
        {
            var systemVariableManager = Substitute.For<ISystemVariableManager>();
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue);
            formula.StrongholdMainBattleMeter((byte)level).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 5000)]
        [InlineData(2, 6000)]
        [InlineData(3, 7000)]
        [InlineData(4, 8000)]
        [InlineData(5, 9500)]
        [InlineData(6, 11000)]
        [InlineData(7, 12500)]
        [InlineData(8, 14500)]
        [InlineData(9, 16500)]
        [InlineData(10, 18500)]
        [InlineData(11, 21000)]
        [InlineData(12, 23500)]
        [InlineData(13, 26000)]
        [InlineData(14, 29000)]
        [InlineData(15, 32000)]
        [InlineData(16, 35000)]
        [InlineData(17, 38500)]
        [InlineData(18, 42000)]
        [InlineData(19, 46000)]
        [InlineData(20, 50000)]
        public void TestGateBaseValue(int level, int expectedValue)
        {
            var systemVariableManager = Substitute.For<ISystemVariableManager>();
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue);
            formula.StrongholdGateLimit((byte)level).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 1, 400)]
        [InlineData(2, 1, 480)]
        [InlineData(3, 1, 560)]
        [InlineData(4, 2, 640)]
        [InlineData(5, 2, 760)]
        [InlineData(6, 3, 880)]
        [InlineData(7, 3, 1000)]
        [InlineData(8, 4, 1160)]
        [InlineData(9, 4, 1320)]
        [InlineData(10, 5, 1480)]
        [InlineData(11, 5, 1680)]
        [InlineData(12, 6, 1880)]
        [InlineData(13, 6, 2080)]
        [InlineData(14, 7, 2320)]
        [InlineData(15, 7, 2560)]
        [InlineData(16, 8, 2800)]
        [InlineData(17, 8, 3080)]
        [InlineData(18, 9, 3360)]
        [InlineData(19, 9, 3680)]
        [InlineData(20, 10, 4000)]
        public void TestUpkeepValue(int level, int expectedLevel, int expectedUpkeep)
        {
            var systemVariableManager = Substitute.For<ISystemVariableManager>();
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue);

            byte unitLevel;
            int upkeep;

            formula.StrongholdUpkeep((byte)level, out upkeep, out unitLevel);
            unitLevel.Should().Be((byte)expectedLevel);
            upkeep.Should().Be(expectedUpkeep);
        }


        public static IEnumerable<object[]> VictoryPointItems
        {
            get
            {
                // no bonus
                yield return new object[] { (byte)1, 5m, 0m, 0m, 125m };
                yield return new object[] { (byte)1, 60m, 0m, 0m, 400m };
                // with bonus
                yield return new object[] { (byte)5, 5m, 2.5m, 0m, 412.50m };
                // level 20
                yield return new object[] { (byte)20, 5m, 2.5m, 0m, 1443.75m };
                // level 20 with 18 server days
                yield return new object[] { (byte)20, 5m, 2.5m, 18m, 1821.75m };
                // level 8 with 40.3 server days
                yield return new object[] { (byte)8, 2.5m, 5m, 40.3m, 981.45m };
            }
        }

        [Theory, PropertyData("VictoryPointItems")]
        public void TestVictoryPoint(byte level, decimal occupiedDays, decimal bonusDays, decimal serverDays, decimal expectedValue)
        {
            var serverDate = SystemClock.Now.Subtract(TimeSpan.FromDays((double)serverDays));

            var systemVariableManager = Substitute.For<ISystemVariableManager>();
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", serverDate));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
            
            var stronghold = Substitute.For<IStronghold>();
            stronghold.StrongholdState.Returns(StrongholdState.Occupied);
            stronghold.Lvl.Returns(level);
            stronghold.BonusDays.Returns(bonusDays);
            var occupiedDate = SystemClock.Now.Subtract(TimeSpan.FromDays((double)occupiedDays));
            stronghold.DateOccupied.Returns(occupiedDate);

            var formula = fixture.Create<Formula>();
            
            formula.StrongholdVictoryPoint(stronghold).Should().Be(expectedValue);
        }

        [Fact]
        public void TestNonOccupiedVictoryPoint()
        {
            var formula = FixtureHelper.Create().Create<Formula>();

            var stronghold = Substitute.For<IStronghold>();
            stronghold.StrongholdState.Returns(StrongholdState.Neutral);
            stronghold.Lvl.Returns((byte)5);
            stronghold.BonusDays.Returns(10);
            stronghold.DateOccupied.Returns(SystemClock.Now.Subtract(TimeSpan.FromDays(10)));

            formula.StrongholdVictoryPoint(stronghold).Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdGateLimit_WhenLessThanOrEqualTo30Days(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30,  0, 0, 0)));
            formula.StrongholdGateLimit(1).Should().Be(5000);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdGateLimit_WhenAt30Days1Hour(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1, 0, 0)));
            formula.StrongholdGateLimit(1).Should().Be(5010);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdGateLimit_WhenLessThanOrEqualTo30Days1040Hours(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1040 , 0, 0))); 
            formula.StrongholdGateLimit(1).Should().Be(39940);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdGateLimit_WhenMoreThan30Days1041Hours(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1041, 0, 0)));
            formula.StrongholdGateLimit(1).Should().Be(40000);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdGateLimit_WhenNegativeTime(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MaxValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MaxValue.Subtract(new TimeSpan(30, 0, 0, 0)));
            formula.StrongholdGateLimit(1).Should().Be(5000);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdGateLimit_WhenLessThanOrEqualTo30Days1040HoursAtLevel20(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1040, 0, 0)));
            formula.StrongholdGateLimit(1).Should().Be(39940);
            formula.StrongholdGateLimit(20).Should().Be(399390);
        }
        [Theory, AutoNSubstituteData]
        public void StrongholdMainBattleMeter_WhenLessThanOrEqualTo30Days(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 0, 0, 0)));
            formula.StrongholdMainBattleMeter(1).Should().Be(500);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdMainBattleMeter_WhenAt30Days1Hour(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1, 0, 0)));
            formula.StrongholdMainBattleMeter(1).Should().Be(501);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdMainBattleMeter_WhenLessThanOrEqualTo30Days1040Hours(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1040, 0, 0)));
            formula.StrongholdMainBattleMeter(1).Should().Be(3994);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdMainBattleMeter_WhenMoreThan30Days1041Hours(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1041, 0, 0)));
            formula.StrongholdMainBattleMeter(1).Should().Be(4000);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdMainBattleMeter_WhenNegativeTime(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MaxValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MaxValue.Subtract(new TimeSpan(30, 0, 0, 0)));
            formula.StrongholdMainBattleMeter(1).Should().Be(500);
        }

        [Theory, AutoNSubstituteData]
        public void StrongholdMainBattleMeter_WhenLessThanOrEqualTo30Days1040HoursAtLevel20(ISystemVariableManager systemVariableManager)
        {
            systemVariableManager["Server.date"].Returns(new SystemVariable("Server.date", DateTime.MinValue));

            var fixture = FixtureHelper.Create();
            fixture.Register(() => systemVariableManager);
            fixture.Customize<Formula>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var formula = fixture.Create<Formula>();
            SystemClock.SetClock(DateTime.MinValue.Add(new TimeSpan(30, 1040, 0, 0)));
            formula.StrongholdMainBattleMeter(1).Should().Be(3994);
            formula.StrongholdMainBattleMeter(20).Should().Be(39939);
        }
    }
}
