#region

using Game.Data;
using Game.Data.Stats;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Xunit;

#endregion

namespace Testing.Balancing
{
    /// <summary>
    ///     Summary description for TroopProcedureTest
    /// </summary>
    public class TestOutnumber
    {
        private const double ERROR_MARGIN = .15;

        public TestOutnumber()
        {
            BaseBattleStats baseBattleStats = new BaseBattleStats((ushort)UnitType.TestSwordsman,
                                                                  1,
                                                                  WeaponType.Sword,
                                                                  WeaponClass.Basic,
                                                                  ArmorType.Ground,
                                                                  ArmorClass.Leather,
                                                                  70,
                                                                  30,
                                                                  1,
                                                                  9,
                                                                  2,
                                                                  9,
                                                                  10,
                                                                  30,
                                                                  0);
            IBaseUnitStats baseUnitStats = new BaseUnitStats("TestSwordsman",
                                                            "SWORDSMAN_UNIT",
                                                            1001,
                                                            1,
                                                            new Resource(),
                                                            new Resource(),
                                                            baseBattleStats,
                                                            300,
                                                            300,
                                                            1);
            Ioc.Kernel.Get<UnitFactory>().AddType(baseUnitStats);
        }

        private static void TestMultiplier(int baseCount, double multiplier, double expectAdvantage)
        {
            /*
            Group defender = new Group();
            defender.AddToLocal(UnitType.Swordsman, 1, (ushort)(baseCount * multiplier));
            Group attacker = new Group();
            attacker.AddToAttack(UnitType.Swordsman, 1, (ushort)baseCount);
            Simulation sim = new Simulation(attacker, defender);
            sim.Run();

            double actualAdvantage = defender.Upkeep() / (multiplier * baseCount);
            if (double.IsNaN(actualAdvantage))
            {
                actualAdvantage = 0;
            }
            Assert.True(
                        expectAdvantage - ERROR_MARGIN < actualAdvantage &&
                        actualAdvantage < expectAdvantage + ERROR_MARGIN,
                        string.Format(
                                      "Multi[{0}] Base[{3}] \tLeft[{4}]'s actual advantage[{1}] is close to [{2}] expect left[{5}]",
                                      multiplier,
                                      actualAdvantage,
                                      expectAdvantage,
                                      baseCount * multiplier,
                                      defender.Upkeep(),
                                      multiplier * baseCount * expectAdvantage));
             */
        }

        [Fact(Skip = "For Anthony only")]
        public void Test20X20()
        {
            TestMultiplier(100, 0, 0);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test20X25()
        {
            TestMultiplier(20, 1.25, .35);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test20X30()
        {
            TestMultiplier(20, 1.5, .5);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test20X40()
        {
            TestMultiplier(20, 2, .75);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test20X70()
        {
            TestMultiplier(20, 3.5, .85);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test20X100()
        {
            TestMultiplier(20, 5, .90);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test100X100()
        {
            TestMultiplier(100, 0, 0);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test100X125()
        {
            TestMultiplier(100, 1.25, .35);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test100X150()
        {
            TestMultiplier(100, 1.5, .5);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test100X200()
        {
            TestMultiplier(100, 2, .75);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test100X350()
        {
            TestMultiplier(100, 3.5, .85);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test100X500()
        {
            TestMultiplier(100, 5, .90);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test500X100()
        {
            TestMultiplier(500, 0, 0);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test500X125()
        {
            TestMultiplier(500, 1.25, .35);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test500X150()
        {
            TestMultiplier(500, 1.5, .5);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test500X200()
        {
            TestMultiplier(500, 2, .75);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test500X350()
        {
            TestMultiplier(500, 3.5, .85);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test500X500()
        {
            TestMultiplier(500, 5, .90);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test1000X100()
        {
            TestMultiplier(1000, 0, 0);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test1000X125()
        {
            TestMultiplier(1000, 1.25, .35);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test1000X150()
        {
            TestMultiplier(1000, 1.5, .5);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test1000X200()
        {
            TestMultiplier(1000, 2, .75);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test1000X350()
        {
            TestMultiplier(1000, 3.5, .85);
        }

        [Fact(Skip = "For Anthony only")]
        public void Test1000X500()
        {
            TestMultiplier(1000, 5, .90);
        }
    }
}