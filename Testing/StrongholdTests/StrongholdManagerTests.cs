using Common.Testing;
using Game.Battle;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.StrongholdTests
{
    public class StrongholdManagerTests
    {
        [Theory, AutoNSubstituteData]
        public void RetreatUnits_WhenStrongholdIsInBattle_ShouldNotRetreatUnits(
            IStronghold stronghold, 
            [Frozen] IActionFactory actionFactory,
            ITroopStub stub,
            IBattleManager battleManager,
            StrongholdManager strongholdManager)
        {
            stronghold.MainBattle.Returns(battleManager);
            stronghold.Troops.StationedHere().Returns(new[] { stub });

            stub.State.Returns(TroopState.Stationed);
            stub.City.Owner.IsInTribe.Returns(false);            

            strongholdManager.RetreatUnits(stronghold);

            stub.City.Worker.DidNotReceiveWithAnyArgs().DoPassive(null, null, false);
        }

        [Theory, AutoNSubstituteData]
        public void RetreatUnits_WhenStubIsNotStationedState_ShouldNotRetreatUnits(
            IStronghold stronghold, 
            [Frozen] IActionFactory actionFactory,
            ITroopStub stub,
            StrongholdManager strongholdManager)
        {
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.Troops.StationedHere().Returns(new[] { stub });

            stub.State.Returns(TroopState.BattleStationed);
            stub.City.Owner.IsInTribe.Returns(false);            

            strongholdManager.RetreatUnits(stronghold);

            stub.City.Worker.DidNotReceiveWithAnyArgs().DoPassive(null, null, false);
        }

        [Theory, AutoNSubstituteData]
        public void RetreatUnits_RetreatsStationedUnitsThatAreNotInATribe(
            IStronghold stronghold, 
            [Frozen] IActionFactory actionFactory,
            ITroopStub stub,
            RetreatChainAction retreatAction,
            StrongholdManager strongholdManager)
        {
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.Troops.StationedHere().Returns(new[] { stub });

            stub.State.Returns(TroopState.Stationed);
            stub.City.Owner.IsInTribe.Returns(false);
            stub.City.Id.Returns<uint>(1234);
            stub.TroopId.Returns<byte>(2);                        

            actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId).Returns(retreatAction);

            strongholdManager.RetreatUnits(stronghold);
            
            stub.City.Worker.Received(1).DoPassive(stub.City, retreatAction, true);
        }

        [Theory, AutoNSubstituteData]
        public void RetreatUnits_RetreatsStationedUnitsThatAreNotInTribeThatOwnsStronghold(
            IStronghold stronghold, 
            [Frozen] IActionFactory actionFactory,
            ITroopStub stub,
            ITribe tribe,
            ITribe shTribe,
            RetreatChainAction retreatAction,
            StrongholdManager strongholdManager)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.City.Owner.IsInTribe.Returns(true);
            stub.City.Id.Returns<uint>(1234);
            stub.TroopId.Returns<byte>(2);
            stub.City.Owner.Tribesman.Tribe.Returns(tribe);

            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.Tribe.Returns(shTribe);
            stronghold.Troops.StationedHere().Returns(new[] { stub });

            actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId).Returns(retreatAction);

            strongholdManager.RetreatUnits(stronghold);
            
            stub.City.Worker.Received(1).DoPassive(stub.City, retreatAction, true);
        }
    }
}