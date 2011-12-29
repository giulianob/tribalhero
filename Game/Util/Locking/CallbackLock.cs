using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Data;
using Game.Setup;
using Ninject;

namespace Game.Util.Locking
{
    public class CallbackLock : IDisposable
    {
        #region Delegates

        public delegate ILockable[] CallbackLockHandler(object[] custom);

        public delegate CallbackLock Factory();

        #endregion

        private IMultiObjectLock currentLock;
        
        public CallbackLock Lock(CallbackLockHandler lockHandler, object[] lockHandlerParams, params ILockable[] baseLocks)
        {
            int count = 0;
            while (currentLock == null)
            {
                if ((++count)%10 == 0)
                    Global.Logger.Info(string.Format("CallbackLock has iterated {0} times from {1}", count, Environment.StackTrace));

                if (count >= 10000)
                    throw new LockException("Callback lock exceeded maximum count");

                var toBeLocked = new List<ILockable>(baseLocks);

                // Lock the base objects
                using (Concurrency.Current.Lock(baseLocks))
                {
                    // Grab the list of objects we need to lock from the callback                    
                    toBeLocked.AddRange(lockHandler(lockHandlerParams));
                }

                // Lock all of the objects we believe should be locked
                currentLock = Concurrency.Current.Lock(toBeLocked.ToArray());

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
                    continue;

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