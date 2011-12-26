using System;
using FluentAssertions;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Util;
using Moq;
using Persistance;
using Xunit;

namespace Testing.Tribe
{
    public class AssignmentTest
    {
        private const uint TARGET_X = 5;

        private const uint TARGET_Y = 10;

        private readonly DateTime targetTime = new DateTime(2012, 1, 1, 10, 1, 1, 1, DateTimeKind.Utc);

        private readonly DateTime startTime = new DateTime(2012, 1, 1, 9, 1, 1, 1, DateTimeKind.Utc);

        /// <summary>
        /// When an assignment is rescheduled for the first time
        /// Then it should schedule itself at the proper time
        /// And save itself
        /// </summary>
        [Fact]
        public void WhenScheduled()
        {
            Mock<ITribe> tribe = new Mock<ITribe>();
            Mock<ICity> targetCity = new Mock<ICity>();
            Mock<ICity> stubCity;
            Mock<ITroopStub> stub = CreateStub(out stubCity);
            Mock<Formula> formula = new Mock<Formula>();
            Mock<IDbManager> dbManager = new Mock<IDbManager>();
            Mock<IGameObjectLocator> gameObjectLocator = new Mock<IGameObjectLocator>();
            Mock<IScheduler> scheduler = new Mock<IScheduler>();
            Mock<Procedure> procedure = new Mock<Procedure>();
            Mock<TileLocator> tileLocator = new Mock<TileLocator>();
            Mock<IActionFactory> actionFactory = new Mock<IActionFactory>();

            SystemClock.SetClock(startTime);

            // troop should be dispatched a minute later
            formula.Setup(m => m.MoveTimeTotal(stub.Object, It.IsAny<int>(), true)).Returns(300);

            Assignment assignment = new Assignment(tribe.Object,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity.Object,
                                                   AttackMode.Normal,
                                                   targetTime,
                                                   stub.Object,
                                                   formula.Object,
                                                   dbManager.Object,
                                                   gameObjectLocator.Object,
                                                   scheduler.Object,
                                                   procedure.Object,
                                                   tileLocator.Object,
                                                   actionFactory.Object);

            assignment.Reschedule();

            scheduler.Verify(m => m.Put(assignment));
            dbManager.Verify(m => m.Save(assignment));
            assignment.Time.Should().Be(targetTime.AddMinutes(-5));
        }

        /// <summary>
        /// Given the assignment is already scheduled
        /// When someone joins
        /// Then it should remove itself from scheduler first 
        /// And reschedule itself
        /// And save itself

        private Mock<ITroopStub> CreateStub(out Mock<ICity> stubCity)
        {
            stubCity = new Mock<ICity>();
            var stub = new Mock<ITroopStub>();
            stub.SetupGet(p => p.City).Returns(stubCity.Object);

            return stub;
        }
    }
}
