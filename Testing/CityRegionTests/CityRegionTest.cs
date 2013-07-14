using System;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Map;
using Game.Util.Locking;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using System.Linq;

namespace Testing.CityRegionTests
{
    public class CityRegionTest
    {
        [Theory, AutoNSubstituteData]
        public void GetCityBytes_WhenPlayersInRegionArentInOrder_ShouldSucceed(
            [Frozen] DefaultMultiObjectLock.Factory lockerFactory,            
            IMultiObjectLock locker,
            ICityRegionObject obj1,
            ICityRegionObject obj2,            
            CityRegion cityRegion)
        {
            obj1.Hash.Returns(1);
            obj2.Hash.Returns(2);

            lockerFactory().Lock(Arg.Is<ILockable[]>(p => p.SequenceEqual(new[] {obj2, obj1})))
                           .Returns(locker);

            locker.When(p => p.SortLocks(Arg.Is<ILockable[]>(a => a.SequenceEqual(new[] {obj2, obj1}))))
                .Do(args => Array.Sort((ILockable[])args[0], DefaultMultiObjectLock.CompareObject));

            cityRegion.Add(obj2);
            cityRegion.Add(obj1);

            cityRegion.GetCityBytes().Should().NotBeNull();
        }
    }
}