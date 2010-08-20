#region

using Game.Data;

#endregion

namespace Game.Logic {
    interface IScriptable {
        void ScriptInit(GameObject obj, string[] parms);
    }
}