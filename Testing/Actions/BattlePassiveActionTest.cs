using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Moq;
using Persistance;
using Xunit;

namespace Testing.Actions
{
    public class BattlePassiveActionTest
    {
        private ITroopStub MockStub(int upkeep)
        {
            var stub = new Mock<ITroopStub>();
            stub.SetupProperty(p => p.City.AlignmentPoint, 30m);
            stub.SetupGet(p => p.Upkeep).Returns(upkeep);
            return stub.Object;
        }

        private CombatObject MockCombatObject(ITroopStub stub)
        {
            var co = new Mock<CombatObject>();
            co.SetupGet(p => p.TroopStub).Returns(stub);
            return co.Object;
        }

        [Fact]
        public void WhenDefenderStrongerShouldNotGiveAnyAp()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var procedure = new Mock<Procedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();

            battleManager.SetupGet(p => p.BattleId).Returns(1);

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(1000);

            // Attacker Stubs
            var attacker1 = MockStub(101);
            var attacker2 = MockStub(74);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(150);
            attackers.Setup(p => p.GetEnumerator()).Returns(() => new List<CombatObject>
                                                                  {
                                                                      // Simulate multiple CO for the same stub
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                  }.GetEnumerator());


            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator()).Returns(() => new List<ITroopStub> { defaultStub.Object }.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);


            var action = new BattlePassiveAction(1,
                                                                 actionFactory.Object,
                                                                 procedure.Object,
                                                                 locker.Object,
                                                                 gameObjectLocator.Object,
                                                                 dbManager.Object,
                                                                 formula.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50m);
            attacker1.City.AlignmentPoint.Should().Be(30m);
            attacker2.City.AlignmentPoint.Should().Be(30m);
        }

        [Fact]
        public void WhenEvenShouldNotGiveAnyAp()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var procedure = new Mock<Procedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();

            battleManager.SetupGet(p => p.BattleId).Returns(1);

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(175);

            // Attacker Stubs
            var attacker1 = MockStub(101);
            var attacker2 = MockStub(74);
            
            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(150);
            attackers.Setup(p => p.GetEnumerator()).Returns(() => new List<CombatObject>
                                                                  {
                                                                      // Simulate multiple CO for the same stub
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                  }.GetEnumerator());

            
            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator()).Returns(() => new List<ITroopStub> {defaultStub.Object}.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
// ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
// ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);
            

            var action = new BattlePassiveAction(1,
                                                                 actionFactory.Object,
                                                                 procedure.Object,
                                                                 locker.Object,
                                                                 gameObjectLocator.Object,
                                                                 dbManager.Object,
                                                                 formula.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50m);
            attacker1.City.AlignmentPoint.Should().Be(30m);
            attacker2.City.AlignmentPoint.Should().Be(30m);
        }

        [Fact]
        public void WhenAttackerTwiceAsStrongShouldGiveAp()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var procedure = new Mock<Procedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(175);

            // Attacker Stubs
            var attacker1 = MockStub(100);
            var attacker2 = MockStub(200);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(300);
            attackers.Setup(p => p.GetEnumerator()).Returns(() => new List<CombatObject>
                                                                  {
                                                                      // Simulate multiple CO for the same stub
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                  }.GetEnumerator());


            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator()).Returns(() => new List<ITroopStub> { defaultStub.Object }.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new BattlePassiveAction(1,
                                                 actionFactory.Object,
                                                 procedure.Object,
                                                 locker.Object,
                                                 gameObjectLocator.Object,
                                                 dbManager.Object,
                                                 formula.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50.05m);
            attacker1.City.AlignmentPoint.Should().Be(29.983333333333333333333333333m);
            attacker2.City.AlignmentPoint.Should().Be(29.966666666666666666666666667m);
        }

        [Fact]
        public void WhenHigherThanMaxShouldCap()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var procedure = new Mock<Procedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(175);

            // Attacker Stubs
            var attacker1 = MockStub(100);
            var attacker2 = MockStub(1000);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(1100);
            attackers.Setup(p => p.GetEnumerator()).Returns(() => new List<CombatObject>
                                                                  {
                                                                      // Simulate multiple CO for the same stub
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                  }.GetEnumerator());


            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator()).Returns(() => new List<ITroopStub> { defaultStub.Object }.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new BattlePassiveAction(1,
                                                 actionFactory.Object,
                                                 procedure.Object,
                                                 locker.Object,
                                                 gameObjectLocator.Object,
                                                 dbManager.Object,
                                                 formula.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50.2m);
            attacker1.City.AlignmentPoint.Should().Be(29.981818181818181818181818182m);
            attacker2.City.AlignmentPoint.Should().Be(29.818181818181818181818181818m);
        }

        [Fact]
        public void WhenNoDefenderShouldUseCap()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var procedure = new Mock<Procedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(0);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(0);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(0);

            // Attacker Stubs
            var attacker1 = MockStub(100);
            var attacker2 = MockStub(1000);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(1100);
            attackers.Setup(p => p.GetEnumerator()).Returns(() => new List<CombatObject>
                                                                  {
                                                                      // Simulate multiple CO for the same stub
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker1),
                                                                          MockCombatObject(attacker2),
                                                                  }.GetEnumerator());


            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator()).Returns(() => new List<ITroopStub> { defaultStub.Object }.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new BattlePassiveAction(1,
                                                 actionFactory.Object,
                                                 procedure.Object,
                                                 locker.Object,
                                                 gameObjectLocator.Object,
                                                 dbManager.Object,
                                                 formula.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50.2m);
            attacker1.City.AlignmentPoint.Should().Be(29.981818181818181818181818182m);
            attacker2.City.AlignmentPoint.Should().Be(29.818181818181818181818181818m);
        }

    }
}
