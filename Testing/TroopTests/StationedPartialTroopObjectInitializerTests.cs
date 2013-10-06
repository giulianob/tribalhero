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
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
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
            unitsToRetreat.TotalCount.Returns<ushort>(1);
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, unitsToRetreat).Returns(false);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.TroopChanged);
            troopObject.Should().BeNull();
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenUnitsToRetreatIsEmpty_ReturnsError(
                [Frozen] ITroopStub stub,
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            stub.State.Returns(TroopState.Stationed);
            unitsToRetreat.TotalCount.Returns<ushort>(0);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.TroopEmpty);
            troopObject.Should().BeNull();
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_AddsNewTroopObjectToCity(
                [Frozen] ITroopStub stub,
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            stub.Station.PrimaryPosition.X.Returns<uint>(10);
            stub.Station.PrimaryPosition.Y.Returns<uint>(20);

            var newStub = Substitute.For<ITroopStub>();
            stub.City.CreateTroopStub().Returns(newStub);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);            
            stub.City.Received(1).CreateTroopObject(newStub, 10, 21);
        }

        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_AddsUnitsToNewStub(
                [Frozen] ITroopStub stub,
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, unitsToRetreat).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            stub.City.CreateTroopStub().Returns(newStub);

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
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);

            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject).Should().Be(Error.Ok);
            world.Regions.Received(1).Add(troopObject);            
        }
        
        [Theory, AutoNSubstituteData]
        public void GetTroopObject_WhenAbleToRetreat_LoadsStats(
                [Frozen] ITroopStub stub,                
                [Frozen] ISimpleStub unitsToRetreat,
                IFixture fixture)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);

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
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);

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
                [Frozen] ISimpleStub unitsToRetreat,                
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);
            stub.City.Troops.Remove(Arg.Any<ushort>()).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            stub.City.CreateTroopStub().Returns(newStub);          

            ITroopObject troopObject = Substitute.For<ITroopObject>();
            troopObject.Stub.Returns(newStub);
            stub.City.CreateTroopObject(newStub, Arg.Any<uint>(), Arg.Any<uint>()).Returns(troopObject);

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
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
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
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
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
                [Frozen] ISimpleStub unitsToRetreat,
                StationedPartialTroopObjectInitializer troopInitializer)
        {
            unitsToRetreat.TotalCount.Returns<ushort>(1);
            stub.State.Returns(TroopState.Stationed);
            stub.RemoveFromFormation(FormationType.Defense, Arg.Any<ISimpleStub>()).Returns(true);

            var newStub = Substitute.For<ITroopStub>();
            newStub.TroopId.Returns<ushort>(5);
            stub.City.CreateTroopStub().Returns(newStub);

            stub.City.Troops.Remove(newStub.TroopId).Returns(false);

            ITroopObject troopObject;
            troopInitializer.GetTroopObject(out troopObject);

            troopInitializer.DeleteTroopObject();

            stub.DidNotReceiveWithAnyArgs().AddAllToFormation(FormationType.Defense, null);
        }        


    }
}