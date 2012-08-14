using System;
using Game.Data;
using Game.Map;

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

        public string GetName()
        {
            switch (Type)
            {
                case BattleLocationType.City:
                    ICity city;
                    return World.Current.TryGetObjects(Id, out city) ? city.Name : string.Empty;
                // TODO: Add Stronghold name support
                default:
                    return string.Empty;
            }
        }
    }
}
