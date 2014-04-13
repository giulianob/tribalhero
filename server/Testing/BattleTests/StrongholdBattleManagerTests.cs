using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Data;
using Game.Data.Stronghold;
using Game.Map;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class StrongholdBattleManagerTests
    {
        [Theory, AutoNSubstituteData]
        public void CanWatchBattle_WhenPlayerIsNotPartOfTribe_ShouldReturnError(
            IPlayer player,
            IStronghold stronghold,            
            IFixture fixture)
        {
            player.IsInTribe.Returns(false);

            stronghold.StrongholdState.Returns(StrongholdState.Occupied);
            stronghold.Tribe.Id.Returns<uint>(212);
            stronghold.GateOpenTo.Id.Returns<uint>(123);

            fixture.Register(() => new BattleLocation(BattleLocationType.Stronghold, 100));

            var gameObjectLocator = fixture.Freeze<IGameObjectLocator>();
            IStronghold outStronghold;
            gameObjectLocator.TryGetObjects(100, out outStronghold).Returns(args =>
            {
                args[1] = stronghold;
                return true;
            });

            var battleManager = fixture.Create<StrongholdMainBattleManager>();

            IEnumerable<string> errorParams;
            battleManager.CanWatchBattle(player, out errorParams).Should().Be(Error.BattleNotViewable);
        }

        [Theory, AutoNSubstituteData]
        public void CanWatchBattle_WhenPlayerIsTribesmateFromTribeWhichHasKnockedDownTheGate_ShouldReturnErrorOk(
            IPlayer player,
            IStronghold stronghold,            
            IFixture fixture)
        {
            player.IsInTribe.Returns(true);
            player.Tribesman.Tribe.Id.Returns<uint>(123);

            stronghold.GateOpenTo.Id.Returns<uint>(123);

            fixture.Register(() => new BattleLocation(BattleLocationType.Stronghold, 100));

            var gameObjectLocator = fixture.Freeze<IGameObjectLocator>();
            IStronghold outStronghold;
            gameObjectLocator.TryGetObjects(100, out outStronghold).Returns(args =>
            {
                args[1] = stronghold;
                return true;
            });

            var battleManager = fixture.Create<StrongholdMainBattleManager>();

            IEnumerable<string> errorParams;
            battleManager.CanWatchBattle(player, out errorParams).Should().Be(Error.Ok);
        }

        [Theory, AutoNSubstituteData]
        public void CanWatchBattle_WhenPlayerIsTribesmateFromOwningTribe_ShouldReturnErrorOk(            
            IPlayer player,
            IStronghold stronghold,            
            IFixture fixture)
        {
            player.IsInTribe.Returns(true);
            player.Tribesman.Tribe.Id.Returns<uint>(123);

            stronghold.StrongholdState = StrongholdState.Occupied;            
            stronghold.Tribe.Id.Returns<uint>(123);

            fixture.Register(() => new BattleLocation(BattleLocationType.Stronghold, 100));

            var gameObjectLocator = fixture.Freeze<IGameObjectLocator>();
            IStronghold outStronghold;
            gameObjectLocator.TryGetObjects(100, out outStronghold).Returns(args =>
            {
                args[1] = stronghold;
                return true;
            });

            var battleManager = fixture.Create<StrongholdMainBattleManager>();

            IEnumerable<string> errorParams;
            battleManager.CanWatchBattle(player, out errorParams).Should().Be(Error.Ok);
        }
    }
}