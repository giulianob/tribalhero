namespace Game.Comm
{
    public interface IPolicyServer
    {
        bool Start(string listenAddress, int port);

        bool Stop();
    }
}