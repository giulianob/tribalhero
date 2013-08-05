using System;
using System.Collections.Generic;
using System.Threading;
using Common.Testing;
using FluentAssertions;
using Game.Util.Locking;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.LockingTests
{
    public class DefaultMultiObjectLockTests : IDisposable
    {
        private readonly DefaultMultiObjectLock locker;

        private readonly List<object> enterObjects;

        private readonly List<object> exitObjects;

        public DefaultMultiObjectLockTests()
        {
            enterObjects = new List<object>();
            exitObjects = new List<object>();

            locker = new DefaultMultiObjectLock(
                    item =>
                        {
                            Monitor.Enter(item);
                            enterObjects.Add(item);
                        },
                    item =>
                        {
                            Monitor.Exit(item);
                            exitObjects.Add(item);
                        });
        }

        public void Dispose()
        {
            locker.Dispose();
        }

        [Theory, AutoNSubstituteData]
        public void Lock_ReturnsThis()
        {
            locker.Lock(new ILockable[0]).Should().Be(locker);
        }

        [Theory, AutoNSubstituteData]
        public void Lock_WhenNested_ShouldThrowException()
        {
            locker.Lock(new ILockable[0]);

            Action act = () => locker.Lock(new ILockable[0]);

            act.ShouldThrow<LockException>();
        }

        [Theory, AutoNSubstituteData]
        public void Lock_WhenCalledWithMultipleObjects_ShouldLockInHashOrder(ILockable lock1, ILockable lock2, ILockable lock3)
        {
            lock1.Lock.Returns(new object());
            lock2.Lock.Returns(new object());
            lock3.Lock.Returns(new object());

            lock1.Hash.Returns(100);
            lock2.Hash.Returns(200);
            lock3.Hash.Returns(300);

            locker.Lock(new[] {lock3, lock1, lock2});

            enterObjects.Should().Equal(lock1.Lock, lock2.Lock, lock3.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void IsLocked_ShouldTellIfObjectsAreLocked(ILockable lock1, ILockable lock2, ILockable lock3)
        {
            lock1.Lock.Returns(new object());
            lock2.Lock.Returns(new object());
            lock3.Lock.Returns(new object());

            lock1.Hash.Returns(100);
            lock2.Hash.Returns(200);
            lock3.Hash.Returns(300);

            locker.Lock(new[] {lock1, lock2});

            DefaultMultiObjectLock.IsLocked(lock1).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(lock2).Should().BeTrue();
            DefaultMultiObjectLock.IsLocked(lock3).Should().BeFalse();
        }

        [Theory, AutoNSubstituteData]
        public void Lock_WhenCalledWithNullObject_ShouldNotLockNullObjects(ILockable lock1, ILockable lock2)
        {
            lock1.Lock.Returns(null);
            lock2.Lock.Returns(new object());

            lock1.Hash.Returns(100);
            lock2.Lock.Returns(200);

            locker.Lock(new[] {lock1, lock2});

            enterObjects.Should().Equal(lock2.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void Lock_WhenCalledWithCollidingHashes_ShouldUseTypeNameToSort(ALock aLock, BLock bLock, CLock cLock)
        {
            aLock.Lock = new object();
            bLock.Lock = new object();
            cLock.Lock = new object();

            aLock.Hash = 100;
            bLock.Hash = 100;
            cLock.Hash = 100;

            locker.Lock(new ILockable[] {aLock, cLock, bLock});

            enterObjects.Should().Equal(aLock.Lock, bLock.Lock, cLock.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void UnlockAll_WhenCalled_ShouldUnlockObjectsInReverseOrder(ALock aLock, BLock bLock, CLock cLock)
        {
            aLock.Lock = new object();
            bLock.Lock = new object();
            cLock.Lock = new object();

            aLock.Hash = 100;
            bLock.Hash = 100;
            cLock.Hash = 100;

            locker.Lock(new ILockable[] {aLock, cLock, bLock});

            exitObjects.Should().BeEmpty();

            locker.UnlockAll();

            exitObjects.Should().Equal(cLock.Lock, bLock.Lock, aLock.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void UnlockAll_WhenCalledMultipleTimes_ShouldNotUnlockObjectsTwice(ALock aLock, BLock bLock, CLock cLock)
        {
            aLock.Lock = new object();
            bLock.Lock = new object();
            cLock.Lock = new object();

            aLock.Hash = 100;
            bLock.Hash = 100;
            cLock.Hash = 100;

            locker.Lock(new ILockable[] {aLock, cLock, bLock});

            locker.UnlockAll();
            locker.UnlockAll();

            exitObjects.Should().Equal(cLock.Lock, bLock.Lock, aLock.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void UnlockAll_WhenCalled_ShouldAllowToLockAgain(ALock aLock, BLock bLock, CLock cLock)
        {
            aLock.Lock = new object();
            bLock.Lock = new object();
            cLock.Lock = new object();

            aLock.Hash = 100;
            bLock.Hash = 100;
            cLock.Hash = 100;

            locker.Lock(new ILockable[] {aLock, cLock, bLock});
            locker.UnlockAll();

            locker.Lock(new ILockable[] {aLock, cLock, bLock});
            locker.UnlockAll();

            enterObjects.Should().Equal(aLock.Lock, bLock.Lock, cLock.Lock, aLock.Lock, bLock.Lock, cLock.Lock);
            exitObjects.Should().Equal(cLock.Lock, bLock.Lock, aLock.Lock, cLock.Lock, bLock.Lock, aLock.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void Dispose_ShouldUnlockAll(ALock aLock, BLock bLock, CLock cLock)
        {
            aLock.Lock = new object();
            bLock.Lock = new object();
            cLock.Lock = new object();

            aLock.Hash = 100;
            bLock.Hash = 100;
            cLock.Hash = 100;

            locker.Lock(new ILockable[] {aLock, cLock, bLock});

            locker.Dispose();

            exitObjects.Should().Equal(cLock.Lock, bLock.Lock, aLock.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void Lock_Integration(ALock aLock, BLock bLock, CLock cLock, ALock nullLock)
        {
            nullLock.Lock = null;
            nullLock.Hash = 0;
            aLock.Lock = new object();
            aLock.Hash = 1;
            bLock.Lock = new object();
            bLock.Hash = 2;
            cLock.Lock = new object();
            cLock.Hash = 3;
            
            Monitor.Enter(cLock.Lock);

            bool locked = false;
            bool hasException = false;
            var thread = new Thread(() =>
                {
                    try
                    {
                        using (locker.Lock(new ILockable[] {cLock, nullLock, bLock, aLock}))
                        {
                            locked = true;
                        }
                    }
                    catch(Exception)
                    {
                        hasException = true;
                    }
                });

            thread.Start();
            Thread.Sleep(200);

            locked.Should().BeFalse();
            enterObjects.Should().Equal(aLock.Lock, bLock.Lock);

            Monitor.Exit(cLock.Lock);
            thread.Join(1000);

            hasException.Should().BeFalse();
            enterObjects.Should().Equal(aLock.Lock, bLock.Lock, cLock.Lock);
            exitObjects.Should().Equal(cLock.Lock, bLock.Lock, aLock.Lock);
        }

        public class ALock : ILockable
        {
            public virtual int Hash { get; set; }

            public virtual object Lock { get; set; }
        }

        public class BLock : ILockable
        {
            public virtual int Hash { get; set; }

            public virtual object Lock { get; set; }
        }

        public class CLock : ILockable
        {
            public virtual int Hash { get; set; }

            public virtual object Lock { get; set; }
        }
    }
}