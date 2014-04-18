using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.ActionsTests
{
    public class TribeContributeActiveActionTest
    {
        [Theory, AutoNSubstituteData]
        public void TestContributeNormal(
            ICity city,
            ICanDo canDo,
            IStructure structure,
            [FrozenMock] Formula formula,
            Fixture fixture)
        {
            var cityResources = new LazyResource(1000, 1000, 1000, 1000, 1000);
            var resource = new Resource(100, 20, 50, 80, 1);
            var capacity = new Resource(500, 500, 500, 500, 5);

            formula.GetContributeCapacity(structure).Returns(capacity);

            Resource contributedResources = null;
            city.Owner.Tribesman.Tribe.Contribute(123, Arg.Is<Resource>(contributed => contributed.CompareTo(resource) == 0))
                .Returns(Error.Ok)
                .AndDoes(callInfo =>
                {
                    contributedResources = new Resource(callInfo.Arg<Resource>());
                });

            city.Id.Returns<uint>(1);
            city.Resource.Returns(cityResources);

            structure.GroupId.Returns<uint>(1);
            structure.ObjectId.Returns<uint>(100);
            structure.City.Returns(city);

            city.Owner.IsInTribe.Returns(true);
            city.Owner.PlayerId.Returns<uint>(123);

            var locator = new GameObjectLocatorStub(city, city.Owner.Tribesman.Tribe, structure);

            var action = new TribeContributeActiveAction(1, 100, resource, locator, formula, new LockerStub(locator))
            {
                    WorkerObject = canDo, 
                    IsDone = false
            };

            // Execute/Verify
            action.Execute().Should().Be(Error.Ok);
            cityResources.Crop.Value.Should().Be(900);
            cityResources.Gold.Value.Should().Be(980);
            cityResources.Iron.Value.Should().Be(950);
            cityResources.Wood.Value.Should().Be(920);
            cityResources.Labor.Value.Should().Be(999);

            action.Callback(null);
            contributedResources.Should().NotBeNull("Should have contributed resources");
            contributedResources.Crop.Should().Be(100);
            contributedResources.Gold.Should().Be(20);
            contributedResources.Iron.Should().Be(50);
            contributedResources.Wood.Should().Be(80);
            contributedResources.Labor.Should().Be(1);
        }
        
        [Theory, AutoNSubstituteData]
        public void TestContributeNotEnoughResource(   
            ICity city,
            ICanDo canDo,
            IStructure structure,
            [Frozen] IGameObjectLocator locator,            
            [FrozenMock] Formula formula,
            Fixture fixture)
        {
            var cityResources = new LazyResource(50, 50, 50, 50, 50);
            var capacity = new Resource(500, 500, 500, 500, 5);
            var resource = new Resource(100, 20, 50, 80, 1);
            fixture.Register(() => resource);

            formula.GetContributeCapacity(structure).Returns(capacity);
            structure.City.Returns(city);
            city.Resource.Returns(cityResources);
            city.Owner.IsInTribe.Returns(true);
            
            ICity outCity;
            IStructure outStructure;
            locator.TryGetObjects(Arg.Any<uint>(), Arg.Any<uint>(), out outCity, out outStructure).Returns(callInfo =>
            {
                callInfo[2] = city;
                callInfo[3] = structure;
                return true;
            });
            
            fixture.PickGreedyConstructor<TribeContributeActiveAction>();
            var action = fixture.Create<TribeContributeActiveAction>();
            action.WorkerObject = canDo;
            action.IsDone = false;

            // Execute/Verify
            action.Execute().Should().Be(Error.ResourceNotEnough);

            cityResources.Crop.Value.Should().Be(50);
            cityResources.Gold.Value.Should().Be(50);
            cityResources.Iron.Value.Should().Be(50);
            cityResources.Wood.Value.Should().Be(50);
            cityResources.Labor.Value.Should().Be(50);
            
            city.Owner.Tribesman.Tribe.DidNotReceiveWithAnyArgs().Contribute(0, null);
        }
        
        [Theory, AutoNSubstituteData]
        public void TestContributeNoTribe(            
            ICity city,
            ICanDo canDo,
            IStructure structure,
            [Frozen] IGameObjectLocator locator,            
            [FrozenMock] Formula formula,
            Fixture fixture)
        {
            structure.City.Returns(city);
            formula.GetContributeCapacity(Arg.Any<IStructure>()).Returns(new Resource(1));
            city.Resource.HasEnough(Arg.Any<Resource>()).Returns(true);
            city.Owner.IsInTribe.Returns(false);
            fixture.Register(() => new Resource(1));
            
            ICity outCity;
            IStructure outStructure;
            locator.TryGetObjects(Arg.Any<uint>(), Arg.Any<uint>(), out outCity, out outStructure).Returns(callInfo =>
            {
                callInfo[2] = city;
                callInfo[3] = structure;
                return true;
            });

            fixture.PickGreedyConstructor<TribeContributeActiveAction>();
            var action = fixture.Create<TribeContributeActiveAction>();
            action.WorkerObject = canDo;
            action.IsDone = false;

            // Execute/Verify
            action.Execute().Should().Be(Error.TribesmanNotPartOfTribe);
        }
        /*
        [Fact]
        public void TestContributeUserCancel()
        {
            // Setup 
            var locator = Substitute.For<IGameObjectLocator>();
            var formula = Substitute.For<Formula>();
            var city = Substitute.For<ICity>();
            var structure = Substitute.For<IStructure>();
            var player = Substitute.For<IPlayer>();
            var tribe = Substitute.For<ITribe>();
            var locker = Substitute.For<ILocker>();
            var canDo = Substitute.For<ICanDo>();
            var cityResources = new LazyResource(1000, 1000, 1000, 1000, 1000);
            var resource = new Resource(100, 20, 50, 80, 1);
            var capacity = new Resource(500, 500, 500, 500, 5);
            // ReSharper disable RedundantAssignment
            var cityObj = city.Object;
            var structureObj = structure.Object;
            var tribeObj = tribe.Object;
            // ReSharper restore RedundantAssignment            
            var action = new TribeContributeActiveAction(1, 100, resource, locator.Object, formula.Object, locker.Object)
            {
                    WorkerObject = canDo.Object,
                    IsDone = false
            };
            var contributedResources = new Resource();

            locator.Setup(m => m.TryGetObjects(1, 100, out cityObj, out structureObj)).Returns(true);
            formula.Setup(m => m.GetContributeCapacity(structureObj)).Returns(capacity);
            formula.Setup(m => m.TradeTime(structureObj, resource)).Returns(300);
            formula.Setup(m => m.GetActionCancelResource(It.IsAny<DateTime>(), It.IsAny<Resource>())).Returns(resource);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            city.Setup(m => m.TryGetStructure(100, out structureObj)).Returns(true);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            locker.Setup(m => m.Lock(1, out cityObj, out tribeObj)).Returns((IMultiObjectLock)null);
            locker.Setup(m => m.Lock(1, out cityObj)).Returns((IMultiObjectLock)null);
            player.SetupGet(p => p.IsInTribe).Returns(true);
            player.SetupGet(p => p.PlayerId).Returns(123);
            tribe.Setup(m => m.Contribute(123, It.IsAny<Resource>())).Returns((uint inPlayerId, Resource inResource) =>
                {
                    contributedResources = new Resource(inResource);
                    return Error.Ok;
                });

            // Execute/Verify
            action.Execute().Should().Be(Error.Ok);
            cityResources.Crop.Value.Should().Be(900);
            cityResources.Gold.Value.Should().Be(980);
            cityResources.Iron.Value.Should().Be(950);
            cityResources.Wood.Value.Should().Be(920);
            cityResources.Labor.Value.Should().Be(999);

            action.UserCancelled();
            cityResources.Crop.Value.Should().Be(1000);
            cityResources.Gold.Value.Should().Be(1000);
            cityResources.Iron.Value.Should().Be(1000);
            cityResources.Wood.Value.Should().Be(1000);
            cityResources.Labor.Value.Should().Be(1000);
        }

        [Fact]
        public void TestContributeStructureDestroyed()
        {
            // Setup 
            var locator = Substitute.For<IGameObjectLocator>();
            var formula = Substitute.For<Formula>();
            var city = Substitute.For<ICity>();
            var structure = Substitute.For<IStructure>();
            var player = Substitute.For<IPlayer>();
            var tribe = Substitute.For<ITribe>();
            var locker = Substitute.For<ILocker>();
            var canDo = Substitute.For<ICanDo>();
            var cityResources = new LazyResource(1000, 1000, 1000, 1000, 1000);
            var resource = new Resource(100, 20, 50, 80, 1);
            var capacity = new Resource(500, 500, 500, 500, 5);
            // ReSharper disable RedundantAssignment
            var cityObj = city.Object;
            var structureObj = structure.Object;
            var tribeObj = tribe.Object;
            // ReSharper restore RedundantAssignment            
            var action = new TribeContributeActiveAction(1, 100, resource, locator.Object, formula.Object, locker.Object)
            {
                    WorkerObject = canDo.Object,
                    IsDone = false
            };
            var contributedResources = new Resource();

            locator.Setup(m => m.TryGetObjects(1, 100, out cityObj, out structureObj)).Returns(true);
            formula.Setup(m => m.GetContributeCapacity(structureObj)).Returns(capacity);
            formula.Setup(m => m.TradeTime(structureObj, resource)).Returns(300);
            formula.Setup(m => m.GetActionCancelResource(It.IsAny<DateTime>(), It.IsAny<Resource>())).Returns(resource);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            city.Setup(m => m.TryGetStructure(100, out structureObj)).Returns(true);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            locker.Setup(m => m.Lock(1, out cityObj, out tribeObj)).Returns((IMultiObjectLock)null);
            locker.Setup(m => m.Lock(1, out cityObj)).Returns((IMultiObjectLock)null);
            player.SetupGet(p => p.IsInTribe).Returns(true);
            player.SetupGet(p => p.PlayerId).Returns(123);
            tribe.Setup(m => m.Contribute(123, It.IsAny<Resource>())).Returns((uint inPlayerId, Resource inResource) =>
                {
                    contributedResources = new Resource(inResource);
                    return Error.Ok;
                });

            // Execute/Verify
            action.Execute().Should().Be(Error.Ok);
            cityResources.Crop.Value.Should().Be(900);
            cityResources.Gold.Value.Should().Be(980);
            cityResources.Iron.Value.Should().Be(950);
            cityResources.Wood.Value.Should().Be(920);
            cityResources.Labor.Value.Should().Be(999);

            action.WorkerRemoved(true);
            cityResources.Crop.Value.Should().Be(900);
            cityResources.Gold.Value.Should().Be(980);
            cityResources.Iron.Value.Should().Be(950);
            cityResources.Wood.Value.Should().Be(920);
            cityResources.Labor.Value.Should().Be(999);
        }
         */
    }
}