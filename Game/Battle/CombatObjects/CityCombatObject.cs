using Game.Data;

namespace Game.Battle.CombatObjects
{
    public abstract class CityCombatObject : CombatObject
    {
        protected CityCombatObject(uint id, uint battleId, BattleFormulas battleFormulas) : base(id, battleId, battleFormulas)
        {
            
        }

        public abstract uint PlayerId { get; }

        public abstract ICity City { get; }        

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
