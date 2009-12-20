using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Data.Stats {
    public class BaseUnitStats {
        string name;
        public string Name {
            get { return name; }
        }

        Resource cost;
        public Resource Cost {
            get { return cost; }
        }

        Resource upgradeCost;
        public Resource UpgradeCost {
            get { return upgradeCost; }
        }

        BaseBattleStats battleStats;
        public BaseBattleStats Battle {
            get { return battleStats; }
        }

        ushort type;
        public ushort Type {
            get { return type; }
        }

        byte lvl;
        public byte Lvl {
            get { return lvl; }
        }

        int buildTime;
        public int BuildTime {
            get { return buildTime; }
        }

        int upgradeTime;
        public int UpgradeTime {
            get { return upgradeTime; }
        }

        byte upkeep;
        public byte Upkeep {
            get { return upkeep; }
        }

        public BaseUnitStats(string name, ushort type, byte lvl, Resource cost, Resource upgradeCost, BaseBattleStats battleStats, int buildTime, int upgradeTime, byte upkeep) {
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
