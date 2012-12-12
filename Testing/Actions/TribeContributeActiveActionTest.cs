using System;
using FluentAssertions;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Moq;
using Xunit;

namespace Testing.Actions
{
    public class TribeContributeActiveActionTest
    {
        [Fact]
        public void TestContributeNormal()
        {
            // Setup 
            var locator = new Mock<IGameObjectLocator>();
            var formula = new Mock<Formula>();
            var city = new Mock<ICity>();
            var structure = new Mock<IStructure>();
            var player = new Mock<IPlayer>();
            var tribe = new Mock<ITribe>();
            var locker = new Mock<ILocker>();
            var canDo = new Mock<ICanDo>();
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
            formula.Setup(m => m.GetSendCapacity(structureObj)).Returns(capacity);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            locker.Setup(m => m.Lock(1, out cityObj, out tribeObj)).Returns((CallbackLock)null);
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

            action.Callback(null);
            contributedResources.Crop.Should().Be(100);
            contributedResources.Gold.Should().Be(20);
            contributedResources.Iron.Should().Be(50);
            contributedResources.Wood.Should().Be(80);
            contributedResources.Labor.Should().Be(1);
        }

        [Fact]
        public void TestContributeNotEnoughResource()
        {
            // Setup 
            var locator = new Mock<IGameObjectLocator>();
            var formula = new Mock<Formula>();
            var city = new Mock<ICity>();
            var structure = new Mock<IStructure>();
            var player = new Mock<IPlayer>();
            var tribe = new Mock<ITribe>();
            var locker = new Mock<ILocker>();
            var canDo = new Mock<ICanDo>();
            var cityResources = new LazyResource(50, 50, 50, 50, 50);
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

            locator.Setup(m => m.TryGetObjects(1, 100, out cityObj, out structureObj)).Returns(true);
            formula.Setup(m => m.GetSendCapacity(structureObj)).Returns(capacity);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            player.SetupGet(p => p.IsInTribe).Returns(true);
            player.SetupGet(p => p.PlayerId).Returns(123);

            // Execute/Verify
            action.Execute().Should().Be(Error.ResourceNotEnough);
            cityResources.Crop.Value.Should().Be(50);
            cityResources.Gold.Value.Should().Be(50);
            cityResources.Iron.Value.Should().Be(50);
            cityResources.Wood.Value.Should().Be(50);
            cityResources.Labor.Value.Should().Be(50);
        }

        [Fact]
        public void TestContributeNoTribe()
        {
            // Setup 
            var locator = new Mock<IGameObjectLocator>();
            var formula = new Mock<Formula>();
            var city = new Mock<ICity>();
            var structure = new Mock<IStructure>();
            var player = new Mock<IPlayer>();
            var tribe = new Mock<ITribe>();
            var locker = new Mock<ILocker>();
            var canDo = new Mock<ICanDo>();
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

            locator.Setup(m => m.TryGetObjects(1, 100, out cityObj, out structureObj)).Returns(true);
            formula.Setup(m => m.GetSendCapacity(structureObj)).Returns(capacity);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            player.SetupGet(p => p.IsInTribe).Returns(false);
            player.SetupGet(p => p.PlayerId).Returns(123);

            // Execute/Verify
            action.Execute().Should().Be(Error.TribesmanNotPartOfTribe);
            cityResources.Crop.Value.Should().Be(1000);
            cityResources.Gold.Value.Should().Be(1000);
            cityResources.Iron.Value.Should().Be(1000);
            cityResources.Wood.Value.Should().Be(1000);
            cityResources.Labor.Value.Should().Be(1000);
        }

        [Fact]
        public void TestContributeUserCancel()
        {
            // Setup 
            var locator = new Mock<IGameObjectLocator>();
            var formula = new Mock<Formula>();
            var city = new Mock<ICity>();
            var structure = new Mock<IStructure>();
            var player = new Mock<IPlayer>();
            var tribe = new Mock<ITribe>();
            var locker = new Mock<ILocker>();
            var canDo = new Mock<ICanDo>();
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
            formula.Setup(m => m.GetSendCapacity(structureObj)).Returns(capacity);
            formula.Setup(m => m.TradeTime(structureObj, resource)).Returns(300);
            formula.Setup(m => m.GetActionCancelResource(It.IsAny<DateTime>(), It.IsAny<Resource>())).Returns(resource);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            city.Setup(m => m.TryGetStructure(100, out structureObj)).Returns(true);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            locker.Setup(m => m.Lock(1, out cityObj, out tribeObj)).Returns((CallbackLock)null);
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
            var locator = new Mock<IGameObjectLocator>();
            var formula = new Mock<Formula>();
            var city = new Mock<ICity>();
            var structure = new Mock<IStructure>();
            var player = new Mock<IPlayer>();
            var tribe = new Mock<ITribe>();
            var locker = new Mock<ILocker>();
            var canDo = new Mock<ICanDo>();
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
            formula.Setup(m => m.GetSendCapacity(structureObj)).Returns(capacity);
            formula.Setup(m => m.TradeTime(structureObj, resource)).Returns(300);
            formula.Setup(m => m.GetActionCancelResource(It.IsAny<DateTime>(), It.IsAny<Resource>())).Returns(resource);
            city.SetupGet(p => p.Resource).Returns(cityResources);
            city.SetupGet(p => p.Owner).Returns(player.Object);
            city.Setup(m => m.TryGetStructure(100, out structureObj)).Returns(true);
            structure.SetupGet(p => p.City).Returns(city.Object);
            locator.Setup(m => m.TryGetObjects(1, out cityObj, out tribeObj)).Returns(true);
            locker.Setup(m => m.Lock(1, out cityObj, out tribeObj)).Returns((CallbackLock)null);
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
    }
}