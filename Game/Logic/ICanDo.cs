#region

using Game.Data;

#endregion

namespace Game.Logic
{
    public interface ICanDo
    {
        ICity City { get; }
        uint WorkerId { get; }
    }
}