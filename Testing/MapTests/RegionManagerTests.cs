using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Map;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.MapTests
{
    public class RegionManagerTests
    {
        [Theory, AutoNSubstituteData]
        public void LockRegions_WhenCalledWithMultipleRegions_ShouldLockRegionInProperOrder(
            [Frozen] ITileLocator tileLocator,
            IRegion region1,
            IRegion region2,
            IRegion region3,
            IRegion region4,
            [Frozen] IRegionLocator regionLocator,
            RegionManager regionManager)
        {
            var lockedRegions = new List<IRegion>();
            region1.When(p => p.EnterWriteLock()).Do(x => lockedRegions.Add(region1));
            region2.When(p => p.EnterWriteLock()).Do(x => lockedRegions.Add(region2));
            region3.When(p => p.EnterWriteLock()).Do(x => lockedRegions.Add(region3));
            region4.When(p => p.EnterWriteLock()).Do(x => lockedRegions.Add(region4));

            regionManager.AddRegion(region1);
            regionManager.AddRegion(region2);
            regionManager.AddRegion(region3);
            regionManager.AddRegion(region4);

            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(3);
            regionLocator.GetRegionIndex(0, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(0, 2).Returns<ushort>(0);
            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(1);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(2);

            tileLocator.ForeachMultitile(10, 20, 3).Returns(new[]
            {
                    new Position(1, 1),
                    new Position(0, 1),                    
                    new Position(2, 1),
                    new Position(0, 2),
            });

            var result = regionManager.LockRegions(10, 20, 3);

            lockedRegions.Should().Equal(region1, region2, region3);
            result.Should().Equal(new ushort[] { 0, 1, 2 });
        }

        [Theory, AutoNSubstituteData]
        public void LockRegions_WhenCalledWithInvalidPosition_IgnoresInvalidRegion(
            [Frozen] ITileLocator tileLocator,
            [Frozen] IRegionLocator regionLocator,
            RegionManager regionManager)
        {
            var lockedRegions = new List<IRegion>();

            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(0);

            tileLocator.ForeachMultitile(10, 20, 3).Returns(new[]
            {
                    new Position(1, 1),
            });

            regionManager.LockRegions(10, 20, 3);

            lockedRegions.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void UnlockRegions_WhenCalledWithMultipleRegions_ShouldUnlocksRegionInProperOrder(
            [Frozen] ITileLocator tileLocator,
            IRegion region1,
            IRegion region2,
            IRegion region3,
            IRegion region4,
            [Frozen] IRegionLocator regionLocator,
            RegionManager regionManager)
        {
            var lockedRegions = new List<IRegion>();
            region1.When(p => p.ExitWriteLock()).Do(x => lockedRegions.Add(region1));
            region2.When(p => p.ExitWriteLock()).Do(x => lockedRegions.Add(region2));
            region3.When(p => p.ExitWriteLock()).Do(x => lockedRegions.Add(region3));
            region4.When(p => p.ExitWriteLock()).Do(x => lockedRegions.Add(region4));

            regionManager.AddRegion(region1);
            regionManager.AddRegion(region2);
            regionManager.AddRegion(region3);
            regionManager.AddRegion(region4);

            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(3);
            regionLocator.GetRegionIndex(0, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(0, 2).Returns<ushort>(0);
            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(1);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(2);

            tileLocator.ForeachMultitile(10, 20, 3).Returns(new[]
            {
                    new Position(1, 1),
                    new Position(0, 1),                    
                    new Position(2, 1),
                    new Position(0, 2),
            });

            regionManager.UnlockRegions(10, 20, 3);

            lockedRegions.Should().Equal(region3, region2, region1);
        }

        [Theory, AutoNSubstituteData]
        public void UnlockRegions_WhenCalledWithInvalidPosition_IgnoresInvalidRegion(
            [Frozen] ITileLocator tileLocator,
            [Frozen] IRegionLocator regionLocator,
            RegionManager regionManager)
        {
            var lockedRegions = new List<IRegion>();

            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(0);

            tileLocator.ForeachMultitile(10, 20, 3).Returns(new[]
            {
                    new Position(1, 1),
            });

            regionManager.UnlockRegions(10, 20, 3);

            lockedRegions.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void Add_ShouldAddObjectToPrimaryRegion(
                ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                IRegion region1,
                RegionManager regionManager)
        {            
            simpleGameObject.PrimaryPosition.Returns(new Position(1, 2));
            regionLocator.GetRegionIndex(simpleGameObject.PrimaryPosition.X, simpleGameObject.PrimaryPosition.Y).Returns<ushort>(1);

            regionManager.AddRegion(region0);
            regionManager.AddRegion(region1);

            regionManager.Add(simpleGameObject);

            region0.DidNotReceive().Add(simpleGameObject);
            region1.Received(1).Add(simpleGameObject);
        }

        [Theory, AutoNSubstituteData]
        public void Add_ShouldAddObjectToAllTiles(
                ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                IRegion region1,
                IRegion region2,
                [Frozen] ITileLocator tileLocator,
                RegionManager regionManager)
        {            
            simpleGameObject.PrimaryPosition.Returns(new Position(1, 2));
            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(2);

            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(1, 2).Returns<ushort>(0);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(1);
            
            tileLocator.ForeachMultitile(simpleGameObject).Returns(new[]
            {
                    new Position(1, 1),
                    new Position(1, 2),
                    new Position(2, 1),
            });

            regionManager.AddRegion(region0);
            regionManager.AddRegion(region1);
            regionManager.AddRegion(region2);

            regionManager.Add(simpleGameObject);

            region0.Received(1).AddObjectToTile(simpleGameObject, 1, 1);
            region0.Received(1).AddObjectToTile(simpleGameObject, 1, 2);
            region1.Received(1).AddObjectToTile(simpleGameObject, 2, 1);
            region2.DidNotReceive().AddObjectToTile(Arg.Any<ISimpleGameObject>(), Arg.Any<uint>(), Arg.Any<uint>());
        }

        [Theory, AutoNSubstituteData]
        public void Add_WhenSomeTilesAreInInvalidRegion_ShouldIgnoreRegions(
                ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                [Frozen] ITileLocator tileLocator,
                RegionManager regionManager)
        {            
            simpleGameObject.PrimaryPosition.Returns(new Position(1, 1));
            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(2);

            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(1);
            
            tileLocator.ForeachMultitile(simpleGameObject).Returns(new[]
            {
                    new Position(1, 1),
                    new Position(2, 1),
            });

            regionManager.AddRegion(region0);

            regionManager.Add(simpleGameObject);

            region0.Received(1).AddObjectToTile(simpleGameObject, 1, 1);
        }

        [Theory, AutoNSubstituteData]
        public void Remove_RemovesObjectFromAllTiles(
            ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                IRegion region1,
                IRegion region2,
                [Frozen] ITileLocator tileLocator,
                RegionManager regionManager)
        {            
            simpleGameObject.InWorld.Returns(true);
            simpleGameObject.PrimaryPosition.Returns(new Position(1, 2));
            simpleGameObject.Size.Returns<byte>(10);
            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(2);

            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(1, 2).Returns<ushort>(0);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(1);
            
            tileLocator.ForeachMultitile(1, 2, 10).Returns(new[]
            {
                    new Position(1, 1),
                    new Position(1, 2),
                    new Position(2, 1),
            });

            regionManager.AddRegion(region0);
            regionManager.AddRegion(region1);
            regionManager.AddRegion(region2);

            regionManager.Remove(simpleGameObject);

            region0.Received(1).Remove(simpleGameObject, 1, 2);
            region0.Received(1).RemoveObjectFromTile(simpleGameObject, 1, 1);
            region0.Received(1).RemoveObjectFromTile(simpleGameObject, 1, 2);
            region1.Received(1).RemoveObjectFromTile(simpleGameObject, 2, 1);
            region2.DidNotReceive().RemoveObjectFromTile(Arg.Any<ISimpleGameObject>(), Arg.Any<uint>(), Arg.Any<uint>());
        }

        [Theory, AutoNSubstituteData]
        public void Remove_ProperlyLocksRegions(
                ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                IRegion region1,
                IRegion region2,
                [Frozen] ITileLocator tileLocator,
                RegionManager regionManager)
        {
            simpleGameObject.InWorld.Returns(true);
            regionLocator.GetRegionIndex(0, 0).ReturnsForAnyArgs<ushort>(2);

            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(1, 2).Returns<ushort>(0);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(1);

            tileLocator.ForeachMultitile(0, 0, 0).ReturnsForAnyArgs(new[]
            {
                    new Position(1, 1),
                    new Position(2, 1),
            });

            regionManager.AddRegion(region0);
            regionManager.AddRegion(region1);
            regionManager.AddRegion(region2);

            regionManager.Remove(simpleGameObject);

            region0.Received(1).EnterWriteLock();
            region1.Received(1).EnterWriteLock();
            region0.Received(1).ExitWriteLock();
            region1.Received(1).ExitWriteLock();
            region2.DidNotReceive().EnterWriteLock();
        }

        [Theory, AutoNSubstituteData]
        public void Remove_WhenATileIsInInvalidRegion_RemovesFromAllOtherTilesAndIgnoresInvalidTile(
                ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                [Frozen] ITileLocator tileLocator,
                RegionManager regionManager)
        {
            simpleGameObject.InWorld.Returns(true);
            simpleGameObject.PrimaryPosition.Returns(new Position(1, 2));

            regionLocator.GetRegionIndex(1, 1).Returns<ushort>(0);
            regionLocator.GetRegionIndex(2, 1).Returns<ushort>(1);

            tileLocator.ForeachMultitile(0, 0, 0).ReturnsForAnyArgs(new[]
            {
                    new Position(1, 1),
                    new Position(2, 1),
            });

            regionManager.AddRegion(region0);

            regionManager.Remove(simpleGameObject);

            region0.Received(1).Remove(simpleGameObject, 1, 2);
            region0.Received(1).RemoveObjectFromTile(simpleGameObject, 1, 1);
        }

        [Theory, AutoNSubstituteData]
        public void ObjectUpdateEvent_LocksBothPreviousAndCurrentRegions(
                ISimpleGameObject simpleGameObject,
                [Frozen] IRegionLocator regionLocator,
                IRegion region0,
                IRegion region1,
                [Frozen] ITileLocator tileLocator,
                RegionManager regionManager)
        {
            simpleGameObject.InWorld.Returns(true);
            simpleGameObject.Size.Returns<byte>(11);
            simpleGameObject.PrimaryPosition.Returns(new Position(1, 2));

            regionManager.AddRegion(region0);
            regionManager.AddRegion(region1);

            regionLocator.GetRegionIndex(1, 9).Returns<ushort>(0);
            regionLocator.GetRegionIndex(2, 9).Returns<ushort>(1);

            tileLocator.ForeachMultitile(1, 2, 11).Returns(new[]
            {
                    new Position(1, 9)
            });

            tileLocator.ForeachMultitile(2, 3, 11).Returns(new[]
            {
                    new Position(2, 9)
            });

            regionManager.UpdateObject(simpleGameObject, 2, 3);

            region0.Received(1).EnterWriteLock();
            region1.Received(1).EnterWriteLock();
            region0.Received(1).ExitWriteLock();
            region1.Received(1).ExitWriteLock();
        }
    }
}