using System;
using System.Data.Common;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Tribe;
using Game.Map;
using Game.Setup;
using Game.Util;
using NSubstitute;
using Persistance;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.TribeTests
{
    public class TribeManagerTests
    {
        [Theory, AutoNSubstituteData]
        public void CreateTribe_IfPlayerHasCreatedAndLeftTribeInPastDay_ShouldGiveError(
                IPlayer player,
                [Frozen] IDbManager dbManager,
                DbDataReader dbDataReader,
                TribeManager tribeManager)
        {
            SystemClock.SetClock(1.March(2010));
            dbDataReader.HasRows.Returns(false);
            dbManager.ReaderQuery(null).ReturnsForAnyArgs(dbDataReader);
            player.Tribesman.Returns((ITribesman)null);
            player.LastDeletedTribe.Returns(SystemClock.Now.AddHours(-23));

            ITribe tribe;
            tribeManager.CreateTribe(player, "TribeName", out tribe).Should().Be(Error.TribeCannotCreateYet);
        }

        [Theory, AutoNSubstituteData]
        public void CreateTribe_IfPlayerHasNotCreatedAndLeftTribeInPastDay_ShouldCreateOk(
                IPlayer player,
                [Frozen] IDbManager dbManager,
                DbDataReader dbDataReader,
                TribeManager tribeManager)
        {
            SystemClock.SetClock(1.March(2010));
            dbDataReader.HasRows.Returns(false);
            dbManager.ReaderQuery(null).ReturnsForAnyArgs(dbDataReader);
            player.Tribesman.Returns((ITribesman)null);
            player.LastDeletedTribe.Returns(SystemClock.Now.AddHours(-24));

            ITribe tribe;
            tribeManager.CreateTribe(player, "TribeName", out tribe).Should().Be(Error.Ok);
        }
    }
}