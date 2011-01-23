#region

using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Testing.Troop
{
    /// <summary>
    ///   Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class CallbackLockTest
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
            var lck = new CallbackLock(lockFunc, null, obj);
            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            lck.Dispose();
            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
        }

        [TestMethod]
        public void TestSingleItemFromCallback()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {obj2};
            var lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
        }

        [TestMethod]
        public void TestMultiItemFromCallback()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            var obj3 = new DummyLockable(3);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {obj3, obj2};
            var lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));
        }

        [TestMethod]
        public void TestMultiBaseItems()
        {
            var obj = new DummyLockable(1);
            var obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {};
            var lck = new CallbackLock(lockFunc, null, obj, obj2);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
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
            var lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));
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
            var lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));
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