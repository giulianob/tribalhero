using System;

namespace Game.Util.Locking
{
    public class LockException : Exception
    {
        public LockException(string message)
                : base(message)
        {
        }
    }
}