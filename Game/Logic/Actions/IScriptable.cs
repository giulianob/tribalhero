using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic {
    interface IScriptable {
        void ScriptInit(GameObject obj, string[] parms);
    }
}
