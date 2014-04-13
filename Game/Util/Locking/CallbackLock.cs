using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;

namespace Game.Util.Locking
{
    public class CallbackLock : ICallbackLock
    {
        private static readonly ILogger logger = LoggerFactory.Current.GetLogger<CallbackLock>();

        private readonly DefaultMultiObjectLock.Factory multiObjectLockFactory;

        private IMultiObjectLock currentLock;

        #region Delegates

        public delegate ILockable[] CallbackLockHandler(object[] custom);

        public delegate ICallbackLock Factory();

        #endregion

        public CallbackLock(DefaultMultiObjectLock.Factory multiObjectLockFactory)
        {
            this.multiObjectLockFactory = multiObjectLockFactory;
        }

        private int GetWaitRetrySleep(int attempts)
        {
            if (attempts <= 1)
            {
                return 0;
            }

            return Math.Min(500, attempts * attempts * 3);
        }

        public IMultiObjectLock Lock(CallbackLockHandler lockHandler,
                                 object[] lockHandlerParams,
                                 params ILockable[] baseLocks)
        {
            int attempts = 0;
            while (currentLock == null)
            {
                if ((++attempts) % 10 == 0)
                {
                    logger.Warn(string.Format("CallbackLock has iterated {0} times {1}", attempts, Environment.StackTrace));
                }

                if (attempts > 300)
                {
                    throw new LockException("Callback lock exceeded maximum count");
                }

                var toBeLocked = new List<ILockable>(baseLocks);

                // Lock the base objects
                multiObjectLockFactory().Lock(baseLocks)
                                        .Do(() => toBeLocked.AddRange(lockHandler(lockHandlerParams)));

                // Lock all of the objects we believe should be locked
                currentLock = multiObjectLockFactory();
                currentLock.Lock(toBeLocked.ToArray());

                // Grab the current list of objects we need to lock from the callback
                var newToBeLocked = new List<ILockable>(baseLocks);
                newToBeLocked.AddRange(lockHandler(lockHandlerParams));

                // Make sure they are still all the same
                if (newToBeLocked.Count != toBeLocked.Count)
                {
                    currentLock.UnlockAll();
                    currentLock = null;
                    Thread.Sleep(GetWaitRetrySleep(attempts));
                    continue;
                }

                if (!newToBeLocked.Where((t, i) => t.Hash != toBeLocked[i].Hash).Any())
                {
                    continue;
                }

                currentLock.UnlockAll();
                currentLock = null;
                Thread.Sleep(GetWaitRetrySleep(attempts));
            }

            return currentLock;
        }
    }
}