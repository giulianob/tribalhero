using System.Collections.Generic;

namespace Game.Util.Locking
{
    public class LockableComparer : IEqualityComparer<ILockable>
    {
        public bool Equals(ILockable x, ILockable y)
        {
            return x.Hash == y.Hash;
        }
            
        public int GetHashCode(ILockable obj)
        {
            return obj.Hash;
        }
    }
}