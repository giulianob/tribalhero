namespace Game.Util.Locking
{
    public interface ILockable
    {
        int Hash { get; }

        object Lock { get; }
    }
}