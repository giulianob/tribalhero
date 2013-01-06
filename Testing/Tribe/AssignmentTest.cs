using System;
using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Moq;
using Persistance;
using Xunit;
using Xunit.Extensions;

namespace Testing.Tribe
{
    public class AssignmentTest : IDisposable
    {
        private const uint TARGET_X = 5;

        private const uint TARGET_Y = 10;

        private static readonly DateTime targetTime = new DateTime(2012, 1, 1, 10, 1, 1, 1, DateTimeKind.Utc);

        private static readonly DateTime startTime = new DateTime(2012, 1, 1, 9, 1, 1, 1, DateTimeKind.Utc);

        public AssignmentTest()
        {
            Concurrency.Current = new Mock<ILocker>().Object;
        }

        public static IEnumerable<object[]> WhenSomeoneJoinsData
        {
            get
            {
                yield return new object[] {new[] {300, 150, 200}, targetTime.AddMinutes(-5)};
                yield return new object[] {new[] {150, 300, 250}, targetTime.AddMinutes(-5)};
                yield return new object[] {new[] {150, 300, 360}, targetTime.AddMinutes(-6)};
            }
        }

        public void Dispose()
        {
            Concurrency.Current = null;
        }

        /// <summary>
        ///     When an assignment is rescheduled for the first time
        ///     Then it should schedule itself at the proper time
        ///     And save itself
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
                                                   "Description",
                                                   true,
                                                   formula.Object,
                                                   dbManager.Object,
                                                   gameObjectLocator.Object,
                                                   scheduler.Object,
                                                   procedure.Object,
                                                   tileLocator.Object,
                                                   actionFactory.Object);
            assignment.Add(stub.Object);
            assignment.Reschedule();

