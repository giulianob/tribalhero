#region

using Game.Data;

#endregion

namespace Game.Logic
{
    public interface IScriptable
    {
        void ScriptInit(IGameObject obj, string[] parms);
    }
}