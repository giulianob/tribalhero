using Game.Data;

namespace Game.Logic.Triggers
{
    public interface IDynamicAction
    {
        string[] Parms { get; }

        void Execute(IGameObject obj);
    }
}