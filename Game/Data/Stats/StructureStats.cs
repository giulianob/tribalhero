using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Data {
    public class StructureStats {

        byte maxLabor;
        public byte MaxLabor {
            get { return maxLabor; }
        }

        BattleStats battleStats;
        public BattleStats Battle {
            get { return battleStats; }
        }
        
        public StructureStats(BattleStats battleStats,byte maxLabor) {
            this.battleStats = battleStats;
            this.maxLabor = maxLabor;
        }
        
    }
}
