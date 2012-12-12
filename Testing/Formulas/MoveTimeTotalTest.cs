using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using Moq;
using Xunit;

namespace Testing.Formulas
{
    public class MoveTimeTotalTest
    {
        [Fact]
        public void TestEmptyEffect()
        {
            var formula = new Formula(new Mock<ObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<StructureFactory>(MockBehavior.Strict).Object);

            formula.MoveTimeTotal(CreateMockedStub(4, new List<Effect>()), 400, true)
                   .Should()
                   .Be(((int)(formula.MoveTime(4) * 400 * Config.seconds_per_unit)));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect>()), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit));
            formula.MoveTimeTotal(CreateMockedStub(50, new List<Effect>()), 1200, true)
                   .Should()
                   .Be((int)(formula.MoveTime(50) * 1200 * Config.seconds_per_unit));
        }

        [Fact]
        public void TestRushAttack()
        {
            var formula = new Formula(new Mock<ObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<StructureFactory>(MockBehavior.Strict).Object);

            Effect e = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "ATTACK"}
            };
            Effect e1 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {4, "ATTACK"}
            };
            Effect e2 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "DEFENSE"}
            };

            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 120));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1, e2}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
        }

        [Fact]
        public void TestRushDefense()
        {
            var formula = new Formula(new Mock<ObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<StructureFactory>(MockBehavior.Strict).Object);

            Effect e = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "DEFENSE"}
            };
            Effect e1 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {4, "DEFENSE"}
            };
            Effect e2 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "ATTACK"}
            };

            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 120));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1, e2}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
        }

        [Fact]
        public void TestDoubleTime()
        {
            var formula = new Formula(new Mock<ObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<StructureFactory>(MockBehavior.Strict).Object);

            var dummy = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "DEFENSE"}
            };
            Effect e = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "DISTANCE"}
            };
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy}), 1200, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 1200 * Config.seconds_per_unit));

            // tiles at 1200
            double expected = formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += formula.MoveTime(20) * 200 * Config.seconds_per_unit * 100 / 140;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e}), 1200, true)
                   .Should()
                   .Be((int)expected);

            // should take the max distance
            Effect e2 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {5, "DISTANCE"}
            };
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e, e2}), 1200, true)
                   .Should()
                   .Be((int)expected);

            // tiles at 3000
            expected = formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 140;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 160;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 180;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 200;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e}), 3000, true)
                   .Should()
                   .Be((int)expected);
        }

        [Fact]
        public void TestDoubleTimeWithRush()
        {
            var formula = new Formula(new Mock<ObjectTypeFactory>(MockBehavior.Strict).Object,
                                      new Mock<UnitFactory>(MockBehavior.Strict).Object,
                                      new Mock<StructureFactory>(MockBehavior.Strict).Object);

            var dummy = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "ATTACK"}
            };
            var e = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {20, "DISTANCE"}
            };
            var e2 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {10, "ATTACK"}
            };

            // with 20 attack rush
            double expected = formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += formula.MoveTime(20) * 200 * Config.seconds_per_unit * 100 / 140;
            expected /= 1.2;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e}), 1200, true)
                   .Should()
                   .Be((int)expected);

            // with 10 + 20 attack rush
            expected = formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 140;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 160;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 180;
            expected += formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 200;
            expected /= 1.3;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e, e2}), 3000, true)
                   .Should()
                   .Be((int)expected);
        }

        private ITroopStub CreateMockedStub(byte speed, List<Effect> effects)
        {
            var technologyManager = new Mock<ITechnologyManager>();
            technologyManager.Setup(m => m.GetEffects(It.IsAny<EffectCode>(), It.IsAny<EffectInheritance>()))
                             .Returns(effects);

            var city = new Mock<ICity>();
            city.SetupGet(p => p.Technologies).Returns(technologyManager.Object);

            var stub = new Mock<ITroopStub>();
            stub.SetupGet(p => p.City).Returns(city.Object);
            stub.SetupGet(p => p.Speed).Returns(speed);

            return stub.Object;
        }
    }
}