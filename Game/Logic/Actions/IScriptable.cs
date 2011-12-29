#region

using Game.Data;

#endregion

namespace Game.Logic
{
    interface IScriptable
    {
        void ScriptInit(IGameObject obj, string[] parms);
    }
}