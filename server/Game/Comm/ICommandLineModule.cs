namespace Game.Comm
{
    public interface ICommandLineModule
    {
        void RegisterCommands(CommandLineProcessor processor);
    }
}