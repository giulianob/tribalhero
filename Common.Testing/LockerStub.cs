using Game.Map;
using Game.Util.Locking;

namespace Common.Testing
{
    public class LockerStub : DefaultLocker
    {
        public LockerStub(IGameObjectLocator locator)
                : base(() => new MultiObjectLockStub(), () => new CallbackLock(() => new MultiObjectLockStub()), locator
                        )
        {
        }

        private class MultiObjectLockStub : IMultiObjectLock
        {
            public void Dispose()
            {
            }

            public IMultiObjectLock Lock(ILockable[] list)
            {
                return this;
            }

            public void UnlockAll()
            {
            }

            public void SortLocks(ILockable[] list)
            {                
            }
        }
    }
}