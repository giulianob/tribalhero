using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Logic.Formulas;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit.Extensions;
using FluentAssertions;

namespace Testing.Formulas
{
    public class LaborMoveTest
    {
        [Theory]
        [InlineData(1, 180)]
        [InlineData(5, 900)]
        [InlineData(200, 36000)]
        public void TestBasicLaborMove(int labor, int expected)
        {
           //ar fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
          //var technologies = fixture.CreateAnonymous<TechnologyManager>();
            var formula = new Fixture().CreateAnonymous<Formula>();
            
            var technologies = Substitute.For<ITechnologyManager>();
            var structure = Substitute.For<IStructure>();

            structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod).Returns(new List<Effect>());
            formula.LaborMoveTime(structure, (byte)labor, technologies).Should().Be(expected);
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
            //ar fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            //var technologies = fixture.CreateAnonymous<TechnologyManager>();
            var formula = new Fixture().CreateAnonymous<Formula>();

            var technologies = Substitute.For<ITechnologyManager>();
            var structure = Substitute.For<IStructure>();

            structure.City.Technologies.GetEffects(EffectCode.LaborMoveTimeMod).Returns(effects);
            formula.LaborMoveTime(structure, (byte)labor, technologies).Should().Be(expected);
        }
    }


}