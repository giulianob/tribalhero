using System;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Map;

namespace Game.Battle
{
    public enum BattleOwnerType
    {
        City = 0,

        Tribe = 1,

        Stronghold = 2,

        BarbarianTribe = 3
    }

    public class BattleOwner
    {
        public BattleOwner(BattleOwnerType type, uint id)
        {
            Type = type;
            Id = id;
        }

        public BattleOwnerType Type { get; set; }

        public uint Id { get; set; }

        public bool IsOwner(IPlayer player)
        {
            switch(Type)
            {
                case BattleOwnerType.City:
                    return player.GetCity(Id) != null;
            }

            return false;
        }

        public string GetName()
        {
            switch(Type)
            {
                case BattleOwnerType.City:
                    ICity city;
                    return World.Current.TryGetObjects(Id, out city) ? city.Name : string.Empty;
                case BattleOwnerType.Tribe:
                    ITribe tribe;
                    return World.Current.TryGetObjects(Id, out tribe) ? tribe.Name : string.Empty;
                case BattleOwnerType.Stronghold:
                    IStronghold stronghold;
                    return World.Current.TryGetObjects(Id, out stronghold) ? stronghold.Name : string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}