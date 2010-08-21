using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Battle;

namespace Game.Logic.Conditons {
    public interface ICombatObjectCondition {
        bool Check(CombatObject obj);
    }
}
