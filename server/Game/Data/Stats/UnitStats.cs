namespace Game.Data.Stats
{
    public class BaseUnitStats : IBaseUnitStats
    {
        public BaseUnitStats(string name,
                             string spriteClass,
                             ushort type,
                             byte lvl,
                             Resource cost,
                             Resource upgradeCost,
                             BaseBattleStats battleStats,
                             int buildTime,
                             int upgradeTime,
                             byte upkeep)
        {
            Name = name;
            SpriteClass = spriteClass;
            Type = type;
            Lvl = lvl;
            Cost = cost;
            UpgradeCost = upgradeCost;
            Battle = battleStats;
            BuildTime = buildTime;
            UpgradeTime = upgradeTime;
            Upkeep = upkeep;
        }

        public string Name { get; private set; }

        public string SpriteClass { get; private set; }

        public Resource Cost { get; private set; }

        public Resource UpgradeCost { get; private set; }

        public BaseBattleStats Battle { get; private set; }

        public ushort Type { get; private set; }

        public byte Lvl { get; private set; }

        public int BuildTime { get; private set; }

        public int UpgradeTime { get; private set; }

        public byte Upkeep { get; private set; }

        public int UnitHash
        {
            get
            {
                return Type * 100 + Lvl;
            }
        }
    }
}