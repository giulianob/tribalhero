#region

using Game.Data;

#endregion

namespace Game.Logic
{
    public interface ICanDo
    {
        City City { get; }
        uint WorkerId { get; }
    }
}