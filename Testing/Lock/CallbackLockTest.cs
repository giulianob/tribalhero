#region

using Game.Util;
using Game.Util.Locking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.Lock
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class CallbackLockTest : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestEmptyListFromCallback()
        {
            var obj = new DummyLockable(1);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {};
            var lck = new CallbackLock().Lock(lockFunc, null, obj);
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj));
            lck.Dispose();
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj));
        }

        [TestMethod]
        public void TestSingleItemFromCallback()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {obj2};
            var lck = new CallbackLock().Lock(lockFunc, null, obj);

            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj2));

            lck.Dispose();

            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj2));
        }

        [TestMethod]
        public void TestMultiItemFromCallback()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            var obj3 = new DummyLockable(3);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {obj3, obj2};
            var lck = new CallbackLock().Lock(lockFunc, null, obj);

            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj2));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj3));
        }

        [TestMethod]
        public void TestMultiBaseItems()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {};
            var lck = new CallbackLock().Lock(lockFunc, null, obj, obj2);

            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj2));

            lck.Dispose();

            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj2));
        }

        [TestMethod]
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
            var lck = new CallbackLock().Lock(lockFunc, null, obj);

            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj2));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj3));
        }

        [TestMethod]
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
            var lck = new CallbackLock().Lock(lockFunc, null, obj);

            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsTrue(DefaultMultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(DefaultMultiObjectLock.IsLocked(obj3));
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