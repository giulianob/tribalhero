using Game.Util.Locking;

namespace Game.Logic
{
    public interface IReferenceManagerFactory
    {
        IReferenceManager CreateReferenceManager(uint cityId, IActionWorker actionWorker, ILockable lockingObj);
    }
}