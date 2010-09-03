using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic.Conditons {
    public interface IBaseBattleStatsCondition {
        bool Check(BaseBattleStats obj);
    }
}
