using System;
using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data.Stronghold;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NSubstitute;
using Persistance;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.StrongholdTests
{
    public class StrongholdActivaitonCheckerTests : IDisposable
    {
        public StrongholdActivaitonCheckerTests()
        {
            SystemClock.SetClock(DateTime.UtcNow);
        }
        
        public void Dispose()
        {
            SystemClock.ResyncClock();
        }

        [Theory, AutoNSubstituteData]
        public void Callback_WhenDoesntHaveNeutralStronghold_ShouldActivateStrongholdWithHighestScore(
            [Frozen] ISystemVariableManager systemVariableManager,
            [Frozen] IStrongholdActivationCondition activationCondition,
            [Frozen] IStrongholdManager strongholdManager,
            IStronghold sh1,
            IStronghold sh2,
            IFixture fixture)
        {
            sh1.StrongholdState.Returns(StrongholdState.Inactive);
            sh2.StrongholdState.Returns(StrongholdState.Inactive);

            var lastCheckTime = Substitute.For<SystemVariable>();
            lastCheckTime.Value.Returns(SystemClock.Now.AddHours(-8));
            systemVariableManager["Stronghold.neutral_check"].Returns(lastCheckTime);

            activationCondition.Score(sh1).Returns(10);
            activationCondition.Score(sh2).Returns(20);

            var locker = new LockerStub();
            fixture.Register<ILocker>(() => locker);

            strongholdManager.GetEnumerator().Returns(_ => new List<IStronghold> {sh1, sh2}.GetEnumerator());

            var checker = fixture.Create<StrongholdActivationChecker>();
            checker.Callback(null);

            strongholdManager.Received(1).Activate(sh2);
            strongholdManager.DidNotReceive().Activate(sh1);
        }

        [Theory, AutoNSubstituteData]
        public void Callback_WhenHasNeutralStronghold_ShouldNotActivateAStronghold(
            [Frozen] ISystemVariableManager systemVariableManager,
            [Frozen] IStrongholdActivationCondition activationCondition,
            [Frozen] IStrongholdManager strongholdManager,
            IStronghold sh1,
            IStronghold sh2,
            IFixture fixture)
        {
            sh1.StrongholdState.Returns(StrongholdState.Inactive);
            sh2.StrongholdState.Returns(StrongholdState.Neutral);

            var lastCheckTime = Substitute.For<SystemVariable>();
            lastCheckTime.Value.Returns(SystemClock.Now.AddHours(-8));
            systemVariableManager["Stronghold.neutral_check"].Returns(lastCheckTime);
            
            var locker = new LockerStub();
            fixture.Register<ILocker>(() => locker);

            strongholdManager.GetEnumerator().Returns(_ => new List<IStronghold> {sh1, sh2}.GetEnumerator());

            var checker = fixture.Create<StrongholdActivationChecker>();
            checker.Callback(null);

            strongholdManager.DidNotReceive().Activate(sh2);
            strongholdManager.DidNotReceive().Activate(sh1);
        }

        [Theory, AutoNSubstituteData]
        public void Callback_WhenInactiveStrongholdsHaveSameScore_ShouldActivateClosestToCenter(
            [Frozen] ISystemVariableManager systemVariableManager,
            [Frozen] IStrongholdActivationCondition activationCondition,
            [Frozen] IStrongholdManager strongholdManager,
            [Frozen] ITileLocator tileLocator,
            IStronghold sh1,
            IStronghold sh2,
            IFixture fixture)
        {
            sh1.StrongholdState.Returns(StrongholdState.Inactive);
            sh1.PrimaryPosition.Returns(new Position(10, 20));

            sh2.StrongholdState.Returns(StrongholdState.Inactive);
            sh2.PrimaryPosition.Returns(new Position(30, 40));
            
            activationCondition.Score(null).ReturnsForAnyArgs(0);

            tileLocator.TileDistance(sh1.PrimaryPosition, 1, Arg.Any<Position>(), 1)
                       .Returns(10);

            tileLocator.TileDistance(sh2.PrimaryPosition, 1, Arg.Any<Position>(), 1)
                       .Returns(5);

            var lastCheckTime = Substitute.For<SystemVariable>();
            lastCheckTime.Value.Returns(SystemClock.Now.AddHours(-8));
            systemVariableManager["Stronghold.neutral_check"].Returns(lastCheckTime);
            
            var locker = new LockerStub();
            fixture.Register<ILocker>(() => locker);

            strongholdManager.GetEnumerator().Returns(_ => new List<IStronghold> {sh1, sh2}.GetEnumerator());

            var checker = fixture.Create<StrongholdActivationChecker>();
            checker.Callback(null);

            strongholdManager.Received().Activate(sh2);
            strongholdManager.DidNotReceive().Activate(sh1);
        }

        [Theory, AutoNSubstituteData]
        public void Callback_WhenNoStrongholdsAreInactive_ShouldNotThrowExceptionAndShouldUpdateNeutralCheckTime(
            [Frozen] ISystemVariableManager systemVariableManager,
            [Frozen] IStrongholdManager strongholdManager,
            [Frozen] IDbManager dbManager,
            IStronghold sh1,
            IStronghold sh2,
            IFixture fixture)
        {
            sh1.StrongholdState.Returns(StrongholdState.Occupied);
            sh2.StrongholdState.Returns(StrongholdState.Occupied);
            
            var lastCheckTime = Substitute.For<SystemVariable>();
            lastCheckTime.Value.Returns(SystemClock.Now.AddHours(-8));
            systemVariableManager["Stronghold.neutral_check"].Returns(lastCheckTime);
            
            var locker = new LockerStub();
            fixture.Register<ILocker>(() => locker);

            strongholdManager.GetEnumerator().Returns(_ => new List<IStronghold> {sh1, sh2}.GetEnumerator());

            var checker = fixture.Create<StrongholdActivationChecker>();

            checker.Invoking(c => c.Callback(null)).ShouldNotThrow();
            
            lastCheckTime.Received().Value = SystemClock.Now;
            dbManager.Received(1).Save(lastCheckTime);
        }

        [Theory, AutoNSubstituteData]
        public void Callback_WhenHasBeenLessThan8HoursSinceLastNeutralActivation_ShouldNotActivateNewStronghold(
            [Frozen] ISystemVariableManager systemVariableManager,
            [Frozen] IStrongholdManager strongholdManager,
            IStronghold sh1,
            IFixture fixture)
        {
            sh1.StrongholdState.Returns(StrongholdState.Inactive);
            
            var lastCheckTime = Substitute.For<SystemVariable>();
            lastCheckTime.Value.Returns(SystemClock.Now.AddHours(-7.9));
            systemVariableManager["Stronghold.neutral_check"].Returns(lastCheckTime);
            
            var locker = new LockerStub();
            fixture.Register<ILocker>(() => locker);

            strongholdManager.GetEnumerator().Returns(_ => new List<IStronghold> {sh1}.GetEnumerator());

            var checker = fixture.Create<StrongholdActivationChecker>();

            checker.Callback(null);

            strongholdManager.DidNotReceive().Activate(sh1);
        }
    }
}