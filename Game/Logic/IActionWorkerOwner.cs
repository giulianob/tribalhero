using Game.Data;

namespace Game.Logic
{
    public interface IActionWorkerOwner : ILocation
    {
        IActionWorker Worker { get; }
    }
}