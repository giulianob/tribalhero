using System;
using Game.Data;
using Game.Data.Stronghold;
using Game.Map;

namespace Game.Battle
{
    public enum BattleLocationType
    {
        City = 0,

        Stronghold = 1,

        StrongholdGate = 2,

        BarbarianTribe = 3
    }

    public class BattleLocation
    {
        public BattleLocation(string type, uint id)
                : this((BattleLocationType)Enum.Parse(typeof(BattleLocationType), type), id)
        {
        }

        public BattleLocation(BattleLocationType type, uint id)
        {
            Type = type;
            Id = id;
        }

        public BattleLocationType Type { get; private set; }

        public uint Id { get; private set; }

        public string GetName()
        {
            switch(Type)
            {
                case BattleLocationType.City:
                    ICity city;
                    return World.Current.TryGetObjects(Id, out city) ? city.Name : string.Empty;

                case BattleLocationType.Stronghold:
                case BattleLocationType.StrongholdGate:
                    IStronghold stronghold;
                    return World.Current.TryGetObjects(Id, out stronghold) ? stronghold.Name : string.Empty;
                case BattleLocationType.BarbarianTribe:
                    return "Barbarian Tribe";
                default:
                    return string.Empty;
            }
        }
    }
}