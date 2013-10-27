using System;
using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using Moq;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class MoveTimeTotalTest
    {
        [Theory, AutoNSubstituteData]
        public void TestMoveTime(Formula formula)
        {
            formula.MoveTime(5).Should().Be(51.4m);
            formula.MoveTime(11).Should().Be(36);
            formula.MoveTime(23).Should().Be(22.5m);
            formula.MoveTime(23.3m).Should().Be(22.3m);
        }

        [Theory, AutoNSubstituteData]
        public void TestTroopSpeed(Formula formula)
        {
            ITroopStub troopstub = Substitute.For<ITroopStub>();
            troopstub.TotalCount.Returns((ushort)1);

            troopstub.City.Template[0].Battle.Speed.Returns((byte)12);  // fighter
            troopstub.City.Template[0].Upkeep.Returns((byte)1);
            troopstub.City.Template[1].Battle.Speed.Returns((byte)16); // Archer
            troopstub.City.Template[1].Upkeep.Returns((byte)1);
            troopstub.City.Template[2].Battle.Speed.Returns((byte)24); // Cavalry
            troopstub.City.Template[2].Upkeep.Returns((byte)2);
            troopstub.City.Template[3].Battle.Speed.Returns((byte)5); // Catapult
            troopstub.City.Template[3].Upkeep.Returns((byte)5);
            troopstub.City.Template[3].Battle.Armor.Returns(ArmorType.Machine);
            // 10 fighters
            IEnumerable<IFormation> formation = new[] {new Formation(FormationType.Normal)
            {
                {0, 10},
            }};
            troopstub.GetEnumerator().Returns(formation.GetEnumerator());
            formula.GetTroopSpeed(troopstub).Should().Be(12);

            // 5 archers
            formation = new[] {new Formation(FormationType.Normal)
            {
                {1, 5},
            }};
            troopstub.GetEnumerator().Returns(formation.GetEnumerator());
            formula.GetTroopSpeed(troopstub).Should().Be(16);

            // 10 fighters and 5 archers
            formation = new[] {new Formation(FormationType.Normal)
            {
                {0, 10},
                {1, 5},
            }};
            troopstub.GetEnumerator().Returns(formation.GetEnumerator());
            formula.GetTroopSpeed(troopstub).Should().Be(Math.Round((10m * 12 + 5 * 16) / 15, 1));

            // 10 fighters, 5 archers, and 2 cavalries
            formation = new[] {new Formation(FormationType.Normal)
            {
                {0, 10},
                {1, 5},
                {2, 2},
            }};
            troopstub.GetEnumerator().Returns(formation.GetEnumerator());
            formula.GetTroopSpeed(troopstub).Should().Be(Math.Round((10m * 12 + 5 * 16 + 4 * 24) / 19, 1));

            // 10 fighters, 5 archers, 2 cavalries and 8 catapults
            formation = new[] {new Formation(FormationType.Normal)
            {
                {0, 10},
                {1, 5},
                {2, 2},
                {3, 8},
            }};
            troopstub.GetEnumerator().Returns(formation.GetEnumerator());
            formula.GetTroopSpeed(troopstub).Should().Be(5);
        }

        [Theory, AutoNSubstituteData]
        public void TestEmptyEffect(Formula formula)
        {
            formula.MoveTimeTotal(CreateMockedStub(4, new List<Effect>()), 400, true)
                   .Should()
                   .Be(((int)(formula.MoveTime(4) * 400 * (decimal)Config.seconds_per_unit)));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect>()), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit));
            formula.MoveTimeTotal(CreateMockedStub(50, new List<Effect>()), 1200, true)
                   .Should()
                   .Be((int)(formula.MoveTime(50) * 1200 * (decimal)Config.seconds_per_unit));
        }

        [Theory, AutoNSubstituteData]
        public void TestRushAttack(Formula formula)
        {
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
            var result = formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit * 100 / 120;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, true).Should().Be((int)(result));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit * 100 / 124));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1, e2}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit * 100 / 124));
        }

        [Theory, AutoNSubstituteData]
        public void TestRushDefense(Formula formula)
        {
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
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit * 100 / 120));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e}), 800, true)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit * 100 / 124));
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {e, e1, e2}), 800, false)
                   .Should()
                   .Be((int)(formula.MoveTime(20) * 800 * (decimal)Config.seconds_per_unit * 100 / 124));
        }

        [Theory, AutoNSubstituteData]
        public void TestDoubleTime(Formula formula)
        {
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
                   .Be((int)(formula.MoveTime(20) * 1200 * (decimal)Config.seconds_per_unit));

            // tiles at 1200
            decimal expected = formula.MoveTime(20) * 500 * (decimal)Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 700 * (decimal)Config.seconds_per_unit * 100 / 120;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e}), 1200, true)
                   .Should()
                   .Be((int)expected);

            // should take the sum distance
            Effect e2 = new Effect
            {
                    Id = EffectCode.TroopSpeedMod,
                    IsPrivate = true,
                    Location = EffectLocation.City,
                    Value = new object[] {5, "DISTANCE"}
            };
            expected = formula.MoveTime(20) * 500 * (decimal)Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 700 * (decimal)Config.seconds_per_unit * 100 / 125;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> { dummy, e, e2 }), 1200, true)
                   .Should()
                   .Be((int)expected);

            // tiles at 3000
            expected = formula.MoveTime(20) * 500 * (decimal)Config.seconds_per_unit;
            expected += formula.MoveTime(20) * 2500 * (decimal)Config.seconds_per_unit * 100 / 120;

            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e}), 3000, true)
                   .Should()
                   .Be((int)expected);
        }

        [Theory, AutoNSubstituteData]
        public void TestDoubleTimeWithRush(Formula formula)
        {

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
            decimal expected = formula.MoveTime(20) * 500 * (decimal)Config.seconds_per_unit * 100 / 120;
            expected += formula.MoveTime(20) * 700 * (decimal)Config.seconds_per_unit * 100 / 140;
            expected = (int)expected;
            formula.MoveTimeTotal(CreateMockedStub(20, new List<Effect> {dummy, e}), 1200, true)
                   .Should()
                   .Be((int)expected);

            // with 10 + 20 attack rush
            expected = formula.MoveTime(20) * 500 * (decimal)Config.seconds_per_unit * 100 / 130;
            expected += formula.MoveTime(20) * 2500 * (decimal)Config.seconds_per_unit * 100 / 150;
            expected = (int)expected;
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