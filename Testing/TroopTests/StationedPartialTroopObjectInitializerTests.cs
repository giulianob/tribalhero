using Common.Testing;
using FluentAssertions;
using Game.Data.Troop;
using Game.Data.Troop.Initializers;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.TroopTests
{
    public class StationedPartialTroopObjectInitializerTests
    {
        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenStubIsNotStationed_ReturnsError(
                [Frozen] ITroopStub stub,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.BattleStationed);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.TroopNotStationed);
            troopObject.Should().BeNull();
        }
        
        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenCannotRemoveFromExistingTroop_ReturnsError(
                [Frozen] ITroopStub stub,
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, unitsToRetreat).Returns(false);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.TroopChanged);
            troopObject.Should().BeNull();
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_AddsNewTroopObjectToCity(
                [Frozen] ITroopStub stub,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            stub.City.Troops.Create().Returns(newStub);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);            
            stub.City.Received(1).Add(troopObject);
            ((object)troopObject.Stub).Should().BeSameAs(newStub);
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_AddsUnitsToNewStub(
                [Frozen] ITroopStub stub,
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, unitsToRetreat).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            stub.City.Troops.Create().Returns(newStub);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            newStub.Received(1).BeginUpdate();
            newStub.Received(1).Add(unitsToRetreat);
            newStub.Received(1).EndUpdate();
        }
        
        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_AddsNewTroopObjectToWorld(
                [Frozen] ITroopStub stub,
                [Frozen] IWorld world,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            stub.Station.X.Returns<uint>(10);
            stub.Station.Y.Returns<uint>(20);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            world.Regions.Received(1).Add(troopObject);
            troopObject.X.Should().Be(10);
            troopObject.Y.Should().Be(21);
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
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);
            
            var troopInitializer = fixture.Create<StationedPartialTroopObjectInitializer>();

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            troopObject.Stats.Speed.Should().Be(44);
            troopObject.Stats.AttackRadius.Should().Be(34);
        }
        
        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenCalledTwice_ReturnsSameInstance(
                [Frozen] ITroopStub stub,                
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            ITroopObject troopObject2;
            troopInitializer.GetTroopObject(out troopObject2).Should().Be(Error.Ok);

            troopObject.Should().BeSameAs(troopObject2);
        }
        
        [Theory, AutoNSubstituteData]
        public void DeleteTroopObject_WhenTroopObjectCreated_AddsUnitsBackToOriginalTroop(
                [Frozen] ITroopStub stub,                
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);
            stub.City.Troops.Remove(Arg.Any<ushort>()).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            stub.City.Troops.Create().Returns(newStub);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject);

            troopInitializer.DeleteTroopObject();

            stub.Received(1).BeginUpdate();
            stub.Received(1).AddAllToFormation(FormationType.Defense, newStub);
            stub.Received(1).EndUpdate();
        }           

        [Theory, AutoNSubstituteData]
        public void DeleteTroopObject_WhenTroopObjectCreated_RemovedFromWorld(
                [Frozen] ITroopStub stub,                
                [Frozen] IWorld world,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);
            stub.City.Troops.Remove(Arg.Any<ushort>()).Returns(true);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject);

            troopInitializer.DeleteTroopObject();

            world.Regions.Received(1).Remove(troopObject);
        }  

        [Theory, AutoNSubstituteData]
        public void DeleteTroopObject_WhenTroopObjectCreated_ScheduleRemovesTheTroopObject(
                [Frozen] ITroopStub stub,                
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);
            stub.City.Troops.Remove(Arg.Any<ushort>()).Returns(true);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject);

            troopInitializer.DeleteTroopObject();

            stub.City.Received(1).ScheduleRemove(troopObject, false);
        }        
        
        [Theory, AutoNSubstituteData]
        public void DeleteTroopObject_WhenTroopObjectFailsToRemoveFromCity_DoesNotAddUnitsBackToStub(
                [Frozen] ITroopStub stub,                
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            newStub.TroopId.Returns<ushort>(5);
            stub.City.Troops.Create().Returns(newStub);

            stub.City.Troops.Remove(newStub.TroopId).Returns(false);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject);

            troopInitializer.DeleteTroopObject();

            stub.DidNotReceive().AddAllToFormation(FormationType.Defense, newStub);
        }        


    }
}