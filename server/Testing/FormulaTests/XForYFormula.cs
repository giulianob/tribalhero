using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Formulas;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.FormulaTests
{
    public class XForYFormula
    {

        [Theory]
        [InlineAutoNSubstituteData(3, 2, 0, 0)]
        [InlineAutoNSubstituteData(3, 2, 1, 1)]
        [InlineAutoNSubstituteData(3, 2, 2, 3)]
        [InlineAutoNSubstituteData(3, 2, 3, 4)]
        [InlineAutoNSubstituteData(3, 2, 4, 6)]
        [InlineAutoNSubstituteData(3, 2, 5, 7)]
        [InlineAutoNSubstituteData(5, 3, 0, 0)]
        [InlineAutoNSubstituteData(5, 3, 1, 1)]
        [InlineAutoNSubstituteData(5, 3, 2, 3)]
        [InlineAutoNSubstituteData(5, 3, 3, 5)]
        [InlineAutoNSubstituteData(5, 3, 4, 6)]
        [InlineAutoNSubstituteData(5, 3, 5, 8)]
        [InlineAutoNSubstituteData(2, 1, 0, 0)]
        [InlineAutoNSubstituteData(2, 1, 1, 2)]
        [InlineAutoNSubstituteData(2, 1, 2, 4)]
        [InlineAutoNSubstituteData(2, 1, 3, 6)]
        [InlineAutoNSubstituteData(2, 1, 4, 8)]
        [InlineAutoNSubstituteData(2, 1, 5, 10)]
        [InlineAutoNSubstituteData(3, 1, 0, 0)]
        [InlineAutoNSubstituteData(3, 1, 1, 3)]
        [InlineAutoNSubstituteData(3, 1, 2, 6)]
        [InlineAutoNSubstituteData(3, 1, 3, 9)]
        [InlineAutoNSubstituteData(3, 1, 4, 12)]
        [InlineAutoNSubstituteData(3, 1, 5, 15)]
        public void GetXForYTotal_WhenXForY(int x, int y, int paidForCount, int expectTotalCount, Formula formula, ITechnologyManager technologyManager)
        {
            var effect = new Effect
            {
                Id = EffectCode.XFor1,
                IsPrivate = true,
                Location = EffectLocation.Object,
                Value = new object[] { x, y }
            };
            technologyManager.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible).Returns(new List<Effect> { effect });
            formula.GetXForYTotal(technologyManager, paidForCount).Should().Be(expectTotalCount);
        }

        [Theory, AutoNSubstituteData]
        public void GetXForYTotal_WhenNoEffect_ShouldReturnPaidFor(Formula formula, ITechnologyManager technologyManager)
        {
            technologyManager.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible).Returns(new List<Effect>());
            formula.GetXForYTotal(technologyManager, 0).Should().Be(0);
            formula.GetXForYTotal(technologyManager, 1).Should().Be(1);
            formula.GetXForYTotal(technologyManager, 99).Should().Be(99);
        }

        [Theory, AutoNSubstituteData]
        public void GetXForYTotal_WhenMultipleEffects_ShouldUseTheHighestValue(Formula formula, ITechnologyManager technologyManager)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.XFor1,
                    IsPrivate = true,
                    Location = EffectLocation.Object,
                    Value = new object[] {5, 3}
                },
                new Effect
                {
                    Id = EffectCode.XFor1,
                    IsPrivate = true,
                    Location = EffectLocation.Object,
                    Value = new object[] {2, 1}
                },
                new Effect
                {
                    Id = EffectCode.XFor1,
                    IsPrivate = true,
                    Location = EffectLocation.Object,
                    Value = new object[] {3, 2}
                }
            };
            technologyManager.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible).Returns(effects);
            formula.GetXForYTotal(technologyManager, 100).Should().Be(200);
        }



        [Theory]
        [InlineAutoNSubstituteData(3, 2, 0, 0)]
        [InlineAutoNSubstituteData(3, 2, 1, 1)]
        [InlineAutoNSubstituteData(3, 2, 2, 2)]
        [InlineAutoNSubstituteData(3, 2, 3, 2)]
        [InlineAutoNSubstituteData(3, 2, 4, 3)]
        [InlineAutoNSubstituteData(3, 2, 5, 4)]
        [InlineAutoNSubstituteData(3, 2, 6, 4)]
        [InlineAutoNSubstituteData(3, 2, 7, 5)]
        [InlineAutoNSubstituteData(3, 2, 8, 6)]
        [InlineAutoNSubstituteData(3, 2, 9, 6)]
        [InlineAutoNSubstituteData(3, 2, 10, 7)]

        [InlineAutoNSubstituteData(5, 3, 0, 0)]
        [InlineAutoNSubstituteData(5, 3, 1, 1)]
        [InlineAutoNSubstituteData(5, 3, 2, 2)]
        [InlineAutoNSubstituteData(5, 3, 3, 2)]
        [InlineAutoNSubstituteData(5, 3, 4, 3)]
        [InlineAutoNSubstituteData(5, 3, 5, 3)]
        [InlineAutoNSubstituteData(5, 3, 6, 4)]
        [InlineAutoNSubstituteData(5, 3, 7, 5)]
        [InlineAutoNSubstituteData(5, 3, 8, 5)]
        [InlineAutoNSubstituteData(5, 3, 9, 6)]
        [InlineAutoNSubstituteData(5, 3, 10, 6)]

        [InlineAutoNSubstituteData(2, 1, 0, 0)]
        [InlineAutoNSubstituteData(2, 1, 1, 1)]
        [InlineAutoNSubstituteData(2, 1, 2, 1)]
        [InlineAutoNSubstituteData(2, 1, 3, 2)]
        [InlineAutoNSubstituteData(2, 1, 4, 2)]
        [InlineAutoNSubstituteData(2, 1, 5, 3)]
        [InlineAutoNSubstituteData(2, 1, 6, 3)]
        [InlineAutoNSubstituteData(2, 1, 7, 4)]
        [InlineAutoNSubstituteData(2, 1, 8, 4)]
        [InlineAutoNSubstituteData(2, 1, 9, 5)]
        [InlineAutoNSubstituteData(2, 1, 10, 5)]

        [InlineAutoNSubstituteData(3, 1, 0, 0)]
        [InlineAutoNSubstituteData(3, 1, 1, 1)]
        [InlineAutoNSubstituteData(3, 1, 2, 1)]
        [InlineAutoNSubstituteData(3, 1, 3, 1)]
        [InlineAutoNSubstituteData(3, 1, 4, 2)]
        [InlineAutoNSubstituteData(3, 1, 5, 2)]
        [InlineAutoNSubstituteData(3, 1, 6, 2)]
        [InlineAutoNSubstituteData(3, 1, 7, 3)]
        [InlineAutoNSubstituteData(3, 1, 8, 3)]
        [InlineAutoNSubstituteData(3, 1, 9, 3)]
        [InlineAutoNSubstituteData(3, 1, 10, 4)]
        public void GetXForYPaidFor_WhenXForY(int x, int y, int totalCount, int expectPaidForCount, Formula formula, ITechnologyManager technologyManager)
        {
            var effect = new Effect
            {
                Id = EffectCode.XFor1,
                IsPrivate = true,
                Location = EffectLocation.Object,
                Value = new object[] { x, y }
            };
            technologyManager.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible).Returns(new List<Effect> { effect });
            formula.GetXForYPaidFor(technologyManager, totalCount).Should().Be(expectPaidForCount);
        }

        [Theory, AutoNSubstituteData]
        public void GetXForYPaidFor_WhenNoEffect_ShouldReturnTotal(Formula formula, ITechnologyManager technologyManager)
        {
            technologyManager.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible).Returns(new List<Effect>());
            formula.GetXForYPaidFor(technologyManager, 0).Should().Be(0);
            formula.GetXForYPaidFor(technologyManager, 1).Should().Be(1);
            formula.GetXForYPaidFor(technologyManager, 99).Should().Be(99);
        }

        [Theory, AutoNSubstituteData]
        public void GetXForYPaidFor_WhenMultipleEffects_ShouldUseTheHighestValue(Formula formula, ITechnologyManager technologyManager)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.XFor1,
                    IsPrivate = true,
                    Location = EffectLocation.Object,
                    Value = new object[] {5, 3}
                },
                new Effect
                {
                    Id = EffectCode.XFor1,
                    IsPrivate = true,
                    Location = EffectLocation.Object,
                    Value = new object[] {2, 1}
                },
                new Effect
                {
                    Id = EffectCode.XFor1,
                    IsPrivate = true,
                    Location = EffectLocation.Object,
                    Value = new object[] {3, 2}
                }
            };
            technologyManager.GetEffects(EffectCode.XFor1, EffectInheritance.Invisible).Returns(effects);
            formula.GetXForYPaidFor(technologyManager, 200).Should().Be(100);
        }

    }
}
