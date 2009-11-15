using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;

namespace Game.Battle {
    public interface ICombatUnit {
        Resource Loot { get; }

        TroopStub TroopStub { get; }

        FormationType Formation { get; }
        
    }
}
