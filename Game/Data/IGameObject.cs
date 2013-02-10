using Game.Logic;

namespace Game.Data
{
    public interface IGameObject : ISimpleGameObject, ICanDo
    {
        bool IsBlocked { get; set; }

        ICity City { get; set; }
    }
}