namespace Game.Util.Locking
{
    public interface ICallbackLock
    {
        IMultiObjectLock Lock(CallbackLock.CallbackLockHandler lockHandler,
                              object[] lockHandlerParams,
                              params ILockable[] baseLocks);
    }
}