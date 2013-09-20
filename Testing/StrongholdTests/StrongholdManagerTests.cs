using System;
using Common.Testing;
using Game.Battle;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using FluentAssertions;

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
            [Frozen] ITroopObjectInitializerFactory troopInitializerFactory,
            ITroopObjectInitializer troopInitializer,
            ITroopStub stub,
            RetreatChainAction retreatAction,
            StrongholdManager strongholdManager)
        {
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.Troops.StationedHere().Returns(new[] { stub });

            stub.State.Returns(TroopState.Stationed);
            stub.City.Owner.IsInTribe.Returns(false);
            stub.City.Id.Returns<uint>(1234);
            stub.TroopId.Returns<ushort>(2);

            troopInitializerFactory.CreateStationedTroopObjectInitializer(stub).Returns(troopInitializer);
            actionFactory.CreateRetreatChainAction(stub.City.Id, troopInitializer).Returns(retreatAction);

            strongholdManager.RetreatUnits(stronghold);
            
            stub.City.Worker.Received(1).DoPassive(stub.City, retreatAction, true);
        }

        [Theory, AutoNSubstituteData]
        public void RetreatUnits_RetreatsStationedUnitsThatAreNotInTribeThatOwnsStronghold(
            IStronghold stronghold, 
            [Frozen] IActionFactory actionFactory,
            [Frozen] ITroopObjectInitializerFactory troopInitializerFactory,
            ITroopObjectInitializer troopInitializer,
            ITroopStub stub,
            ITribe tribe,
            ITribe shTribe,
            RetreatChainAction retreatAction,
            StrongholdManager strongholdManager)
        {
            stub.State.Returns(TroopState.Stationed);
            stub.City.Owner.IsInTribe.Returns(true);
            stub.City.Id.Returns<uint>(1234);
            stub.TroopId.Returns<ushort>(2);
            stub.City.Owner.Tribesman.Tribe.Returns(tribe);

            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.Tribe.Returns(shTribe);
            stronghold.Troops.StationedHere().Returns(new[] { stub });

            troopInitializerFactory.CreateStationedTroopObjectInitializer(stub).Returns(troopInitializer);
            actionFactory.CreateRetreatChainAction(stub.City.Id, troopInitializer).Returns(retreatAction);

            strongholdManager.RetreatUnits(stronghold);
            
            stub.City.Worker.Received(1).DoPassive(stub.City, retreatAction, true);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenStrongholdInGateBattle(IBattleManager battleManager, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            var initialGate = stronghold.GateMax = 1234;
            stronghold.GateBattle.Returns(battleManager);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.StrongholdNotUpdatableInBattle);

            stronghold.GateMax.Should().Be(initialGate);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenStrongholdInMainBattle(IBattleManager battleManager, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            var initialGate = stronghold.GateMax = 1234;
            stronghold.MainBattle.Returns(battleManager);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.StrongholdNotUpdatableInBattle);

            stronghold.GateMax.Should().Be(initialGate);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenMaxDidNotChanged([FrozenMock] Formula formula, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            formula.StrongholdGateLimit(0).ReturnsForAnyArgs(5000);

            stronghold.GateMax = 5000;
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.GateBattle.Returns((IBattleManager)null);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.Ok);

            stronghold.Received(0).BeginUpdate();
            stronghold.GateMax.Should().Be(5000);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenNewMaxIsHigher([FrozenMock] Formula formula, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            formula.StrongholdGateLimit(0).ReturnsForAnyArgs(5000);

            stronghold.GateMax = 1000;
            stronghold.Gate = 1000;
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.GateBattle.Returns((IBattleManager)null);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.Ok);

            stronghold.Received(1).BeginUpdate();
            stronghold.GateMax.Should().Be(5000);
            stronghold.GateMax.Should().Be(5000);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenNewMaxIsLower([FrozenMock] Formula formula, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            formula.StrongholdGateLimit(0).ReturnsForAnyArgs(1000);

            stronghold.GateMax = 5000;
            stronghold.Gate = 5000;
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.GateBattle.Returns((IBattleManager)null);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.Ok);

            stronghold.Received(1).BeginUpdate();
            stronghold.GateMax.Should().Be(1000);
            stronghold.GateMax.Should().Be(1000);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenIncreasedMaxAt50PercentHp([FrozenMock] Formula formula, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            formula.StrongholdGateLimit(0).ReturnsForAnyArgs(5000);

            stronghold.GateMax = 1000;
            stronghold.Gate = 500;
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.GateBattle.Returns((IBattleManager)null);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.Ok);

            stronghold.Gate.Should().Be(2500);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateGate_WhenReducedMaxAt50PercentHp([FrozenMock] Formula formula, IStronghold stronghold, StrongholdManager strongholdManager)
        {
            formula.StrongholdGateLimit(0).ReturnsForAnyArgs(1000);

            stronghold.GateMax = 5000;
            stronghold.Gate = 2500;
            stronghold.MainBattle.Returns((IBattleManager)null);
            stronghold.GateBattle.Returns((IBattleManager)null);

            strongholdManager.UpdateGate(stronghold).Should().Be(Error.Ok);

            stronghold.Gate.Should().Be(500);
        }
    }
}