using System;
using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Map;
using NSubstitute;
using Persistance;
using Xunit.Extensions;

namespace Testing.BattleTests
{
    
    public class CityCombatObjectTest
    {
        [Theory, AutoNSubstituteData]
        public void AttackBonus_WhenNoEffectFound_ShouldReturn0(ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            cityCombatObject.City.Technologies.GetEffects(EffectCode.AttackBonus).Returns(new List<Effect>());
            cityCombatObject.AttackBonus(target).Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void AttackBonus_WhenOneEffectFound_ShouldReturn0_02(ushort targetType, ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect {Value = new object[] {(int)targetType, 2}}
            };
            target.Type.Returns(targetType);
            cityCombatObject.City.Technologies.GetEffects(EffectCode.AttackBonus).Returns(effects);
            cityCombatObject.AttackBonus(target).Should().Be(0.02m);
        }

        [Theory, AutoNSubstituteData]
        public void AttackBonus_WhenThreeEffectsFound_ShouldReturnTheSum(ushort targetType, ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect {Value = new object[] {(int)targetType, 2}},
                new Effect {Value = new object[] {(int)targetType, 3}},
                new Effect {Value = new object[] {(int)targetType, 4}},
            };
            target.Type.Returns(targetType);
            cityCombatObject.City.Technologies.GetEffects(EffectCode.AttackBonus).Returns(effects);
            cityCombatObject.AttackBonus(target).Should().Be(0.09m);
        }


        [Theory, AutoNSubstituteData]
        public void AttackBonus_WhenValueIs101_ShouldReturn100(ushort targetType, ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect {Value = new object[] {(int)targetType, 101}},
            };
            target.Type.Returns(targetType);
            cityCombatObject.City.Technologies.GetEffects(EffectCode.AttackBonus).Returns(effects);
            cityCombatObject.AttackBonus(target).Should().Be(1m);
        }

        [Theory, AutoNSubstituteData]
        public void DefenseBonus_WhenNoEffectFound_ShouldReturn0(ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            cityCombatObject.City.Technologies.GetEffects(EffectCode.DefenseBonus).Returns(new List<Effect>());
            cityCombatObject.DefenseBonus(target).Should().Be(0);
        }

        [Theory, AutoNSubstituteData]
        public void DefenseBonus_WhenOneEffectFound_ShouldReturn0_02(ushort targetType, ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect {Value = new object[] {(int)targetType, 2}}
            };
            target.Type.Returns(targetType);
            cityCombatObject.City.Technologies.GetEffects(EffectCode.DefenseBonus).Returns(effects);
            cityCombatObject.DefenseBonus(target).Should().Be(0.02m);
        }

        [Theory, AutoNSubstituteData]
        public void DefenseBonus_WhenThreeEffectsFound_ShouldReturnTheSum(ushort targetType, ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect {Value = new object[] {(int)targetType, 2}},
                new Effect {Value = new object[] {(int)targetType, 3}},
                new Effect {Value = new object[] {(int)targetType, 4}},
            };
            target.Type.Returns(targetType);
            cityCombatObject.City.Technologies.GetEffects(EffectCode.DefenseBonus).Returns(effects);
            cityCombatObject.DefenseBonus(target).Should().Be(0.09m);
        }


        [Theory, AutoNSubstituteData]
        public void DefenseBonus_WhenValueIs101_ShouldReturn100(ushort targetType, ICombatObject target, StubCityCombatObject cityCombatObject)
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect {Value = new object[] {(int)targetType, 101}},
            };
            target.Type.Returns(targetType);
            cityCombatObject.City.Technologies.GetEffects(EffectCode.DefenseBonus).Returns(effects);
            cityCombatObject.DefenseBonus(target).Should().Be(1m);
        }

        public class StubCityCombatObject : CityCombatObject
        {
            private byte size;
            private int upkeep;
            private bool isDead;
            private BattleStats stats;
            private ushort type;
            private Resource loot;
            private BattleClass classType;
            private ushort count;
            private decimal hp;
            private uint visibility;
            private byte lvl;
            private string dbTable;
            private DbColumn[] dbPrimaryKey;
            private IEnumerable<DbDependency> dbDependencies;
            private DbColumn[] dbColumns;
            private ICity city;
            private ITroopStub troopStub;

            public StubCityCombatObject(uint id, uint battleId, IBattleFormulas battleFormulas, IDbManager dbManager) : base(id, battleId, battleFormulas, dbManager)
            {
                city = Substitute.For<ICity>();
            }

            public override byte Lvl
            {
                get
                {
                    return lvl;
                }
            }

            public override byte Size
            {
                get
                {
                    return size;
                }
            }

            public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
            {
                throw new NotImplementedException();
            }

            public override void CalcActualDmgToBeTaken(ICombatList attackers, ICombatList defenders, IBattleRandom random, decimal baseDmg, int attackIndex, out decimal actualDmg)
            {
                throw new NotImplementedException();
            }

            public override bool InRange(ICombatObject obj)
            {
                throw new NotImplementedException();
            }

            public override Position Location()
            {
                throw new NotImplementedException();
            }

            public override byte AttackRadius()
            {
                throw new NotImplementedException();
            }

            public override void ReceiveReward(int reward, Resource resource)
            {
                throw new NotImplementedException();
            }

            public override int LootPerRound()
            {
                throw new NotImplementedException();
            }

            public override string DbTable
            {
                get
                {
                    return dbTable;
                }
            }

            public override DbColumn[] DbPrimaryKey
            {
                get
                {
                    return dbPrimaryKey;
                }
            }

            public override DbColumn[] DbColumns
            {
                get
                {
                    return dbColumns;
                }
            }

            public override IEnumerable<DbDependency> DbDependencies
            {
                get
                {
                    return dbDependencies;
                }
            }

            public override int Upkeep
            {
                get
                {
                    return upkeep;
                }
            }

            public override bool IsDead
            {
                get
                {
                    return isDead;
                }
            }

            public override BattleStats Stats
            {
                get
                {
                    return stats;
                }
            }

            public override ushort Type
            {
                get
                {
                    return type;
                }
            }

            public override Resource Loot
            {
                get
                {
                    return loot;
                }
            }

            public override BattleClass ClassType
            {
                get
                {
                    return classType;
                }
            }

            public override ushort Count
            {
                get
                {
                    return count;
                }
            }

            public override decimal Hp
            {
                get
                {
                    return hp;
                }
            }

            public override uint Visibility
            {
                get
                {
                    return visibility;
                }
            }

            public override ICity City
            {
                get
                {
                    return city;
                }
            }

            public override ITroopStub TroopStub
            {
                get
                {
                    return troopStub;
                }
            }
        }
    }
}
