using FluentAssertions;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data.Troop;
using Moq;
using Persistance;
using Xunit;

namespace Testing.BattleTests
{
    public class CombatGroupTest
    {
        /// <summary>
        ///     When an object is added to the group
        ///     Then the group id of the object should be set
        /// </summary>
        [Fact]
        public void TestGroupIdSetWhenObjectAdded()
        {
            Mock<ICombatObject> combatObject = new Mock<ICombatObject>();
            Mock<ITroopStub> troopStub = new Mock<ITroopStub>();
            Mock<IDbManager> dbManager = new Mock<IDbManager>();

            troopStub.SetupGet(p => p.City.Id).Returns(1);

            combatObject.SetupProperty(p => p.GroupId);

            CityDefensiveCombatGroup combatGroup = new CityDefensiveCombatGroup(1,
                                                                                10,
                                                                                troopStub.Object,
                                                                                dbManager.Object);
            combatGroup.Add(combatObject.Object);

            combatObject.Object.GroupId.Should().Be(10);
        }
    }
}