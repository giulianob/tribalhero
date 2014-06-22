using Common.Testing;
using FluentAssertions;
using Game.Data.Troop;
using Game.Data.Troop.Initializers;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.TroopTests
{
    public class StationedTroopObjectInitializerTests
    {
        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenStubIsNotStationed_ReturnsError(
                [Frozen] ITroopStub stub,
                StationedTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.BattleStationed);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.TroopNotStationed);
            troopObject.Should().BeNull();
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenCannotRemoveFromStationedCity_ReturnsError(
                [Frozen] ITroopStub stub,
                StationedTroopObjectInitializer troopInitializer)
        {
            stub.StationTroopId = 100;
            stub.State.Returns(TroopState.Stationed);
            stub.Station.Troops.RemoveStationed(100).Returns(false);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.TroopNotStationed);
            troopObject.Should().BeNull();
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_CreatesTroopObjectFromCity(
                [Frozen] ITroopStub stub,
                StationedTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            
            stub.Station.Troops.RemoveStationed(Arg.Any<ushort>()).Returns(true);

            stub.Station.PrimaryPosition.Returns(new Position(10, 20));

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            troopObject.Should().NotBeNull();
            stub.City.Received(1).CreateTroopObject(stub, 10, 21);
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_AddsNewTroopObjectToWorld(
                [Frozen] ITroopStub stub,
                [Frozen] IWorld world,
                StationedTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.Station.Troops.RemoveStationed(Arg.Any<ushort>()).Returns(true);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            world.Regions.Received(1).Add(troopObject);
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_LoadsStats(
                [Frozen] ITroopStub stub,                
                IFixture fixture)
        {
            var formula = Substitute.For<Formula>();
            formula.GetTroopRadius(stub, null).Returns<byte>(34);
            formula.GetTroopSpeed(stub).Returns(44);

            fixture.Register(() => formula);
            stub.State.Returns(TroopState.Stationed);
            stub.Station.Troops.RemoveStationed(Arg.Any<ushort>()).Returns(true);

            var troopInitializer = fixture.Create<StationedTroopObjectInitializer>();

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            troopObject.Stats.Speed.Should().Be(44);
            troopObject.Stats.AttackRadius.Should().Be(34);
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenCalledTwice_ReturnsSameInstance(
                [Frozen] ITroopStub stub,                
                StationedTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.Station.Troops.RemoveStationed(Arg.Any<ushort>()).Returns(true);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            ITroopObject troopObject2;
            troopInitializer.GetTroopObject(out troopObject2).Should().Be(Error.Ok);

            troopObject.Should().BeSameAs(troopObject2);
        }

        [Theory, AutoNSubstituteData]
        public void DeleteTroopObject_WhenTroopObjectCreated_StationsInOriginalLocation(
                [Frozen] ITroopStub stub,                
                IFixture fixture)
        {
            var procedure = Substitute.For<Procedure>();                       
            fixture.Register(() => procedure);

            stub.State.Returns(TroopState.Stationed);
            stub.Station.Troops.RemoveStationed(Arg.Any<ushort>()).Returns(true);

            var troopInitializer = fixture.Create<StationedTroopObjectInitializer>();

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject);

            troopInitializer.DeleteTroopObject();

            procedure.Received(1).TroopObjectStation(troopObject, stub.Station);
        }
    }
}