using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Battle;

namespace Game.Logic.Conditons {
    public interface IICombatUnitCondition {
        bool Check(ICombatUnit obj);
    }
}
