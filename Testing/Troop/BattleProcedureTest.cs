#region

using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Moq;
using Xunit;

#endregion

namespace Testing.Troop
{
    public class BattleProcedureTest
    {
        [Fact]
        public void TestMoveFromBattleToNormal()
        {
            Mock<RadiusLocator> radiusLocator = new Mock<RadiusLocator>();
            Mock<IBattleManagerFactory> battleManagerFactory = new Mock<IBattleManagerFactory>();
            Mock<IActionFactory> actionFactory = new Mock<IActionFactory>();
            Mock<ICombatUnitFactory> combatUnitFactory = new Mock<ICombatUnitFactory>();
            Mock<ICombatGroupFactory> combatGroupFactory = new Mock<ICombatGroupFactory>();
            Mock<ObjectTypeFactory> objectTypeFactory = new Mock<ObjectTypeFactory>();
            Mock<Formula> formula = new Mock<Formula>();

            var stub = new TroopStub(0, null);
            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);

            stub.AddUnit(FormationType.Normal, 101, 10);

            var battleProcedure = new BattleProcedure(combatUnitFactory.Object,
                                                      combatGroupFactory.Object,
                                                      radiusLocator.Object,
                                                      battleManagerFactory.Object,
                                                      actionFactory.Object,
                                                      objectTypeFactory.Object,
                                                      formula.Object);
            battleProcedure.MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            battleProcedure.MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.True(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.True(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }
    }
}