            scheduler.Verify(m => m.Put(assignment));
            dbManager.Verify(m => m.Save(assignment));
            assignment.Time.Should().Be(targetTime.AddMinutes(-5));
        }

        /// <summary>
        ///     Given the assignment is already scheduled
        ///     When someone joins
        ///     Then it should remove itself from scheduler first
        ///     And reschedule itself at the right time
        ///     And save itself
        /// </summary>
        [Theory, PropertyData("WhenSomeoneJoinsData")]
        public void WhenSomeoneJoins(IEnumerable<int> moveTimes, DateTime expectedTime)
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
            Mock<ICity> newCity;
            Mock<ITroopStub> newStub = CreateStub(out newCity);
            Mock<ICity> newCity2;
            Mock<ITroopStub> newStub2 = CreateStub(out newCity2);

            SystemClock.SetClock(startTime);

            Queue<int> moveTimeReturns = new Queue<int>();
            foreach (var moveTime in moveTimes)
            {
                moveTimeReturns.Enqueue(moveTime);
            }
            formula.Setup(m => m.MoveTimeTotal(It.IsAny<ITroopStub>(), 0, true)).Returns(moveTimeReturns.Dequeue);

            Assignment assignment = new Assignment(tribe.Object,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity.Object,
                                                   AttackMode.Normal,
                                                   targetTime,
                                                   "Description",
                                                   true,
                                                   formula.Object,
                                                   dbManager.Object,
                                                   gameObjectLocator.Object,
                                                   scheduler.Object,
                                                   procedure.Object,
                                                   tileLocator.Object,
                                                   actionFactory.Object);
            assignment.Add(stub.Object);
            assignment.Add(newStub.Object);
            assignment.Add(newStub2.Object);

            scheduler.Verify(m => m.Put(assignment), Times.Exactly(3));
            assignment.Time.Should().Be(expectedTime);
        }

        // Given original player has already been dispatched
        // When new player joins
        // Then assignment should reschedule
        [Fact]
        public void WhenSomeoneAlreadyDispatched()
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
            Mock<ICity> newCity;
            Mock<ITroopStub> newStub = CreateStub(out newCity);
            Mock<IStructure> targetStructure = new Mock<IStructure>();
            Mock<IActionWorker> actionWorker = new Mock<IActionWorker>();
            Mock<ITroopObject> troopObject = new Mock<ITroopObject>();

            SystemClock.SetClock(startTime);

            troopObject.SetupGet(p => p.ObjectId).Returns(95);

            gameObjectLocator.Setup(m => m.GetObjects(TARGET_X, TARGET_Y))
                             .Returns(new List<ISimpleGameObject> {targetStructure.Object});

            targetStructure.SetupGet(p => p.City).Returns(targetCity.Object);

            targetCity.SetupGet(p => p.Id).Returns(100);

            actionWorker.Setup(m => m.DoPassive(It.IsAny<ICity>(), It.IsAny<PassiveAction>(), true)).Returns(Error.Ok);

            stubCity.SetupGet(p => p.Worker).Returns(actionWorker.Object);

            // ReSharper disable RedundantAssignment
            ITroopObject outTroopObject = troopObject.Object;
            // ReSharper restore RedundantAssignment
            procedure.Setup(m => m.TroopObjectCreate(stubCity.Object, stub.Object, out outTroopObject));

            Queue<int> moveTimes = new Queue<int>();
            moveTimes.Enqueue(300);
            moveTimes.Enqueue(120);
            formula.Setup(m => m.MoveTimeTotal(It.IsAny<ITroopStub>(), 0, true)).Returns(moveTimes.Dequeue);

            Assignment assignment = new Assignment(tribe.Object,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity.Object,
                                                   AttackMode.Normal,
                                                   targetTime,
                                                   "Description",
                                                   true,
                                                   formula.Object,
                                                   dbManager.Object,
                                                   gameObjectLocator.Object,
                                                   scheduler.Object,
                                                   procedure.Object,
                                                   tileLocator.Object,
                                                   actionFactory.Object) {stub.Object};

            SystemClock.SetClock(targetTime.AddSeconds(-90));

            // Dispatch first troop
            assignment.Callback(null);

            // Add new troop
            assignment.Add(newStub.Object);

            scheduler.Verify(m => m.Put(assignment), Times.Exactly(3));
            assignment.Time.Should().Be(targetTime.AddSeconds(-120));
        }

        /// <summary>
        ///     When an defensive assignment is rescheduled for the first time
        ///     Then it should schedule itself at the proper time
        ///     And save itself
        /// </summary>
        [Fact]
        public void WhenDefensiveAssignmentScheduled()
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
            Mock<IStructure> targetStructure = new Mock<IStructure>();
            Mock<IActionWorker> actionWorker = new Mock<IActionWorker>();
            Mock<ITroopObject> troopObject = new Mock<ITroopObject>();

            troopObject.SetupGet(p => p.ObjectId).Returns(99);

            gameObjectLocator.Setup(m => m.GetObjects(TARGET_X, TARGET_Y))
                             .Returns(new List<ISimpleGameObject> {targetStructure.Object});

            targetStructure.SetupGet(p => p.City).Returns(targetCity.Object);

            targetCity.SetupGet(p => p.Id).Returns(100);
            targetCity.SetupGet(p => p.LocationType).Returns(LocationType.City);
            targetCity.SetupGet(p => p.LocationId).Returns(100);

            actionWorker.Setup(m => m.DoPassive(It.IsAny<ICity>(), It.IsAny<PassiveAction>(), true)).Returns(Error.Ok);

            stubCity.SetupGet(p => p.Worker).Returns(actionWorker.Object);
            stubCity.SetupGet(p => p.Id).Returns(20);

            SystemClock.SetClock(startTime);

            // troop should be dispatched a minute later
            formula.Setup(m => m.MoveTimeTotal(stub.Object, It.IsAny<int>(), true)).Returns(300);

            // ReSharper disable RedundantAssignment
            ITroopObject outTroopObject = troopObject.Object;
            // ReSharper restore RedundantAssignment
            procedure.Setup(m => m.TroopObjectCreate(stubCity.Object, stub.Object, out outTroopObject));

            Assignment assignment = new Assignment(tribe.Object,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity.Object,
                                                   AttackMode.Strong,
                                                   targetTime,
                                                   "Description",
                                                   false,
                                                   formula.Object,
                                                   dbManager.Object,
                                                   gameObjectLocator.Object,
                                                   scheduler.Object,
                                                   procedure.Object,
                                                   tileLocator.Object,
                                                   actionFactory.Object) {stub.Object};
            assignment.Reschedule();
            assignment.Time.Should().Be(targetTime.AddMinutes(-5));

            SystemClock.SetClock(targetTime.AddSeconds(-90));

            // Dispatch first troop
            assignment.Callback(null);
            actionFactory.Verify(m => m.CreateCityDefenseChainAction(20, 99, 100, AttackMode.Strong));
        }

        private Mock<ITroopStub> CreateStub(out Mock<ICity> stubCity)
        {
            stubCity = new Mock<ICity>();
            var stub = new Mock<ITroopStub>();
            stub.SetupGet(p => p.City).Returns(stubCity.Object);

            return stub;
        }
    }
}