namespace Game.Data.Stats {
    public class StructureStats : BaseStats {
        private ushort hp;

        public ushort Hp {
            get { return hp; }
            set {
                hp = value;
                fireStatsUpdate();
            }
        }

        private byte labor = 0;

        public byte Labor {
            get { return labor; }
            set {
                labor = value;
                fireStatsUpdate();
            }
        }

        private StructureBaseStats baseStats;

        public StructureBaseStats Base {
            get { return baseStats; }
        }

        public StructureStats(StructureBaseStats baseStats) {
            this.baseStats = baseStats;

            hp = baseStats.Battle.MaxHp;
        }
    }
}