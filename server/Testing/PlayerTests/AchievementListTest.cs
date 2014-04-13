using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Ploeh.AutoFixture;
using Xunit.Extensions;

namespace Testing.PlayerTests
{
    public class AchievementListTest
    {
        [Theory, AutoNSubstituteData]
        public void GetAchievementCountByTier_ShouldReturnTierCount(Fixture fixture, AchievementList achievements)
        {
            new List<AchievementTier>
            {
                AchievementTier.Bronze,
                AchievementTier.Gold,
                AchievementTier.Silver,
                AchievementTier.Silver,
                AchievementTier.Silver,
                AchievementTier.Gold
            }.ForEach(tier => achievements.Add(new Achievement {Tier = tier}));

            var groups = achievements.GetAchievementCountByTier();

            groups.Keys.Should().HaveCount(3);
            groups[AchievementTier.Gold].Should().Be(2);
            groups[AchievementTier.Silver].Should().Be(3);
            groups[AchievementTier.Bronze].Should().Be(1);
        }

        [Theory, AutoNSubstituteData]
        public void GetAchievementCountByTier_WhenMoreThan255Achievements_ShouldReturnTierCount(Fixture fixture, AchievementList achievements)
        {
            achievements.AddMany(() => new Achievement { Tier = AchievementTier.Gold }, 257);

            var groups = achievements.GetAchievementCountByTier();

            groups.Keys.Should().HaveCount(1);
            groups[AchievementTier.Gold].Should().Be(255);
        }
    }
}