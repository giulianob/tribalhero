using System;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Tribe;
using Game.Setup;
using Game.Util;
using NSubstitute;
using Persistance;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;

namespace Testing.TribeTests
{
    public class TribeTest
    {
        [Theory, AutoNSubstituteData]
        public void AddTribesman_ShouldAddPlayerToTribe([Frozen] IDbManager dbManager, IPlayer player, ITribesman tribesman, Tribe tribe)
        {
            tribesman.Player.Returns(player);

            tribe.AddTribesman(tribesman).Should().Be(Error.Ok);
            tribe.Tribesmen.Should().Equal(tribesman);
            player.Tribesman.Should().Be(tribesman);
            dbManager.Received(1).Save(tribesman);
        }

        [Theory, AutoNSubstituteData]
        public void AddTribesman_ShouldReturnErrorIfTribesmanAlreadyExists(ITribesman tribesman, Tribe tribe)
        {
            tribe.AddTribesman(tribesman).Should().Be(Error.Ok);
            tribe.AddTribesman(tribesman).Should().Be(Error.TribesmanAlreadyInTribe);
        }

        [Theory, AutoNSubstituteData]
        public void AddTribesman_ShouldNotAllowTribesmanWhoLeftToRejoinWithinXDays(ITribesman tribesman, Tribe tribe)
        {
            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));

            tribesman.Player.PlayerId.Returns((uint)1234);

            tribe.AddTribesman(tribesman);
            tribe.RemoveTribesman(1234, false, false);

