#region

using System;
using FluentAssertions;
using Game.Util.Locking;
using NSubstitute;
using Persistance;
using Xunit;

#endregion

namespace Testing.LockingTests
{
    /// <summary>
    ///     Summary description for TroopProcedureTest
    /// </summary>
    public class CallbackLockTest
    {
        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory = () => new DefaultMultiObjectLock(Substitute.For<IDbManager>());

        public CallbackLockTest()
        {
            DefaultMultiObjectLock.ClearCurrentLock();
        }

        [Fact]
        public void TestEmptyListFromCallback()
        {
            var obj = new DummyLockable(1);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {};
            var lck = new CallbackLock(multiObjectLockFactory).Lock(lockFunc, null, obj);
            DefaultMultiObjectLock.IsLocked(obj).Should().BeTrue();
            lck.UnlockAll();
            DefaultMultiObjectLock.IsLocked(obj).Should().BeFalse();
        }

        [Fact]
        public void TestSingleItemFromCallback()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {obj2};
            var lck = new CallbackLock(multiObjectLockFactory).Lock(lockFunc, null, obj);

            DefaultMultiObjectLock.IsLocked(obj).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeTrue();

            lck.UnlockAll();

            DefaultMultiObjectLock.IsLocked(obj).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeFalse();
        }

        [Fact]
        public void TestMultiItemFromCallback()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            var obj3 = new DummyLockable(3);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {obj3, obj2};
            var lck = new CallbackLock(multiObjectLockFactory).Lock(lockFunc, null, obj);

            DefaultMultiObjectLock.IsLocked(obj).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj3).Should().BeTrue();

            lck.UnlockAll();

            DefaultMultiObjectLock.IsLocked(obj).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj3).Should().BeFalse();
        }

        [Fact]
        public void TestMultiBaseItems()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {};
            var lck = new CallbackLock(multiObjectLockFactory).Lock(lockFunc, null, obj, obj2);

            DefaultMultiObjectLock.IsLocked(obj).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeTrue();

            lck.UnlockAll();

            DefaultMultiObjectLock.IsLocked(obj).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeFalse();
        }

        [Fact]
        public void TestChangingCallbackAddLocks()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            var obj3 = new DummyLockable(3);

            int callCount = 0;
            CallbackLock.CallbackLockHandler lockFunc = delegate
                {
                    if (callCount == 0)
                    {
                        callCount++;
                        return new ILockable[] {obj2};
                    }

                    callCount++;
                    return new ILockable[] {obj3, obj2};
                };
            var lck = new CallbackLock(multiObjectLockFactory).Lock(lockFunc, null, obj);

            DefaultMultiObjectLock.IsLocked(obj).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj3).Should().BeTrue();

            lck.UnlockAll();

            DefaultMultiObjectLock.IsLocked(obj).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj3).Should().BeFalse();
        }

        [Fact]
        public void TestChangingCallbackRemoveLocks()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            var obj3 = new DummyLockable(3);

            int callCount = 0;
            CallbackLock.CallbackLockHandler lockFunc = delegate
                {
                    if (callCount == 0)
                    {
                        callCount++;
                        return new ILockable[] {obj2, obj3};
                    }

                    callCount++;
                    return new ILockable[] {obj2};
                };
            var lck = new CallbackLock(multiObjectLockFactory).Lock(lockFunc, null, obj);

            DefaultMultiObjectLock.IsLocked(obj).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(obj3).Should().BeFalse();

            lck.UnlockAll();

            DefaultMultiObjectLock.IsLocked(obj).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj2).Should().BeFalse();
            DefaultMultiObjectLock.IsLocked(obj3).Should().BeFalse();
        }

        #region Nested type: DummyLockable

        private class DummyLockable : ILockable
        {
            public DummyLockable(int id)
            {
                Hash = id;
            }

            #region ILockable Members

            public int Hash { get; private set; }

            public object Lock
            {
                get
                {
                    return this;
                }
            }

            #endregion
        }

        #endregion
    }
}