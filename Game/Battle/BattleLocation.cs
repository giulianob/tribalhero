using System;

namespace Game.Battle
{
    public enum BattleLocationType
    {
        City,
        Stronghold
    }

    public class BattleLocation
    {
        public BattleLocationType Type { get; set; }
        public uint Id { get; set; }

        public BattleLocation(string type, uint id) :
            this((BattleLocationType)Enum.Parse(typeof(BattleLocationType), type), id)
        {            
        }

        public BattleLocation(BattleLocationType type, uint id)
        {
            Type = type;
            Id = id;
        }
    }
}
