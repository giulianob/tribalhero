using ConsoleSimulator;
using FluentAssertions;
using Game.Setup;
using Ninject;

namespace Testing.Balancing
{
    public class TestUtility
    {
        public static void TestUnitPerUpkeep(UnitType type,
                                             UnitType target,
                                             double multiplier,
                                             double expectAdvantage,
                                             double errorMargin = .15)
        {
            ushort typeUpkeep = Ioc.Kernel.Get<UnitFactory>().GetUnitStats((ushort)type, 1).Upkeep;
            ushort targetUpkeep = Ioc.Kernel.Get<UnitFactory>().GetUnitStats((ushort)target, 1).Upkeep;
            int count = (int)(typeUpkeep * targetUpkeep * multiplier);

            Group defender = new Group();
            defender.AddToLocal(type, 1, (ushort)(count / typeUpkeep));

            Group attacker = new Group();
            attacker.AddToAttack(target, 1, (ushort)(count / targetUpkeep));
            Simulation sim = new Simulation(attacker, defender);
            sim.Run();

            double actualAdvantage;
            if (attacker.Upkeep() == 0)
            {
                actualAdvantage = ((double)defender.Upkeep() / typeUpkeep) / ((double)count / typeUpkeep);
            }
            else
            {
                actualAdvantage = -1 * ((double)attacker.Upkeep() / targetUpkeep) / ((double)count / targetUpkeep);
            }
            if (double.IsNaN(actualAdvantage))
            {
                actualAdvantage = 0;
            }

            (expectAdvantage - errorMargin < actualAdvantage && actualAdvantage < expectAdvantage + errorMargin).Should()
                                                                                                                .BeTrue(
                                                                                                                        "Def[{0}/{6}] Atk[{1}/{7}]'s actual advantage[{2}] is not close to expected [{3}] Def[{4}] Atk[{5}]",
                                                                                                                        defender
                                                                                                                                .Upkeep
                                                                                                                                () /
                                                                                                                        typeUpkeep,
                                                                                                                        attacker
                                                                                                                                .Upkeep
                                                                                                                                () /
                                                                                                                        targetUpkeep,
                                                                                                                        actualAdvantage,
                                                                                                                        expectAdvantage,
                                                                                                                        expectAdvantage >
                                                                                                                        0
                                                                                                                                ? expectAdvantage *
                                                                                                                                  ((
                                                                                                                                   double
                                                                                                                                   )
                                                                                                                                   count /
                                                                                                                                   typeUpkeep)
                                                                                                                                : 0,
                                                                                                                        expectAdvantage <=
                                                                                                                        0
                                                                                                                                ? -1 *
                                                                                                                                  expectAdvantage *
                                                                                                                                  ((
                                                                                                                                   double
                                                                                                                                   )
                                                                                                                                   count /
                                                                                                                                   targetUpkeep)
                                                                                                                                : 0,
                                                                                                                        ((
                                                                                                                         double
                                                                                                                         )
                                                                                                                         count /
                                                                                                                         typeUpkeep),
                                                                                                                        ((
                                                                                                                         double
                                                                                                                         )
                                                                                                                         count /
                                                                                                                         targetUpkeep));
        }
    }
}