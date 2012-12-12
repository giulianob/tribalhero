using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data.Stats;
using Game.Map;
using Moq;
using Persistance;
using Xunit;

namespace Testing.Battle
{
    public class CombatListTest
    {
        #region GetBestTargets

        /// <summary>
        ///     Given combat list is empty
        ///     When GetBestTargets is called
        ///     Then the result should be CombatList.BestTargetResult.NoneInRange
        ///     And out combatList should be empty
        /// </summary>
        [Fact]
        public void TestNoObjects()
        {
            Mock<IDbManager> manager = new Mock<IDbManager>();
            Mock<RadiusLocator> radiusLocator = new Mock<RadiusLocator>();
            Mock<BattleFormulas> battleFormulas = new Mock<BattleFormulas>();
            Mock<ICombatObject> combatObject = new Mock<ICombatObject>();

            CombatList list = new CombatList(manager.Object, radiusLocator.Object, battleFormulas.Object);

            IList<CombatList.Target> result;
            CombatList.BestTargetResult targetResult = list.GetBestTargets(combatObject.Object, out result, 1);

            result.Should().BeEmpty();
            targetResult.Should().Be(CombatList.BestTargetResult.NoneInRange);
        }

        /// <summary>
        ///     Given combat list is not empty
        ///     And object is in range
        ///     When GetBestTargets is called
        ///     Then the result should be CombatList.BestTargetResult.Ok
        ///     And out combatList should have the defender
        /// </summary>
        [Fact]
        public void TestInRange()
        {
            Mock<IDbManager> manager = new Mock<IDbManager>();
            Mock<RadiusLocator> radiusLocator = new Mock<RadiusLocator>();
            Mock<BattleFormulas> battleFormulas = new Mock<BattleFormulas>();
            Mock<BattleStats> attackerStats = new Mock<BattleStats>();
            Mock<ICombatObject> attacker = new Mock<ICombatObject>();
            Mock<BattleStats> defenderStats = new Mock<BattleStats>();
            Mock<ICombatObject> defender = new Mock<ICombatObject>();

            attackerStats.SetupGet(p => p.Stl).Returns(1);

            attacker.Setup(m => m.InRange(defender.Object)).Returns(true);
            attacker.Setup(m => m.Location()).Returns(new Position(0, 0));
            attacker.SetupGet(p => p.Visibility).Returns(1);
            attacker.SetupGet(p => p.IsDead).Returns(false);
            attacker.Setup(m => m.CanSee(It.IsAny<ICombatObject>(), It.IsAny<uint>())).Returns(true);

            defenderStats.SetupGet(p => p.Stl).Returns(2);

            defender.Setup(m => m.InRange(attacker.Object)).Returns(true);
            defender.Setup(m => m.Location()).Returns(new Position(0, 0));
            defender.SetupGet(p => p.IsDead).Returns(false);
            defender.SetupGet(p => p.Stats).Returns(defenderStats.Object);

            Mock<ICombatGroup> combatGroup = new Mock<ICombatGroup>();
            combatGroup.Setup(p => p.GetEnumerator())
                       .Returns(() => new List<ICombatObject> {defender.Object}.GetEnumerator());

            CombatList list = new CombatList(manager.Object, radiusLocator.Object, battleFormulas.Object)
            {
                    {combatGroup.Object, false}
            };

            IList<CombatList.Target> result;
            CombatList.BestTargetResult targetResult = list.GetBestTargets(attacker.Object, out result, 1);

            result.Should().HaveCount(1);
            result[0].CombatObject.Should().Be(defender.Object);
            targetResult.Should().Be(CombatList.BestTargetResult.Ok);
        }

        /// <summary>
        ///     When a group is removed
        ///     Then the group should be deleted
        /// </summary>
        [Fact]
        public void TestGroupIsClearedWhenRemoved()
        {
            Mock<IDbManager> manager = new Mock<IDbManager>();
            Mock<RadiusLocator> radiusLocator = new Mock<RadiusLocator>();
            Mock<BattleFormulas> battleFormulas = new Mock<BattleFormulas>();
            Mock<ICombatGroup> combatGroup = new Mock<ICombatGroup>();

            CombatList list = new CombatList(manager.Object, radiusLocator.Object, battleFormulas.Object)
            {
                    {combatGroup.Object, false}
            };

            list.Remove(combatGroup.Object);

            manager.Verify(m => m.Delete(combatGroup.Object), Times.Once());
        }

        #endregion
    }
}