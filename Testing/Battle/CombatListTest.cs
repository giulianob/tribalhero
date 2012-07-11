using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Game.Battle;
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
        /// Given combat list is empty
        /// When GetBestTargets is called
        /// Then the result should be CombatList.BestTargetResult.NoneInRange
        /// And out combatList should be empty
        /// </summary>
        [Fact]
        public void TestNoObjects()
        {
            Mock<IDbManager> manager = new Mock<IDbManager>();
            Mock<RadiusLocator> radiusLocator = new Mock<RadiusLocator>();
            Mock<BattleFormulas> battleFormulas = new Mock<BattleFormulas>();
            Mock<CombatObject> combatObject = new Mock<CombatObject>();

            CombatList list = new CombatList(manager.Object, radiusLocator.Object, battleFormulas.Object);

            IList<CombatObject> result;
            CombatList.BestTargetResult targetResult = list.GetBestTargets(combatObject.Object, out result, 1);

            result.Should().BeEmpty();
            targetResult.Should().Be(CombatList.BestTargetResult.NoneInRange);
        }

        /// <summary>
        /// Given combat list is not empty
        /// And object is in range
        /// When GetBestTargets is called
        /// Then the result should be CombatList.BestTargetResult.Ok
        /// And out combatList should be empty
        /// </summary>
        [Fact]
        public void TestInRange()
        {
            Mock<IDbManager> manager = new Mock<IDbManager>();
            Mock<RadiusLocator> radiusLocator = new Mock<RadiusLocator>();
            Mock<BattleFormulas> battleFormulas = new Mock<BattleFormulas>();
            Mock<BattleStats> attackerStats = new Mock<BattleStats>();
            Mock<CombatObject> attacker = new Mock<CombatObject>();
            Mock<BattleStats> defenderStats = new Mock<BattleStats>();
            Mock<CombatObject> defender = new Mock<CombatObject>();

            attackerStats.SetupGet(p => p.Stl).Returns(1);
            
            attacker.Setup(m => m.InRange(defender.Object)).Returns(true);
            attacker.SetupGet(p => p.Visibility).Returns(1);
            attacker.SetupGet(p => p.IsDead).Returns(false);

            defenderStats.SetupGet(p => p.Stl).Returns(2);
            
            defender.Setup(m => m.InRange(attacker.Object)).Returns(true);            
            defender.SetupGet(p => p.IsDead).Returns(false);            
            defender.SetupGet(p => p.Stats).Returns(defenderStats.Object);

            CombatList list = new CombatList(manager.Object, radiusLocator.Object, battleFormulas.Object) {{defender.Object, false}};

            IList<CombatObject> result;
            CombatList.BestTargetResult targetResult = list.GetBestTargets(attacker.Object, out result, 1);

            result.Should().HaveCount(1).And.BeEquivalentTo(defender.Object);
            targetResult.Should().Be(CombatList.BestTargetResult.Ok);
        }
        #endregion
    }
}
