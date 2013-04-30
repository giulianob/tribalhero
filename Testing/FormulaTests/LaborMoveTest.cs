using System.Collections.Generic;
using Common.Testing;
using Game.Data;
using Game.Logic.Formulas;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit.Extensions;
using FluentAssertions;

namespace Testing.FormulaTests
{
    public class LaborMoveTest
    {
        [Theory]
        [InlineAutoNSubstituteData(1, 180)]
        [InlineAutoNSubstituteData(5, 900)]
        [InlineAutoNSubstituteData(200, 36000)]       
        public void TestBasicLaborMove(int labor, int expected, IStructure structure, Formula formula)
        {
            structure.City.GetTotalLaborers().Returns(101);
            structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod).Returns(new List<Effect>());

            formula.LaborMoveTime(structure, (ushort)labor, true).Should().Be(expected);
        }

        public static IEnumerable<object[]> WithDifferentOvertime
        {
            get
            {
                // with level 1 overtime
                yield return
                        new object[]
                        {
                                1,
                                new List<Effect>
                                {new Effect {Id = EffectCode.LaborMoveTimeMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {1}}},
                                162
                        };
                // with level 2 overtime
                yield return
                        new object[]
                        {
                                1,
                                new List<Effect>
                                {new Effect {Id = EffectCode.LaborMoveTimeMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {2}}},
                                144
                        };
                // with level 1 and level 3 overtime
                yield return
                        new object[]
                        {
                                1,
                                new List<Effect>
                                {
                                        new Effect {Id = EffectCode.LaborMoveTimeMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {1}},
                                        new Effect {Id = EffectCode.LaborMoveTimeMod, IsPrivate = true, Location = EffectLocation.City, Value = new object[] {3}}
                                }, 
                                126
                        };
            }
        
        }

        [Theory]
        [PropertyData("WithDifferentOvertime")]
        public void TestLaborMoveWithOvertime(int labor, IEnumerable<Effect> effects, int expected)
        {
            var formula = new Fixture().Create<Formula>();
            var structure = Substitute.For<IStructure>();
            structure.City.GetTotalLaborers().Returns(101);
            structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod).Returns(effects);

            formula.LaborMoveTime(structure, (ushort)labor, true).Should().Be(expected);
        }
    }


}