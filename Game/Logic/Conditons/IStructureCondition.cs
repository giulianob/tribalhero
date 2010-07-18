using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Battle;
using CSScriptLibrary;
using Game.Data;

namespace Game.Logic.Conditons {
    public interface IStructureCondition {
        bool Check(Structure obj);
    }
}
