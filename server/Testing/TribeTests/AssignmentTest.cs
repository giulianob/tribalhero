using System;
using System.Collections.Generic;
using Common.Testing;
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
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;

namespace Testing.TribeTests
{
    public class AssignmentTest
    {
        private const uint TARGET_X = 5;

        private const uint TARGET_Y = 10;

        private static readonly DateTime targetTime = new DateTime(2012, 1, 1, 10, 1, 1, 1, DateTimeKind.Utc);

        private static readonly DateTime startTime = new DateTime(2012, 1, 1, 9, 1, 1, 1, DateTimeKind.Utc);
        
        public static IEnumerable<object[]> WhenSomeoneJoinsData
        {
            get
            {
                yield return new object[] {new[] {300, 150, 200}, targetTime.AddMinutes(-5)};
                yield return new object[] {new[] {150, 300, 250}, targetTime.AddMinutes(-5)};
                yield return new object[] {new[] {150, 300, 360}, targetTime.AddMinutes(-6)};
            }
        }

        /// <summary>
        ///     When an assignment is rescheduled for the first time
        ///     Then it should schedule itself immediatelly
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
            var radiusLocator = Substitute.For<ITileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            var locker = Substitute.For<ILocker>();
            var initializer = Substitute.For<ITroopObjectInitializerFactory>();

            SystemClock.SetClock(startTime);

            Assignment assignment = new Assignment(tribe,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   targetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   radiusLocator,
                                                   actionFactory,
                                                   locker,
                                                   initializer);
            assignment.Add(stub);
            assignment.Reschedule();

            scheduler.Received().Put(assignment);
            dbManager.Received().Save(assignment);
            assignment.Time.Should().Be(startTime);
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
            var radiusLocator = Substitute.For<ITileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            var locker = new LockerStub(gameObjectLocator);
            var initializer = Substitute.For<ITroopObjectInitializerFactory>();
            ICity newCity;
            var newStub = CreateStub(out newCity);
            ICity newCity2;
            var newStub2 = CreateStub(out newCity2);

            SystemClock.SetClock(startTime);

            Queue<int> moveTimeReturns = new Queue<int>();
            foreach (var moveTime in moveTimes)
            {
                moveTimeReturns.Enqueue(moveTime);
            }
            formula.MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true).Returns(x => moveTimeReturns.Dequeue());

            Assignment assignment = new Assignment(tribe,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   targetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   radiusLocator,
                                                   actionFactory,
                                                   locker,
                                                   initializer);
            assignment.Add(stub);
            assignment.Add(newStub);
            assignment.Add(newStub2);
            assignment.TroopCount.Should().Be(3);
            dbManager.Received(3).Save(assignment);
            scheduler.Received(3).Put(assignment);

            assignment.Callback(null);
            
