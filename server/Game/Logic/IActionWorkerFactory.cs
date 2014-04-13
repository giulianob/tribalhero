using System;
using Game.Data;
using Game.Util.Locking;

namespace Game.Logic
{
    public interface IActionWorkerFactory
    {
        IActionWorker CreateActionWorker(Func<ILockable> lockDelegate, ILocation location);
    }
}