namespace Game.Comm
{
    public interface IProcessor
    {
        void RegisterCommand(Command cmd, Processor.DoWork func);

        void RegisterEvent(Command cmd, Processor.DoWork func);

        void Execute(Session session, Packet packet);

        void ExecuteEvent(Session session, Packet packet);
    }
}