using Game.Data;

namespace Game.Battle.CombatObjects
{
    public abstract class CityCombatObject : CombatObject
    {
        protected CityCombatObject(uint battleId, BattleFormulas battleFormulas) : base(battleId, battleFormulas)
        {
            
        }

        public abstract uint PlayerId { get; }

        public abstract ICity City { get; }

        public override bool BelongsTo(IPlayer player)
        {
            return City.Owner == player;
        }

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

        public abstract Resource GroupLoot { get; }
    }
}
