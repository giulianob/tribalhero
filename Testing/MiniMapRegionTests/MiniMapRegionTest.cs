using System;
using System.Linq;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Map;
using Game.Util.Locking;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.MiniMapRegionTests
{
    public class MiniMapRegionTest
    {
        [Theory, AutoNSubstituteData]
        public void GetCityBytes_WhenPlayersInRegionArentInOrder_ShouldSucceed(
            [Frozen] DefaultMultiObjectLock.Factory lockerFactory,            
            IMultiObjectLock locker,
            IMiniMapRegionObject obj1,
            IMiniMapRegionObject obj2,            
            MiniMapRegion miniMapRegion)
        {
            obj1.Hash.Returns(1);
            obj2.Hash.Returns(2);

            lockerFactory().Lock(Arg.Is<ILockable[]>(p => p.SequenceEqual(new[] {obj2, obj1})))
                           .Returns(locker);

            locker.When(p => p.SortLocks(Arg.Is<ILockable[]>(a => a.SequenceEqual(new[] {obj2, obj1}))))
                .Do(args => Array.Sort((ILockable[])args[0], DefaultMultiObjectLock.CompareObject));

            miniMapRegion.Add(obj2);
            miniMapRegion.Add(obj1);

            miniMapRegion.GetCityBytes().Should().NotBeNull();
        }
    }
}