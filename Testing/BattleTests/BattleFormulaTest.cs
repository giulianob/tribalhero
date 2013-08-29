using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Data;
using Game.Setup;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using Game.Battle.CombatObjects;
using NSubstitute;

namespace Testing.BattleTests
{
    public class BattleFormulaTest
    {

        public static IEnumerable<object[]> TestTowerReductionObjects
        {
            get
            {
                // Tower
                yield return new object[] { WeaponType.Sword, ArmorType.Building1, .2 };
                yield return new object[] { WeaponType.Pike, ArmorType.Building1, .2 };
                yield return new object[] { WeaponType.Bow, ArmorType.Building1, .2 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Building1, .2 };
                yield return new object[] { WeaponType.Ball, ArmorType.Building1, .2 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Building1, .2 };
                yield return new object[] { WeaponType.Tower, ArmorType.Building1, .2 };

                // Cannon Tower
                yield return new object[] { WeaponType.Sword, ArmorType.Building2, .2 };
                yield return new object[] { WeaponType.Pike, ArmorType.Building2, .2 };
                yield return new object[] { WeaponType.Bow, ArmorType.Building2, .2 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Building2, .2 };
                yield return new object[] { WeaponType.Ball, ArmorType.Building2, .2 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Building2, .2 };
                yield return new object[] { WeaponType.Tower, ArmorType.Building2, .2 };

                // Structure
                yield return new object[] { WeaponType.Sword, ArmorType.Building3, 1};
                yield return new object[] { WeaponType.Pike, ArmorType.Building3, 1 };
                yield return new object[] { WeaponType.Bow, ArmorType.Building3, 1 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Building3, 1 };
                yield return new object[] { WeaponType.Ball, ArmorType.Building3, 1 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Building3, 1 };
                yield return new object[] { WeaponType.Tower, ArmorType.Building3, 1 };

                // Ground
                yield return new object[] { WeaponType.Sword, ArmorType.Ground, 1 };
                yield return new object[] { WeaponType.Pike, ArmorType.Ground, 1 };
                yield return new object[] { WeaponType.Bow, ArmorType.Ground, 1 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Ground, 1 };
                yield return new object[] { WeaponType.Ball, ArmorType.Ground, 1 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Ground, 1 };
                yield return new object[] { WeaponType.Tower, ArmorType.Ground, 1 };

                // Mount
                yield return new object[] { WeaponType.Sword, ArmorType.Mount, 1 };
                yield return new object[] { WeaponType.Pike, ArmorType.Mount, 1 };
                yield return new object[] { WeaponType.Bow, ArmorType.Mount, 1 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Mount, 1 };
                yield return new object[] { WeaponType.Ball, ArmorType.Mount, 1 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Mount, 1 };
                yield return new object[] { WeaponType.Tower, ArmorType.Mount, 1 };

                // Machine
                yield return new object[] { WeaponType.Sword, ArmorType.Machine, 1 };
                yield return new object[] { WeaponType.Pike, ArmorType.Machine, 1 };
                yield return new object[] { WeaponType.Bow, ArmorType.Machine, 1 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Machine, 1 };
                yield return new object[] { WeaponType.Ball, ArmorType.Machine, 1 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Machine, 1 };
                yield return new object[] { WeaponType.Tower, ArmorType.Machine, 1 };

                // Gate
                yield return new object[] { WeaponType.Sword, ArmorType.Gate, 1 };
                yield return new object[] { WeaponType.Pike, ArmorType.Gate, 1 };
                yield return new object[] { WeaponType.Bow, ArmorType.Gate, 1 };
                yield return new object[] { WeaponType.Cannon, ArmorType.Gate, 1 };
                yield return new object[] { WeaponType.Ball, ArmorType.Gate, 1 };
                yield return new object[] { WeaponType.Barricade, ArmorType.Gate, 1 };
                yield return new object[] { WeaponType.Tower, ArmorType.Gate, 1 };
            }
        }

