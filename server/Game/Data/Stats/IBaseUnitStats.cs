namespace Game.Data.Stats
{
    public interface IBaseUnitStats
    {
        string Name { get; }

        string SpriteClass { get; }

        Resource Cost { get; }

        Resource UpgradeCost { get; }

        BaseBattleStats Battle { get; }

        ushort Type { get; }

        byte Lvl { get; }

        int BuildTime { get; }

        int UpgradeTime { get; }

        byte Upkeep { get; }

        int UnitHash { get; }
    }
}