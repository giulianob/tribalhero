using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    public class StaminaMonitorTests
    {
        [Fact]
        public void TestStaminaDoesntGoBelowZero()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());            
            var staminaMonitor = fixture.Create<StaminaMonitor>();
            staminaMonitor.Stamina = -10;
            staminaMonitor.Stamina.Should().Be(0);
            staminaMonitor.Stamina = 1;
            staminaMonitor.Stamina.Should().Be(1);
            staminaMonitor.Stamina--;
            staminaMonitor.Stamina.Should().Be(0);
            staminaMonitor.Stamina--;
            staminaMonitor.Stamina.Should().Be(0);
        }

        [Fact]
        public void RemovesTroopWhenEnterRoundAndStaminaIsZero()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var battleManager = fixture.Freeze<IBattleManager>();
            var combatGroup = fixture.Freeze<ICombatGroup>();
            var staminaMonitor = fixture.Create<StaminaMonitor>();
            staminaMonitor.Stamina = 2;
            battleManager.EnterRound += Raise.Event<BattleManager.OnRound>(battleManager,
                                                                           Substitute.For<ICombatList>(),
                                                                           Substitute.For<ICombatList>(),
                                                                           1u);                        
            staminaMonitor.Stamina.Should().Be(1);
            battleManager.DidNotReceiveWithAnyArgs().Remove(null, BattleManager.BattleSide.Attack, ReportState.Entering);           

            battleManager.EnterRound += Raise.Event<BattleManager.OnRound>(battleManager,
                                                                           Substitute.For<ICombatList>(),
                                                                           Substitute.For<ICombatList>(),
                                                                           1u);                        
            battleManager.Received(1).Remove(combatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);           
        }

        [Fact]
        public void RemovesTroopWhenExitTurnAndStaminaIsZero()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var battleManager = fixture.Freeze<IBattleManager>();
            var combatGroup = fixture.Freeze<ICombatGroup>();
            var staminaMonitor = fixture.Create<StaminaMonitor>();
            staminaMonitor.Stamina = 1;
            battleManager.ExitTurn += Raise.Event<BattleManager.OnTurn>(battleManager,
                                                                           Substitute.For<ICombatList>(),
                                                                           Substitute.For<ICombatList>(),
                                                                           1);                        
            staminaMonitor.Stamina.Should().Be(1);
            battleManager.DidNotReceiveWithAnyArgs().Remove(null, BattleManager.BattleSide.Attack, ReportState.Entering);
            
            staminaMonitor.Stamina = 0;

            battleManager.ExitTurn += Raise.Event<BattleManager.OnTurn>(battleManager,
                                                                           Substitute.For<ICombatList>(),
                                                                           Substitute.For<ICombatList>(),
                                                                           2);                        
            battleManager.Received(1).Remove(combatGroup, BattleManager.BattleSide.Attack, ReportState.OutOfStamina);           
        }

        [Theory]
        [InlineData(BattleManager.BattleSide.Defense, BattleClass.Structure, true, false, 10)]
        [InlineData(BattleManager.BattleSide.Attack, BattleClass.Unit, true, false, 10)]
        [InlineData(BattleManager.BattleSide.Attack, BattleClass.Structure, false, false, 10)]
        [InlineData(BattleManager.BattleSide.Attack, BattleClass.Structure, true, false, 7)]
        [InlineData(BattleManager.BattleSide.Attack, BattleClass.Structure, true, true, 10)]        
        public void TestReduceStaminaWhenStructureDestroyed(BattleManager.BattleSide attackingSide,
                                                            BattleClass targetClassType,
                                                            bool targetIsDead,
                                                            bool isNoReductionType,
                                                            int expectedStamina)
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            var target = Substitute.For<ICombatObject>();
            target.Type.Returns((ushort)100);
            target.ClassType.Returns(targetClassType);
            target.IsDead.Returns(targetIsDead);

            var objectTypeFactory = Substitute.For<IObjectTypeFactory>();
            objectTypeFactory.IsObjectType("BattleNoStaminaReduction", 100).Returns(isNoReductionType);
            fixture.Register(() => objectTypeFactory);

            var battleFormulas = Substitute.For<IBattleFormulas>();
            battleFormulas.GetStaminaStructureDestroyed(10, target).Returns((short)7);
            fixture.Register(() => battleFormulas);
            
            var battleManager = fixture.Freeze<IBattleManager>();

            var staminaMonitor = fixture.Create<StaminaMonitor>();
            staminaMonitor.Stamina = 10;

            battleManager.ActionAttacked += Raise.Event<BattleManager.OnAttack>(battleManager,
                                                                           attackingSide,
                                                                           Substitute.For<ICombatGroup>(),
                                                                           Substitute.For<ICombatObject>(),
                                                                           Substitute.For<ICombatGroup>(),
                                                                           target,
                                                                           20m,
                                                                           1,
                                                                           1);

            staminaMonitor.Stamina.Should().Be((short)expectedStamina);
        }

        [Theory]
        [InlineData((byte)4, 10)]
        [InlineData((byte)5, 7)]
        public void TestReduceStaminaWhenStructureDestroyedAndIsNoEarlyLevelReductionType(byte level, int expectedStamina)
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            var target = Substitute.For<ICombatObject>();
            target.Type.Returns((ushort)100);
            target.Lvl.Returns(level);
            target.ClassType.Returns(BattleClass.Structure);
            target.IsDead.Returns(true);

            var objectTypeFactory = Substitute.For<IObjectTypeFactory>();
            objectTypeFactory.IsObjectType("BattleNoStaminaReductionEarlyLevels", 100).Returns(true);
            fixture.Register(() => objectTypeFactory);

            var battleFormulas = Substitute.For<IBattleFormulas>();
            battleFormulas.GetStaminaStructureDestroyed(10, target).Returns((short)7);
            fixture.Register(() => battleFormulas);
            
            var battleManager = fixture.Freeze<IBattleManager>();

            var staminaMonitor = fixture.Create<StaminaMonitor>();
            staminaMonitor.Stamina = 10;

            battleManager.ActionAttacked += Raise.Event<BattleManager.OnAttack>(battleManager,
                                                                           BattleManager.BattleSide.Attack,
                                                                           Substitute.For<ICombatGroup>(),
                                                                           Substitute.For<ICombatObject>(),
                                                                           Substitute.For<ICombatGroup>(),
                                                                           target,
                                                                           20m,
                                                                           1,
                                                                           1);

            staminaMonitor.Stamina.Should().Be((short)expectedStamina);
        }
    }
}
