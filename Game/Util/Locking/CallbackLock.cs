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

        public IMultiObjectLock Lock(CallbackLockHandler lockHandler,
                                 object[] lockHandlerParams,
                                 params ILockable[] baseLocks)
        {
            int count = 0;
            while (currentLock == null)
            {
                if ((++count) % 10 == 0)
                {
                    logger.Warn(string.Format("CallbackLock has iterated {0} times {1}", count, Environment.StackTrace));
                }

                if (count >= 10000)
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
                    Thread.Sleep(0);
                    continue;
                }

                if (!newToBeLocked.Where((t, i) => t.Hash != toBeLocked[i].Hash).Any())
                {
                    continue;
                }

                currentLock.UnlockAll();
                currentLock = null;
                Thread.Sleep(0);
            }

            return currentLock;
        }
    }
}