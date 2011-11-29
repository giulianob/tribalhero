using System.Collections.Generic;
using FluentAssertions;
using Game.Data;
using Game.Setup;
using Game.Logic.Formulas;
using Xunit;

namespace Testing.Formulas
{
    public class MoveTimeTotalTest
    {
        [Fact]
        public void TestEmptyEffect()
        {
            Formula.MoveTimeTotal(4, 400, true, new List<Effect>()).Should().Be(((int)(Formula.MoveTime(4) * 400 * Config.seconds_per_unit)));
            Formula.MoveTimeTotal(20, 800, true, new List<Effect>()).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit));
            Formula.MoveTimeTotal(50, 1200, true, new List<Effect>()).Should().Be((int)(Formula.MoveTime(50) * 1200 * Config.seconds_per_unit));
        }

        [Fact]
        public void TestRushAttack()
        {
            Effect e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "ATTACK" } };
            Effect e1 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 4, "ATTACK" } };
            Effect e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DEFENSE" } };

            Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 120));
            Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit));
            Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e, e1 }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
            Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e, e1, e2 }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
        }

        [Fact]
        public void TestRushDefense()
        {
            Effect e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DEFENSE" } };
            Effect e1 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 4, "DEFENSE" } };
            Effect e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "ATTACK" } };

            Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 120));
            Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit));
            Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e, e1 }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
            Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e, e1, e2 }).Should().Be((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124));
        }

        [Fact]
        public void TestDoubleTime()
        {
            var dummy = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DEFENSE" } };
            Effect e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DISTANCE" } };
            Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy }).Should().Be((int)(Formula.MoveTime(20) * 1200 * Config.seconds_per_unit));

            // tiles at 1200
            double expected = Formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += Formula.MoveTime(20) * 200 * Config.seconds_per_unit * 100 / 140;
            Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy, e }).Should().Be((int)expected);

            // should take the max distance
            Effect e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 5, "DISTANCE" } };
            Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy, e, e2 }).Should().Be((int)expected);

            // tiles at 3000
            expected = Formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 140;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 160;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 180;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 200;
            Formula.MoveTimeTotal(20, 3000, true, new List<Effect> { dummy, e }).Should().Be((int)expected);
        }

        [Fact]
        public void TestDoubleTimeWithRush()
        {
            var dummy = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "ATTACK" } };
            var e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DISTANCE" } };
            var e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 10, "ATTACK" } };

            // with 20 attack rush
            double expected = Formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += Formula.MoveTime(20) * 200 * Config.seconds_per_unit * 100 / 140;
            expected /= 1.2;
            Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy, e }).Should().Be((int)expected);

            // with 10 + 20 attack rush
            expected = Formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 140;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 160;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 180;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 200;
            expected /= 1.3;
            Formula.MoveTimeTotal(20, 3000, true, new List<Effect> { dummy, e, e2 }).Should().Be((int)expected);
        }
    }
}