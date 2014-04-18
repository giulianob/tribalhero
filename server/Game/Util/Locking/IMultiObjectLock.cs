using System;
using JetBrains.Annotations;

namespace Game.Util.Locking
{
    public interface IMultiObjectLock
    {
        IMultiObjectLock Lock(ILockable[] list);

        void UnlockAll();

        void SortLocks(ILockable[] list);

        T Do<T>([InstantHandle] Func<T> protectedBlock);
        
        void Do([InstantHandle] Action protectedBlock);
    }
}