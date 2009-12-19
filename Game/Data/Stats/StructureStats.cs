using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Data.Stats {    
    public class StructureStats : BaseStats {
        ushort hp;
        public ushort Hp {
            get { return hp; }
            set { hp = value; fireStatsUpdate(); }
        }

        byte labor = 0;
        public byte Labor {
            get { return labor; }
            set { labor = value; fireStatsUpdate(); }
        }

        StructureBaseStats baseStats;
        public StructureBaseStats Base {
            get { return baseStats; }
        }
        
        public StructureStats(StructureBaseStats baseStats) {
            this.baseStats = baseStats;
            
            this.hp = baseStats.Battle.MaxHp;
        }        
    }
}
