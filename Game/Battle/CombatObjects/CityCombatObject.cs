using System;
using System.Linq;
using Game.Data;
using Game.Data.Troop;

namespace Game.Battle.CombatObjects
{
    public abstract class CityCombatObject : CombatObject
    {
        protected CityCombatObject(uint id, uint battleId, IBattleFormulas battleFormulas)
                : base(id, battleId, battleFormulas)
        {
        }

        public abstract ICity City { get; }

        public abstract ITroopStub TroopStub { get; }

        public override int Hash
        {
            get
            {
                return City.Hash;
            }
        }

        public override object Lock
        {
            get
            {
                return City.Lock;
            }
        }

        public override decimal AttackBonus(ICombatObject target)
        {
            return Math.Min(1, City.Technologies.GetEffects(EffectCode.AttackBonus).Sum(e => (int)e.Value[0] == target.Type ? (int)e.Value[1] : 0) / 100m);
        }

        public override decimal DefenseBonus(ICombatObject attacker)
        {
            return Math.Min(1, City.Technologies.GetEffects(EffectCode.DefenseBonus).Sum(e => (int)e.Value[0] == attacker.Type ? (int)e.Value[1] : 0) / 100m);
        }
    }
}