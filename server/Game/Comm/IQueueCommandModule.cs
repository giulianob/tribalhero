namespace Game.Comm
{
    public interface IQueueCommandModule
    {
        void RegisterCommands(IQueueCommandProcessor queueCommandProcessor);
    }
}