            SystemClock.SetClock(SystemClock.Now.AddDays(Tribe.DAYS_BEFORE_REJOIN_ALLOWED).AddMinutes(-1));
            tribe.AddTribesman(tribesman).Should().Be(Error.TribeCannotRejoinYet);
        }

        [Theory, AutoNSubstituteData]
        public void AddTribesman_ShouldAllowTribesmanWhoLeftToRejoinAfterXHours(ITribesman tribesman, Tribe tribe)
        {
            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));

            tribesman.Player.PlayerId.Returns((uint)1234);

            tribe.AddTribesman(tribesman);
            tribe.RemoveTribesman(1234, false, false);

            SystemClock.SetClock(SystemClock.Now.AddDays(Tribe.DAYS_BEFORE_REJOIN_ALLOWED));
            tribe.AddTribesman(tribesman).Should().Be(Error.Ok);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(10, 50)]
        [InlineData(20, 100)]
        public void AddTribesman_ShouldNotAllowMoreThanXNumbersOfMembers(int level, int expectedMembers)
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            var tribe = fixture.Build<Tribe>().OmitAutoProperties().Create();

            tribe.Level = (byte)level;

            for (int i = 0; i < expectedMembers; i++)
            {
                var tribesman = Substitute.For<ITribesman>();
                tribesman.Player.PlayerId.Returns((uint)i);
                tribe.AddTribesman(tribesman).Should().Be(Error.Ok);
            }

            var errorTribesman = Substitute.For<ITribesman>();
            errorTribesman.Player.PlayerId.Returns((uint)99999999);
            tribe.AddTribesman(errorTribesman).Should().Be(Error.TribeFull);
        }

        [Fact]
        public void AddTribesman_ShouldNotAllowNewMemberAfterSomeoneLeavesAndLessThanXHoursHasPassed()
        {
            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));

            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            var tribe = fixture.Build<Tribe>().OmitAutoProperties().Create();

            tribe.Level = 1;

            for (int i = 0; i < 5; i++)
            {
                var tribesman = Substitute.For<ITribesman>();
                tribesman.Player.PlayerId.Returns((uint)i);
                tribe.AddTribesman(tribesman).Should().Be(Error.Ok);
            }

            tribe.RemoveTribesman(0, true);

            SystemClock.SetClock(SystemClock.Now.AddHours(Tribe.HOURS_BEFORE_SLOT_REOPENS).AddSeconds(-1));
            
            var errorTribesman = Substitute.For<ITribesman>();
            errorTribesman.Player.PlayerId.Returns((uint)99999999);
            tribe.AddTribesman(errorTribesman).Should().Be(Error.TribeFull);
        }

        [Fact]
        public void AddTribesman_ShouldAllowNewMemberAfterSomeoneLeavesAndMoreThanXHoursHasPassed()
        {
            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));

            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            var tribe = fixture.Build<Tribe>().OmitAutoProperties().Create();

            tribe.Level = 1;

            for (int i = 0; i < 5; i++)
            {
                var tribesman = Substitute.For<ITribesman>();
                tribesman.Player.PlayerId.Returns((uint)i);
                tribe.AddTribesman(tribesman).Should().Be(Error.Ok);
            }

            tribe.RemoveTribesman(0, true);

            SystemClock.SetClock(SystemClock.Now.AddHours(Tribe.HOURS_BEFORE_SLOT_REOPENS));
            
            var errorTribesman = Substitute.For<ITribesman>();
            errorTribesman.Player.PlayerId.Returns<uint>(99999999);
            tribe.AddTribesman(errorTribesman).Should().Be(Error.Ok);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveTribesman_ShouldRecordPlayerLeftInLeavingTribesmates(ITribesman tribesman, Tribe tribe)        
        {
            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));

            tribesman.Player.PlayerId.Returns<uint>(1234);

            tribe.AddTribesman(tribesman);
            tribe.RemoveTribesman(1234, false, false);

            tribe.LeavingTribesmates.Should().OnlyContain(p => p.PlayerId == 1234 && p.TimeLeft == SystemClock.Now);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveTribesman_ShouldRemoveDuplicateEntriesForSamePlayer(ITribesman tribesman, Tribe tribe)        
        {
            tribesman.Player.PlayerId.Returns<uint>(1234);

            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));            
            tribe.AddTribesman(tribesman);
            tribe.RemoveTribesman(1234, false, false);

            SystemClock.SetClock(new DateTime(2001, 1, 1, 1, 1, 2));
            tribe.AddTribesman(tribesman);
            tribe.RemoveTribesman(1234, false, false);

            tribe.LeavingTribesmates.Should().OnlyContain(p => p.PlayerId == 1234 && p.TimeLeft == SystemClock.Now);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveTribesman_ShouldRemoveOlderRecords(ITribesman tribesman1, ITribesman tribesman2, Tribe tribe)
        {
            tribesman1.Player.PlayerId.Returns<uint>(1234);
            tribesman2.Player.PlayerId.Returns<uint>(1235);

            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));            
            tribe.AddTribesman(tribesman1);
            tribe.AddTribesman(tribesman2);
            tribe.RemoveTribesman(1234, false, false);
            tribe.LeavingTribesmates.Should().OnlyContain(p => p.PlayerId == 1234);

            SystemClock.SetClock(SystemClock.Now.AddDays(Tribe.DAYS_BEFORE_REJOIN_ALLOWED).AddSeconds(1));
            tribe.RemoveTribesman(1235, false, false);

            tribe.LeavingTribesmates.Should().OnlyContain(p => p.PlayerId == 1235);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveTribesman_ShouldSupportMultipleRemoves(ITribesman tribesman1, ITribesman tribesman2, Tribe tribe)
        {
            tribesman1.Player.PlayerId.Returns<uint>(1234);
            tribesman2.Player.PlayerId.Returns<uint>(1235);

            SystemClock.SetClock(new DateTime(2000, 1, 1, 1, 1, 1));            
            tribe.AddTribesman(tribesman1);
            tribe.AddTribesman(tribesman2);
            tribe.RemoveTribesman(1234, false, false);
            tribe.RemoveTribesman(1235, false, false);
            tribe.LeavingTribesmates.Should().HaveCount(2)
                .And.Contain(p => p.PlayerId == 1234)
                .And.Contain(p => p.PlayerId == 1235);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveTribesman_ShouldDeleteTribesman([Frozen] IDbManager dbManager, IPlayer player, ITribesman tribesman, Tribe tribe)
        {
            player.PlayerId.Returns<uint>(1234);
            tribesman.Player.Returns(player);

            tribe.AddTribesman(tribesman);
            tribe.RemoveTribesman(1234, false, false);

            player.Tribesman.Should().BeNull();
            dbManager.Received(1).Delete(tribesman);
            tribe.Tribesmen.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void RemoveTribesman_ShouldNotBeAbleToRemoveNonExistingTribesmate(Tribe tribe)
        {        
            tribe.RemoveTribesman(1234, false, false).Should().Be(Error.TribesmanNotFound);
        }
    }
}