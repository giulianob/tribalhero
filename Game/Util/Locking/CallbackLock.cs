using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Util.Locking
{
    public class CallbackLock : IDisposable
    {
        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        private IMultiObjectLock currentLock;

        #region Delegates

        public delegate ILockable[] CallbackLockHandler(object[] custom);

        public delegate CallbackLock Factory();

        #endregion

        public CallbackLock(DefaultMultiObjectLock.Factory multiObjectLockFactory)
        {
            this.multiObjectLockFactory = multiObjectLockFactory;
        }

        public CallbackLock Lock(CallbackLockHandler lockHandler,
                                 object[] lockHandlerParams,
                                 params ILockable[] baseLocks)
        {
            int count = 0;
            while (currentLock == null)
            {
                if ((++count) % 10 == 0)
                {
                    LoggerFactory.Current.GetCurrentClassLogger()
                                 .Warn(string.Format("CallbackLock has iterated {0} times", count));
                }

                if (count >= 10000)
                {
                    throw new LockException("Callback lock exceeded maximum count");
                }

                var toBeLocked = new List<ILockable>(baseLocks);

                // Lock the base objects
                using (var baseLock = multiObjectLockFactory())
                {
                    baseLock.Lock(baseLocks);
                    // Grab the list of objects we need to lock from the callback                    
                    toBeLocked.AddRange(lockHandler(lockHandlerParams));
                }

                // Lock all of the objects we believe should be locked
                currentLock = multiObjectLockFactory();
                currentLock.Lock(toBeLocked.ToArray());

                // Grab the current list of objects we need to lock from the callback
                var newToBeLocked = new List<ILockable>(baseLocks);
                newToBeLocked.AddRange(lockHandler(lockHandlerParams));

                // Make sure they are still all the same
                if (newToBeLocked.Count != toBeLocked.Count)
                {
                    currentLock.Dispose();
                    currentLock = null;
                    Thread.Sleep(0);
                    continue;
                }

                if (!newToBeLocked.Where((t, i) => t.Hash != toBeLocked[i].Hash).Any())
                {
                    continue;
                }

                currentLock.Dispose();
                currentLock = null;
                Thread.Sleep(0);
            }

            return this;
        }

        #region IDisposable Members

        public void Dispose()
        {
            currentLock.Dispose();
        }

        #endregion
    }
}