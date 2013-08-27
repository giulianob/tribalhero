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
    }
}