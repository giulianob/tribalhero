﻿using Game.Map;
using Game.Util.Locking;

namespace Common.Testing
{
    public class LockerStub : DefaultLocker
    {
        private class MultiObjectLockStub : IMultiObjectLock
        {
            public void Dispose()
            {
            }

            public void Lock(ILockable[] list)
            {
            }

            public void UnlockAll()
            {
            }
        }

        public LockerStub(IGameObjectLocator locator)
                : base(() => new MultiObjectLockStub(), () => new CallbackLock(() => new MultiObjectLockStub()), locator)
        {
        }
    }
}
