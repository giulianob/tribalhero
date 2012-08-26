using Game.Logic;

namespace Game.Data
{
    public interface IGameObject : ISimpleGameObject, ICanDo
    {
        bool IsBlocked { get; set; }

        new ICity City { get; set; }
    }
}