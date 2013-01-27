using System.Collections.Generic;
using FluentAssertions;
using Game.Battle;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Procedures;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.Formulas
{
    public class UpkeepTest
    {
        public static IEnumerable<object[]> WithDifferentTroops
        {
            get
            {
                // Single troop with one unit and no effects
                yield return new object[] { 
                    // Stubs
                    new[] { 
                        MockStub(new[] {
                            new Formation(FormationType.Normal) {
                                    { 1, 10 }
                            }
                        })
                    },
                    // Effects
                    new List<Effect>(),
                    // Expected upkeep
                    100
                };

                // Single troop with one unit and one effect for that unit
                yield return new object[] { 
                    // Stubs
                    new[] { 
                        MockStub(new[] {
                            new Formation(FormationType.Normal) {
                                    { 1, 10 }
                            }
                        })
                    },
                    // Effects
                    new List<Effect>
                    {
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 10 /* reduction */ , "1" /* unit type */ }
                            }
                    },
                    // Expected upkeep
                    90
                };

                // Testing truncation, should use ceiling
                yield return new object[] { 
                    // Stubs
                    new[] { 
                        MockStub(new[] {
                            new Formation(FormationType.Normal) {
                                    { 2, 10 }
                            }
                        })
                    },
                    // Effects
                    new List<Effect>
                    {
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 12 /* reduction */ , "2" /* unit type */ }
                            }
                    },
                    // Expected upkeep
                    97
                };

                // Testing caps at 30% max reduction
                yield return new object[] { 
                    // Stubs
                    new[] { 
                        MockStub(new[] {
                            new Formation(FormationType.Normal) {
                                    { 1, 10 }
                            }
                        })
                    },
                    // Effects
                    new List<Effect>
                    {
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 10 /* reduction */ , "1" /* unit type */ }
                            },
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 65 /* reduction */ , "1" /* unit type */ }
                            }
                    },
                    // Expected upkeep
                    70
                };

                // Hiding units should pay penalty
                yield return new object[] { 
                    // Stubs
                    new[] { 
                        MockStub(new[] {
                            new Formation(FormationType.Garrison) {
                                    { 1, 10 }
                            }
                        })
                    },
                    // Effects
                    new List<Effect>(),
                    // Expected upkeep
                    125
                };

                // Multiple troops w/ multiple formations and several effects
                yield return new object[] { 
                    // Stubs
                    new[] { 
                        MockStub(new[] {
                            new Formation(FormationType.Normal) {
                                    { 2, 10 }
                            },
                            new Formation(FormationType.Garrison) {
                                    { 1, 10 }
                            }
                        }),
                        MockStub(new[] {
                            new Formation(FormationType.Normal) {
                                    { 1, 10 },
                                    { 3, 5 },
                            },
                        })
                    },
                    // Effects
                    new List<Effect>
                    {
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 5 /* reduction */ , "1" /* unit type */ }
                            },
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 5 /* reduction */ , "1" /* unit type */ }
                            },
                            new Effect
                            {
                                    Id = EffectCode.UpkeepReduce, 
                                    IsPrivate = true, 
                                    Location = EffectLocation.Object,
                                    Value = new object[] { 12 /* reduction */ , "2" /* unit type */ }
                            }
                    },
                    // Expected upkeep
                    450
                };

            }
        }        

        public static ITroopStub MockStub(IEnumerable<Formation> formations)
        {
            var stub = Substitute.For<ITroopStub>();

            stub.GetEnumerator().Returns(formations.GetEnumerator());

            return stub;
        }

        [Theory, PropertyData("WithDifferentTroops")]
        public void CityUpkeepShouldReturnProperValue(IEnumerable<ITroopStub> troops, List<Effect> effects, int expected)
        {
            // Setup
            var procedure = new Procedure();

            var city = Substitute.For<ICity>();
            city.Troops.MyStubs().Returns(troops);
            city.Technologies.GetEffects(EffectCode.UpkeepReduce).Returns(effects);

            var battleFormulas = BattleFormulas.Current = Substitute.For<BattleFormulas>();

            // Just making UnitStatModCheck return true/false depending on whether the effect arg matches the type of the unit
            battleFormulas.UnitStatModCheck(Arg.Any<BaseBattleStats>(), TroopBattleGroup.Any, Arg.Any<string>())
                          .Returns(x => x.Arg<BaseBattleStats>().Type == ushort.Parse(x.Arg<string>()));

            // Unit with type 1 has 10 upkeep
            // Unit with type 2 has 11 upkeep            
            // Unit with type 3 has 30 upkeep
            var baseUnitStats1 = Substitute.For<IBaseUnitStats>();
            baseUnitStats1.Upkeep.Returns((byte)10);
            baseUnitStats1.Type.Returns((byte)1);
            var baseBattleStats1 = Substitute.For<BaseBattleStats>();
            baseBattleStats1.Type.Returns((byte)1);
            baseUnitStats1.Battle.Returns(baseBattleStats1);
            city.Template[1].Returns(baseUnitStats1);           
 
            var baseUnitStats2 = Substitute.For<IBaseUnitStats>();
            baseUnitStats2.Upkeep.Returns((byte)11);
            baseUnitStats2.Type.Returns((byte)2);
            var baseBattleStats2 = Substitute.For<BaseBattleStats>();
            baseBattleStats2.Type.Returns((byte)2);
            baseUnitStats2.Battle.Returns(baseBattleStats2);
            city.Template[2].Returns(baseUnitStats2);            

            var baseUnitStats3 = Substitute.For<IBaseUnitStats>();
            baseUnitStats3.Upkeep.Returns((byte)30);
            baseUnitStats3.Type.Returns((byte)3);
            var baseBattleStats3 = Substitute.For<BaseBattleStats>();
            baseBattleStats3.Type.Returns((byte)3);
            baseUnitStats3.Battle.Returns(baseBattleStats3);
            city.Template[3].Returns(baseUnitStats3);        

            // Execute
            procedure.UpkeepForCity(city).Should().Be(expected);            
            city.Technologies.Received(1).GetEffects(EffectCode.UpkeepReduce);
        }
    }
}