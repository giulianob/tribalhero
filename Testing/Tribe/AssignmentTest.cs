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
using NSubstitute;
using Persistance;
using Xunit;
using Xunit.Extensions;

namespace Testing.Tribe
{
    public class AssignmentTest
    {
        private const uint TargetX = 5;

        private const uint TargetY = 10;

        private static readonly DateTime TargetTime = new DateTime(2012, 1, 1, 10, 1, 1, 1, DateTimeKind.Utc);

        private static readonly DateTime StartTime = new DateTime(2012, 1, 1, 9, 1, 1, 1, DateTimeKind.Utc);
        
        public static IEnumerable<object[]> WhenSomeoneJoinsData
        {
            get
            {
                yield return new object[] {new[] {300, 150, 200}, TargetTime.AddMinutes(-5)};
                yield return new object[] {new[] {150, 300, 250}, TargetTime.AddMinutes(-5)};
                yield return new object[] {new[] {150, 300, 360}, TargetTime.AddMinutes(-6)};
            }
        }

        /// <summary>
        ///     When an assignment is rescheduled for the first time
        ///     Then it should schedule itself at the proper time
        ///     And save itself
        /// </summary>
        [Fact]
        public void WhenScheduled()
        {
            var tribe = Substitute.For<ITribe>();
            var targetCity = Substitute.For<ICity>();
            ICity stubCity;
            var stub = CreateStub(out stubCity);
            var formula = Substitute.For<Formula>();
            var dbManager = Substitute.For<IDbManager>();
            var gameObjectLocator = Substitute.For<IGameObjectLocator>();
            var scheduler = Substitute.For<IScheduler>();
            var procedure = Substitute.For<Procedure>();
            var tileLocator = Substitute.For<TileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            var locker = Substitute.For<ILocker>();

            SystemClock.SetClock(StartTime);

            // troop should be dispatched a minute later
            formula.MoveTimeTotal(stub, Arg.Any<int>(), true).Returns(300);

            Assignment assignment = new Assignment(tribe,
                                                   TargetX,
                                                   TargetY,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   TargetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   tileLocator,
                                                   actionFactory,
                                                   locker) {stub};
            assignment.Reschedule();

            scheduler.Received().Put(assignment);
            dbManager.Received().Save(assignment);
            assignment.Time.Should().Be(TargetTime.AddMinutes(-5));
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
            var tribe = Substitute.For<ITribe>();
            var targetCity = Substitute.For<ICity>();
            ICity stubCity;
            var stub = CreateStub(out stubCity);
            var formula = Substitute.For<Formula>();
            var dbManager = Substitute.For<IDbManager>();
            var gameObjectLocator = Substitute.For<IGameObjectLocator>();
            var scheduler = Substitute.For<IScheduler>();
            var procedure = Substitute.For<Procedure>();
            var tileLocator = Substitute.For<TileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            var locker = Substitute.For<ILocker>();
            ICity newCity;
            var newStub = CreateStub(out newCity);
            ICity newCity2;
            var newStub2 = CreateStub(out newCity2);

            SystemClock.SetClock(StartTime);

            Queue<int> moveTimeReturns = new Queue<int>();
            foreach (var moveTime in moveTimes)
            {
                moveTimeReturns.Enqueue(moveTime);
            }
            formula.MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true).Returns(x => moveTimeReturns.Dequeue());

            Assignment assignment = new Assignment(tribe,
                                                   TargetX,
                                                   TargetY,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   TargetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   tileLocator,
                                                   actionFactory,
                                                   locker) {stub, newStub, newStub2};

            scheduler.Received(3).Put(assignment);
            assignment.Time.Should().Be(expectedTime);
        }


        /// <summary>
        /// Given original player has already been dispatched
        /// When new player joins
        /// Then assignment should reschedule
        /// </summary>
        [Fact]
        public void WhenSomeoneAlreadyDispatched()
        {
            var tribe = Substitute.For<ITribe>();
            var targetCity = Substitute.For<ICity>();
            ICity stubCity;
            var stub = CreateStub(out stubCity);
            var formula = Substitute.For<Formula>();
            var dbManager = Substitute.For<IDbManager>();
            var gameObjectLocator = Substitute.For<IGameObjectLocator>();
            var scheduler = Substitute.For<IScheduler>();
            var procedure = Substitute.For<Procedure>();
            var tileLocator = Substitute.For<TileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            ICity newCity;
            var newStub = CreateStub(out newCity);
            var targetStructure = Substitute.For<IStructure>();
            var actionWorker = Substitute.For<IActionWorker>();
            var troopObject = Substitute.For<ITroopObject>();
            var locker = Substitute.For<ILocker>();

            SystemClock.SetClock(StartTime);

            troopObject.ObjectId.Returns((uint)95);

            gameObjectLocator.GetObjects(TargetX, TargetY).Returns(new List<ISimpleGameObject> {targetStructure});

            targetStructure.City.Returns(targetCity);

            targetCity.Id.Returns((uint)100);

            actionWorker.DoPassive(Arg.Any<ICity>(), Arg.Any<PassiveAction>(), true).Returns(Error.Ok);

            stubCity.Worker.Returns(actionWorker);

            ITroopObject outTroopObject;
            procedure.When(x => x.TroopObjectCreate(stubCity, stub, out outTroopObject)).Do(x =>
                { x[2] = troopObject; });

            formula.MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true).Returns(300, 300, 120);

            Assignment assignment = new Assignment(tribe,
                                                   TargetX,
                                                   TargetY,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   TargetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   tileLocator,
                                                   actionFactory,
                                                   locker) {stub};

