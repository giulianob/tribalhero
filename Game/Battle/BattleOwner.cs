using System;

namespace Game.Battle
{
    public enum BattleOwnerType
    {
        City,
        Tribe
    }

    public class BattleOwner
    {
        public BattleOwnerType Type { get; set; }
        public uint Id { get; set; }

        public BattleOwner(string type, uint id) :
            this((BattleOwnerType)Enum.Parse(typeof(BattleOwnerType), type), id)
        {            
        }

        public BattleOwner(BattleOwnerType type, uint id)
        {
            Type = type;
            Id = id;
        }
    }
}
