using Game.Logic;

namespace Game.Data
{
    public interface IGameObject : ISimpleGameObject, ICanDo
    {
        bool CheckBlocked(uint actionId);

        ICity City { get; set; }
    }
}