#region

using Game.Data;

#endregion

namespace Game.Logic
{
    public interface ICanDo
    {
        uint WorkerId { get; }

        bool IsBlocked { get; set; }
    }
}