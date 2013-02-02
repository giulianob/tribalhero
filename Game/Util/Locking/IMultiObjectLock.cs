using System;

namespace Game.Util.Locking
{
    public interface IMultiObjectLock : IDisposable
    {
        IMultiObjectLock Lock(ILockable[] list);

        void UnlockAll();
    }
}