using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Moq;
using Persistance;
using Xunit;

namespace Testing.ActionsTests
{
    public class StrongholdGateBattlePassiveActionTest
    {
        /// <summary>
        ///     Tests that if a tribe that deals a lot of dmg to the gate
        ///     leaves and someone else knocks it down, the one that knocks it down
        ///     gets the gate open
        /// </summary>
        [Fact]
        public void TestIgnoresTribesThatLeave()
        {
            var tribe1 = new Mock<ITribe>();
            tribe1.SetupGet(p => p.Id).Returns(201);

            var tribe2 = new Mock<ITribe>();
            tribe2.SetupGet(p => p.Id).Returns(202);

            var city1 = new Mock<ICity>();
            city1.SetupGet(p => p.Id).Returns(101);
            city1.SetupGet(p => p.Owner.IsInTribe).Returns(true);
            city1.SetupGet(p => p.Owner.Tribesman.Tribe.Id).Returns(201);

            var city2 = new Mock<ICity>();
            city2.SetupGet(p => p.Id).Returns(102);
            city2.SetupGet(p => p.Owner.IsInTribe).Returns(true);
            city2.SetupGet(p => p.Owner.Tribesman.Tribe.Id).Returns(202);

            var gateGroup = new Mock<CombatGroup>();
            gateGroup.SetupGet(p => p.Id).Returns(1);
            gateGroup.SetupGet(p => p.Owner).Returns(new BattleOwner(BattleOwnerType.Stronghold, 300));

            var attackGroup1 = new Mock<CombatGroup>();
            attackGroup1.SetupGet(p => p.Id).Returns(2);
            attackGroup1.SetupGet(p => p.Owner).Returns(new BattleOwner(BattleOwnerType.City, 101));

            var attackGroup2 = new Mock<CombatGroup>();
            attackGroup2.SetupGet(p => p.Id).Returns(3);
            attackGroup2.SetupGet(p => p.Owner).Returns(new BattleOwner(BattleOwnerType.City, 102));

            var battle = new Mock<IBattleManager>();
            var attackers = new Mock<ICombatList>();
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackGroup1.Object}.GetEnumerator());
            battle.SetupGet(m => m.Attackers).Returns(attackers.Object);

            var stronghold = new Mock<IStronghold>();
            stronghold.SetupGet(p => p.Id).Returns(300);
            stronghold.SetupGet(p => p.GateBattle).Returns(battle.Object);
            stronghold.SetupProperty(p => p.GateOpenTo);

            IGameObjectLocator locator = new GameObjectLocatorStub(stronghold.Object,
                                                                   city1.Object,
                                                                   city2.Object,
                                                                   tribe1.Object,
                                                                   tribe2.Object);

            var strongholdBattleProcedure = new Mock<StrongholdBattleProcedure>();
            strongholdBattleProcedure.Setup(m => m.AddStrongholdGateToBattle(battle.Object, stronghold.Object))
                           .Returns(gateGroup.Object);

            var action = new StrongholdGateBattlePassiveAction(300,
                                                               strongholdBattleProcedure.Object,
                                                               new LockerStub(locator),
                                                               locator,
                                                               new Mock<IDbManager>().Object,
                                                               new Mock<Formula>().Object,
                                                               new Mock<IWorld>().Object);

            // Execute action to set it up
            action.Execute().Should().Be(Error.Ok);