        [Theory, PropertyData("TestTowerReductionObjects")]
        public void GetDmgModifier_WhenUnder5Rounds(WeaponType weaponType, ArmorType armorType, double dmg)
        {
            var unitModFactory = Substitute.For<UnitModFactory>();
            var unitFactory = Substitute.For<UnitFactory>();

            var fixture = FixtureHelper.Create();
            fixture.Register(() => unitModFactory);
            fixture.Register(() => unitFactory);

            var battleFormulas = fixture.Create<BattleFormulas>();

            unitModFactory.GetModifier(0, 0).ReturnsForAnyArgs(1);

            var attacker = Substitute.For<ICombatObject>();
            var defender = Substitute.For<ICombatObject>();
            attacker.Stats.Base.Weapon.Returns(weaponType);
            defender.Stats.Base.Armor.Returns(armorType);

            battleFormulas.GetDmgModifier(attacker, defender, 0).Should().Be(dmg);
            battleFormulas.GetDmgModifier(attacker, defender, 4).Should().Be(dmg);
        }

        [Theory, PropertyData("TestTowerReductionObjects")]
        public void GetDmgModifier_WhenOver5Rounds(WeaponType weaponType, ArmorType armorType, double dmg)
        {
            var unitModFactory = Substitute.For<UnitModFactory>();

            var fixture = FixtureHelper.Create();
            fixture.Register(() => unitModFactory);
            var battleFormulas = fixture.Create<BattleFormulas>();

            unitModFactory.GetModifier(0, 0).ReturnsForAnyArgs(1);

            var attacker = Substitute.For<ICombatObject>();
            var defender = Substitute.For<ICombatObject>();
            attacker.Stats.Base.Weapon.Returns(WeaponType.Sword);
            defender.Stats.Base.Armor.Returns(ArmorType.Building1);

            battleFormulas.GetDmgModifier(attacker, defender, 5).Should().Be(1);
        }

        [Theory, AutoNSubstituteData]
        public void GetNumberOfHits_WhenObjectIsSplashEvery200(ICombatObject attacker,
                                                               ICombatList defenderCombatList)
        {
            var objectTypeFactory = Substitute.For<ObjectTypeFactory>();
            var fixture = FixtureHelper.Create();
            fixture.Register(() => objectTypeFactory);
            var battleFormulas = fixture.Create<BattleFormulas>();

            objectTypeFactory.IsObjectType(string.Empty, 0).ReturnsForAnyArgs(true);
            attacker.Stats.Splash.Returns((byte)2);
            defenderCombatList.Upkeep.Returns(800);
            battleFormulas.GetNumberOfHits(attacker, defenderCombatList).Should().Be(6);
        }

        [Theory, AutoNSubstituteData]
        public void GetNumberOfHits_WhenObjectIsNotSplashEvery200(ICombatObject attacker, ICombatList defenderCombatList)
        {
            var objectTypeFactory = Substitute.For<ObjectTypeFactory>();
            var fixture = FixtureHelper.Create();
            fixture.Register(() => objectTypeFactory);
            var battleFormulas = fixture.Create<BattleFormulas>();

            objectTypeFactory.IsObjectType(string.Empty, 0).ReturnsForAnyArgs(false);
            attacker.Stats.Splash.Returns((byte)2);
            defenderCombatList.Upkeep.Returns(800);
            battleFormulas.GetNumberOfHits(attacker, defenderCombatList).Should().Be(2);
        }

        [Theory, AutoNSubstituteData]
        public void GetNumberOfHits_WhenDefenderUpkeepOver4000(ICombatObject attacker,
                                                               ICombatList defenderCombatList)
        {
            var objectTypeFactory = Substitute.For<ObjectTypeFactory>();
            var fixture = FixtureHelper.Create();
            fixture.Register(() => objectTypeFactory);
            var battleFormulas = fixture.Create<BattleFormulas>();

            objectTypeFactory.IsObjectType(string.Empty, 0).ReturnsForAnyArgs(true);
            attacker.Stats.Splash.Returns((byte)2);
            defenderCombatList.Upkeep.Returns(8000);
            battleFormulas.GetNumberOfHits(attacker, defenderCombatList).Should().Be(22);
        }

        [Theory, AutoNSubstituteData]
        public void GetNumberOfHits_WhenDefenderUpkeepUnder200(ICombatObject attacker,
                                                               ICombatList defenderCombatList)
        {
            var objectTypeFactory = Substitute.For<ObjectTypeFactory>();
            var fixture = FixtureHelper.Create();
            fixture.Register(() => objectTypeFactory);
            var battleFormulas = fixture.Create<BattleFormulas>();

            objectTypeFactory.IsObjectType(string.Empty,0).ReturnsForAnyArgs(true);
            attacker.Stats.Splash.Returns((byte)2);
            defenderCombatList.Upkeep.Returns(199);
            battleFormulas.GetNumberOfHits(attacker, defenderCombatList).Should().Be(2);
        }

    }
}
