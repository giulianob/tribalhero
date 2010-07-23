namespace Game.Data.Stats {
    public class StructureStats : BaseStats {


        private ushort hp;

        public ushort Hp {
            get { return hp; }
            set {
                hp = value;
                FireStatsUpdate();
            }
        }

        private byte labor;

        public byte Labor {
            get { return labor; }
            set {
                labor = value;
                FireStatsUpdate();
            }
        }

        public StructureBaseStats Base { get; private set; }

        public StructureStats(StructureBaseStats baseStats) {
            Base = baseStats;

            hp = baseStats.Battle.MaxHp;
        }
    }
}