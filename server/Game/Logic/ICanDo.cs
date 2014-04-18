#region



#endregion

namespace Game.Logic
{
    public interface ICanDo
    {
        uint WorkerId { get; }

        uint IsBlocked { get; set; }        
    }
}