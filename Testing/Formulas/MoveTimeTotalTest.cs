using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Logic.Formulas;
namespace Testing.Formulas {
    [TestClass]
    public class MoveTimeTotalTest {
        [TestMethod]
        public void TestEmptyEffect() {
            Assert.AreEqual(Formula.MoveTime(4) * 400 * Config.seconds_per_unit, Formula.MoveTimeTotal(4, 400, true, new List<Effect>()));
            Assert.AreEqual(Formula.MoveTime(20) * 800 * Config.seconds_per_unit, Formula.MoveTimeTotal(20, 800, true, new List<Effect>()));
            Assert.AreEqual(Formula.MoveTime(50) * 1200 * Config.seconds_per_unit, Formula.MoveTimeTotal(50, 1200, true, new List<Effect>()));
        }

        [TestMethod]
        public void TestRushAttack()
        {
            Effect e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "ATTACK" } };
            Effect e1 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 4, "ATTACK" } };
            Effect e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DEFENSE" } };
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 120), Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e }));
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit), Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e }));
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124), Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e, e1 }));
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124), Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e, e1, e2 }));
        }

        [TestMethod]
        public void TestRushDefense() {
            Effect e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DEFENSE" } };
            Effect e1 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 4, "DEFENSE" } };
            Effect e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "ATTACK" } };
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 120), Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e }));
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit), Formula.MoveTimeTotal(20, 800, true, new List<Effect> { e }));
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124), Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e, e1 }));
            Assert.AreEqual((int)(Formula.MoveTime(20) * 800 * Config.seconds_per_unit * 100 / 124), Formula.MoveTimeTotal(20, 800, false, new List<Effect> { e, e1, e2 }));
        }
        [TestMethod]
        public void TestDoubleTime() {
            var dummy = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DEFENSE" } };
            Effect e = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 20, "DISTANCE" } };
            Assert.AreEqual((int)(Formula.MoveTime(20) * 1200 * Config.seconds_per_unit), Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy }));

            // tiles at 1200
            double expected = Formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += Formula.MoveTime(20) * 200 * Config.seconds_per_unit * 100 / 140;
            Assert.AreEqual((int)expected, Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy, e }));

            // should take the max distance
            Effect e2 = new Effect { Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] { 5, "DISTANCE" } };
            Assert.AreEqual((int)expected, Formula.MoveTimeTotal(20, 1200, true, new List<Effect> { dummy, e, e2 }));

            // tiles at 3000
            expected = Formula.MoveTime(20) * 500 * Config.seconds_per_unit;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 120;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 140;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 160;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 180;
            expected += Formula.MoveTime(20) * 500 * Config.seconds_per_unit * 100 / 200;
            Assert.AreEqual((int)expected, Formula.MoveTimeTotal(20, 3000, true, new List<Effect> { dummy, e }));
        }

        [TestMethod]
        public void TestDoubleTimeWithRush()
        {
            var dummy = new Effect {Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {20, "ATTACK"}};
            var e = new Effect {Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {20, "DISTANCE"}};
            var e2 = new Effect {Id = EffectCode.TroopSpeedMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {10, "ATTACK"}};

            // with 20 attack rush
            double expected = Formula.MoveTime(20)*500*Config.seconds_per_unit;
            expected += Formula.MoveTime(20)*500*Config.seconds_per_unit*100/120;
            expected += Formula.MoveTime(20)*200*Config.seconds_per_unit*100/140;
            expected /= 1.2;
            Assert.AreEqual((int)expected, Formula.MoveTimeTotal(20, 1200, true, new List<Effect> {dummy, e}));

            // with 10 + 20 attack rush
            expected = Formula.MoveTime(20)*500*Config.seconds_per_unit;
            expected += Formula.MoveTime(20)*500*Config.seconds_per_unit*100/120;
            expected += Formula.MoveTime(20)*500*Config.seconds_per_unit*100/140;
            expected += Formula.MoveTime(20)*500*Config.seconds_per_unit*100/160;
            expected += Formula.MoveTime(20)*500*Config.seconds_per_unit*100/180;
            expected += Formula.MoveTime(20)*500*Config.seconds_per_unit*100/200;
            expected /= 1.3;
            Assert.AreEqual((int)expected, Formula.MoveTimeTotal(20, 3000, true, new List<Effect> {dummy, e, e2}));
        }
    }
}
