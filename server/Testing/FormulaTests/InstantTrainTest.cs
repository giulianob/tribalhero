using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.FormulaTests
{

    public class InstantTrainTest
    {
        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenNoEffectIsFound(Formula formula, IStructure structure)
        {
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(new List<Effect>());
            formula.GetInstantTrainCount(structure).Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenEffectIsAssociateWithDifferentStructure(Formula formula,
                                                                                     IStructure structure)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City, 
                    Value = new object[] {1,"BarrackUnits",5,150}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            formula.GetInstantTrainCount(structure).Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenEffectIsAssociateWithMultipleUnits([Frozen] IObjectTypeFactory objectTypeFactory,
                                                                                Formula formula,
                                                                                IStructure structure,
                                                                                ITroopStub troopStub)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City,
                    Value = new object[] {1, "BarrackUnits", 50, 50}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            objectTypeFactory.IsObjectType("BarrackUnits", 101).Returns(true);
            objectTypeFactory.IsObjectType("BarrackUnits", 102).Returns(true);
            objectTypeFactory.IsObjectType("BarrackUnits", 105).Returns(false);
            structure.Type.Returns((ushort)1);

            troopStub.ToUnitList().ReturnsForAnyArgs(new List<Unit>
            {
                new Unit(101, 10),
                new Unit(102, 25),
                new Unit(105, 10)
            });
            structure.City.Troops.MyStubs().Returns(new List<ITroopStub> {troopStub});

            formula.GetInstantTrainCount(structure).Should().Be(15);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenThereAreMultipleEffects([Frozen] IObjectTypeFactory objectTypeFactory,
                                                                     Formula formula,
                                                                     IStructure structure)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City,
                    Value = new object[] {1, "BarrackUnits", 20, 150}
                },
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City,
                    Value = new object[] {1, "BarrackUnits", 30, 150}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            objectTypeFactory.IsObjectType("BarrackUnits", 101).Returns(true);
            structure.Type.Returns((ushort)1);
            formula.GetInstantTrainCount(structure).Should().Be(50);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenEffectIsOverCapacity([Frozen] IObjectTypeFactory objectTypeFactory,
                                                                  Formula formula,
                                                                  IStructure structure)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City, 
                    Value = new object[] {1,"BarrackUnits",51,50}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            objectTypeFactory.IsObjectType("BarrackUnits", 101).Returns(true);
            structure.Type.Returns((ushort)1);
            formula.GetInstantTrainCount(structure).Should().Be(50);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenEffectIsUnderCapacity([Frozen] IObjectTypeFactory objectTypeFactory,
                                                                   Formula formula,
                                                                   IStructure structure)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City,
                    Value = new object[] {1, "BarrackUnits", 50, 50}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            objectTypeFactory.IsObjectType("BarrackUnits", 101).Returns(true);
            structure.Type.Returns((ushort)1);
            formula.GetInstantTrainCount(structure).Should().Be(50);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenCurrentCountIsOverCapacity([Frozen] IObjectTypeFactory objectTypeFactory,
                                                                        Formula formula,
                                                                        IStructure structure,
                                                                        ITroopStub troopStub)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City,
                    Value = new object[] {1, "BarrackUnits", 50, 50}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            objectTypeFactory.IsObjectType("BarrackUnits", 101).Returns(true);
            structure.Type.Returns((ushort)1);

            troopStub.ToUnitList().ReturnsForAnyArgs(new List<Unit>
            {
                new Unit(101, 51)
            });
            structure.City.Troops.MyStubs().Returns(new List<ITroopStub> {troopStub});

            formula.GetInstantTrainCount(structure).Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void GetInstantTrainCount_WhenCurrentCountIsUnderCapacity([Frozen] IObjectTypeFactory objectTypeFactory,
                                                                         Formula formula,
                                                                         IStructure structure,
                                                                         ITroopStub troopStub)
        {
            var effects = new List<Effect>
            {
                new Effect
                {
                    Id = EffectCode.UnitTrainInstantTime,
                    IsPrivate = false,
                    Location = EffectLocation.City,
                    Value = new object[] {1, "BarrackUnits", 50, 50}
                }
            };
            structure.City.Technologies.GetEffects(EffectCode.UnitTrainInstantTime).Returns(effects);
            objectTypeFactory.IsObjectType("BarrackUnits", 101).Returns(true);
            structure.Type.Returns((ushort)1);

            troopStub.ToUnitList().ReturnsForAnyArgs(new List<Unit>
            {
                new Unit(101, 49)
            });
            structure.City.Troops.MyStubs().Returns(new List<ITroopStub> {troopStub});

            formula.GetInstantTrainCount(structure).Should().Be(1);
        }
    }
}
