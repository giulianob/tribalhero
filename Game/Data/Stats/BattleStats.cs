using System;
using System.Collections.Generic;
using System.Text;
using Game.Setup;

namespace Game.Data.Stats {    
    public class BattleStats : BaseStats {

        #region Base Stats

        ushort maxHp = 0;
        public ushort MaxHp {
            get { return maxHp; }
            set { maxHp = value; }
        }

        byte atk = 0;
        public byte Atk {
            get { return atk; }
            set { atk = value; }
        }

        byte def = 0;
        public byte Def {
            get { return def; }
            set { def = value; }
        }

        byte rng = 0;
        public byte Rng {
            get { return rng; }
            set { rng = value; }
        }

        byte stl = 0;
        public byte Stl {
            get { return stl; }
            set { stl = value; }
        }

        byte spd = 0;
        public byte Spd {
            get { return spd; }
            set { spd = value; }
        }

        BaseBattleStats baseStats;
        public BaseBattleStats Base {
            get { return baseStats; }
        }
        #endregion

        #region Constructors
        public BattleStats(BaseBattleStats baseStats) {
            this.baseStats = baseStats;
            this.maxHp = baseStats.MaxHp;
            this.atk = baseStats.Atk;
            this.def = baseStats.Def;
            this.rng = baseStats.Rng;
            this.stl = baseStats.Stl;
            this.spd = baseStats.Spd;
        }
        #endregion
    }
}
