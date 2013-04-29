using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
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

namespace Testing.ActionsTests
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

        private ITroopObject MockTroopObject(ITroopStub troopStub)
        {
            var troopObject = new Mock<ITroopObject>();
            troopObject.SetupGet(p => p.Stub).Returns(troopStub);
            troopObject.SetupGet(p => p.City).Returns(troopStub.City);
            return troopObject.Object;
        }

        private CityOffensiveCombatGroup CreateCityOffensiveCombatGroup(ITroopObject troopObject)
        {
            return new CityOffensiveCombatGroup(1, 1, troopObject, new Mock<IDbManager>().Object);
        }

        #region AP Tests

        [Fact]
        public void WhenDefenderStrongerShouldNotGiveAnyAp()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var battleProcedure = new Mock<BattleProcedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var cityBattleProcedure = new Mock<CityBattleProcedure>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();
            var world = new Mock<IWorld>();

            battleManager.SetupGet(p => p.BattleId).Returns(1);

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(1000);

            // Attacker Stubs
            var attackerStub1 = MockStub(101);
            var attackerStub2 = MockStub(74);

            // Attacker Troop objects
            var attackerTroop1 = MockTroopObject(attackerStub1);
            var attackerTroop2 = MockTroopObject(attackerStub2);

            // Setup groups
            var attackerGroup1 = CreateCityOffensiveCombatGroup(attackerTroop1);
            var attackerGroup2 = CreateCityOffensiveCombatGroup(attackerTroop2);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(150);
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackerGroup1, attackerGroup2}.GetEnumerator());

            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator())
                       .Returns(() => new List<ITroopStub> {defaultStub.Object}.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new CityBattlePassiveAction(1,
                                                     actionFactory.Object,
                                                     battleProcedure.Object,
                                                     locker.Object,
                                                     gameObjectLocator.Object,
                                                     dbManager.Object,
                                                     formula.Object,
                                                     cityBattleProcedure.Object,
                                                     world.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50m);
            attackerStub1.City.AlignmentPoint.Should().Be(30m);
            attackerStub2.City.AlignmentPoint.Should().Be(30m);
        }

        [Fact]
        public void WhenEvenShouldNotGiveAnyAp()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var battleProcedure = new Mock<BattleProcedure>();
            var cityBattleProcedure = new Mock<CityBattleProcedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();
            var world = new Mock<IWorld>();

            battleManager.SetupGet(p => p.BattleId).Returns(1);

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(175);

            // Attacker Stubs
            var attackerStub1 = MockStub(101);
            var attackerStub2 = MockStub(74);

            // Attacker Troop objects
            var attackerTroop1 = MockTroopObject(attackerStub1);
            var attackerTroop2 = MockTroopObject(attackerStub2);

            // Setup groups
            var attackerGroup1 = CreateCityOffensiveCombatGroup(attackerTroop1);
            var attackerGroup2 = CreateCityOffensiveCombatGroup(attackerTroop2);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(150);            
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackerGroup1, attackerGroup2}.GetEnumerator());

            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator())
                       .Returns(() => new List<ITroopStub> {defaultStub.Object}.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new CityBattlePassiveAction(1,
                                                     actionFactory.Object,
                                                     battleProcedure.Object,
                                                     locker.Object,
                                                     gameObjectLocator.Object,
                                                     dbManager.Object,
                                                     formula.Object,
                                                     cityBattleProcedure.Object,
                                                     world.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50m);
            attackerStub1.City.AlignmentPoint.Should().Be(30m);
            attackerStub2.City.AlignmentPoint.Should().Be(30m);
        }

        [Fact]
        public void WhenAttackerTwiceAsStrongShouldGiveAp()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var battleProcedure = new Mock<BattleProcedure>();
            var formula = new Mock<Formula>();
            var cityBattleProcedure = new Mock<CityBattleProcedure>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var battleManager = new Mock<IBattleManager>();
            var world = new Mock<IWorld>();

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(175);

            // Attacker Stubs
            var attackerStub1 = MockStub(100);
            var attackerStub2 = MockStub(200);

            // Attacker Troop objects
            var attackerTroop1 = MockTroopObject(attackerStub1);
            var attackerTroop2 = MockTroopObject(attackerStub2);

            // Setup groups
            var attackerGroup1 = CreateCityOffensiveCombatGroup(attackerTroop1);
            var attackerGroup2 = CreateCityOffensiveCombatGroup(attackerTroop2);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(300);
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackerGroup1, attackerGroup2}.GetEnumerator());

            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator())
                       .Returns(() => new List<ITroopStub> {defaultStub.Object}.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new CityBattlePassiveAction(1,
                                                     actionFactory.Object,
                                                     battleProcedure.Object,
                                                     locker.Object,
                                                     gameObjectLocator.Object,
                                                     dbManager.Object,
                                                     formula.Object,
                                                     cityBattleProcedure.Object,
                                                     world.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50.05m);
            attackerStub1.City.AlignmentPoint.Should().Be(29.983333333333333333333333333m);
            attackerStub2.City.AlignmentPoint.Should().Be(29.966666666666666666666666667m);
        }

        [Fact]
        public void WhenHigherThanMaxShouldCap()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var battleProcedure = new Mock<BattleProcedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var cityBattleProcedure = new Mock<CityBattleProcedure>();
            var battleManager = new Mock<IBattleManager>();
            var world = new Mock<IWorld>();

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(125);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(25);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(175);

            // Attacker Stubs
            var attackerStub1 = MockStub(100);
            var attackerStub2 = MockStub(1000);

            // Attacker Troop objects
            var attackerTroop1 = MockTroopObject(attackerStub1);
            var attackerTroop2 = MockTroopObject(attackerStub2);

            // Setup groups
            var attackerGroup1 = CreateCityOffensiveCombatGroup(attackerTroop1);
            var attackerGroup2 = CreateCityOffensiveCombatGroup(attackerTroop2);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(1100);
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackerGroup1, attackerGroup2}.GetEnumerator());

            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator())
                       .Returns(() => new List<ITroopStub> {defaultStub.Object}.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new CityBattlePassiveAction(1,
                                                     actionFactory.Object,
                                                     battleProcedure.Object,
                                                     locker.Object,
                                                     gameObjectLocator.Object,
                                                     dbManager.Object,
                                                     formula.Object,
                                                     cityBattleProcedure.Object,
                                                     world.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50.2m);
            attackerStub1.City.AlignmentPoint.Should().Be(29.981818181818181818181818182m);
            attackerStub2.City.AlignmentPoint.Should().Be(29.818181818181818181818181818m);
        }

        [Fact]
        public void WhenNoDefenderShouldUseCap()
        {
            Config.ap_max_per_battle = 4;

            var actionFactory = new Mock<IActionFactory>();
            var battleProcedure = new Mock<BattleProcedure>();
            var formula = new Mock<Formula>();
            var locker = new Mock<ILocker>();
            var gameObjectLocator = new Mock<IGameObjectLocator>();
            var dbManager = new Mock<IDbManager>();
            var cityBattleProcedure = new Mock<CityBattleProcedure>();
            var battleManager = new Mock<IBattleManager>();
            var world = new Mock<IWorld>();

            // Local city
            var defaultStub = new Mock<ITroopStub>();
            defaultStub.SetupGet(p => p.Upkeep).Returns(0);
            defaultStub.Setup(p => p.UpkeepForFormation(FormationType.InBattle)).Returns(0);

            // Defender Stubs                       
            var defenders = new Mock<ICombatList>();
            defenders.SetupGet(p => p.Upkeep).Returns(0);

            // Attacker Stubs
            var attackerStub1 = MockStub(100);
            var attackerStub2 = MockStub(1000);

            // Attacker Troop objects
            var attackerTroop1 = MockTroopObject(attackerStub1);
            var attackerTroop2 = MockTroopObject(attackerStub2);

            // Setup groups
            var attackerGroup1 = CreateCityOffensiveCombatGroup(attackerTroop1);
            var attackerGroup2 = CreateCityOffensiveCombatGroup(attackerTroop2);

            var attackers = new Mock<ICombatList>();
            attackers.SetupGet(p => p.Upkeep).Returns(1100);
            attackers.Setup(p => p.GetEnumerator())
                     .Returns(() => new List<ICombatGroup> {attackerGroup1, attackerGroup2}.GetEnumerator());

            // Local city + troops
            var localTroops = new Mock<ITroopManager>();
            localTroops.Setup(p => p.GetEnumerator())
                       .Returns(() => new List<ITroopStub> {defaultStub.Object}.GetEnumerator());
            var localCity = new Mock<ICity>();
            localCity.SetupGet(p => p.DefaultTroop).Returns(defaultStub.Object);
            localCity.SetupGet(p => p.Troops).Returns(localTroops.Object);
            localCity.SetupProperty(p => p.AlignmentPoint, 50m);
            localCity.SetupGet(p => p.Battle).Returns(battleManager.Object);
            // ReSharper disable RedundantAssignment
            ICity localCityObj = localCity.Object;
            // ReSharper restore RedundantAssignment
            gameObjectLocator.Setup(p => p.TryGetObjects(1, out localCityObj)).Returns(true);

            var action = new CityBattlePassiveAction(1,
                                                     actionFactory.Object,
                                                     battleProcedure.Object,
                                                     locker.Object,
                                                     gameObjectLocator.Object,
                                                     dbManager.Object,
                                                     formula.Object,
                                                     cityBattleProcedure.Object,
                                                     world.Object);

            action.BattleEnterRound(battleManager.Object, attackers.Object, defenders.Object, 1);

            localCity.Object.AlignmentPoint.Should().Be(50.2m);
            attackerStub1.City.AlignmentPoint.Should().Be(29.981818181818181818181818182m);
            attackerStub2.City.AlignmentPoint.Should().Be(29.818181818181818181818181818m);
        }

        #endregion
    }
}