            SystemClock.SetClock(TargetTime.AddSeconds(-90));

            // Dispatch first troop
            assignment.Callback(null);

            // Add new troop
            assignment.Add(newStub);

            formula.Received(3).MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true);
            scheduler.Received(3).Put(assignment);
            assignment.Time.Should().Be(TargetTime.AddSeconds(-120));
        }

        /// <summary>
        /// Given a troop joins an assignment
        /// And the troops speed changes before its dispatched
        /// Then the assignment should reschedule the troop until the new time once dispatched
        /// </summary>
        [Fact]
        public void WhenTroopSpeedChanges()
        {
            var tribe = Substitute.For<ITribe>();
            var targetCity = Substitute.For<ICity>();
            ICity stubCity;
            var stub = CreateStub(out stubCity);
            var formula = Substitute.For<Formula>();
            var dbManager = Substitute.For<IDbManager>();
            var gameObjectLocator = Substitute.For<IGameObjectLocator>();
            var scheduler = Substitute.For<IScheduler>();
            var procedure = Substitute.For<Procedure>();
            var tileLocator = Substitute.For<TileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            
            var targetStructure = Substitute.For<IStructure>();
            var actionWorker = Substitute.For<IActionWorker>();
            var troopObject = Substitute.For<ITroopObject>();
            var locker = Substitute.For<ILocker>();

            SystemClock.SetClock(StartTime);

            troopObject.ObjectId.Returns((uint)95);

            gameObjectLocator.GetObjects(TargetX, TargetY).Returns(new List<ISimpleGameObject> {targetStructure});

            targetStructure.City.Returns(targetCity);

            targetCity.Id.Returns((uint)100);

            actionWorker.DoPassive(Arg.Any<ICity>(), Arg.Any<PassiveAction>(), true).Returns(Error.Ok);

            stubCity.Worker.Returns(actionWorker);

            ITroopObject outTroopObject;
            procedure.When(x => x.TroopObjectCreate(stubCity, stub, out outTroopObject)).Do(x =>
                { x[2] = troopObject; });

            formula.MoveTimeTotal(stub, 0, true).Returns(300, 120);

            Assignment assignment = new Assignment(tribe,
                                                   TargetX,
                                                   TargetY,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   TargetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   tileLocator,
                                                   actionFactory,
                                                   locker) {stub};

            SystemClock.SetClock(TargetTime.AddSeconds(-300));

            // Dispatch troop
            assignment.Callback(null);

            formula.Received(2).MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true);
            scheduler.Received(2).Put(assignment);
            assignment.Time.Should().Be(TargetTime.AddSeconds(-120));
            procedure.Received(0).TroopObjectCreate(Arg.Any<ICity>(), Arg.Any<ITroopStub>(), out troopObject);
        }

        /// <summary>
        ///     When an defensive assignment is rescheduled for the first time
        ///     Then it should schedule itself at the proper time
        ///     And save itself
        /// </summary>
        [Fact]
        public void WhenDefensiveAssignmentScheduled()
        {
            var tribe = Substitute.For<ITribe>();
            var targetCity = Substitute.For<ICity>();
            ICity stubCity;
            var stub = CreateStub(out stubCity);
            var formula = Substitute.For<Formula>();
            var dbManager = Substitute.For<IDbManager>();
            var gameObjectLocator = Substitute.For<IGameObjectLocator>();
            var scheduler = Substitute.For<IScheduler>();
            var procedure = Substitute.For<Procedure>();
            var tileLocator = Substitute.For<TileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            var targetStructure = Substitute.For<IStructure>();
            var actionWorker = Substitute.For<IActionWorker>();
            var troopObject = Substitute.For<ITroopObject>();
            var locker = Substitute.For<ILocker>();

            troopObject.ObjectId.Returns((uint)99);

            gameObjectLocator.GetObjects(TargetX, TargetY).Returns(new List<ISimpleGameObject> {targetStructure});

            targetStructure.City.Returns(targetCity);

            targetCity.Id.Returns((uint)100);
            targetCity.LocationType.Returns(LocationType.City);
            targetCity.LocationId.Returns((uint)100);

            actionWorker.DoPassive(Arg.Any<ICity>(), Arg.Any<PassiveAction>(), true).Returns(Error.Ok);

            stubCity.Worker.Returns(actionWorker);
            stubCity.Id.Returns((uint)20);

            SystemClock.SetClock(StartTime);

            // troop should be dispatched a minute later
            formula.MoveTimeTotal(stub, Arg.Any<int>(), true).Returns(300);

            ITroopObject outTroopObject;
            procedure.When(m => m.TroopObjectCreate(stubCity, stub, out outTroopObject)).Do(x =>
                { x[2] = troopObject; });

            Assignment assignment = new Assignment(tribe,
                                                   TargetX,
                                                   TargetY,
                                                   targetCity,
                                                   AttackMode.Strong,
                                                   TargetTime,
                                                   "Description",
                                                   false,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   tileLocator,
                                                   actionFactory,
                                                   locker) {stub};
            assignment.Reschedule();
            assignment.Time.Should().Be(TargetTime.AddMinutes(-5));

            SystemClock.SetClock(TargetTime.AddSeconds(-90));

            // Dispatch first troop
            assignment.Callback(null);
            actionFactory.Received().CreateCityDefenseChainAction(20, 99, 100, AttackMode.Strong);
        }

        private ITroopStub CreateStub(out ICity stubCity)
        {
            stubCity = Substitute.For<ICity>();
            var stub = Substitute.For<ITroopStub>();
            stub.City.Returns(stubCity);

            return stub;
        }
    }
}