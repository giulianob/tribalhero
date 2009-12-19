using System;
using System.Collections.Generic;
using System.Text;
using Game.Setup;

namespace Game.Data.Stats {
    public class StructureBaseStats {
        string name;
        public string Name {
            get { return name; }
        }

        ushort type;
        public ushort Type {
            get { return type; }
        }

        byte lvl;
        public byte Lvl {
            get { return lvl; }
        }

        Resource cost;
        public Resource Cost {
            get { return cost; }
        }

        int buildTime;
        public int BuildTime {
            get { return buildTime; }
        }

        ClassID baseClass;
        public ClassID BaseClass {
            get { return baseClass; }
        }

        int workerId;
        public int WorkerId {
            get { return workerId; }
        }

        byte maxLabor;
        public byte MaxLabor {
            get { return maxLabor; }
        }

        BaseBattleStats battleStats;
        public BaseBattleStats Battle {
            get { return battleStats; }
        }

        public StructureBaseStats(string name, ushort type, byte lvl, Resource cost, BaseBattleStats baseBattleStats, byte maxLabor, int buildTime, int workerId, ClassID baseClass) {
            this.name = name;
            this.type = type;
            this.lvl = lvl;
            this.cost = cost;
            this.battleStats = baseBattleStats;
            this.maxLabor = maxLabor;
            this.buildTime = buildTime;
            this.baseClass = baseClass;
            this.workerId = workerId;
        }

    }
}