using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Util.Locking
{
    public static class Concurrency
    {
        private static ILocker locker = new DefaultLocker(() => new TransactionalMultiObjectLock(new DefaultMultiObjectLock()));

        public static ILocker Current
        {
            get
            {
                return locker;
            }
            set
            {
                locker = value;
            }
        }
    }
}
