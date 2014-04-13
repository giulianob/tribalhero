using Game.Data;

namespace Game.Logic.Conditons
{
    public interface ICityEvent
    {
        IGameObject GameObject { get; }
        dynamic Parameters { get; }
    }
}