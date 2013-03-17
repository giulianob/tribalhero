using Game.Data;
using Game.Util.Locking;

namespace Game.Logic
{
    public interface IReferenceManagerFactory
    {
        ReferenceManager CreateReferenceManager(uint cityId, IActionWorker actionWorker, ILockable lockingObj);
    }
}