using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Procedures;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Troop {
    /// <summary>
    /// Summary description for TroopProcedureTest
    /// </summary>
    [TestClass]
    public class CallbackLockTest {

        class DummyLockable : ILockable {

            public DummyLockable(int id) {
                Hash = id;
            }

            public int Hash { get; private set; }

            public object Lock {
                get { return this; }
            }
        }

        [TestInitialize]
        public void TestInitialize() { }

        [TestCleanup]
        public void TestCleanup() { }

        [TestMethod]
        public void TestEmptyListFromCallback() {
            DummyLockable obj = new DummyLockable(1);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {};
            CallbackLock lck = new CallbackLock(lockFunc, null, obj);
            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            lck.Dispose();
            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
        }

        [TestMethod]
        public void TestSingleItemFromCallback() {
            DummyLockable obj = new DummyLockable(1);
            DummyLockable obj2 = new DummyLockable(2);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] { obj2 };
            CallbackLock lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
        }

        [TestMethod]
        public void TestMultiItemFromCallback() {
            DummyLockable obj = new DummyLockable(1);
            DummyLockable obj2 = new DummyLockable(2);
            DummyLockable obj3 = new DummyLockable(3);
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] { obj3, obj2 };
            CallbackLock lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));
        }

        [TestMethod]
        public void TestMultiBaseItems() {
            DummyLockable obj = new DummyLockable(1);
            DummyLockable obj2 = new DummyLockable(2);            
            CallbackLock.CallbackLockHandler lockFunc = custom => new ILockable[] {  };
            CallbackLock lck = new CallbackLock(lockFunc, null, obj, obj2);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));            

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));            
        }

        [TestMethod]
        public void TestChangingCallbackAddLocks() {
            DummyLockable obj = new DummyLockable(1);
            DummyLockable obj2 = new DummyLockable(2);
            DummyLockable obj3 = new DummyLockable(3);

            int callCount = 0;
            CallbackLock.CallbackLockHandler lockFunc = delegate {
                if (callCount == 0) {
                    callCount++;
                    return new ILockable[] { obj2 };                    
                }
                                                            
                callCount++;
                return new ILockable[] { obj3, obj2 };
            };
            CallbackLock lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));
        }

        [TestMethod]
        public void TestChangingCallbackRemoveLocks() {
            DummyLockable obj = new DummyLockable(1);
            DummyLockable obj2 = new DummyLockable(2);
            DummyLockable obj3 = new DummyLockable(3);

            int callCount = 0;
            CallbackLock.CallbackLockHandler lockFunc = delegate {
                if (callCount == 0) {
                    callCount++;
                    return new ILockable[] { obj2, obj3 };
                }

                callCount++;
                return new ILockable[] { obj2 };
            };
            CallbackLock lck = new CallbackLock(lockFunc, null, obj);

            Assert.IsTrue(MultiObjectLock.IsLocked(obj));
            Assert.IsTrue(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));

            lck.Dispose();

            Assert.IsFalse(MultiObjectLock.IsLocked(obj));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj2));
            Assert.IsFalse(MultiObjectLock.IsLocked(obj3));
        }
    }
}