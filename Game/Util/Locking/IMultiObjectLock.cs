using System;

namespace Game.Util.Locking
{
    public interface IMultiObjectLock : IDisposable
    {
        void Lock(ILockable[] list);

        void UnlockAll();
    }
}