using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit.Extensions;

namespace Testing.Actions
{
    public class BattlePassiveActionTest
    {
        public static IEnumerable<object[]> ShouldGiveApWhenEnterRoundData
        {
            get
            {
                yield return new object[]{};
            }
        }
            
        [Theory, PropertyData("ShouldGiveApWhenEnterRoundData")]
        public void ShouldGiveApWhenEnterRound()
        {
        }
    }
}
