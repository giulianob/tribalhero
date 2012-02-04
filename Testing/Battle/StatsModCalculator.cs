﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Game.Battle;
using Game.Data.Stats;
using Game.Map;
using Moq;
using Persistance;
using Xunit;

namespace Testing.Battle
{
    public class StatsModCalculator
    {
        #region AtkDmgModCalculator
        /// <summary>
        /// Test AtkDmgModCalculator
        /// </summary>
        [Fact]
        public void TestAtkDmgModCalculatorPercentBonus()
        {
            AtkDmgModCalculator atk = new AtkDmgModCalculator(10);
            // when it's empty
            atk.GetResult().Should().BeInRange(9.9, 10.1);
            atk.AddMod("PERCENT_BONUS", 6);
            // when it has 1 mod
            atk.GetResult().Should().BeInRange(10.59, 10.61);
            // when it has multi mods
            atk.AddMod("PERCENT_BONUS", 3);
            atk.AddMod("PERCENT_BONUS", 15);
            atk.GetResult().Should().BeInRange(12.39, 12.41);
            // when it reach maximum 150%
            atk.AddMod("PERCENT_BONUS", 500);
            atk.GetResult().Should().BeInRange(24.9, 25.1);
        }
        #endregion
    }
}
