namespace Game.Comm
{
    public interface ITcpServer
    {
        bool Start(string listenAddress, int port);

        bool Stop();

        int GetSessionCount();

        string GetAllSocketStatus();

        string DisconnectAll();
    }
}