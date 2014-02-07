using System;
using System.Collections.Generic;
using System.Threading;
using Common.Testing;
using FluentAssertions;
using Game.Util.Locking;
using NSubstitute;
using Persistance;
using Xunit.Extensions;

namespace Testing.LockingTests
{
    public class DefaultMultiObjectLockTests : IDisposable
    {
        private readonly DefaultMultiObjectLock locker;

        private readonly List<object> enterObjects;

        private readonly List<object> exitObjects;

        private readonly IDbManager dbManager;

        public DefaultMultiObjectLockTests()
        {
            dbManager = Substitute.For<IDbManager>();
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
                        },
                        dbManager);
        }

        public void Dispose()
        {
            DefaultMultiObjectLock.ClearCurrentLock();
        }

        [Theory, AutoNSubstituteData]
        public void Lock_ReturnsThis()
        {
            locker.Lock(new ILockable[0]).Should().Be(locker);
        }

        [Theory, AutoNSubstituteData]
        public void Lock_InitializesThreadTransaction()
        {
            locker.Lock(new ILockable[0]);

            dbManager.Received(1).GetThreadTransaction();
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
        public void UnlockAll_WhenHasThreadedTransaction_ShouldDisposeTransaction()
        {
            var transaction = Substitute.For<DbTransaction>();
            dbManager.GetThreadTransaction(true).Returns(transaction);
            dbManager.GetThreadTransaction().Returns(transaction);

            locker.Lock(new ILockable[0]);
            locker.UnlockAll();

            transaction.Received(1).Dispose();
        }    
        
        [Theory, AutoNSubstituteData]
        public void UnlockAll_WhenDoesNotHaveThreadedTransaction_ShouldStillUnlock(
            ALock aLock)
        {
            dbManager.GetThreadTransaction(true).Returns((DbTransaction) null);

            locker.Lock(new ILockable[] { aLock });
            locker.UnlockAll();

            exitObjects.Should().Equal(aLock.Lock);
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
                        locker.Lock(new ILockable[] {cLock, nullLock, bLock, aLock}).Do(() =>
                        {
                            locked = true;
                        });
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

        [Theory, AutoNSubstituteData]
        public void Do_Void_ExecutesProtectedBlock(
            ILockable lck)
        {
            bool wasCalled = false;
            Action block = () => { wasCalled = true; };
            locker.Lock(new[] {lck}).Do(block);

            wasCalled.Should().BeTrue();
        }

        [Theory, AutoNSubstituteData]
        public void Do_Void_CommitsTransactionAndUnlocksAll(
            ALock lck, 
            DbTransaction transaction)
        {
            dbManager.GetThreadTransaction(Arg.Any<bool>()).Returns(transaction);

            locker.Lock(new[] {lck}).Do(() => { });

            transaction.Received(1).Dispose();
            exitObjects.Should().Equal(lck.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void Do_Void_WhenBlockThrowsException_ShouldNotUnlockAndShouldRollback(
            ALock lck, 
            DbTransaction transaction)
        {
            dbManager.GetThreadTransaction(Arg.Any<bool>()).Returns(transaction);

            Action act = () =>
            {
                locker.Lock(new[] {lck}).Do(() => { throw new Exception("Test"); });
            };

            act.ShouldThrow<Exception>().WithMessage("Test");
            transaction.Received(1).Rollback();
            transaction.Received(1).Dispose();
            exitObjects.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void Do_Generic_ExecutesProtectedBlock(
            ILockable lck)
        {
            bool wasCalled = false;
            Func<string> block = () =>
            {
                wasCalled = true;
                return "hello";
            };
            locker.Lock(new[] {lck}).Do(block);

            wasCalled.Should().BeTrue();
        }

        [Theory, AutoNSubstituteData]
        public void Do_Generic_CommitsTransactionAndUnlocksAll(
            ALock lck, 
            DbTransaction transaction)
        {
            dbManager.GetThreadTransaction(Arg.Any<bool>()).Returns(transaction);

            locker.Lock(new[] {lck}).Do(() => "Hello");

            transaction.Received(1).Dispose();
            exitObjects.Should().Equal(lck.Lock);
        }

        [Theory, AutoNSubstituteData]
        public void Do_Generic_WhenBlockThrowsException_ShouldNotUnlockAndShouldRollback(
            ALock lck, 
            DbTransaction transaction)
        {
            dbManager.GetThreadTransaction(Arg.Any<bool>()).Returns(transaction);

            Action act = () => { locker.Lock(new[] {lck}).Do<string>(() => { throw new Exception("Test"); }); };

            act.ShouldThrow<Exception>().WithMessage("Test");
            transaction.Received(1).Rollback();
            transaction.Received(1).Dispose();
            exitObjects.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void Do_Generic_ShouldReturnValueBlockReturns(
            ALock lck, 
            DbTransaction transaction,
            string blockResult)
        {
            dbManager.GetThreadTransaction(Arg.Any<bool>()).Returns(transaction);

            var actualResult = locker.Lock(new[] {lck}).Do(() => blockResult);

            actualResult.Should().Be(blockResult);
        }
        
        public class ALock : ILockable
        {
            public ALock()
            {
                Lock = new object();
            }

            public virtual int Hash { get; set; }

            public virtual object Lock { get; set; }
        }

        public class BLock : ILockable
        {
            public BLock()
            {
                Lock = new object();
            }

            public virtual int Hash { get; set; }

            public virtual object Lock { get; set; }
        }

        public class CLock : ILockable
        {
            public CLock()
            {
                Lock = new object();
            }

            public virtual int Hash { get; set; }

            public virtual object Lock { get; set; }
        }
    }
}