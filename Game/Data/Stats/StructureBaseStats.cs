#region

using Game.Setup;

#endregion

namespace Game.Data.Stats {
    public class StructureBaseStats {
        private string name;

        public string Name {
            get { return name; }
        }

        private ushort type;

        public ushort Type {
            get { return type; }
        }

        private byte lvl;

        public byte Lvl {
            get { return lvl; }
        }

        private Resource cost;

        public Resource Cost {
            get { return cost; }
        }

        private int buildTime;

        public int BuildTime {
            get { return buildTime; }
        }

        private ClassID baseClass;

        public ClassID BaseClass {
            get { return baseClass; }
        }

        private int workerId;

        public int WorkerId {
            get { return workerId; }
        }

        private byte maxLabor;

        public byte MaxLabor {
            get { return maxLabor; }
        }

        private byte radius;

        public byte Radius {
            get { return radius; }
        }

        private BaseBattleStats battleStats;

        public BaseBattleStats Battle {
            get { return battleStats; }
        }

        public StructureBaseStats(string name, ushort type, byte lvl, byte radius, Resource cost, BaseBattleStats baseBattleStats,
                                  byte maxLabor, int buildTime, int workerId, ClassID baseClass) {
            this.name = name;
            this.radius = radius;
            this.type = type;
            this.lvl = lvl;
            this.cost = cost;
            battleStats = baseBattleStats;
            this.maxLabor = maxLabor;
            this.buildTime = buildTime;
            this.baseClass = baseClass;
            this.workerId = workerId;
        }
    }
}