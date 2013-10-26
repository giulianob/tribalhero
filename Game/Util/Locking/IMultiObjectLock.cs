using System;

namespace Game.Util.Locking
{
    public interface IMultiObjectLock
    {
        IMultiObjectLock Lock(ILockable[] list);

        void UnlockAll();

        void SortLocks(ILockable[] list);

        T Do<T>(Func<T> protectedBlock);
        
        void Do(Action protectedBlock);
    }
}