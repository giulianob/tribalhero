namespace Game.Data.Stats {
    public class BattleStats : BaseStats {
        #region Base Stats

        private ushort maxHp = 0;

        public ushort MaxHp {
            get { return maxHp; }
            set { maxHp = value; }
        }

        private byte atk = 0;

        public byte Atk {
            get { return atk; }
            set { atk = value; }
        }

        private byte def = 0;

        public byte Def {
            get { return def; }
            set { def = value; }
        }

        private byte rng = 0;

        public byte Rng {
            get { return rng; }
            set { rng = value; }
        }

        private byte stl = 0;

        public byte Stl {
            get { return stl; }
            set { stl = value; }
        }

        private byte spd = 0;

        public byte Spd {
            get { return spd; }
            set { spd = value; }
        }

        private BaseBattleStats baseStats;

        public BaseBattleStats Base {
            get { return baseStats; }
        }

        #endregion

        #region Constructors

        public BattleStats(BaseBattleStats baseStats) {
            this.baseStats = baseStats;
            maxHp = baseStats.MaxHp;
            atk = baseStats.Atk;
            def = baseStats.Def;
            rng = baseStats.Rng;
            stl = baseStats.Stl;
            spd = baseStats.Spd;
        }

        #endregion
    }
}