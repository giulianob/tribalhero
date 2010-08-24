namespace Game.Data.Stats {
    public class BaseUnitStats {
        private string name;

        public string Name {
            get { return name; }
        }

        private Resource cost;

        public Resource Cost {
            get { return cost; }
        }

        private Resource upgradeCost;

        public Resource UpgradeCost {
            get { return upgradeCost; }
        }

        private BaseBattleStats battleStats;

        public BaseBattleStats Battle {
            get { return battleStats; }
        }

        private ushort type;

        public ushort Type {
            get { return type; }
        }

        private byte lvl;

        public byte Lvl {
            get { return lvl; }
        }

        private int buildTime;

        public int BuildTime {
            get { return buildTime; }
        }

        private int upgradeTime;

        public int UpgradeTime {
            get { return upgradeTime; }
        }

        private byte upkeep;

        public byte Upkeep {
            get { return upkeep; }
        }

        public int UnitHash {
            get { return type*100 + lvl; }
        }

        public BaseUnitStats(string name, ushort type, byte lvl, Resource cost, Resource upgradeCost,
                             BaseBattleStats battleStats, int buildTime, int upgradeTime, byte upkeep) {
            this.name = name;
            this.type = type;
            this.lvl = lvl;
            this.cost = cost;
            this.upgradeCost = upgradeCost;
            this.battleStats = battleStats;
            this.buildTime = buildTime;
            this.upgradeTime = upgradeTime;
            this.upkeep = upkeep;
        }
    }
}