            assignment.Time.Should().Be(expectedTime);            
        }
        
        /// <summary>
        /// Given original player has already been dispatched
        /// When new player joins then assignment should reschedule
        /// and dispatching the new troop should set the time to the target time
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
            var radiusLocator = Substitute.For<ITileLocator>();
            var actionFactory = Substitute.For<IActionFactory>();
            ICity newCity;
            var newStub = CreateStub(out newCity);
            var targetStructure = Substitute.For<IStructure>();
            var actionWorker = Substitute.For<IActionWorker>();
            var troopObject = Substitute.For<ITroopObject>();
            var initializer = Substitute.For<ITroopObjectInitializerFactory>();

            SystemClock.SetClock(startTime);

            troopObject.ObjectId.Returns((uint)95);

            gameObjectLocator.Regions.GetObjectsInTile(TARGET_X, TARGET_Y).Returns(new List<ISimpleGameObject> {targetStructure});

            targetStructure.City.Returns(targetCity);

            targetCity.Id.Returns((uint)100);

            actionWorker.DoPassive(Arg.Any<ICity>(), Arg.Any<PassiveAction>(), true).Returns(Error.Ok);

            stubCity.Worker.Returns(actionWorker);

            ITroopObject outTroopObject;
            procedure.When(x => x.TroopObjectCreate(stubCity, stub, out outTroopObject)).Do(x =>
                { x[2] = troopObject; });

            formula.MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true).Returns(300, 300, 120);

            Assignment assignment = new Assignment(tribe,
                                                   TARGET_X,
                                                   TARGET_Y,
                                                   targetCity,
                                                   AttackMode.Normal,
                                                   targetTime,
                                                   "Description",
                                                   true,
                                                   formula,
                                                   dbManager,
                                                   gameObjectLocator,
                                                   scheduler,
                                                   procedure,
                                                   radiusLocator,
                                                   actionFactory,
                                                   new LockerStub(gameObjectLocator),
                                                   initializer) { stub };

            SystemClock.SetClock(targetTime.AddSeconds(-90));

            // Dispatch first troop
            assignment.Callback(null);
            assignment.TroopCount.Should().Be(0);

            // Add new troop
            assignment.Add(newStub);
            assignment.TroopCount.Should().Be(1);
            assignment.Time.Should().Be(SystemClock.Now);

            assignment.Callback(null);
            assignment.Time.Should().Be(targetTime);
            assignment.TroopCount.Should().Be(0);
        }

        /// <summary>
        /// Given a troop joins an assignment
        /// And the troops speed changes before its dispatched
        /// Then the assignment should reschedule the troop until the new time once dispatched
        /// </summary>
        [Theory, AutoNSubstituteData]
        public void WhenTroopSpeedChanges(
            [Frozen] ICity targetCity, 
            [Frozen] IScheduler scheduler, 
            [Frozen] IGameObjectLocator gameObjectLocator, 
            [FrozenMock] Formula formula,
            [FrozenMock] Procedure procedure,
            IFixture fixture)
        {
            fixture.Register(() => targetTime);
            fixture.Register<ILocker>(() => new LockerStub(gameObjectLocator));

            ICity stubCity;
            var stub = CreateStub(out stubCity);            
            
            var targetStructure = Substitute.For<IStructure>();
            var troopObject = Substitute.For<ITroopObject>();

            SystemClock.SetClock(startTime);

            gameObjectLocator.Regions.GetObjectsInTile(0, 0).ReturnsForAnyArgs(new List<ISimpleGameObject> {targetStructure});

            stubCity.Worker.DoPassive(null, null, true).ReturnsForAnyArgs(Error.Ok);

            ITroopObject outTroopObject;
            procedure.When(x => x.TroopObjectCreate(stubCity, stub, out outTroopObject)).Do(x => { x[2] = troopObject; });

            formula.MoveTimeTotal(Arg.Any<ITroopStub>(), 0, true).ReturnsForAnyArgs(300, 120);

            // Execute
            var assignment = fixture.Create<Assignment>();
            assignment.Add(stub);

            SystemClock.SetClock(targetTime.AddSeconds(-300));

            // Dispatch troop
            assignment.Callback(null);

            scheduler.Received(2).Put(assignment);
            assignment.Time.Should().Be(targetTime.AddSeconds(-120));
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
            var gameObjectLocator = Substitute.For<IGameObjectLocator>();
            var targetStructure = Substitute.For<IStructure>();
            var actionWorker = Substitute.For<IActionWorker>();
            var troopObject = Substitute.For<ITroopObject>();            
            var troopInitializerFactory = Substitute.For<ITroopObjectInitializerFactory>();

            troopObject.ObjectId.Returns((uint)99);

            gameObjectLocator.Regions.GetObjectsInTile(TARGET_X, TARGET_Y).Returns(new List<ISimpleGameObject> {targetStructure});

            targetStructure.City.Returns(targetCity);

            targetCity.Id.Returns((uint)100);
            targetCity.LocationType.Returns(LocationType.City);
            targetCity.LocationId.Returns((uint)100);

            actionWorker.DoPassive(Arg.Any<ICity>(), Arg.Any<PassiveAction>(), true).Returns(Error.Ok);

            stubCity.Worker.Returns(actionWorker);
            stubCity.Id.Returns((uint)20);

            var troopInitializer = Substitute.For<ITroopObjectInitializer>();
            troopInitializerFactory.CreateAssignmentTroopObjectInitializer(troopObject, TroopBattleGroup.Defense, AttackMode.Strong).Returns(troopInitializer);

            SystemClock.SetClock(startTime);            

            // troop should be dispatched a minute later
            formula.MoveTimeTotal(stub, Arg.Any<int>(), false).Returns(300);

            ITroopObject outTroopObject;
            var procedure = Substitute.For<Procedure>();
            procedure.When(m => m.TroopObjectCreate(stubCity, stub, out outTroopObject)).Do(x =>
            {
                x[2] = troopObject;
            });

            var actionFactory = Substitute.For<IActionFactory>();

            var assignment = new Assignment(tribe,
                                            TARGET_X,
                                            TARGET_Y,
                                            targetCity,
                                            AttackMode.Strong,
                                            targetTime,
                                            "Description",
                                            false,
                                            formula,
                                            Substitute.For<IDbManager>(),
                                            gameObjectLocator,
                                            Substitute.For<IScheduler>(),
                                            procedure,
                                            Substitute.For<ITileLocator>(),
                                            actionFactory,
                                            new LockerStub(gameObjectLocator),
                                            troopInitializerFactory)
            {
                stub
            };

            assignment.Callback(null);
            assignment.Time.Should().Be(targetTime.AddMinutes(-5));

            SystemClock.SetClock(targetTime.AddSeconds(-90));

            // Dispatch first troop
            assignment.Callback(null);
            actionFactory.Received().CreateCityDefenseChainAction(20, troopInitializer, 100);
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