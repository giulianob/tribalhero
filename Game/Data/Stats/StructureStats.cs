using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Data.Stats {    
    public class StructureStats : BaseStats {

        byte maxLabor;
        public byte MaxLabor {
            get { return maxLabor; }
            set { maxLabor = value; fireStatsUpdate(); }
        }

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
            
            //Set the defaults below for stats that can be modified
            this.maxLabor = baseStats.MaxLabor;
            this.hp = baseStats.Battle.MaxHp;
        }        
    }
}