            // Fire attack a few times
            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Attack,
                         attackGroup2.Object,
                         new Mock<ICombatObject>().Object,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         31m,
                         1,
                         1);

            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Attack,
                         attackGroup1.Object,
                         new Mock<ICombatObject>().Object,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         35m,
                         1,
                         1);

            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Attack,
                         attackGroup2.Object,
                         new Mock<ICombatObject>().Object,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         5m,
                         1,
                         1);

            // Attack from defense which should be totally ignored
            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Defense,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         attackGroup1.Object,
                         new Mock<ICombatObject>().Object,
                         5000m,
                         1,
                         1);

            // Send event that gate is dead
            battle.Raise(m => m.GroupKilled += null, battle.Object, gateGroup.Object);

            // Assert
            stronghold.Object.GateOpenTo.Should().NotBeNull();
            stronghold.Object.GateOpenTo.Id.Should().Be(201);
        }

        /// <summary>
        ///     Tests that if a couple of tribes are attacking the gate
        ///     the one with the most dmg is the one that opens the gate
        /// </summary>
        [Fact]
        public void TestTribeThatDealsMostDamageOpensGate()
        {
            var tribe1 = new Mock<ITribe>();
            tribe1.SetupGet(p => p.Id).Returns(201);

            var tribe2 = new Mock<ITribe>();
            tribe2.SetupGet(p => p.Id).Returns(202);

            var city1 = new Mock<ICity>();
            city1.SetupGet(p => p.Id).Returns(101);
            city1.SetupGet(p => p.Owner.IsInTribe).Returns(true);
            city1.SetupGet(p => p.Owner.Tribesman.Tribe.Id).Returns(201);

            var city2 = new Mock<ICity>();
            city2.SetupGet(p => p.Id).Returns(102);
            city2.SetupGet(p => p.Owner.IsInTribe).Returns(true);
            city2.SetupGet(p => p.Owner.Tribesman.Tribe.Id).Returns(202);

            var gateGroup = new Mock<CombatGroup>();
            gateGroup.SetupGet(p => p.Id).Returns(1);
            gateGroup.SetupGet(p => p.Owner).Returns(new BattleOwner(BattleOwnerType.Stronghold, 300));

            var attackGroup1 = new Mock<CombatGroup>();
            attackGroup1.SetupGet(p => p.Id).Returns(2);
            attackGroup1.SetupGet(p => p.Owner).Returns(new BattleOwner(BattleOwnerType.City, 101));

            var attackGroup2 = new Mock<CombatGroup>();
            attackGroup2.SetupGet(p => p.Id).Returns(3);
            attackGroup2.SetupGet(p => p.Owner).Returns(new BattleOwner(BattleOwnerType.City, 102));

            var battle = new Mock<IBattleManager>();
            var attackers = new Mock<ICombatList>();
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackGroup1.Object, attackGroup2.Object}.GetEnumerator());
            battle.SetupGet(m => m.Attackers).Returns(attackers.Object);

            var stronghold = new Mock<IStronghold>();
            stronghold.SetupGet(p => p.Id).Returns(300);
            stronghold.SetupGet(p => p.GateBattle).Returns(battle.Object);
            stronghold.SetupProperty(p => p.GateOpenTo);

            IGameObjectLocator locator = new GameObjectLocatorStub(stronghold.Object,
                                                                   city1.Object,
                                                                   city2.Object,
                                                                   tribe1.Object,
                                                                   tribe2.Object);

            var strongholdBattleProcedure = new Mock<StrongholdBattleProcedure>();
            strongholdBattleProcedure.Setup(m => m.AddStrongholdGateToBattle(battle.Object, stronghold.Object))
                           .Returns(gateGroup.Object);

            var action = new StrongholdGateBattlePassiveAction(300,
                                                               strongholdBattleProcedure.Object,
                                                               new LockerStub(locator),
                                                               locator,
                                                               new Mock<IDbManager>().Object,
                                                               new Mock<Formula>().Object,
                                                               new Mock<IWorld>().Object);

            // Execute action to set it up
            action.Execute().Should().Be(Error.Ok);

            // Fire attack a few times
            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Attack,
                         attackGroup2.Object,
                         new Mock<ICombatObject>().Object,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         31m,
                         1,
                         1);

            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Attack,
                         attackGroup1.Object,
                         new Mock<ICombatObject>().Object,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         35m,
                         1,
                         1);

            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Attack,
                         attackGroup2.Object,
                         new Mock<ICombatObject>().Object,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         5m,
                         1,
                         1);

            // Attack from defense which should be totally ignored
            battle.Raise(m => m.ActionAttacked += null,
                         battle.Object,
                         BattleManager.BattleSide.Defense,
                         gateGroup.Object,
                         new Mock<ICombatObject>().Object,
                         attackGroup1.Object,
                         new Mock<ICombatObject>().Object,
                         5000m,
                         1,
                         1);

            // Send event that gate is dead
            battle.Raise(m => m.GroupKilled += null, battle.Object, gateGroup.Object);

            // Assert
            stronghold.Object.GateOpenTo.Should().NotBeNull();
            stronghold.Object.GateOpenTo.Id.Should().Be(202);
        }
    }
}